using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using swpumc.Models;

namespace swpumc.Services
{
    public class ConfigService : IConfigService
    {
        private readonly string _configDirectory;
        private readonly string _appSettingsPath;
        private readonly string _launchConfigPath;
        private readonly Dictionary<string, ConfigModel> _configs;
        private AppSettings _appSettings;
        private UserManagementConfig _userManagementConfig = new UserManagementConfig();
        private LaunchConfig _launchConfig = new LaunchConfig();
        
        // 防抖保存相关
        private readonly Dictionary<string, Timer> _debounceTimers = new();
        private readonly object _debounceLock = new object();
        private const int DEBOUNCE_DELAY_MS = 100;
        
        public AppSettings AppSettings => _appSettings;
        public LaunchConfig LaunchConfig => _launchConfig;
        
        public event EventHandler<ConfigModel>? ConfigChanged;
        public event EventHandler<AppSettings>? AppSettingsChanged;
        public event EventHandler<LaunchConfig>? LaunchConfigChanged;
        
        public ConfigService()
        {
            // 使用项目根目录的 Assets/JSON 文件夹
            var projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;
            _configDirectory = Path.Combine(projectRoot ?? AppDomain.CurrentDomain.BaseDirectory, "Assets", "JSON");
            _appSettingsPath = Path.Combine(_configDirectory, "AppSettings.json");
            _launchConfigPath = Path.Combine(_configDirectory, "LaunchConfig.json");
            _configs = new Dictionary<string, ConfigModel>();
            _appSettings = new AppSettings();
            _launchConfig = new LaunchConfig();
            
            Console.WriteLine($"[ConfigService] 项目根目录: {projectRoot}");
            Console.WriteLine($"[ConfigService] 配置目录路径: {_configDirectory}");
            Console.WriteLine($"[ConfigService] 应用设置文件路径: {_appSettingsPath}");
            Console.WriteLine($"[ConfigService] 启动配置文件路径: {_launchConfigPath}");
            
            // 确保配置目录存在
            Directory.CreateDirectory(_configDirectory);
        }
        
        public ConfigModel? GetConfig(string configName)
        {
            return _configs.TryGetValue(configName, out var config) ? config : null;
        }
        
        public async Task SetConfigAsync(ConfigModel config)
        {
            _configs[config.ConfigName] = config;
            config.LastModified = DateTime.Now;
            
            // 保存到文件
            var filePath = Path.Combine(_configDirectory, $"{config.ConfigName}.json");
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            
            await File.WriteAllTextAsync(filePath, json);
            
            // 触发事件
            ConfigChanged?.Invoke(this, config);
        }
        
        public async Task SaveAppSettingsAsync()
        {
            var json = JsonSerializer.Serialize(_appSettings, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            
            await File.WriteAllTextAsync(_appSettingsPath, json);
            
            // 触发事件
            AppSettingsChanged?.Invoke(this, _appSettings);
        }
        
        public async Task LoadAllConfigsAsync()
        {
            try
            {
                // 加载应用设置
                await LoadAppSettingsAsync();
                
                // 加载用户管理配置
                await LoadUserManagementConfigAsync();
                
                // 加载启动配置
                await LoadLaunchConfigAsync();
                
                // 加载所有JSON配置文件
                var jsonFiles = Directory.GetFiles(_configDirectory, "*.json");
                
                foreach (var filePath in jsonFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    
                    // 跳过已单独处理的文件
                    if (fileName == "AppSettings" || fileName == "UserManagement" || fileName == "LaunchConfig")
                        continue;
                    
                    try
                    {
                        var json = await File.ReadAllTextAsync(filePath);
                        var config = JsonSerializer.Deserialize<ConfigModel>(json);
                        
                        if (config != null)
                        {
                            _configs[config.ConfigName] = config;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ConfigService] 加载配置文件失败: {filePath}, 错误: {ex.Message}");
                    }
                }
                
                
                Console.WriteLine($"[ConfigService] 成功加载 {_configs.Count} 个配置文件");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConfigService] 加载配置失败: {ex.Message}");
            }
        }
        
        private async Task LoadAppSettingsAsync()
        {
            try
            {
                if (File.Exists(_appSettingsPath))
                {
                    var json = await File.ReadAllTextAsync(_appSettingsPath);
                    _appSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    // 如果文件不存在，创建默认设置
                    _appSettings = new AppSettings();
                    await SaveAppSettingsAsync();
                }
                
                Console.WriteLine($"[ConfigService] 应用设置加载完成");
                Console.WriteLine($"[ConfigService] 应用设置内容: {JsonSerializer.Serialize(_appSettings, new JsonSerializerOptions { WriteIndented = true })}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConfigService] 加载应用设置失败: {ex.Message}");
                _appSettings = new AppSettings();
            }
        }
        
        /// <summary>
        /// 更新应用设置中的特定属性
        /// </summary>
        public async Task UpdateAppSettingAsync<T>(string key, T value)
        {
            var property = typeof(AppSettings).GetProperty(key);
            if (property != null && property.CanWrite)
            {
                property.SetValue(_appSettings, value);
                await SaveAppSettingsAsync();
            }
            else
            {
                // 如果属性不存在，添加到自定义设置中
                _appSettings.CustomSettings[key] = value;
                await SaveAppSettingsAsync();
            }
        }
        
        /// <summary>
        /// 获取应用设置中的特定属性
        /// </summary>
        public T? GetAppSetting<T>(string key)
        {
            var property = typeof(AppSettings).GetProperty(key);
            if (property != null && property.CanRead)
            {
                return (T?)property.GetValue(_appSettings);
            }
            
            // 从自定义设置中获取
            if (_appSettings.CustomSettings.TryGetValue(key, out var value))
            {
                return (T?)value;
            }
            
            return default;
        }
        
        
        
        
        public T? GetCustomSetting<T>(string key)
        {
            if (_appSettings.CustomSettings.TryGetValue(key, out var value))
            {
                if (value is JsonElement jsonElement)
                {
                    return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
                }
                return (T?)value;
            }
            return default(T);
        }
        
        public void SetCustomSetting<T>(string key, T value)
        {
            _appSettings.CustomSettings[key] = value;
        }

        // 用户管理配置相关方法
        public UserManagementConfig GetUserManagementConfig()
        {
            return _userManagementConfig;
        }

        public async Task SaveUserManagementConfigAsync(UserManagementConfig config)
        {
            _userManagementConfig = config;
            var filePath = Path.Combine(_configDirectory, "UserManagement.json");
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            await File.WriteAllTextAsync(filePath, json);
            // ConfigChanged事件暂时不触发，因为UserManagementConfig不是ConfigModel类型
        }

        public UserAccount? GetCurrentUserAccount()
        {
            if (string.IsNullOrEmpty(_userManagementConfig.CurrentUser))
                return null;

            return _userManagementConfig.Users.FirstOrDefault(u => u.Email == _userManagementConfig.CurrentUser);
        }

        public async Task SetCurrentUserAsync(string email)
        {
            _userManagementConfig.CurrentUser = email;
            await SaveUserManagementConfigAsync(_userManagementConfig);
        }

        public async Task AddOrUpdateUserAccountAsync(UserAccount userAccount)
        {
            var existingUser = _userManagementConfig.Users.FirstOrDefault(u => u.Email == userAccount.Email);
            if (existingUser != null)
            {
                // 更新现有用户
                var index = _userManagementConfig.Users.IndexOf(existingUser);
                _userManagementConfig.Users[index] = userAccount;
            }
            else
            {
                // 添加新用户
                _userManagementConfig.Users.Add(userAccount);
            }
            await SaveUserManagementConfigAsync(_userManagementConfig);
        }

        public async Task RemoveUserAccountAsync(string email)
        {
            var userToRemove = _userManagementConfig.Users.FirstOrDefault(u => u.Email == email);
            if (userToRemove != null)
            {
                _userManagementConfig.Users.Remove(userToRemove);
                // 如果删除的是当前用户，清空当前用户
                if (_userManagementConfig.CurrentUser == email)
                {
                    _userManagementConfig.CurrentUser = string.Empty;
                }
                await SaveUserManagementConfigAsync(_userManagementConfig);
            }
        }

        private async Task LoadUserManagementConfigAsync()
        {
            var filePath = Path.Combine(_configDirectory, "UserManagement.json");
            if (File.Exists(filePath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    _userManagementConfig = JsonSerializer.Deserialize<UserManagementConfig>(json) ?? new UserManagementConfig();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ConfigService] 加载用户管理配置失败: {ex.Message}");
                    _userManagementConfig = new UserManagementConfig();
                }
            }
            else
            {
                _userManagementConfig = new UserManagementConfig();
            }
        }

        #region Java环境管理

        /// <summary>
        /// 更新Java环境列表
        /// </summary>
        public async Task UpdateJavaEnvironmentsAsync(List<JavaEnvironmentInfo> javaEnvironments)
        {
            _appSettings.JavaEnvironments = javaEnvironments;
            
            // 设置默认Java路径（第一个）
            if (javaEnvironments.Count > 0)
            {
                _appSettings.DefaultJavaPath = javaEnvironments[0].JavaPath;
            }
            
            await SaveAppSettingsAsync();
            Console.WriteLine($"[ConfigService] 更新Java环境列表，共 {javaEnvironments.Count} 个环境");
        }

        /// <summary>
        /// 获取Java环境列表
        /// </summary>
        public List<JavaEnvironmentInfo> GetJavaEnvironments()
        {
            return _appSettings.JavaEnvironments;
        }

        /// <summary>
        /// 获取默认Java路径
        /// </summary>
        public string GetDefaultJavaPath()
        {
            return _appSettings.DefaultJavaPath;
        }

        /// <summary>
        /// 设置默认Java路径
        /// </summary>
        public async Task SetDefaultJavaPathAsync(string javaPath)
        {
            _appSettings.DefaultJavaPath = javaPath;
            await SaveAppSettingsAsync();
        }

        #endregion

        #region Minecraft核心管理

        /// <summary>
        /// 更新Minecraft核心列表
        /// </summary>
        public async Task UpdateMinecraftCoresAsync(List<MinecraftCoreInfo> minecraftCores)
        {
            _appSettings.MinecraftCores = minecraftCores;
            
            // 设置默认Minecraft核心（第一个）
            if (minecraftCores.Count > 0)
            {
                _appSettings.DefaultMinecraftCore = minecraftCores[0].Id;
            }
            
            await SaveAppSettingsAsync();
            Console.WriteLine($"[ConfigService] 更新Minecraft核心列表，共 {minecraftCores.Count} 个核心");
        }

        /// <summary>
        /// 获取Minecraft核心列表
        /// </summary>
        public List<MinecraftCoreInfo> GetMinecraftCores()
        {
            return _appSettings.MinecraftCores;
        }

        /// <summary>
        /// 获取默认Minecraft核心
        /// </summary>
        public string GetDefaultMinecraftCore()
        {
            return _appSettings.DefaultMinecraftCore;
        }

        /// <summary>
        /// 设置默认Minecraft核心
        /// </summary>
        public async Task SetDefaultMinecraftCoreAsync(string coreId)
        {
            _appSettings.DefaultMinecraftCore = coreId;
            await SaveAppSettingsAsync();
        }

        /// <summary>
        /// 根据ID获取Minecraft核心信息
        /// </summary>
        public MinecraftCoreInfo? GetMinecraftCoreById(string coreId)
        {
            return _appSettings.MinecraftCores.FirstOrDefault(core => core.Id == coreId);
        }

        #endregion

        #region 启动配置管理

        /// <summary>
        /// 加载启动配置
        /// </summary>
        private async Task LoadLaunchConfigAsync()
        {
            try
            {
                if (File.Exists(_launchConfigPath))
                {
                    var json = await File.ReadAllTextAsync(_launchConfigPath);
                    _launchConfig = JsonSerializer.Deserialize<LaunchConfig>(json) ?? new LaunchConfig();
                }
                else
                {
                    // 如果文件不存在，创建默认配置
                    _launchConfig = new LaunchConfig();
                    await SaveLaunchConfigAsync();
                }
                
                Console.WriteLine($"[ConfigService] 启动配置加载完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConfigService] 加载启动配置失败: {ex.Message}");
                _launchConfig = new LaunchConfig();
            }
        }

        /// <summary>
        /// 保存启动配置（防抖保存）
        /// </summary>
        public async Task SaveLaunchConfigAsync()
        {
            try
            {
                _launchConfig.LastModified = DateTime.Now;
                var json = JsonSerializer.Serialize(_launchConfig, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                
                await File.WriteAllTextAsync(_launchConfigPath, json);
                
                // 触发事件
                LaunchConfigChanged?.Invoke(this, _launchConfig);
                
                Console.WriteLine($"[ConfigService] 启动配置保存完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConfigService] 保存启动配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 防抖保存启动配置
        /// </summary>
        public void DebouncedSaveLaunchConfig()
        {
            lock (_debounceLock)
            {
                // 取消之前的定时器
                if (_debounceTimers.TryGetValue("LaunchConfig", out var existingTimer))
                {
                    existingTimer?.Dispose();
                }

                // 创建新的定时器
                var timer = new Timer(async _ =>
                {
                    await SaveLaunchConfigAsync();
                    lock (_debounceLock)
                    {
                        _debounceTimers.Remove("LaunchConfig");
                    }
                }, null, DEBOUNCE_DELAY_MS, Timeout.Infinite);

                _debounceTimers["LaunchConfig"] = timer;
            }
        }

        /// <summary>
        /// 更新启动配置
        /// </summary>
        public void UpdateLaunchConfig(LaunchConfig newConfig)
        {
            _launchConfig = newConfig;
            DebouncedSaveLaunchConfig();
        }

        /// <summary>
        /// 更新最后使用的核心
        /// </summary>
        public void UpdateLastUsedCore(string coreId)
        {
            _launchConfig.LastUsedCore = coreId;
            DebouncedSaveLaunchConfig();
        }

        /// <summary>
        /// 更新内存配置
        /// </summary>
        public void UpdateMemoryConfig(int minMemory, int maxMemory)
        {
            _launchConfig.LaunchSettings.MemoryConfig.MinMemorySize = minMemory;
            _launchConfig.LaunchSettings.MemoryConfig.MaxMemorySize = maxMemory;
            DebouncedSaveLaunchConfig();
        }

        /// <summary>
        /// 更新窗口配置
        /// </summary>
        public void UpdateWindowConfig(int width, int height, bool isFullScreen)
        {
            _launchConfig.LaunchSettings.WindowConfig.GameWidth = width;
            _launchConfig.LaunchSettings.WindowConfig.GameHeight = height;
            _launchConfig.LaunchSettings.WindowConfig.IsFullScreen = isFullScreen;
            DebouncedSaveLaunchConfig();
        }

        /// <summary>
        /// 更新服务器配置
        /// </summary>
        public void UpdateServerConfig(string serverAddress)
        {
            _launchConfig.LaunchSettings.ServerConfig.ServerAddress = serverAddress;
            DebouncedSaveLaunchConfig();
        }

        /// <summary>
        /// 更新Java配置
        /// </summary>
        public void UpdateJavaConfig(string javaPath, List<string> javaEnvironments)
        {
            _launchConfig.LaunchSettings.JavaConfig.JavaPath = javaPath;
            _launchConfig.LaunchSettings.JavaConfig.JavaEnvironments = javaEnvironments;
            DebouncedSaveLaunchConfig();
        }

        /// <summary>
        /// 更新高级配置
        /// </summary>
        public void UpdateAdvancedConfig(string jvmArgs, string gameArgs)
        {
            _launchConfig.LaunchSettings.AdvancedConfig.CustomJvmArgs = jvmArgs;
            _launchConfig.LaunchSettings.AdvancedConfig.CustomGameArgs = gameArgs;
            DebouncedSaveLaunchConfig();
        }

        #endregion
    }
}
