namespace MCPSDK.API.Commands
{
    public class CommandRegistry : ICommandRegistry
    {
        private readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();
        public void RegisterCommand(ICommand command)
        {
            _commands[command.CommandName] = command;
        }

        public bool TryGetCommand(string commandName, out ICommand? command)
        {
            return _commands.TryGetValue(commandName, out command);
        }

        public void ClearCommands()
        {
            _commands.Clear();
        }

        public IEnumerable<string> GetRegisteredCommands()
        {
            return _commands.Keys;
        }
    }
}
