using System;
using System.Collections.Generic;

namespace swpumc.Models
{
    public class ConfigModel
    {
        public string ConfigName { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0.0";
        public DateTime LastModified { get; set; } = DateTime.Now;
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
    }
}
