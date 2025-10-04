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
/// Forge版本服务
/// 负责查找和下载Forge版本
/// </summary>
public class ForgeVersionService
{
    private readonly string _minecraftFolder;
    private readonly string _javaPath;

    public ForgeVersionService(string minecraftFolder, string javaPath)
    {
        _minecraftFolder = minecraftFolder;
        _javaPath = javaPath;
    }

    /// <summary>
    /// 获取指定Minecraft版本的所有Forge版本
    /// </summary>
    /// <param name="mcVersion">Minecraft版本</param>
    /// <param name="isNeoforge">是否为NeoForge</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Forge版本列表</returns>
    public async Task<IEnumerable<MinecraftVersion>> GetAvailableVersionsAsync(string mcVersion, 
        bool isNeoforge = false, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var forgeEntries = await ForgeInstaller.EnumerableForgeAsync(mcVersion);
            
            return forgeEntries.Select(entry => new MinecraftVersion
            {
                Id = $"{entry.McVersion}-{(isNeoforge ? "neoforge" : "forge")}-{entry.ForgeVersion}",
                Type = isNeoforge ? "neoforge" : "forge",
                ReleaseTime = entry.ModifiedTime,
                Time = entry.ModifiedTime,
                Url = string.Empty, // Forge条目没有直接的URL
                IsLatest = false, // Forge版本通常不标记为最新
                IsRecommended = IsRecommendedForgeVersion(entry)
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"获取Forge版本失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 下载指定的Forge版本
    /// </summary>
    /// <param name="mcVersion">Minecraft版本</param>
    /// <param name="forgeVersion">Forge版本</param>
    /// <param name="isNeoforge">是否为NeoForge</param>
    /// <param name="progressCallback">进度回调</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>安装结果</returns>
    public async Task<bool> DownloadVersionAsync(string mcVersion, string forgeVersion, 
        bool isNeoforge = false,
        Action<double, string>? progressCallback = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 获取Forge版本条目
            var forgeEntries = await ForgeInstaller.EnumerableForgeAsync(mcVersion);
            var targetEntry = forgeEntries.FirstOrDefault(e => e.ForgeVersion == forgeVersion);
            
            if (targetEntry == null)
            {
                throw new ArgumentException($"未找到Forge版本: {mcVersion}-{forgeVersion}");
            }

            // 创建安装器
            var installer = ForgeInstaller.Create(_minecraftFolder, _javaPath, targetEntry);
            
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
            throw new InvalidOperationException($"下载Forge版本失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 检查是否为推荐的Forge版本
    /// </summary>
    private static bool IsRecommendedForgeVersion(ForgeInstallEntry entry)
    {
        // 这里可以根据需要实现更复杂的逻辑来判断是否为推荐版本
        // 例如：检查版本号、发布时间等
        return entry.Branch == "recommended" || string.IsNullOrEmpty(entry.Branch);
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
            InstallStep.DownloadPackage => "下载Forge安装包",
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
