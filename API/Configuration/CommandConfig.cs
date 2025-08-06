using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCPSDK.API.Configuration
{
    public class CommandConfig
    {
        /// <summary>
        /// Command Name - corresponds to IRevitCommand.CommandName
        /// </summary>
        [JsonProperty("commandName")]
        public string? CommandName { get; set; }

        /// <summary>
        /// Assembly Path - the DLL that contains this command
        /// </summary>
        [JsonProperty("assemblyPath")]
        public string? AssemblyPath { get; set; }

        /// <summary>
        ///  Whether to enable the command
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Command Description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; } = "";
    }
}
