using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace swpumc.Models
{
    /// <summary>
    /// 启动配置模型
    /// </summary>
    public class LaunchConfig
    {
        [JsonPropertyName("lastUsedCore")]
        public string LastUsedCore { get; set; } = string.Empty;

        [JsonPropertyName("launchSettings")]
        public LaunchSettings LaunchSettings { get; set; } = new();

        [JsonPropertyName("lastModified")]
        public DateTime LastModified { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 启动设置
    /// </summary>
    public class LaunchSettings
    {
        [JsonPropertyName("memoryConfig")]
        public MemoryConfig MemoryConfig { get; set; } = new();

        [JsonPropertyName("windowConfig")]
        public WindowConfig WindowConfig { get; set; } = new();

        [JsonPropertyName("serverConfig")]
        public ServerConfig ServerConfig { get; set; } = new();

        [JsonPropertyName("javaConfig")]
        public JavaConfig JavaConfig { get; set; } = new();

        [JsonPropertyName("advancedConfig")]
        public AdvancedConfig AdvancedConfig { get; set; } = new();
    }

    /// <summary>
    /// 内存配置
    /// </summary>
    public class MemoryConfig
    {
        [JsonPropertyName("minMemorySize")]
        public int MinMemorySize { get; set; } = 1024;

        [JsonPropertyName("maxMemorySize")]
        public int MaxMemorySize { get; set; } = 4096;
    }

    /// <summary>
    /// 窗口配置
    /// </summary>
    public class WindowConfig
    {
        [JsonPropertyName("gameWidth")]
        public int GameWidth { get; set; } = 1280;

        [JsonPropertyName("gameHeight")]
        public int GameHeight { get; set; } = 720;

        [JsonPropertyName("isFullScreen")]
        public bool IsFullScreen { get; set; } = false;
    }

    /// <summary>
    /// 服务器配置
    /// </summary>
    public class ServerConfig
    {
        [JsonPropertyName("serverAddress")]
        public string ServerAddress { get; set; } = string.Empty;
    }

    /// <summary>
    /// Java配置
    /// </summary>
    public class JavaConfig
    {
        [JsonPropertyName("javaPath")]
        public string JavaPath { get; set; } = string.Empty;

        [JsonPropertyName("javaEnvironments")]
        public List<string> JavaEnvironments { get; set; } = new();
    }

    /// <summary>
    /// 高级配置
    /// </summary>
    public class AdvancedConfig
    {
        [JsonPropertyName("customJvmArgs")]
        public string CustomJvmArgs { get; set; } = string.Empty;

        [JsonPropertyName("customGameArgs")]
        public string CustomGameArgs { get; set; } = string.Empty;
    }
}
