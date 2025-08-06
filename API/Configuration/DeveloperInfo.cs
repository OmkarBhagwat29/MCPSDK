using Newtonsoft.Json;


namespace MCPSDK.API.Configuration
{
    public class DeveloperInfo
    {
        /// <summary>
        /// Developer Name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = "";


        /// <summary>
        /// Email
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; } = "";
    }
}
