using Newtonsoft.Json;

namespace MCPSDK.API.Configuration
{
    /// <summary>
    /// 服务设置类
    /// </summary>
    public class ServiceSettings
    {
        /// <summary>
        /// 日志级别
        /// </summary>
        [JsonProperty("logLevel")]
        public string LogLevel { get; set; } = "Info";

        /// <summary>
        /// socket服务端口
        /// </summary>
        [JsonProperty("port")]
        public int Port { get; set; } = 8080;

    }
}
