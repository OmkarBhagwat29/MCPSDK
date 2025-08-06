
namespace MCPSDK.Models.JsonRPC
{
    public class JsonRPCSerializationException : Exception
    {
        public JsonRPCSerializationException(string message) : base(message)
        {
        }

        public JsonRPCSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
