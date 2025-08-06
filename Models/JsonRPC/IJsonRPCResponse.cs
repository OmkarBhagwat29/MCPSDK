using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCPSDK.Models.JsonRPC
{
    public interface IJsonRPCResponse
    {
        /// <summary>
        ///     JSON-RPC version, always "2.0"
        /// </summary>
        string JsonRpc { get; }

        /// <summary>
        ///     Request ID, used to associate requests and responses
        /// </summary>
        string Id { get; set; }

        /// <summary>
        ///     Converts the response to a JSON string
        /// </summary>
        string ToJson();
    }

    /// <summary>
    ///     JSON-RPC 2.0 success response
    /// </summary>
    public class JsonRPCSuccessResponse : IJsonRPCResponse
    {
        /// <summary>
        ///     Response result
        /// </summary>
        [JsonProperty("result")]
        public JToken? Result { get; set; }

        /// <summary>
        ///     JSON-RPC version
        /// </summary>
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        /// <summary>
        ///     Request ID
        /// </summary>
        [JsonProperty("id")]
        public string? Id { get; set; }

        /// <summary>
        ///     Converts the response to a JSON string
        /// </summary>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }


        public static string CreateSuccessResponse(string id, object result)
        {
            var response = new JsonRPCSuccessResponse
            {
                Id = id,
                Result = result is JToken jToken ? jToken : JToken.FromObject(result)
            };

            return response.ToJson();
        }
    }

    /// <summary>
    ///     JSON-RPC 2.0 error response
    /// </summary>
    public class JsonRPCErrorResponse : IJsonRPCResponse
    {
        /// <summary>
        ///     Error information
        /// </summary>
        [JsonProperty("error")]
        public JsonRPCError? Error { get; set; }

        /// <summary>
        ///     JSON-RPC version
        /// </summary>
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        /// <summary>
        ///     Request ID
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        ///     Converts the response to a JSON string
        /// </summary>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static string CreateErrorResponse(string? id, int code, string message, object? data = null)
        {
            var rpcError = JsonRPCError.CreateJsonRPCError(code,message, data);

            var response =  new JsonRPCErrorResponse
            {
                Id = id,
                Error = rpcError
            };

            return response.ToJson();
        }


    }

    /// <summary>
    ///     JSON-RPC 2.0 error object
    /// </summary>
    public class JsonRPCError
    {
        /// <summary>
        ///     Error code
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        ///     Error message
        /// </summary>
        [JsonProperty("message")]
        public string? Message { get; set; }

        /// <summary>
        ///     Optional error data
        /// </summary>
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public JToken? Data { get; set; }

        public static JsonRPCError CreateJsonRPCError(int code,string message, object? data = null)
        {
            return new JsonRPCError { 
            
                Code = code,
                Message = message,
                Data = data != null ? JToken.FromObject(data) : null
            };
        }
    }
}
