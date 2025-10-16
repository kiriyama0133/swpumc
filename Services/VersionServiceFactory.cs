using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MinecraftLaunch.Components.Installer;
using swpumc.Models;

namespace swpumc.Services;

/// <summary>
/// 版本服务工厂
/// 负责创建和管理各种版本服务
/// </summary>
public class VersionServiceFactory
{
    private readonly IConfigService _configService;
    private readonly IJavaEnvironmentService _javaEnvironmentService;
    private readonly Dictionary<string, IVersionService> _services;
    private readonly MinecraftVersionService _minecraftVersionService;
    private static string _cachedMinecraftFolder;
    private static string _cachedJavaPath;
    private static bool _isInitialized = false; // 添加初始化状态标志

    public VersionServiceFactory(IConfigService configService, IJavaEnvironmentService javaEnvironmentService, MinecraftVersionService minecraftVersionService)
    {
        _configService = configService;
        _javaEnvironmentService = javaEnvironmentService;
        _minecraftVersionService = minecraftVersionService;
        _services = new Dictionary<string, IVersionService>();
        InitializeServices();
    }

    /// <summary>
    /// 获取指定类型的版本服务
    /// </summary>
    /// <param name="versionType">版本类型</param>
    /// <returns>版本服务实例</returns>
    public IVersionService GetService(string versionType)
    {
        var lowerVersionType = versionType.ToLower();
        
        // 确保服务已初始化
        if (!_isInitialized)
        {
            InitializeServices();
        }
        
        if (_services.TryGetValue(lowerVersionType, out var service))
        {
            return service;
        }
        
        // 如果仍未找到服务，尝试强制重新初始化一次
        _isInitialized = false;
        InitializeServices();
        
        if (_services.TryGetValue(lowerVersionType, out var retryService))
        {
            return retryService;
        }
        
        throw new ArgumentException($"不支持的版本类型: {versionType}");
    }

    /// <summary>
    /// 获取所有支持的版本类型
    /// </summary>
    /// <returns>版本类型列表</returns>
    public IEnumerable<string> GetSupportedVersionTypes()
    {
        // 确保服务已初始化
        if (!_isInitialized)
        {
            InitializeServices();
        }
        
        return _services.Keys;
    }
    
    /// <summary>
    /// 诊断服务工厂状态
    /// </summary>
    public void Diagnose()
    {
        Console.WriteLine($"[VersionServiceFactory] 诊断信息: 初始化={_isInitialized}, 服务数={_services.Count}, 类型=[{string.Join(", ", _services.Keys)}]");
    }
    
    /// <summary>
    /// 初始化所有版本服务
    /// </summary>
    private void InitializeServices()
    {
        // 检查是否已经初始化
        if (_isInitialized)
        {
            return;
        }
        
        // 获取正确的Minecraft文件夹路径
        _cachedMinecraftFolder = GetMinecraftFolderPath();
        _cachedJavaPath = GetJavaPath();
        
        try
        {
            // 使用现有的MinecraftVersionService作为原版服务
            _services["vanilla"] = new MinecraftVersionServiceWrapper(_minecraftVersionService);
            _services["forge"] = new ForgeVersionServiceWrapper(new ForgeVersionService(_cachedMinecraftFolder, _cachedJavaPath));
            _services["neoforge"] = new ForgeVersionServiceWrapper(new ForgeVersionService(_cachedMinecraftFolder, _cachedJavaPath));
            _services["fabric"] = new FabricVersionServiceWrapper(new FabricVersionService(_cachedMinecraftFolder));
            _services["quilt"] = new QuiltVersionServiceWrapper(new QuiltVersionService(_cachedMinecraftFolder));
            _services["optifine"] = new OptifineVersionServiceWrapper(new OptifineVersionService(_cachedMinecraftFolder, _cachedJavaPath));
            
            // 标记为已初始化
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VersionServiceFactory] 初始化服务时发生异常: {ex.Message}");
            // 重置初始化状态，以便下次重试
            _isInitialized = false;
            _services.Clear();
        }
    }
    
    /// <summary>
    /// 获取Minecraft文件夹路径
    /// </summary>
    public string GetMinecraftFolderPath()
    {
        // 如果已经有缓存的路径，直接返回
        if (!string.IsNullOrEmpty(_cachedMinecraftFolder))
        {
            return _cachedMinecraftFolder;
        }
        
        // 1. 优先使用用户配置的Minecraft路径（如果存在）
        var userMinecraftPath = GetUserMinecraftPath();
        if (!string.IsNullOrEmpty(userMinecraftPath) && Directory.Exists(userMinecraftPath))
        {
            Console.WriteLine($"[VersionServiceFactory] 使用用户配置的Minecraft路径: {userMinecraftPath}");
            return userMinecraftPath;
        }
        
        // 2. 尝试使用标准的Minecraft安装路径
        var standardPaths = GetStandardMinecraftPaths();
        foreach (var path in standardPaths)
        {
            if (Directory.Exists(path) && HasGameCores(path))
            {
                Console.WriteLine($"[VersionServiceFactory] 使用标准Minecraft路径: {path}");
                return path;
            }
        }
        
        // 3. 回退到应用程序目录（用于开发环境）
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var currentDirectory = Path.Combine(baseDirectory, ".minecraft");
        
        // 如果当前目录存在游戏核心，使用当前目录
        if (HasGameCores(currentDirectory))
        {
            Console.WriteLine($"[VersionServiceFactory] 使用应用程序目录: {currentDirectory}");
            return currentDirectory;
        }
        
        // 尝试Debug目录
        var debugDirectory = Path.Combine(baseDirectory.Replace("Release", "Debug"), ".minecraft");
        if (HasGameCores(debugDirectory))
        {
            Console.WriteLine($"[VersionServiceFactory] 使用Debug目录: {debugDirectory}");
            return debugDirectory;
        }
        
        // 尝试Release目录
        var releaseDirectory = Path.Combine(baseDirectory.Replace("Debug", "Release"), ".minecraft");
        if (HasGameCores(releaseDirectory))
        {
            Console.WriteLine($"[VersionServiceFactory] 使用Release目录: {releaseDirectory}");
            return releaseDirectory;
        }
        
        // 4. 默认使用标准路径
        var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
        Console.WriteLine($"[VersionServiceFactory] 使用默认路径: {defaultPath}");
        return defaultPath;
    }
    
    /// <summary>
    /// 获取用户配置的Minecraft路径
    /// </summary>
    private string GetUserMinecraftPath()
    {
        // 这里可以从配置文件或用户设置中获取
        // 暂时返回空字符串，表示没有用户配置
        return string.Empty;
    }
    
    /// <summary>
    /// 获取标准的Minecraft安装路径
    /// </summary>
    private List<string> GetStandardMinecraftPaths()
    {
        var paths = new List<string>();
        
        // Windows标准路径
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            // %APPDATA%\.minecraft
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
            paths.Add(appDataPath);
            
            // 用户文档目录
            var documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".minecraft");
            paths.Add(documentsPath);
            
            // 用户桌面目录
            var desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), ".minecraft");
            paths.Add(desktopPath);
        }
        
        // Linux/macOS标准路径
        else
        {
            // ~/.minecraft
            var homePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".minecraft");
            paths.Add(homePath);
        }
        
        return paths;
    }
    
    /// <summary>
    /// 获取Java路径
    /// </summary>
    private string GetJavaPath()
    {
        // 如果已经有缓存的Java路径，直接返回
        if (!string.IsNullOrEmpty(_cachedJavaPath))
        {
            return _cachedJavaPath;
        }
        
        Console.WriteLine("[VersionServiceFactory] 开始获取Java路径");
        
        // 1. 优先使用配置的默认Java路径
        var configuredJavaPath = GetConfiguredJavaPath();
        if (!string.IsNullOrEmpty(configuredJavaPath))
        {
            return configuredJavaPath;
        }
        
        // 2. 使用JavaEnvironmentService检测到的Java环境
        var detectedJavaPath = GetDetectedJavaPath();
        if (!string.IsNullOrEmpty(detectedJavaPath))
        {
            return detectedJavaPath;
        }
        
        // 3. 最后回退到系统PATH中的java
        Console.WriteLine("[VersionServiceFactory] 回退到系统PATH中的java");
        return "java";
    }
    
    /// <summary>
    /// 获取配置的Java路径
    /// </summary>
    private string GetConfiguredJavaPath()
    {
        // 从配置服务获取默认Java路径
        var defaultJavaPath = _configService.AppSettings.DefaultJavaPath;
        Console.WriteLine($"[VersionServiceFactory] 默认Java路径: {defaultJavaPath}");
        if (!string.IsNullOrEmpty(defaultJavaPath) && File.Exists(defaultJavaPath))
        {
            Console.WriteLine($"[VersionServiceFactory] 使用默认Java路径: {defaultJavaPath}");
            return defaultJavaPath;
        }
        
        // 从配置的Java环境列表获取
        var javaEnvironments = _configService.AppSettings.JavaEnvironments;
        Console.WriteLine($"[VersionServiceFactory] 配置的Java环境数量: {javaEnvironments?.Count ?? 0}");
        
        if (javaEnvironments?.Any() == true)
        {
            var defaultJava = javaEnvironments.FirstOrDefault(j => j.IsDefault);
            if (defaultJava != null && !string.IsNullOrEmpty(defaultJava.JavaPath) && File.Exists(defaultJava.JavaPath))
            {
                Console.WriteLine($"[VersionServiceFactory] 使用配置的默认Java环境: {defaultJava.JavaPath}");
                return defaultJava.JavaPath;
            }
            
            var firstJava = javaEnvironments.FirstOrDefault(j => !string.IsNullOrEmpty(j.JavaPath) && File.Exists(j.JavaPath));
            if (firstJava != null)
            {
                Console.WriteLine($"[VersionServiceFactory] 使用配置的第一个可用Java: {firstJava.JavaPath}");
                return firstJava.JavaPath;
            }
        }
        
        return string.Empty;
    }
    
    /// <summary>
    /// 获取检测到的Java路径
    /// </summary>
    private string GetDetectedJavaPath()
    {
        var javaInstallations = _javaEnvironmentService.JavaInstallations;
        Console.WriteLine($"[VersionServiceFactory] 检测到的Java环境数量: {javaInstallations.Count}");
        
        if (!javaInstallations.Any())
        {
            return string.Empty;
        }
        
        // 优先使用默认的Java环境
        var defaultJava = javaInstallations.FirstOrDefault(j => j.IsDefault);
        if (defaultJava != null && IsValidJavaPath(defaultJava.JavaExecutable))
        {
            Console.WriteLine($"[VersionServiceFactory] 使用默认Java环境: {defaultJava.JavaExecutable}");
            return defaultJava.JavaExecutable;
        }
        
        // 如果没有默认的，使用第一个可用的
        var firstJava = javaInstallations.FirstOrDefault(j => IsValidJavaPath(j.JavaExecutable));
        if (firstJava != null)
        {
            Console.WriteLine($"[VersionServiceFactory] 使用第一个可用Java: {firstJava.JavaExecutable}");
            return firstJava.JavaExecutable;
        }
        
        return string.Empty;
    }
    
    /// <summary>
    /// 检查Java路径是否有效
    /// </summary>
    private static bool IsValidJavaPath(string javaPath)
    {
        return !string.IsNullOrEmpty(javaPath) && File.Exists(javaPath);
    }
    
    
    /// <summary>
    /// 检查目录是否有游戏核心
    /// </summary>
    private static bool HasGameCores(string directory)
    {
        if (!Directory.Exists(directory))
            return false;
            
        var versionsPath = Path.Combine(directory, "versions");
        if (!Directory.Exists(versionsPath))
            return false;
            
        // 检查是否有版本目录
        var versionDirs = Directory.GetDirectories(versionsPath);
        return versionDirs.Length > 0;
    }
}

/// <summary>
/// 原版版本服务包装器 - 使用现有的MinecraftVersionService
/// </summary>
public class MinecraftVersionServiceWrapper : IVersionService
{
    private readonly MinecraftVersionService _service;

    public MinecraftVersionServiceWrapper(MinecraftVersionService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<MinecraftVersion>> GetAvailableVersionsAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        return await _service.GetAllVersionsAsync(cancellationToken);
    }

    public async Task<bool> DownloadVersionAsync(string versionId, Dictionary<string, object> parameters, Action<double, string>? progressCallback = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine($"[MinecraftVersionServiceWrapper] 开始下载原版Minecraft: {versionId}");
            progressCallback?.Invoke(0, "正在准备下载原版Minecraft...");
            
            // 获取Minecraft文件夹路径
            var minecraftFolder = GetMinecraftFolderPath();
            Console.WriteLine($"[MinecraftVersionServiceWrapper] 使用Minecraft文件夹: {minecraftFolder}");
            
            // 获取版本信息
            var allVersions = await _service.GetAllVersionsAsync(cancellationToken);
            var targetVersion = allVersions.FirstOrDefault(v => v.Id == versionId);
            
            if (targetVersion == null)
            {
                Console.WriteLine($"[MinecraftVersionServiceWrapper] 未找到版本: {versionId}");
                progressCallback?.Invoke(100, "未找到指定的Minecraft版本");
                return false;
            }
            
            Console.WriteLine($"[MinecraftVersionServiceWrapper] 找到目标版本: {targetVersion.Id}");
            progressCallback?.Invoke(10, "版本验证成功，开始下载...");
            
            // 获取版本清单条目
            var versionEntries = await VanillaInstaller.EnumerableMinecraftAsync(cancellationToken);
            var versionEntry = versionEntries.FirstOrDefault(v => v.Id == versionId);
            
            if (versionEntry == null)
            {
                Console.WriteLine($"[MinecraftVersionServiceWrapper] 未找到版本清单条目: {versionId}");
                progressCallback?.Invoke(100, "未找到版本清单条目");
                return false;
            }
            
            Console.WriteLine($"[MinecraftVersionServiceWrapper] 找到版本清单条目: {versionEntry.Id}");
            progressCallback?.Invoke(20, "开始下载Minecraft核心文件...");
            
            // 使用VanillaInstaller下载Minecraft核心
            var installer = VanillaInstaller.Create(minecraftFolder, versionEntry);
            
            // 设置进度回调
            installer.ProgressChanged += (sender, args) =>
            {
                var progress = args.Progress * 0.8 + 20; // 20-100% 的进度范围
                var status = GetInstallStepDescription(args.StepName);
                progressCallback?.Invoke(progress, status);
                Console.WriteLine($"[MinecraftVersionServiceWrapper] 安装进度: {args.Progress:P2} - {status}");
            };
            
            // 执行安装
            try
            {
                var minecraftEntry = await installer.InstallAsync(cancellationToken);
                
                if (minecraftEntry != null)
                {
                    Console.WriteLine($"[MinecraftVersionServiceWrapper] Minecraft核心下载完成: {versionId}");
                    progressCallback?.Invoke(100, "Minecraft核心下载完成");
                    return true;
                }
                else
                {
                    Console.WriteLine($"[MinecraftVersionServiceWrapper] Minecraft核心下载失败: {versionId}");
                    progressCallback?.Invoke(100, "Minecraft核心下载失败");
                    return false;
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Some dependent files encountered errors during download"))
            {
                Console.WriteLine($"[MinecraftVersionServiceWrapper] 依赖文件下载失败，尝试诊断问题: {ex.Message}");
                progressCallback?.Invoke(95, "依赖文件下载失败，正在重试...");
                
                // 尝试重试下载
                try
                {
                    Console.WriteLine($"[MinecraftVersionServiceWrapper] 开始重试下载...");
                    var retryInstaller = VanillaInstaller.Create(minecraftFolder, versionEntry);
                    
                    // 设置重试的进度回调
                    retryInstaller.ProgressChanged += (sender, args) =>
                    {
                        var progress = args.Progress * 0.8 + 20; // 20-100% 的进度范围
                        var status = GetInstallStepDescription(args.StepName);
                        progressCallback?.Invoke(progress, $"重试: {status}");
                        Console.WriteLine($"[MinecraftVersionServiceWrapper] 重试进度: {args.Progress:P2} - {status}");
                    };
                    
                    var retryResult = await retryInstaller.InstallAsync(cancellationToken);
                    
                    if (retryResult != null)
                    {
                        Console.WriteLine($"[MinecraftVersionServiceWrapper] 重试下载成功: {versionId}");
                        progressCallback?.Invoke(100, "重试下载成功");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"[MinecraftVersionServiceWrapper] 重试下载仍然失败: {versionId}");
                        progressCallback?.Invoke(100, "重试下载失败");
                        return false;
                    }
                }
                catch (Exception retryEx)
                {
                    Console.WriteLine($"[MinecraftVersionServiceWrapper] 重试下载异常: {retryEx.Message}");
                    progressCallback?.Invoke(100, $"重试失败: {retryEx.Message}");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MinecraftVersionServiceWrapper] 下载原版Minecraft失败: {ex.Message}");
            Console.WriteLine($"[MinecraftVersionServiceWrapper] 错误详情: {ex}");
            progressCallback?.Invoke(100, $"下载失败: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 获取Minecraft文件夹路径
    /// </summary>
    private string GetMinecraftFolderPath()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var currentDirectory = Path.Combine(baseDirectory, ".minecraft");
        
        // 如果当前目录存在游戏核心，使用当前目录
        if (HasGameCores(currentDirectory))
        {
            return currentDirectory;
        }
        
        // 尝试Debug目录
        var debugDirectory = Path.Combine(baseDirectory.Replace("Release", "Debug"), ".minecraft");
        if (HasGameCores(debugDirectory))
        {
            return debugDirectory;
        }
        
        // 尝试Release目录
        var releaseDirectory = Path.Combine(baseDirectory.Replace("Debug", "Release"), ".minecraft");
        if (HasGameCores(releaseDirectory))
        {
            return releaseDirectory;
        }
        
        // 默认使用当前目录
        return currentDirectory;
    }
    
    /// <summary>
    /// 检查目录是否有游戏核心
    /// </summary>
    private static bool HasGameCores(string directory)
    {
        if (!Directory.Exists(directory))
            return false;
            
        var versionsPath = Path.Combine(directory, "versions");
        if (!Directory.Exists(versionsPath))
            return false;
            
        // 检查是否有版本目录
        var versionDirs = Directory.GetDirectories(versionsPath);
        return versionDirs.Length > 0;
    }
    
    /// <summary>
    /// 获取安装步骤描述
    /// </summary>
    private static string GetInstallStepDescription(MinecraftLaunch.Base.Enums.InstallStep step)
    {
        return step switch
        {
            MinecraftLaunch.Base.Enums.InstallStep.Started => "开始安装",
            MinecraftLaunch.Base.Enums.InstallStep.DownloadVersionJson => "下载版本信息",
            MinecraftLaunch.Base.Enums.InstallStep.ParseMinecraft => "解析Minecraft信息",
            MinecraftLaunch.Base.Enums.InstallStep.DownloadAssetIndexFile => "下载资源索引",
            MinecraftLaunch.Base.Enums.InstallStep.DownloadLibraries => "下载依赖库",
            MinecraftLaunch.Base.Enums.InstallStep.RanToCompletion => "安装完成",
            _ => "安装中..."
        };
    }
}

/// <summary>
/// Forge版本服务包装器
/// </summary>
public class ForgeVersionServiceWrapper : IVersionService
{
    private readonly ForgeVersionService _service;

    public ForgeVersionServiceWrapper(ForgeVersionService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<MinecraftVersion>> GetAvailableVersionsAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        var mcVersion = parameters.GetValueOrDefault("mcVersion")?.ToString() ?? throw new ArgumentException("缺少mcVersion参数");
        var isNeoforge = parameters.GetValueOrDefault("isNeoforge", false);
        return await _service.GetAvailableVersionsAsync(mcVersion, (bool)isNeoforge, cancellationToken);
    }

    public async Task<bool> DownloadVersionAsync(string versionId, Dictionary<string, object> parameters, Action<double, string>? progressCallback = null, CancellationToken cancellationToken = default)
    {
        var mcVersion = parameters.GetValueOrDefault("mcVersion")?.ToString() ?? throw new ArgumentException("缺少mcVersion参数");
        var isNeoforge = parameters.GetValueOrDefault("isNeoforge", false);
        
        // 从版本ID中提取Forge版本
        var forgeVersion = versionId.Split('-').Last();
        return await _service.DownloadVersionAsync(mcVersion, forgeVersion, (bool)isNeoforge, progressCallback, cancellationToken);
    }
}

/// <summary>
/// Fabric版本服务包装器
/// </summary>
public class FabricVersionServiceWrapper : IVersionService
{
    private readonly FabricVersionService _service;

    public FabricVersionServiceWrapper(FabricVersionService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<MinecraftVersion>> GetAvailableVersionsAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        var mcVersion = parameters.GetValueOrDefault("mcVersion")?.ToString() ?? throw new ArgumentException("缺少mcVersion参数");
        return await _service.GetAvailableVersionsAsync(mcVersion, cancellationToken);
    }

    public async Task<bool> DownloadVersionAsync(string versionId, Dictionary<string, object> parameters, Action<double, string>? progressCallback = null, CancellationToken cancellationToken = default)
    {
        var mcVersion = parameters.GetValueOrDefault("mcVersion")?.ToString() ?? throw new ArgumentException("缺少mcVersion参数");
        
        // 从版本ID中提取Fabric Loader版本
        var loaderVersion = versionId.Split('_').First().Split('-').Last();
        return await _service.DownloadVersionAsync(mcVersion, loaderVersion, progressCallback, cancellationToken);
    }
}

/// <summary>
/// Quilt版本服务包装器
/// </summary>
public class QuiltVersionServiceWrapper : IVersionService
{
    private readonly QuiltVersionService _service;

    public QuiltVersionServiceWrapper(QuiltVersionService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<MinecraftVersion>> GetAvailableVersionsAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        var mcVersion = parameters.GetValueOrDefault("mcVersion")?.ToString() ?? throw new ArgumentException("缺少mcVersion参数");
        return await _service.GetAvailableVersionsAsync(mcVersion, cancellationToken);
    }

    public async Task<bool> DownloadVersionAsync(string versionId, Dictionary<string, object> parameters, Action<double, string>? progressCallback = null, CancellationToken cancellationToken = default)
    {
        var mcVersion = parameters.GetValueOrDefault("mcVersion")?.ToString() ?? throw new ArgumentException("缺少mcVersion参数");
        
        // 从版本ID中提取Quilt Loader版本
        var loaderVersion = versionId.Split('_').First().Split('-').Last();
        return await _service.DownloadVersionAsync(mcVersion, loaderVersion, progressCallback, cancellationToken);
    }
}

/// <summary>
/// Optifine版本服务包装器
/// </summary>
public class OptifineVersionServiceWrapper : IVersionService
{
    private readonly OptifineVersionService _service;

    public OptifineVersionServiceWrapper(OptifineVersionService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<MinecraftVersion>> GetAvailableVersionsAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        var mcVersion = parameters.GetValueOrDefault("mcVersion")?.ToString() ?? throw new ArgumentException("缺少mcVersion参数");
        return await _service.GetAvailableVersionsAsync(mcVersion, cancellationToken);
    }

    public async Task<bool> DownloadVersionAsync(string versionId, Dictionary<string, object> parameters, Action<double, string>? progressCallback = null, CancellationToken cancellationToken = default)
    {
        var mcVersion = parameters.GetValueOrDefault("mcVersion")?.ToString() ?? throw new ArgumentException("缺少mcVersion参数");
        
        // 从版本ID中提取Optifine补丁版本
        var patch = versionId.Split('_').Last();
        return await _service.DownloadVersionAsync(mcVersion, patch, progressCallback, cancellationToken);
    }
}