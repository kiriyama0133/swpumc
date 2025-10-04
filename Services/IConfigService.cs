using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using swpumc.Models;

namespace swpumc.Services
{
    public interface IConfigService
    {
        /// <summary>
        /// 获取应用设置
        /// </summary>
        AppSettings AppSettings { get; }
        
        /// <summary>
        /// 获取启动配置
        /// </summary>
        LaunchConfig LaunchConfig { get; }
        
        /// <summary>
        /// 获取指定名称的配置
        /// </summary>
        /// <param name="configName">配置名称</param>
        /// <returns>配置模型</returns>
        ConfigModel? GetConfig(string configName);
        
        /// <summary>
        /// 设置配置
        /// </summary>
        /// <param name="config">配置模型</param>
        Task SetConfigAsync(ConfigModel config);
        
        /// <summary>
        /// 保存应用设置
        /// </summary>
        Task SaveAppSettingsAsync();
        
        /// <summary>
        /// 加载所有配置
        /// </summary>
        Task LoadAllConfigsAsync();
        
        /// <summary>
        /// 配置更改事件
        /// </summary>
        event EventHandler<ConfigModel>? ConfigChanged;
        
        /// <summary>
        /// 应用设置更改事件
        /// </summary>
        event EventHandler<AppSettings>? AppSettingsChanged;
        
        /// <summary>
        /// 启动配置更改事件
        /// </summary>
        event EventHandler<LaunchConfig>? LaunchConfigChanged;
        

        /// <summary>
        /// 获取用户管理配置
        /// </summary>
        UserManagementConfig GetUserManagementConfig();
        
        /// <summary>
        /// 保存用户管理配置
        /// </summary>
        Task SaveUserManagementConfigAsync(UserManagementConfig config);

        /// <summary>
        /// 获取当前用户账户
        /// </summary>
        UserAccount? GetCurrentUserAccount();

        /// <summary>
        /// 设置当前用户
        /// </summary>
        Task SetCurrentUserAsync(string email);

        /// <summary>
        /// 添加或更新用户账户
        /// </summary>
        Task AddOrUpdateUserAccountAsync(UserAccount userAccount);

        /// <summary>
        /// 删除用户账户
        /// </summary>
        Task RemoveUserAccountAsync(string email);
        
        
        /// <summary>
        /// 获取自定义设置
        /// </summary>
        /// <typeparam name="T">设置类型</typeparam>
        /// <param name="key">设置键</param>
        /// <returns>设置值</returns>
        T? GetCustomSetting<T>(string key);
        
        /// <summary>
        /// 设置自定义设置
        /// </summary>
        /// <typeparam name="T">设置类型</typeparam>
        /// <param name="key">设置键</param>
        /// <param name="value">设置值</param>
        void SetCustomSetting<T>(string key, T value);

        #region Java环境管理

        /// <summary>
        /// 更新Java环境列表
        /// </summary>
        Task UpdateJavaEnvironmentsAsync(List<JavaEnvironmentInfo> javaEnvironments);

        /// <summary>
        /// 获取Java环境列表
        /// </summary>
        List<JavaEnvironmentInfo> GetJavaEnvironments();

        /// <summary>
        /// 获取默认Java路径
        /// </summary>
        string GetDefaultJavaPath();

        /// <summary>
        /// 设置默认Java路径
        /// </summary>
        Task SetDefaultJavaPathAsync(string javaPath);

        #endregion

        #region Minecraft核心管理

        /// <summary>
        /// 更新Minecraft核心列表
        /// </summary>
        Task UpdateMinecraftCoresAsync(List<MinecraftCoreInfo> minecraftCores);

        /// <summary>
        /// 获取Minecraft核心列表
        /// </summary>
        List<MinecraftCoreInfo> GetMinecraftCores();

        /// <summary>
        /// 获取默认Minecraft核心
        /// </summary>
        string GetDefaultMinecraftCore();

        /// <summary>
        /// 设置默认Minecraft核心
        /// </summary>
        Task SetDefaultMinecraftCoreAsync(string coreId);

        /// <summary>
        /// 根据ID获取Minecraft核心信息
        /// </summary>
        MinecraftCoreInfo? GetMinecraftCoreById(string coreId);

        #endregion

        #region 启动配置管理

        /// <summary>
        /// 保存启动配置
        /// </summary>
        Task SaveLaunchConfigAsync();

        /// <summary>
        /// 防抖保存启动配置
        /// </summary>
        void DebouncedSaveLaunchConfig();

        /// <summary>
        /// 更新启动配置
        /// </summary>
        void UpdateLaunchConfig(LaunchConfig newConfig);

        /// <summary>
        /// 更新最后使用的核心
        /// </summary>
        void UpdateLastUsedCore(string coreId);

        /// <summary>
        /// 更新内存配置
        /// </summary>
        void UpdateMemoryConfig(int minMemory, int maxMemory);

        /// <summary>
        /// 更新窗口配置
        /// </summary>
        void UpdateWindowConfig(int width, int height, bool isFullScreen);

        /// <summary>
        /// 更新服务器配置
        /// </summary>
        void UpdateServerConfig(string serverAddress);

        /// <summary>
        /// 更新Java配置
        /// </summary>
        void UpdateJavaConfig(string javaPath, List<string> javaEnvironments);

        /// <summary>
        /// 更新高级配置
        /// </summary>
        void UpdateAdvancedConfig(string jvmArgs, string gameArgs);

        #endregion
    }
}
