using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SWPUMC.Models
{
    /// <summary>
    /// 离线账户数据模型
    /// </summary>
    public class OfflineAccountModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = string.Empty;

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// 离线账户配置容器
    /// </summary>
    public class OfflineAccountsConfig
    {
        [JsonPropertyName("offlineAccounts")]
        public List<OfflineAccountModel> OfflineAccounts { get; set; } = new List<OfflineAccountModel>();
    }
}
