

namespace MCPSDK.Exceptions
{
    public class CommandExecutionException : Exception
    {
        public int ErrorCode { get; }

        public object? ErrorData { get; }

        public CommandExecutionException(string message)
            : base(message)
        {
            ErrorCode = -32603;
        }

        public CommandExecutionException(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public CommandExecutionException(string message, int errorCode, object errorData)
            : base(message)
        {
            ErrorCode = errorCode;
            ErrorData = errorData;
        }
    }
}
