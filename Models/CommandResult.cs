
namespace MCPSDK.Models
{
    public class CommandResult
    {
        public bool Success { get; set; }

        public object? Data { get; set; }

        public string? ErrorMessage { get; set; }

        public static CommandResult CreateSuccess(object? data = null)
        {
            return new CommandResult
            {
                Success = true,
                Data = data,
                ErrorMessage = null
            };
        }

        public static CommandResult CreateError(string errorMessage, object? data = null)
        {
            return new CommandResult
            {
                Success = false,
                Data = data,
                ErrorMessage = errorMessage
            };
        }
    }
}
