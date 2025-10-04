using System;
using System.Collections.Generic;

namespace swpumc.Models
{
    public class UnonlinePlayerConfig
    {
        public List<string> Exclude { get; set; } = new List<string>
        {
            "**/bin",
            "**/bower_components",
            "**/jspm_packages",
            "**/node_modules",
            "**/obj",
            "**/platforms"
        };
        
        public List<UserModel> Player { get; set; } = new List<UserModel>();
    }
}
