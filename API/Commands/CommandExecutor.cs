using MCPSDK.Exceptions;
using MCPSDK.Models.JsonRPC;
using Microsoft.Extensions.Logging;
using Serilog.Core;


namespace MCPSDK.API.Commands
{
    public class CommandExecutor
    {
        private readonly ICommandRegistry _commandRegistry;
        private readonly ILogger _logger;
        public CommandExecutor(ICommandRegistry commandRegistry, ILogger logger)
        {
            _logger = logger;
            _commandRegistry = commandRegistry;
        }

        public string ExecuteCommand(JsonRPCRequest request)
        {
                var methodName = request.Method;
                var args = request.Params;

                //try get command
                if (!_commandRegistry.TryGetCommand(request.Method, out var command) || command is null)
                {
                    _logger.LogWarning($"No command found with name: {request.Method}");

                    return JsonRPCErrorResponse.CreateErrorResponse(request.Id,
                        JsonRPCErrorCodes.MethodNotFound,
                        $"No command found with name: {request.Method}");
                }

                _logger.LogInformation($"Command found: {request.Method}");

                try
                {
                    var paramObj = request.GetParamsObject();
                    object result = command.Execute(paramObj, request.Id);
                    _logger.LogInformation($"command executed successfully: {request.Method}");

                    return JsonRPCSuccessResponse.CreateSuccessResponse(request.Id, result);
                }
                catch (CommandExecutionException ex)
                {
                    _logger.LogError("Error while executing {0}\n Message: {1}", request.Method, ex.Message);
                    return JsonRPCErrorResponse.CreateErrorResponse(request.Id,
                                                ex.ErrorCode,
                                                ex.Message,
                                                ex.ErrorData);
                }
            


        }
    }
}
