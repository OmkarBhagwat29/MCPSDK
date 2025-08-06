
using ModelContextProtocol.Client;

namespace MCPSDK
{
    public class MCPManager
    {
        private IMcpClient? _client;

        public IMcpClient MCPClient => _client ?? throw new InvalidOperationException("MCP client not initialized.");

        public IList<McpClientTool> Tools { get; set; } = [];

        public async Task InitialzeClientAsync(string serverPath)
        {

            var (command, arguments) = GetCommandAndArguments([serverPath]);
            var clientTransport = new StdioClientTransport(new()
            {
                Name = "Revit MCP Server",
                Command = command,
                Arguments = arguments,
            });


            _client = await McpClientFactory.CreateAsync(clientTransport);
            
            this.Tools = await _client.ListToolsAsync();
        }


        static (string command, string[] arguments) GetCommandAndArguments(string[] args)
        {
            return args switch
            {
                [var script] when script.EndsWith(".py") => ("python", args),
                [var script] when script.EndsWith(".js") => ("node", args),
                [var script] when Directory.Exists(script) || (File.Exists(script) && script.EndsWith(".csproj")) => ("dotnet", ["run", "--project", script, "--no-build"]),
                _ => throw new NotSupportedException("An unsupported server script was provided. Supported scripts are .py, .js, or .csproj")
            };
        }
    }
}
