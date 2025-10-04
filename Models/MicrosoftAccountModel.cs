using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SWPUMC.Models
{
    /// <summary>
    /// 微软账户数据模型
    /// </summary>
    public class MicrosoftAccountModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = string.Empty;

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("lastRefreshTime")]
        public string LastRefreshTime { get; set; } = string.Empty;
    }

    /// <summary>
    /// 微软账户配置容器
    /// </summary>
    public class MicrosoftAccountsConfig
    {
        [JsonPropertyName("microsoftAccounts")]
        public List<MicrosoftAccountModel> MicrosoftAccounts { get; set; } = new List<MicrosoftAccountModel>();
    }
}
