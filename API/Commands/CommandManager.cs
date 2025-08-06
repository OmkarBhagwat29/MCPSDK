using MCPSDK.API.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace MCPSDK.API.Commands
{
    public abstract class CommandManager
    {
        public readonly ILogger _logger;

        public readonly ICommandRegistry _commandRegistry;
        public CommandManager(ILogger logger, ICommandRegistry commandRegistry)
        {
            _commandRegistry = commandRegistry;
            _logger = logger;
        }

        public abstract void LoadCommands();
    }
}
