
using ModelContextProtocol.Client;
using OpenAI.Chat;
using System.Text;

namespace MCPSDK.Agents
{
    public abstract class OpenAiAgentBase : IAgent
    {
        public string OpenAIApiKey { get; private set; }

        public int HistoryCount { get; set; }

        public ChatMessage SystemMessage { get; private set; }

        public List<ChatMessage> ChatMessages { get; private set; } = [];

        public ChatClient Agent { get; private set; }

        private readonly ChatCompletionOptions Options = new();

        protected OpenAiAgentBase(string openAiKey, string systemMessage, string aiModelName, int historyCount)
        {
            this.SystemMessage = ChatMessage.CreateSystemMessage(systemMessage);
            this.OpenAIApiKey = openAiKey;

            this.Agent = new ChatClient(model: aiModelName, apiKey: openAiKey);

            this.ChatMessages.Add(this.SystemMessage);
            HistoryCount = historyCount;
        }


        protected void SetChatTools(IList<McpClientTool> _tools)
        {

            foreach (var mcpTool in _tools)
            {
                if (mcpTool is null) continue;

                var mcpToolSchema = mcpTool.JsonSchema;

                var args = BinaryData.FromObjectAsJson(mcpToolSchema);


                var chatTool = ChatTool.CreateFunctionTool(mcpTool.Name, mcpTool.Description, args);

                if (chatTool is not null)
                {
                    this.Options.Tools.Add(chatTool);
                    //this.Tools.Add(mcpTool);
                }
            }
        }

        public void ClearHisotry()
        {
            this.ChatMessages = [];
            this.ChatMessages.Add(this.SystemMessage);
        }

        private int EstimateTokens(string text)
        {
            // Rough estimate: 4 characters per token (very approximate)
            return text.Length / 4;
        }

        private int EstimateChatMessageTokens(ChatMessage msg)
        {
            var content = string.Join("", msg.Content.Select(c => c.Text));

            int baseTokens = EstimateTokens(content) + 10; // 10 for role and formatting overhead
            return baseTokens;
        }

        private void TrimHistory()
        {
            int maxMessages = this.HistoryCount * 2;

            var nonSystemMessages = this.ChatMessages.Skip(1).ToList();

            if (nonSystemMessages.Count > maxMessages)
            {
                nonSystemMessages = nonSystemMessages.Skip(nonSystemMessages.Count - maxMessages).ToList();
            }

            this.ChatMessages = new List<ChatMessage> { this.SystemMessage };
            this.ChatMessages.AddRange(nonSystemMessages);
        }


        private void TrimHistory(int modelContextWindow, int maxOutputTokens)
        {
            int availablePromptTokens = modelContextWindow - maxOutputTokens;

            // Always include SystemMessage first
            List<ChatMessage> trimmedMessages = new List<ChatMessage> { this.SystemMessage };

            var messages = this.ChatMessages.Skip(1).ToList();
            int totalTokens = EstimateChatMessageTokens(this.SystemMessage);

            // Iterate backwards to keep latest messages first
            for (int i = messages.Count - 1; i >= 0; i--)
            {
                var msg = messages[i];
                int msgTokens = EstimateChatMessageTokens(msg);

                if (totalTokens + msgTokens > availablePromptTokens)
                    break;

                trimmedMessages.Insert(1, msg);  // Insert before user query
                totalTokens += msgTokens;
            }

            this.ChatMessages = trimmedMessages;
        }


        protected async Task ProcessUserQuery(string query,int maxTokens,
            Action<string>onAiResponse,
            Func<string,BinaryData,Task<string>>toolCallFunction,
            Action<string>onError)
        {
            try
            {
                bool requiresAction;
                var userQuery = new UserChatMessage(query);
                this.ChatMessages.Add(userQuery);

                // After adding user query, trim history to fit within context window
                TrimHistory();
                string toolResponseJsonString = string.Empty;
                do {
                    requiresAction = false;

                    // Inject maxTokens into Options
                    //this.Options.MaxOutputTokenCount = maxTokens;

                    ChatCompletion completion = await this.Agent.CompleteChatAsync(this.ChatMessages, this.Options);
              
                    switch (completion.FinishReason)
                    {
                        case ChatFinishReason.Stop:
                            //Add the assistant message to the conversation hisotry
                            this.ChatMessages.Add(new AssistantChatMessage(completion));
                            onAiResponse(GetAiResponseText(completion));
                            break;
                        case ChatFinishReason.ToolCalls:

                            if (!string.IsNullOrEmpty(toolResponseJsonString))
                            {
                               
                                onAiResponse(toolResponseJsonString);
                            }
                            // First, add the assistant message with tool calls to the conversation history.
                            this.ChatMessages.Add(new AssistantChatMessage(completion));

                            // Then, add a new tool message for each tool call that is resolved.
                            foreach (ChatToolCall toolCall in completion.ToolCalls)
                            {
                              toolResponseJsonString = await toolCallFunction(toolCall.FunctionName,
                                  toolCall.FunctionArguments);

                                if (toolResponseJsonString != null && toolResponseJsonString != string.Empty)
                                {
                                    var toolChatMessage = new ToolChatMessage(toolCall.Id, toolResponseJsonString);
                                    this.ChatMessages.Add(toolChatMessage);
                                }
                            }
                            requiresAction = true;
                            break;
                        case ChatFinishReason.Length:
                            onError("Incomplete model output due to MaxTokens parameter or token limit exceeded.");
                            break;
                        case ChatFinishReason.ContentFilter:
                            onError("Omitted content due to a content filter flag.");
                            break;
                        case ChatFinishReason.FunctionCall:
                            onError("Deprecated in favor of tool calls.");
                            break;
                        default:
                            onError(completion.FinishReason.ToString());
                            break;
                    }

                } while (requiresAction);

            }
            catch (Exception ex)
            {
                this.ClearHisotry();
                onError("Unable to process the query. Something went wrong");
            }
        }




        public string GetAiResponseText(ChatCompletion completion)
        {
            var chatParts = completion.Content.ToList();

            var builder = new StringBuilder();
            foreach (var part in chatParts)
            {
                builder.Append(part.Text);
            }

            return builder.ToString();
        }
    }
}
