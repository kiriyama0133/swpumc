using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace swpumc.Models
{
    public class SkinLoginConfig
    {
        [JsonPropertyName("exclude")]
        public List<string> Exclude { get; set; } = new List<string>
        {
            "**/bin",
            "**/bower_components",
            "**/jspm_packages",
            "**/node_modules",
            "**/obj",
            "**/platforms"
        };
        
        [JsonPropertyName("baseUrl")]
        public string BaseUrl { get; set; } = "https://littleskin.cn/api/yggdrasil";
        
        [JsonPropertyName("User")]
        public UserModel User { get; set; } = new UserModel();
    }
}
