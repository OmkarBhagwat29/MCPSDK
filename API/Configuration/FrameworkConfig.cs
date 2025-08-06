
using Newtonsoft.Json;


namespace MCPSDK.API.Configuration
{
    /// <summary>
    /// 框架配置类
    /// </summary>
    public class FrameworkConfig
    {
        /// <summary>
        /// 命令配置列表
        /// </summary>
        [JsonProperty("commands")]
        public List<CommandConfig> Commands { get; set; } = new List<CommandConfig>();

        /// <summary>
        /// 全局设置
        /// </summary>
        [JsonProperty("settings")]
        public ServiceSettings Settings { get; set; } = new ServiceSettings();
    }
}
