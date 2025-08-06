using MCPSDK.Models;
using Newtonsoft.Json.Linq;


namespace MCPSDK.API.Commands
{
    public interface ICommand
    {
        string CommandName { get; }

        CommandResult Execute(JObject parameters, string requestId);
    }
}
