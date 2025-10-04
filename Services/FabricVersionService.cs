using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Base.Enums;
using swpumc.Models;
using MinecraftVersion = swpumc.Models.MinecraftVersion;

namespace swpumc.Services;

/// <summary>
/// Fabric版本服务
/// 负责查找和下载Fabric版本
/// </summary>
public class FabricVersionService
{
    private readonly string _minecraftFolder;

    public FabricVersionService(string minecraftFolder)
    {
        _minecraftFolder = minecraftFolder;
    }

    /// <summary>
    /// 获取指定Minecraft版本的所有Fabric版本
    /// </summary>
    /// <param name="mcVersion">Minecraft版本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Fabric版本列表</returns>
    public async Task<IEnumerable<MinecraftVersion>> GetAvailableVersionsAsync(string mcVersion, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine($"[FabricVersionService] 开始获取Fabric版本，Minecraft版本: {mcVersion}");
            
            // 使用安全的包装方法获取Fabric条目
            var fabricEntries = await GetFabricEntriesSafelyAsync(mcVersion, cancellationToken);
            Console.WriteLine($"[FabricVersionService] 获取到{fabricEntries?.Count() ?? 0}个Fabric条目");
            
            if (fabricEntries == null)
            {
                Console.WriteLine("[FabricVersionService] Fabric条目为null，返回空列表");
                return new List<MinecraftVersion>();
            }
            
            var validEntries = fabricEntries.Where(entry => entry?.Loader != null).ToList();
            Console.WriteLine($"[FabricVersionService] 过滤后有效条目数: {validEntries.Count}");
            
            var result = validEntries.Select(entry => 
            {
                try
                {
                    // 确保 Loader 不为 null
                    if (entry?.Loader == null)
                    {
                        Console.WriteLine("[FabricVersionService] 条目Loader为null，跳过");
                        return null;
                    }
                    
                    Console.WriteLine($"[FabricVersionService] 处理条目: LoaderVersion={entry.Loader.Version}");
                    return new MinecraftVersion
                    {
                        Id = $"fabric-loader-{entry.Loader.Version}_{mcVersion}",
                        Type = "fabric",
                        ReleaseTime = DateTime.Now, // Fabric条目没有发布时间信息
                        Time = DateTime.Now,
                        Url = string.Empty, // Fabric条目没有直接的URL
                        IsLatest = false, // Fabric版本通常不标记为最新
                        IsRecommended = IsRecommendedFabricVersion(entry)
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FabricVersionService] 处理条目时出错: {ex.Message}");
                    Console.WriteLine($"[FabricVersionService] 错误详情: {ex}");
                    return null;
                }
            }).Where(v => v != null).Cast<MinecraftVersion>().ToList();
            
            Console.WriteLine($"[FabricVersionService] 最终返回{result.Count}个版本");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FabricVersionService] 获取Fabric版本异常: {ex.Message}");
            Console.WriteLine($"[FabricVersionService] 异常类型: {ex.GetType().Name}");
            Console.WriteLine($"[FabricVersionService] 堆栈跟踪: {ex.StackTrace}");
            throw new InvalidOperationException($"获取Fabric版本失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 下载指定的Fabric版本
    /// </summary>
    /// <param name="mcVersion">Minecraft版本</param>
    /// <param name="loaderVersion">Fabric Loader版本</param>
    /// <param name="progressCallback">进度回调</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>安装结果</returns>
    public async Task<bool> DownloadVersionAsync(string mcVersion, string loaderVersion,
        Action<double, string>? progressCallback = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 获取Fabric版本条目
            var fabricEntries = await FabricInstaller.EnumerableFabricAsync(mcVersion);
            var targetEntry = fabricEntries.Where(e => e?.Loader != null)
                .FirstOrDefault(e => e.Loader.Version == loaderVersion);
            
            if (targetEntry == null)
            {
                throw new ArgumentException($"未找到Fabric版本: {mcVersion}-{loaderVersion}");
            }

            // 创建安装器
            var installer = FabricInstaller.Create(_minecraftFolder, targetEntry);
            
            // 订阅进度事件
            installer.ProgressChanged += (sender, args) =>
            {
                progressCallback?.Invoke(args.Progress * 100, GetStepDescription(args.StepName));
            };

            // 执行安装
            var result = await installer.InstallAsync(cancellationToken);
            
            return result != null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"下载Fabric版本失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 检查是否为推荐的Fabric版本
    /// </summary>
    private static bool IsRecommendedFabricVersion(FabricInstallEntry entry)
    {
        // 确保 entry 和 Loader 不为 null
        if (entry?.Loader == null)
        {
            return false;
        }
        
        // 这里可以根据需要实现更复杂的逻辑来判断是否为推荐版本
        // 例如：检查版本号、发布时间等
        return entry.Loader.IsStable;
    }

    /// <summary>
    /// 获取安装步骤描述
    /// </summary>
    private static string GetStepDescription(InstallStep step)
    {
        return step switch
        {
            InstallStep.Started => "开始安装",
            InstallStep.ParseMinecraft => "解析Minecraft信息",
            InstallStep.DownloadVersionJson => "下载版本JSON",
            InstallStep.DownloadLibraries => "下载依赖库",
            InstallStep.RanToCompletion => "安装完成",
            InstallStep.Interrupted => "安装中断",
            _ => "处理中..."
        };
    }

    /// <summary>
    /// 安全地获取Fabric条目，处理MinecraftLaunch库的空引用问题
    /// </summary>
    private async Task<IEnumerable<FabricInstallEntry>> GetFabricEntriesSafelyAsync(string mcVersion, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"[FabricVersionService] 尝试直接调用FabricInstaller.EnumerableFabricAsync");
            var entries = await FabricInstaller.EnumerableFabricAsync(mcVersion, cancellationToken);
            Console.WriteLine($"[FabricVersionService] 直接调用成功，获取到{entries?.Count() ?? 0}个条目");
            return entries;
        }
        catch (NullReferenceException ex)
        {
            Console.WriteLine($"[FabricVersionService] 捕获到空引用异常，尝试手动处理: {ex.Message}");
            
            // 如果直接调用失败，尝试手动获取和处理数据
            try
            {
                return await GetFabricEntriesManuallyAsync(mcVersion, cancellationToken);
            }
            catch (Exception manualEx)
            {
                Console.WriteLine($"[FabricVersionService] 手动获取也失败: {manualEx.Message}");
                throw new InvalidOperationException($"无法获取Fabric版本信息: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// 手动获取Fabric条目，绕过MinecraftLaunch库的问题
    /// </summary>
    private async Task<IEnumerable<FabricInstallEntry>> GetFabricEntriesManuallyAsync(string mcVersion, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[FabricVersionService] 开始手动获取Fabric条目");
        
        // 直接调用Fabric API
        string requestUrl = $"https://meta.fabricmc.net/v2/versions/loader/{mcVersion}";
        Console.WriteLine($"[FabricVersionService] 请求URL: {requestUrl}");
        
        using var httpClient = new HttpClient();
        var json = await httpClient.GetStringAsync(requestUrl, cancellationToken);
        Console.WriteLine($"[FabricVersionService] 获取到JSON数据，长度: {json?.Length ?? 0}");
        
        if (string.IsNullOrEmpty(json))
        {
            Console.WriteLine("[FabricVersionService] JSON数据为空");
            return new List<FabricInstallEntry>();
        }
        
        // 手动解析JSON，过滤掉有问题的条目
        var entries = System.Text.Json.JsonSerializer.Deserialize<FabricInstallEntry[]>(json);
        Console.WriteLine($"[FabricVersionService] 反序列化得到{entries?.Length ?? 0}个条目");
        
        if (entries == null)
        {
            Console.WriteLine("[FabricVersionService] 反序列化结果为null");
            return new List<FabricInstallEntry>();
        }
        
        // 过滤掉Loader为null的条目
        var validEntries = entries.Where(entry => entry?.Loader != null).ToList();
        Console.WriteLine($"[FabricVersionService] 过滤后有效条目数: {validEntries.Count}");
        
        // 手动排序，避免访问null的Loader
        try
        {
            var sortedEntries = validEntries
                .Where(entry => entry.Loader?.Version != null && entry.Loader?.Separator != null)
                .OrderByDescending(entry => 
                {
                    try
                    {
                        var versionString = entry.Loader.Version.Replace(entry.Loader.Separator, ".");
                        return new Version(versionString);
                    }
                    catch
                    {
                        // 如果版本解析失败，使用默认版本
                        return new Version("0.0.0");
                    }
                })
                .ToList();
            
            Console.WriteLine($"[FabricVersionService] 排序后条目数: {sortedEntries.Count}");
            return sortedEntries;
        }
        catch (Exception sortEx)
        {
            Console.WriteLine($"[FabricVersionService] 排序失败: {sortEx.Message}");
            // 返回未排序的条目
            return validEntries;
        }
    }
}
