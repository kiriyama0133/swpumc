using System;
using System.Collections.Generic;
using System.Linq;
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
/// Optifine版本服务
/// 负责查找和下载Optifine版本
/// </summary>
public class OptifineVersionService
{
    private readonly string _minecraftFolder;
    private readonly string _javaPath;

    public OptifineVersionService(string minecraftFolder, string javaPath)
    {
        _minecraftFolder = minecraftFolder;
        _javaPath = javaPath;
    }

    /// <summary>
    /// 获取指定Minecraft版本的所有Optifine版本
    /// </summary>
    /// <param name="mcVersion">Minecraft版本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Optifine版本列表</returns>
    public async Task<IEnumerable<MinecraftVersion>> GetAvailableVersionsAsync(string mcVersion, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine($"[OptifineVersionService] 开始获取Optifine版本，Minecraft版本: {mcVersion}");
            var optifineEntries = await OptifineInstaller.EnumerableOptifineAsync(mcVersion);
            Console.WriteLine($"[OptifineVersionService] 获取到{optifineEntries?.Count() ?? 0}个Optifine条目");
            
            if (optifineEntries == null)
            {
                Console.WriteLine("[OptifineVersionService] Optifine条目为null，返回空列表");
                return new List<MinecraftVersion>();
            }
            
            var validEntries = optifineEntries.Where(entry => entry != null).ToList();
            Console.WriteLine($"[OptifineVersionService] 过滤后有效条目数: {validEntries.Count}");
            
            var result = validEntries.Select(entry => 
            {
                try
                {
                    Console.WriteLine($"[OptifineVersionService] 处理条目: McVersion={entry.McVersion}, Patch={entry.Patch}");
                    return new MinecraftVersion
                    {
                        Id = $"{entry.McVersion}-Optifine_{entry.Patch}",
                        Type = "optifine",
                        ReleaseTime = DateTime.Now, // Optifine条目没有发布时间信息
                        Time = DateTime.Now,
                        Url = string.Empty, // Optifine条目没有直接的URL
                        IsLatest = false, // Optifine版本通常不标记为最新
                        IsRecommended = IsRecommendedOptifineVersion(entry)
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OptifineVersionService] 处理条目时出错: {ex.Message}");
                    return null;
                }
            }).Where(v => v != null).Cast<MinecraftVersion>().ToList();
            
            Console.WriteLine($"[OptifineVersionService] 最终返回{result.Count}个版本");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OptifineVersionService] 获取Optifine版本异常: {ex.Message}");
            throw new InvalidOperationException($"获取Optifine版本失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 下载指定的Optifine版本
    /// </summary>
    /// <param name="mcVersion">Minecraft版本</param>
    /// <param name="patch">Optifine补丁版本</param>
    /// <param name="progressCallback">进度回调</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>安装结果</returns>
    public async Task<bool> DownloadVersionAsync(string mcVersion, string patch,
        Action<double, string>? progressCallback = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine($"[OptifineVersionService] 开始下载Optifine版本: {mcVersion}-{patch}");
            
            // 获取Optifine版本条目
            var optifineEntries = await OptifineInstaller.EnumerableOptifineAsync(mcVersion);
            Console.WriteLine($"[OptifineVersionService] 获取到{optifineEntries?.Count() ?? 0}个Optifine条目用于下载");
            
            if (optifineEntries == null)
            {
                Console.WriteLine("[OptifineVersionService] Optifine条目为null，无法下载");
                throw new ArgumentException("无法获取Optifine版本列表");
            }
            
            var targetEntry = optifineEntries.Where(e => e != null)
                .FirstOrDefault(e => e.Patch == patch);
            
            if (targetEntry == null)
            {
                Console.WriteLine($"[OptifineVersionService] 未找到匹配的Optifine版本: {mcVersion}-{patch}");
                throw new ArgumentException($"未找到Optifine版本: {mcVersion}-{patch}");
            }
            
            Console.WriteLine($"[OptifineVersionService] 找到目标条目: McVersion={targetEntry.McVersion}, Patch={targetEntry.Patch}");

            // 创建安装器
            Console.WriteLine($"[OptifineVersionService] 创建Optifine安装器，Minecraft文件夹: {_minecraftFolder}, Java路径: {_javaPath}");
            var installer = OptifineInstaller.Create(_minecraftFolder, _javaPath, targetEntry);
            
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
            throw new InvalidOperationException($"下载Optifine版本失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 检查是否为推荐的Optifine版本
    /// </summary>
    private static bool IsRecommendedOptifineVersion(OptifineInstallEntry entry)
    {
        // 这里可以根据需要实现更复杂的逻辑来判断是否为推荐版本
        // 例如：检查版本号、发布时间等
        return entry?.Type == "HD_U" && entry?.Patch?.Contains("_G") == true;
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
            InstallStep.DownloadPackage => "下载Optifine安装包",
            InstallStep.ParsePackage => "解析安装包",
            InstallStep.WriteVersionJsonAndSomeDependencies => "写入版本JSON和依赖",
            InstallStep.DownloadLibraries => "下载依赖库",
            InstallStep.RunInstallProcessor => "运行安装处理器",
            InstallStep.RanToCompletion => "安装完成",
            InstallStep.Interrupted => "安装中断",
            _ => "处理中..."
        };
    }
}
