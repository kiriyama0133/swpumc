using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SWPUMC.Models;

namespace SWPUMC.Services
{
    /// <summary>
    /// 账户配置管理器
    /// 负责加载和保存不同类型的账户配置
    /// </summary>
    public class AccountConfigManager
    {
        private readonly string _configDirectory;
        private readonly JsonSerializerOptions _jsonOptions;

        public AccountConfigManager(string configDirectory = "Assets/JSON")
        {
            _configDirectory = configDirectory;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        #region 离线账户配置管理

        /// <summary>
        /// 加载离线账户配置
        /// </summary>
        /// <returns>离线账户配置</returns>
        public async Task<OfflineAccountsConfig> LoadOfflineAccountsAsync()
        {
            try
            {
                var filePath = Path.Combine(_configDirectory, "OfflineAccounts.json");
                if (!File.Exists(filePath))
                {
                    return new OfflineAccountsConfig();
                }

                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<OfflineAccountsConfig>(json, _jsonOptions) ?? new OfflineAccountsConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载离线账户配置失败: {ex.Message}");
                return new OfflineAccountsConfig();
            }
        }

        /// <summary>
        /// 保存离线账户配置
        /// </summary>
        /// <param name="config">离线账户配置</param>
        public async Task SaveOfflineAccountsAsync(OfflineAccountsConfig config)
        {
            try
            {
                var filePath = Path.Combine(_configDirectory, "OfflineAccounts.json");
                var json = JsonSerializer.Serialize(config, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存离线账户配置失败: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region 微软账户配置管理

        /// <summary>
        /// 加载微软账户配置
        /// </summary>
        /// <returns>微软账户配置</returns>
        public async Task<MicrosoftAccountsConfig> LoadMicrosoftAccountsAsync()
        {
            try
            {
                var filePath = Path.Combine(_configDirectory, "MicrosoftAccounts.json");
                if (!File.Exists(filePath))
                {
                    return new MicrosoftAccountsConfig();
                }

                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<MicrosoftAccountsConfig>(json, _jsonOptions) ?? new MicrosoftAccountsConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载微软账户配置失败: {ex.Message}");
                return new MicrosoftAccountsConfig();
            }
        }

        /// <summary>
        /// 保存微软账户配置
        /// </summary>
        /// <param name="config">微软账户配置</param>
        public async Task SaveMicrosoftAccountsAsync(MicrosoftAccountsConfig config)
        {
            try
            {
                var filePath = Path.Combine(_configDirectory, "MicrosoftAccounts.json");
                var json = JsonSerializer.Serialize(config, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存微软账户配置失败: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region 便捷方法

        /// <summary>
        /// 添加离线账户
        /// </summary>
        /// <param name="account">离线账户</param>
        public async Task AddOfflineAccountAsync(OfflineAccountModel account)
        {
            var config = await LoadOfflineAccountsAsync();
            config.OfflineAccounts.Add(account);
            await SaveOfflineAccountsAsync(config);
        }

        /// <summary>
        /// 添加微软账户
        /// </summary>
        /// <param name="account">微软账户</param>
        public async Task AddMicrosoftAccountAsync(MicrosoftAccountModel account)
        {
            var config = await LoadMicrosoftAccountsAsync();
            config.MicrosoftAccounts.Add(account);
            await SaveMicrosoftAccountsAsync(config);
        }

        /// <summary>
        /// 删除离线账户
        /// </summary>
        /// <param name="uuid">账户UUID</param>
        public async Task RemoveOfflineAccountAsync(string uuid)
        {
            var config = await LoadOfflineAccountsAsync();
            config.OfflineAccounts.RemoveAll(a => a.Uuid == uuid);
            await SaveOfflineAccountsAsync(config);
        }

        /// <summary>
        /// 删除微软账户
        /// </summary>
        /// <param name="uuid">账户UUID</param>
        public async Task RemoveMicrosoftAccountAsync(string uuid)
        {
            var config = await LoadMicrosoftAccountsAsync();
            config.MicrosoftAccounts.RemoveAll(a => a.Uuid == uuid);
            await SaveMicrosoftAccountsAsync(config);
        }

        #endregion
    }
}
