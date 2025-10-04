using System;
using System.Collections.Generic;

namespace swpumc.Models
{
    public class AppSettings
    {
        public string Theme { get; set; } = "Dark";
        public string Language { get; set; } = "zh-CN";
        public bool AutoStart { get; set; } = false;
        public bool MinimizeToTray { get; set; } = true;
        public int WindowWidth { get; set; } = 900;
        public int WindowHeight { get; set; } = 600;
        public string LastUsedProfile { get; set; } = string.Empty;
        
        // Java环境配置
        public List<JavaEnvironmentInfo> JavaEnvironments { get; set; } = new List<JavaEnvironmentInfo>();
        public string DefaultJavaPath { get; set; } = string.Empty;
        
        // Minecraft核心配置
        public List<MinecraftCoreInfo> MinecraftCores { get; set; } = new List<MinecraftCoreInfo>();
        public string DefaultMinecraftCore { get; set; } = string.Empty;
        
        // Minecraft版本缓存配置
        public List<MinecraftVersionInfo> CachedMinecraftVersions { get; set; } = new List<MinecraftVersionInfo>();
        public DateTime LastVersionUpdate { get; set; } = DateTime.MinValue;
        public int VersionCacheExpirationHours { get; set; } = 24; // 24小时过期
        
        public Dictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();
    }
    
    public class JavaEnvironmentInfo
    {
        public string JavaPath { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string Architecture { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
        public DateTime LastDetected { get; set; } = DateTime.Now;
    }
    
    public class MinecraftCoreInfo
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string MainClass { get; set; } = string.Empty;
        public string Assets { get; set; } = string.Empty;
        public string JavaVersion { get; set; } = string.Empty;
        public string ForgeVersion { get; set; } = string.Empty;
        public string FabricVersion { get; set; } = string.Empty;
        public string QuiltVersion { get; set; } = string.Empty;
        public DateTime LastDetected { get; set; } = DateTime.Now;
        public bool IsValid { get; set; } = true;
    }
    
    public class MinecraftVersionInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime ReleaseTime { get; set; }
        public DateTime Time { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsLatest { get; set; } = false;
        public bool IsRecommended { get; set; } = false;
        public DateTime CachedAt { get; set; } = DateTime.Now;
    }
}
