using MCPSDK.Models;
using Newtonsoft.Json.Linq;


namespace MCPSDK.API.Commands
{
    public abstract class ExternalCommandBase : ICommand
    {
        public abstract string CommandName { get; }

        public abstract CommandResult Execute(JObject parameters, string requestId);
    }
}
