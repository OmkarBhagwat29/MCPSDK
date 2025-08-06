using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCPSDK.Models.JsonRPC
{
    public static class JsonRPCErrorCodes
    {
        #region Standard JSON-RPC 2.0 Error Codes (-32768 to -32000)

        /// <summary>
        ///     Invalid JSON format.
        ///     The server received invalid JSON.
        /// </summary>
        public const int ParseError = -32700;

        /// <summary>
        ///     Invalid JSON-RPC request.
        ///     The JSON sent is not a valid Request object.
        /// </summary>
        public const int InvalidRequest = -32600;

        /// <summary>
        ///     The requested method does not exist or is unavailable.
        /// </summary>
        public const int MethodNotFound = -32601;

        /// <summary>
        ///     Invalid method parameters.
        ///     Method parameters are invalid or incorrectly formatted.
        /// </summary>
        public const int InvalidParams = -32602;

        /// <summary>
        ///     Internal JSON-RPC error.
        ///     Generic server error occurred while processing the request.
        /// </summary>
        public const int InternalError = -32603;

        /// <summary>
        ///     Start range for server errors.
        ///     Reserved for implementation-defined server errors.
        /// </summary>
        public const int ServerErrorStart = -32000;

        /// <summary>
        ///     End range for server errors.
        /// </summary>
        public const int ServerErrorEnd = -32099;

        #endregion

        public static string GetErrorDescription(int errorCode)
        {
            string errorMessage = string.Empty;
            switch (errorCode)
            {
                // Standard JSON-RPC errors
                case ParseError:
                    errorMessage = "Invalid JSON was received by the server.";
                    break;
                case InvalidRequest:
                    errorMessage = "The JSON sent is not a valid Request object.";
                    break;
                case MethodNotFound:
                    errorMessage = "The method does not exist / is not available.";
                        break;
                case InvalidParams: 
                    errorMessage = "Invalid method parameter(s).";
                    break;
                case InternalError: 
                    errorMessage = "Internal JSON-RPC error.";
                    break;

                // Server error range
                default:
                    if (errorCode >= ServerErrorStart && errorCode <= ServerErrorEnd)
                        errorMessage = "Server error.";
                    break;
            }

            return errorMessage;
        }
    }
}
