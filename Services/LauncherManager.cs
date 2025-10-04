using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Components.Parser;
using MinecraftLaunch.Launch;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Base.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinecraftLaunch.Base.Interfaces;

namespace swpumc.Services;

/// <summary>
/// 启动器管理器
/// 负责管理游戏启动配置和启动游戏
/// </summary>
public class LauncherManager
{
    private readonly JavaEnvironmentService _javaService;

    public LauncherManager()
    {
        _javaService = new JavaEnvironmentService();
    }

    #region 启动配置属性

    /// <summary>
    /// 当前账户名称
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// 最大内存大小 (MB)
    /// </summary>
    public int MaxMemorySize { get; set; } = 4096;

    /// <summary>
    /// 最小内存大小 (MB)
    /// </summary>
    public int MinMemorySize { get; set; } = 1024;

    /// <summary>
    /// Java路径
    /// </summary>
    public string JavaPath { get; set; } = "java";

    /// <summary>
    /// 窗口宽度
    /// </summary>
    public int Width { get; set; } = 1280;

    /// <summary>
    /// 窗口高度
    /// </summary>
    public int Height { get; set; } = 720;

    /// <summary>
    /// 服务器地址
    /// </summary>
    public string ServerAddress { get; set; } = string.Empty;

    /// <summary>
    /// 启动器名称
    /// </summary>
    public string LauncherName { get; set; } = "SukiMC";

    /// <summary>
    /// 是否全屏
    /// </summary>
    public bool IsFullScreen { get; set; } = false;

    /// <summary>
    /// 游戏根目录
    /// </summary>
    public string GameDirectory { get; set; } = GetGameDirectory();

    /// <summary>
    /// 游戏核心ID
    /// </summary>
    public string GameCoreId { get; set; } = string.Empty;

    /// <summary>
    /// 自定义JVM参数
    /// </summary>
    public List<string> CustomJvmArgs { get; set; } = new List<string>();

    /// <summary>
    /// 自定义游戏参数
    /// </summary>
    public List<string> CustomGameArgs { get; set; } = new List<string>();

    /// <summary>
    /// 是否启用调试模式
    /// </summary>
    public bool EnableDebug { get; set; } = false;

    /// <summary>
    /// 是否启用快速启动
    /// </summary>
    public bool EnableQuickPlay { get; set; } = false;

    #endregion

    #region Java环境管理

    /// <summary>
    /// 获取所有可用的Java安装
    /// </summary>
    public async Task<List<JavaInstallation>> GetAvailableJavaInstallationsAsync()
    {
        return await _javaService.DetectJavaEnvironmentsAsync();
    }

    /// <summary>
    /// 设置Java路径
    /// </summary>
    /// <param name="javaHome">Java安装路径</param>
    public void SetJavaPath(string javaHome)
    {
        // 如果传入的已经是可执行文件路径，直接使用
        if (File.Exists(javaHome))
        {
            JavaPath = javaHome;
            return;
        }

        // 否则尝试构建可执行文件路径
        var javaExe = Path.Combine(javaHome, "bin", "java.exe");
        if (!File.Exists(javaExe))
        {
            javaExe = Path.Combine(javaHome, "bin", "java");
        }

        if (File.Exists(javaExe))
        {
            JavaPath = javaExe;
        }
        else
        {
            throw new FileNotFoundException($"Java可执行文件未找到: {javaHome}");
        }
    }

    /// <summary>
    /// 自动选择最佳Java版本
    /// </summary>
    public async Task<bool> AutoSelectJavaAsync()
    {
        var installations = await GetAvailableJavaInstallationsAsync();
        if (installations.Count == 0) return false;

        // 优先选择默认Java安装
        var defaultJava = installations.FirstOrDefault(j => j.IsDefault);
        if (defaultJava != null)
        {
            JavaPath = defaultJava.JavaExecutable;
            return true;
        }

        // 选择最新版本的Java
        var latestJava = installations.OrderByDescending(j => j.Version).First();
        JavaPath = latestJava.JavaExecutable;
        return true;
    }

    #endregion

    #region 启动配置

    /// <summary>
    /// 创建启动配置
    /// </summary>
    public LaunchConfig CreateLaunchConfig()
    {
        if (string.IsNullOrEmpty(AccountName))
            throw new InvalidOperationException("账户未设置");

        if (string.IsNullOrEmpty(GameCoreId))
            throw new InvalidOperationException("游戏核心未设置");

        // 创建离线账户
        var account = new OfflineAccount(AccountName, Guid.NewGuid(), "Offline");

        var config = new LaunchConfig
        {
            Account = account,
            MaxMemorySize = MaxMemorySize,
            MinMemorySize = MinMemorySize,
            JavaPath = new JavaEntry { JavaPath = JavaPath },
            Width = Width,
            Height = Height,
            LauncherName = LauncherName,
            IsFullscreen = IsFullScreen,
            ServerInfo = !string.IsNullOrEmpty(ServerAddress) ? ParseServerAddress(ServerAddress) : null,
            JvmArguments = GetDefaultJvmArgs().Concat(CustomJvmArgs).ToArray()
        };

        return config;
    }

    /// <summary>
    /// 获取默认JVM参数
    /// </summary>
    private List<string> GetDefaultJvmArgs()
    {
        var args = new List<string>();

        // 现代GC优化（适用于1.13+）
        args.AddRange(new[]
        {
            "-XX:+UseG1GC",
            "-XX:+UnlockExperimentalVMOptions",
            "-XX:G1NewSizePercent=20",
            "-XX:G1ReservePercent=20",
            "-XX:MaxGCPauseMillis=50",
            "-XX:G1HeapRegionSize=32M"
        });

        // 调试模式
        if (EnableDebug)
        {
            args.Add("-Ddebug=true");
        }

        return args;
    }

    /// <summary>
    /// 获取默认游戏参数
    /// </summary>
    private List<string> GetDefaultGameArgs()
    {
        var args = new List<string>();

        if (EnableQuickPlay)
        {
            args.Add("--quickPlayMode=true");
        }

        return args;
    }

    /// <summary>
    /// 解析服务器地址
    /// </summary>
    private dynamic ParseServerAddress(string address)
    {
        var parts = address.Split(':');
        return new
        {
            Address = parts[0],
            Port = parts.Length > 1 && int.TryParse(parts[1], out var port) ? port : 25565
        };
    }

    #endregion

    #region 游戏目录管理

    /// <summary>
    /// 智能获取游戏目录
    /// </summary>
    private static string GetGameDirectory()
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
    /// 检查目录是否包含游戏核心
    /// </summary>
    private static bool HasGameCores(string directory)
    {
        try
        {
            if (!Directory.Exists(directory))
                return false;
                
            var versionsDirectory = Path.Combine(directory, "versions");
            if (!Directory.Exists(versionsDirectory))
                return false;
                
            var versionDirectories = Directory.GetDirectories(versionsDirectory);
            return versionDirectories.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region 游戏启动

    /// <summary>
    /// 启动游戏
    /// </summary>
    public async Task<MinecraftProcess> LaunchGameAsync()
    {
        var config = CreateLaunchConfig();
        var parser = new MinecraftParser(GameDirectory);
        var runner = new MinecraftRunner(config, parser);

        // 启动游戏
        var process = await runner.RunAsync(GameCoreId);
        return process;
    }


    #endregion

    #region 便捷配置方法

    /// <summary>
    /// 设置内存配置
    /// </summary>
    public void SetMemoryConfig(int minMemory, int maxMemory)
    {
        MinMemorySize = minMemory;
        MaxMemorySize = maxMemory;
    }

    /// <summary>
    /// 设置窗口配置
    /// </summary>
    public void SetWindowConfig(int width, int height, bool fullScreen = false)
    {
        Width = width;
        Height = height;
        IsFullScreen = fullScreen;
    }

    /// <summary>
    /// 添加自定义JVM参数
    /// </summary>
    public void AddJvmArg(string arg)
    {
        if (!CustomJvmArgs.Contains(arg))
        {
            CustomJvmArgs.Add(arg);
        }
    }

    /// <summary>
    /// 添加自定义游戏参数
    /// </summary>
    public void AddGameArg(string arg)
    {
        if (!CustomGameArgs.Contains(arg))
        {
            CustomGameArgs.Add(arg);
        }
    }

    /// <summary>
    /// 清除所有自定义参数
    /// </summary>
    public void ClearCustomArgs()
    {
        CustomJvmArgs.Clear();
        CustomGameArgs.Clear();
    }

    /// <summary>
    /// 设置服务器连接
    /// </summary>
    public void SetServerConnection(string address, int port = 25565)
    {
        ServerAddress = $"{address}:{port}";
    }

    #endregion

    #region 预设配置

    /// <summary>
    /// 应用性能优化预设
    /// </summary>
    public void ApplyPerformancePreset()
    {
        // 内存配置
        var totalMemory = GC.GetTotalMemory(false) / 1024 / 1024; // 获取系统内存
        var recommendedMax = Math.Min(totalMemory / 2, 8192); // 推荐最大内存为系统内存的一半，但不超过8GB
        var recommendedMin = Math.Max(recommendedMax / 4, 1024); // 最小内存为最大内存的1/4，但不少于1GB

        SetMemoryConfig((int)recommendedMin, (int)recommendedMax);

        // JVM优化参数
        ClearCustomArgs();
        AddJvmArg("-XX:+UseG1GC");
        AddJvmArg("-XX:+UnlockExperimentalVMOptions");
        AddJvmArg("-XX:G1NewSizePercent=20");
        AddJvmArg("-XX:G1ReservePercent=20");
        AddJvmArg("-XX:MaxGCPauseMillis=50");
        AddJvmArg("-XX:G1HeapRegionSize=32M");
        AddJvmArg("-XX:+UseStringDeduplication");
        AddJvmArg("-XX:+OptimizeStringConcat");
    }

    /// <summary>
    /// 应用调试预设
    /// </summary>
    public void ApplyDebugPreset()
    {
        EnableDebug = true;
        AddJvmArg("-Ddebug=true");
        AddJvmArg("-Dlog4j.configurationFile=log4j2.xml");
        AddGameArg("--debugMode");
    }

    /// <summary>
    /// 应用快速启动预设
    /// </summary>
    public void ApplyQuickStartPreset()
    {
        EnableQuickPlay = true;
        AddGameArg("--quickPlayMode=true");
        AddGameArg("--skipServer");
    }

    #endregion

    #region 游戏核心管理

    /// <summary>
    /// 检测本地所有游戏核心
    /// </summary>
    /// <param name="gameDirectory">游戏目录，默认为当前设置的游戏目录</param>
    /// <returns>游戏核心列表</returns>
    public async Task<List<MinecraftEntry>> GetLocalGameCoresAsync(string gameDirectory = null)
    {
        var directory = gameDirectory ?? GameDirectory;
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"游戏目录不存在: {directory}");
        }

        try
        {
            var parser = new MinecraftParser(directory);
            var cores = parser.GetMinecrafts();
            return cores.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"检测游戏核心失败: {ex.Message}");
            return new List<MinecraftEntry>();
        }
    }

    /// <summary>
    /// 获取特定版本的游戏核心
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <param name="gameDirectory">游戏目录</param>
    /// <returns>游戏核心</returns>
    public async Task<MinecraftEntry> GetGameCoreAsync(string versionId, string gameDirectory = null)
    {
        var directory = gameDirectory ?? GameDirectory;
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"游戏目录不存在: {directory}");
        }

        try
        {
            var parser = new MinecraftParser(directory);
            return parser.GetMinecraft(versionId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取游戏核心失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 获取MinecraftEntry对象（GetGameCoreAsync的别名）
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <param name="gameDirectory">游戏目录</param>
    /// <returns>MinecraftEntry对象</returns>
    public async Task<MinecraftEntry> GetMinecraftEntryAsync(string versionId, string gameDirectory = null)
    {
        return await GetGameCoreAsync(versionId, gameDirectory);
    }

    /// <summary>
    /// 检查游戏核心的依赖库完整性
    /// </summary>
    /// <param name="minecraftEntry">Minecraft游戏核心</param>
    /// <returns>检查结果</returns>
    public async Task<dynamic> CheckGameCoreLibrariesAsync(MinecraftEntry minecraftEntry)
    {
        if (minecraftEntry == null)
            throw new ArgumentNullException(nameof(minecraftEntry));

        try
        {
            // 获取所需的库文件
            var (libraries, nativeLibraries) = minecraftEntry.GetRequiredLibraries();
            var allLibraries = libraries.Concat(nativeLibraries).ToList();
            
            var missingLibraries = new List<object>();
            var failedLibraries = new List<object>();
            var isComplete = true;

            // 检查每个库文件是否存在且完整
            foreach (var library in allLibraries)
            {
                if (library is IVerifiableDependency verifiableDependency)
                {
                    var libraryPath = library.FullPath;
                    
                    if (!File.Exists(libraryPath))
                    {
                        missingLibraries.Add(new { Name = library.MavenName, Path = libraryPath });
                        isComplete = false;
                    }
                    else
                    {
                        // 验证文件大小和哈希值
                        var fileInfo = new FileInfo(libraryPath);
                        if (verifiableDependency.Size.HasValue && fileInfo.Length != verifiableDependency.Size.Value)
                        {
                            failedLibraries.Add(new { Name = library.MavenName, Path = libraryPath, Reason = "文件大小不匹配" });
                            isComplete = false;
                        }
                    }
                }
            }

            return new
            {
                IsComplete = isComplete,
                TotalLibraries = allLibraries.Count,
                MissingLibraries = missingLibraries,
                FailedLibraries = failedLibraries
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"检查依赖库失败: {ex.Message}");
            return new { IsComplete = false, Error = ex.Message };
        }
    }

    /// <summary>
    /// 检查游戏核心的资源文件完整性
    /// </summary>
    /// <param name="minecraftEntry">Minecraft游戏核心</param>
    /// <returns>检查结果</returns>
    public async Task<dynamic> CheckGameCoreAssetsAsync(MinecraftEntry minecraftEntry)
    {
        if (minecraftEntry == null)
            throw new ArgumentNullException(nameof(minecraftEntry));

        try
        {
            // 获取所需的资源文件
            var requiredAssets = minecraftEntry.GetRequiredAssets().ToList();
            
            var missingAssets = new List<object>();
            var failedAssets = new List<object>();
            var isComplete = true;

            // 检查每个资源文件是否存在且完整
            foreach (var asset in requiredAssets)
            {
                var assetPath = asset.FullPath;
                
                if (!File.Exists(assetPath))
                {
                    missingAssets.Add(new { Key = asset.Key, Path = assetPath, Sha1 = asset.Sha1 });
                    isComplete = false;
                }
                else
                {
                    // 验证文件大小和哈希值
                    var fileInfo = new FileInfo(assetPath);
                    if (asset.Size.HasValue && fileInfo.Length != asset.Size.Value)
                    {
                        failedAssets.Add(new { Key = asset.Key, Path = assetPath, Reason = "文件大小不匹配" });
                        isComplete = false;
                    }
                }
            }

            return new
            {
                IsComplete = isComplete,
                TotalAssets = requiredAssets.Count,
                MissingAssets = missingAssets,
                FailedAssets = failedAssets
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"检查资源文件失败: {ex.Message}");
            return new { IsComplete = false, Error = ex.Message };
        }
    }

    /// <summary>
    /// 下载缺失的依赖库
    /// </summary>
    /// <param name="minecraftEntry">Minecraft游戏核心</param>
    /// <param name="librariesPath">库文件路径</param>
    /// <returns>下载结果</returns>
    public async Task<bool> DownloadMissingLibrariesAsync(MinecraftEntry minecraftEntry, string librariesPath = null)
    {
        if (minecraftEntry == null)
            throw new ArgumentNullException(nameof(minecraftEntry));

        try
        {
            // 使用 MinecraftResourceDownloader 进行依赖下载
            var resourceDownloader = new MinecraftResourceDownloader(minecraftEntry);
            
            // 使用 VerifyAndDownloadDependenciesAsync 方法下载所有依赖
            var result = await resourceDownloader.VerifyAndDownloadDependenciesAsync();
            
            if (result.Failed.Any())
            {
                Console.WriteLine($"下载依赖库时遇到错误: {result.Failed.Count()} 个文件下载失败");
                return false;
            }

            Console.WriteLine($"成功下载依赖库文件");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"下载依赖库失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 下载缺失的资源文件
    /// </summary>
    /// <param name="minecraftEntry">Minecraft游戏核心</param>
    /// <returns>下载结果</returns>
    public async Task<bool> DownloadMissingAssetsAsync(MinecraftEntry minecraftEntry)
    {
        if (minecraftEntry == null)
            throw new ArgumentNullException(nameof(minecraftEntry));

        try
        {
            // 使用 MinecraftResourceDownloader 进行资源下载
            var resourceDownloader = new MinecraftResourceDownloader(minecraftEntry);
            
            // 使用 VerifyAndDownloadDependenciesAsync 方法下载所有依赖（包括资源文件）
            var result = await resourceDownloader.VerifyAndDownloadDependenciesAsync();
            
            if (result.Failed.Any())
            {
                Console.WriteLine($"下载资源文件时遇到错误: {result.Failed.Count()} 个文件下载失败");
                return false;
            }

            Console.WriteLine($"成功下载资源文件");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"下载资源文件失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 验证游戏核心完整性
    /// </summary>
    /// <param name="minecraftEntry">Minecraft游戏核心</param>
    /// <returns>验证结果</returns>
    public async Task<GameCoreValidationResult> ValidateGameCoreAsync(MinecraftEntry minecraftEntry)
    {
        if (minecraftEntry == null)
            throw new ArgumentNullException(nameof(minecraftEntry));

        var result = new GameCoreValidationResult
        {
            Core = minecraftEntry,
            IsValid = true,
            Issues = new List<string>()
        };

        try
        {
            // 检查依赖库
            var libraryResult = await CheckGameCoreLibrariesAsync(minecraftEntry);
            if (!libraryResult.IsComplete)
            {
                result.IsValid = false;
                result.Issues.Add($"缺失 {libraryResult.MissingLibraries.Count} 个依赖库");
                result.MissingLibraries = libraryResult.MissingLibraries.ToList();
            }

            // 检查资源文件
            var assetResult = await CheckGameCoreAssetsAsync(minecraftEntry);
            if (!assetResult.IsComplete)
            {
                result.IsValid = false;
                result.Issues.Add($"缺失 {assetResult.MissingAssets.Count} 个资源文件");
                result.MissingAssets = assetResult.MissingAssets.ToList();
            }

            // 检查客户端jar文件
            if (!File.Exists(minecraftEntry.ClientJarPath))
            {
                result.IsValid = false;
                result.Issues.Add("客户端jar文件不存在");
            }

            return result;
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Issues.Add($"验证过程出错: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// 获取游戏核心详细信息
    /// </summary>
    /// <param name="minecraftEntry">Minecraft游戏核心</param>
    /// <returns>详细信息</returns>
    public GameCoreInfo GetGameCoreInfo(MinecraftEntry minecraftEntry)
    {
        if (minecraftEntry == null)
            throw new ArgumentNullException(nameof(minecraftEntry));

        return new GameCoreInfo
        {
            Id = minecraftEntry.Id,
            Type = minecraftEntry.Version.Type.ToString(),
            Source = minecraftEntry.Version.VersionId,
            MainClass = minecraftEntry.Version.VersionId,
            Assets = minecraftEntry.Version.VersionId,
            ReleaseTime = minecraftEntry.ReleaseTime,
            IsForge = minecraftEntry is ModifiedMinecraftEntry modEntry && modEntry.ModLoaders.Any(m => m.Type == MinecraftLaunch.Base.Enums.ModLoaderType.Forge),
            IsFabric = minecraftEntry is ModifiedMinecraftEntry modEntry2 && modEntry2.ModLoaders.Any(m => m.Type == MinecraftLaunch.Base.Enums.ModLoaderType.Fabric),
            IsQuilt = minecraftEntry is ModifiedMinecraftEntry modEntry3 && modEntry3.ModLoaders.Any(m => m.Type == MinecraftLaunch.Base.Enums.ModLoaderType.Quilt),
            ForgeVersion = GetModLoaderVersion(minecraftEntry, MinecraftLaunch.Base.Enums.ModLoaderType.Forge),
            FabricVersion = GetModLoaderVersion(minecraftEntry, MinecraftLaunch.Base.Enums.ModLoaderType.Fabric),
            QuiltVersion = GetModLoaderVersion(minecraftEntry, MinecraftLaunch.Base.Enums.ModLoaderType.Quilt),
            JavaVersion = "8", // 默认Java版本
            DisplayName = GetDisplayName(minecraftEntry)
        };
    }

    /// <summary>
    /// 获取ModLoader版本
    /// </summary>
    private string GetModLoaderVersion(MinecraftEntry minecraftEntry, MinecraftLaunch.Base.Enums.ModLoaderType loaderType)
    {
        if (minecraftEntry is ModifiedMinecraftEntry modEntry)
        {
            var loader = modEntry.ModLoaders.FirstOrDefault(m => m.Type == loaderType);
            return loader.Version ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// 获取游戏核心显示名称
    /// </summary>
    private string GetDisplayName(MinecraftEntry minecraftEntry)
    {
        var name = minecraftEntry.Id;
        
        if (minecraftEntry is ModifiedMinecraftEntry modEntry)
        {
            var forgeLoader = modEntry.ModLoaders.FirstOrDefault(m => m.Type == MinecraftLaunch.Base.Enums.ModLoaderType.Forge);
            if (forgeLoader.Version != null)
            {
                name += $" (Forge {forgeLoader.Version})";
            }
            else
            {
                var fabricLoader = modEntry.ModLoaders.FirstOrDefault(m => m.Type == MinecraftLaunch.Base.Enums.ModLoaderType.Fabric);
                if (fabricLoader.Version != null)
                {
                    name += $" (Fabric {fabricLoader.Version})";
                }
                else
                {
                    var quiltLoader = modEntry.ModLoaders.FirstOrDefault(m => m.Type == MinecraftLaunch.Base.Enums.ModLoaderType.Quilt);
                    if (quiltLoader.Version != null)
                    {
                        name += $" (Quilt {quiltLoader.Version})";
                    }
                }
            }
        }

        return name;
    }

    /// <summary>
    /// 验证和下载游戏核心依赖
    /// </summary>
    /// <param name="minecraftEntry">Minecraft游戏核心</param>
    /// <returns>验证和下载结果</returns>
    public async Task<bool> VerifyAndDownloadGameCoreDependenciesAsync(MinecraftEntry minecraftEntry)
    {
        if (minecraftEntry == null)
            throw new ArgumentNullException(nameof(minecraftEntry));

        try
        {
            Console.WriteLine($"开始验证游戏核心依赖: {minecraftEntry.Id}");

            // 使用 MinecraftResourceDownloader 进行依赖验证和下载
            var resourceDownloader = new MinecraftResourceDownloader(minecraftEntry);
            
            // 使用 VerifyAndDownloadDependenciesAsync 方法验证和下载所有依赖
            var result = await resourceDownloader.VerifyAndDownloadDependenciesAsync();
            
            if (result.Failed.Any())
            {
                Console.WriteLine($"验证游戏核心依赖时遇到错误: {result.Failed.Count()} 个文件下载失败");
                return false;
            }

            Console.WriteLine($"游戏核心依赖验证完成");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"验证游戏核心依赖失败: {ex.Message}");
            return false;
        }
    }


    #endregion

    #region 游戏核心扫描和检测

    /// <summary>
    /// 扫描并检测所有Minecraft游戏核心
    /// </summary>
    /// <param name="gameDirectory">游戏目录，默认为当前设置的游戏目录</param>
    /// <returns>扫描结果</returns>
    public async Task<GameCoreScanResult> ScanGameCoresAsync(string gameDirectory = null)
    {
        var result = new GameCoreScanResult
        {
            GameDirectory = gameDirectory ?? GameDirectory,
            Success = false,
            GameCores = new List<GameCoreInfo>(),
            Issues = new List<string>()
        };

        try
        {
            Console.WriteLine("[LauncherManager] 开始扫描Minecraft游戏核心...");
            
            var directory = gameDirectory ?? GameDirectory;
            result.GameDirectory = directory;
            
            Console.WriteLine($"[LauncherManager] 扫描游戏目录: {directory}");
            
            // 检查.minecraft目录是否存在，如果不存在则创建
            if (!Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                    Console.WriteLine($"[LauncherManager] 创建游戏目录: {directory}");
                }
                catch (Exception ex)
                {
                    result.Issues.Add($"无法创建游戏目录: {directory}, 错误: {ex.Message}");
                    Console.WriteLine($"[LauncherManager] 无法创建游戏目录: {ex.Message}");
                    return result;
                }
            }
            
            // 检查versions目录是否存在，如果不存在则创建
            var versionsDirectory = Path.Combine(directory, "versions");
            if (!Directory.Exists(versionsDirectory))
            {
                try
                {
                    Directory.CreateDirectory(versionsDirectory);
                    Console.WriteLine($"[LauncherManager] 创建versions目录: {versionsDirectory}");
                }
                catch (Exception ex)
                {
                    result.Issues.Add($"无法创建versions目录: {versionsDirectory}, 错误: {ex.Message}");
                    Console.WriteLine($"[LauncherManager] 无法创建versions目录: {ex.Message}");
                    return result;
                }
            }
            
            Console.WriteLine($"[LauncherManager] 开始扫描versions目录: {versionsDirectory}");
            
            // 扫描versions目录下的所有子目录
            var versionDirectories = Directory.GetDirectories(versionsDirectory);
            Console.WriteLine($"[LauncherManager] 发现 {versionDirectories.Length} 个版本目录");
            
            // 检测所有游戏核心
            var gameCores = await GetLocalGameCoresAsync(directory);
            Console.WriteLine($"[LauncherManager] 检测到 {gameCores.Count} 个游戏核心");
            
            // 处理每个游戏核心
            foreach (var core in gameCores)
            {
                try
                {
                    var coreInfo = GetGameCoreInfo(core);
                    result.GameCores.Add(coreInfo);
                    
                    Console.WriteLine($"[LauncherManager] 游戏核心: {coreInfo.DisplayName}");
                    Console.WriteLine($"  - ID: {coreInfo.Id}");
                    Console.WriteLine($"  - 类型: {coreInfo.Type}");
                    Console.WriteLine($"  - 来源: {coreInfo.Source}");
                    Console.WriteLine($"  - 主类: {coreInfo.MainClass}");
                    Console.WriteLine($"  - 资源版本: {coreInfo.Assets}");
                    Console.WriteLine($"  - 发布时间: {coreInfo.ReleaseTime:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"  - Java版本: {coreInfo.JavaVersion}");
                    
                    // 检查加载器类型
                    if (coreInfo.IsForge)
                    {
                        Console.WriteLine($"  - Forge版本: {coreInfo.ForgeVersion}");
                    }
                    if (coreInfo.IsFabric)
                    {
                        Console.WriteLine($"  - Fabric版本: {coreInfo.FabricVersion}");
                    }
                    if (coreInfo.IsQuilt)
                    {
                        Console.WriteLine($"  - Quilt版本: {coreInfo.QuiltVersion}");
                    }
                    
                    Console.WriteLine("  --------------------");
                }
                catch (Exception ex)
                {
                    var errorMsg = $"处理游戏核心时出错: {ex.Message}";
                    result.Issues.Add(errorMsg);
                    Console.WriteLine($"[LauncherManager] {errorMsg}");
                }
            }
            
            result.Success = true;
            Console.WriteLine($"[LauncherManager] Minecraft游戏核心扫描完成，共检测到 {result.GameCores.Count} 个游戏核心");
            
            return result;
        }
        catch (Exception ex)
        {
            var errorMsg = $"扫描游戏核心失败: {ex.Message}";
            result.Issues.Add(errorMsg);
            Console.WriteLine($"[LauncherManager] {errorMsg}");
            Console.WriteLine($"[LauncherManager] 错误详情: {ex}");
            return result;
        }
    }

    /// <summary>
    /// 快速扫描游戏核心（仅检测基本信息，不进行完整性验证）
    /// </summary>
    /// <param name="gameDirectory">游戏目录</param>
    /// <returns>扫描结果</returns>
    public async Task<GameCoreScanResult> QuickScanGameCoresAsync(string gameDirectory = null)
    {
        var result = new GameCoreScanResult
        {
            GameDirectory = gameDirectory ?? GameDirectory,
            Success = false,
            GameCores = new List<GameCoreInfo>(),
            Issues = new List<string>()
        };

        try
        {
            Console.WriteLine("[LauncherManager] 开始快速扫描Minecraft游戏核心...");
            
            var directory = gameDirectory ?? GameDirectory;
            result.GameDirectory = directory;
            
            // 检查目录是否存在
            if (!Directory.Exists(directory))
            {
                result.Issues.Add($"游戏目录不存在: {directory}");
                return result;
            }
            
            var versionsDirectory = Path.Combine(directory, "versions");
            if (!Directory.Exists(versionsDirectory))
            {
                result.Issues.Add($"versions目录不存在: {versionsDirectory}");
                return result;
            }
            
            // 检测所有游戏核心
            var gameCores = await GetLocalGameCoresAsync(directory);
            Console.WriteLine($"[LauncherManager] 快速扫描检测到 {gameCores.Count} 个游戏核心");
            
            // 处理每个游戏核心（不进行完整性验证）
            foreach (var core in gameCores)
            {
                try
                {
                    var coreInfo = GetGameCoreInfo(core);
                    coreInfo.IsValid = null; // 未验证
                    result.GameCores.Add(coreInfo);
                    
                    Console.WriteLine($"[LauncherManager] 游戏核心: {coreInfo.DisplayName}");
                }
                catch (Exception ex)
                {
                    var errorMsg = $"处理游戏核心 {core.Id} 时出错: {ex.Message}";
                    result.Issues.Add(errorMsg);
                    Console.WriteLine($"[LauncherManager] {errorMsg}");
                }
            }
            
            result.Success = true;
            Console.WriteLine($"[LauncherManager] 快速扫描完成，共检测到 {result.GameCores.Count} 个游戏核心");
            
            return result;
        }
        catch (Exception ex)
        {
            var errorMsg = $"快速扫描游戏核心失败: {ex.Message}";
            result.Issues.Add(errorMsg);
            Console.WriteLine($"[LauncherManager] {errorMsg}");
            return result;
        }
    }

    /// <summary>
    /// 获取游戏核心统计信息
    /// </summary>
    /// <param name="scanResult">扫描结果</param>
    /// <returns>统计信息</returns>
    public GameCoreStatistics GetGameCoreStatistics(GameCoreScanResult scanResult)
    {
        if (scanResult?.GameCores == null)
        {
            return new GameCoreStatistics();
        }

        var stats = new GameCoreStatistics
        {
            TotalCores = scanResult.GameCores.Count,
            ValidCores = scanResult.GameCores.Count(c => c.IsValid == true),
            InvalidCores = scanResult.GameCores.Count(c => c.IsValid == false),
            UnverifiedCores = scanResult.GameCores.Count(c => c.IsValid == null),
            ForgeCores = scanResult.GameCores.Count(c => c.IsForge),
            FabricCores = scanResult.GameCores.Count(c => c.IsFabric),
            QuiltCores = scanResult.GameCores.Count(c => c.IsQuilt),
            VanillaCores = scanResult.GameCores.Count(c => !c.IsForge && !c.IsFabric && !c.IsQuilt)
        };

        return stats;
    }

    #region 初始化方法

    /// <summary>
    /// 初始化Minecraft游戏核心并保存到全局配置
    /// </summary>
    public async Task InitializeMinecraftCoresAsync(IConfigService configService)
    {
        try
        {
            Console.WriteLine("[LauncherManager] 开始初始化Minecraft游戏核心...");
            
            // 智能获取游戏目录
            var gameDirectory = GetGameDirectory();
            GameDirectory = gameDirectory;
            
            // 如果目录不存在，创建它
            if (!Directory.Exists(gameDirectory))
            {
                Directory.CreateDirectory(gameDirectory);
                Console.WriteLine($"[LauncherManager] 创建游戏目录: {gameDirectory}");
            }
            
            // 使用扫描功能
            var scanResult = await ScanGameCoresAsync(gameDirectory);
            
            if (scanResult.Success)
            {
                Console.WriteLine($"[LauncherManager] Minecraft游戏核心扫描完成，共检测到 {scanResult.GameCores.Count} 个游戏核心");
                
                // 获取统计信息
                var statistics = GetGameCoreStatistics(scanResult);
                Console.WriteLine($"[LauncherManager] 统计信息:");
                Console.WriteLine($"  - 总核心数: {statistics.TotalCores}");
                Console.WriteLine($"  - 完整核心: {statistics.ValidCores}");
                Console.WriteLine($"  - 不完整核心: {statistics.InvalidCores}");
                Console.WriteLine($"  - 未验证核心: {statistics.UnverifiedCores}");
                Console.WriteLine($"  - Forge核心: {statistics.ForgeCores}");
                Console.WriteLine($"  - Fabric核心: {statistics.FabricCores}");
                Console.WriteLine($"  - Quilt核心: {statistics.QuiltCores}");
                Console.WriteLine($"  - 原版核心: {statistics.VanillaCores}");
                
                // 转换为MinecraftCoreInfo格式
                var minecraftCores = scanResult.GameCores.Select(core => new Models.MinecraftCoreInfo
                {
                    Id = core.Id,
                    DisplayName = core.DisplayName,
                    Type = core.Type,
                    Source = core.Source,
                    MainClass = core.MainClass,
                    Assets = core.Assets,
                    JavaVersion = core.JavaVersion,
                    ForgeVersion = core.ForgeVersion,
                    FabricVersion = core.FabricVersion,
                    QuiltVersion = core.QuiltVersion,
                    LastDetected = DateTime.Now,
                    IsValid = true
                }).ToList();
                
                // 保存到全局配置
                await configService.UpdateMinecraftCoresAsync(minecraftCores);
                Console.WriteLine($"[LauncherManager] Minecraft核心信息已保存到全局配置");
            }
            else
            {
                Console.WriteLine($"[LauncherManager] Minecraft游戏核心扫描失败");
                foreach (var issue in scanResult.Issues)
                {
                    Console.WriteLine($"[LauncherManager] 问题: {issue}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LauncherManager] Minecraft游戏核心初始化失败: {ex.Message}");
            Console.WriteLine($"[LauncherManager] 错误详情: {ex}");
        }
    }

    #endregion

    #endregion
}

#region 游戏核心相关数据模型

/// <summary>
/// 游戏核心验证结果
/// </summary>
public class GameCoreValidationResult
{
    public MinecraftEntry? Core { get; set; }
    public bool IsValid { get; set; }
    public List<string> Issues { get; set; } = new List<string>();
    public List<object> MissingLibraries { get; set; } = new List<object>();
    public List<object> MissingAssets { get; set; } = new List<object>();
}

/// <summary>
/// 游戏核心详细信息
/// </summary>
public class GameCoreInfo
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string MainClass { get; set; } = string.Empty;
    public string Assets { get; set; } = string.Empty;
    public DateTime ReleaseTime { get; set; }
    public bool IsForge { get; set; }
    public bool IsFabric { get; set; }
    public bool IsQuilt { get; set; }
    public string ForgeVersion { get; set; } = string.Empty;
    public string FabricVersion { get; set; } = string.Empty;
    public string QuiltVersion { get; set; } = string.Empty;
    public string JavaVersion { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool? IsValid { get; set; } // null = 未验证, true = 完整, false = 不完整
    public List<string> ValidationIssues { get; set; } = new List<string>();
}

/// <summary>
/// 游戏核心扫描结果
/// </summary>
public class GameCoreScanResult
{
    public string GameDirectory { get; set; } = string.Empty;
    public bool Success { get; set; }
    public List<GameCoreInfo> GameCores { get; set; } = new List<GameCoreInfo>();
    public List<string> Issues { get; set; } = new List<string>();
}

/// <summary>
/// 游戏核心统计信息
/// </summary>
public class GameCoreStatistics
{
    public int TotalCores { get; set; }
    public int ValidCores { get; set; }
    public int InvalidCores { get; set; }
    public int UnverifiedCores { get; set; }
    public int ForgeCores { get; set; }
    public int FabricCores { get; set; }
    public int QuiltCores { get; set; }
    public int VanillaCores { get; set; }
}

#endregion
