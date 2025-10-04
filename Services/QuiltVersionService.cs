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
/// Quilt版本服务
/// 负责查找和下载Quilt版本
/// </summary>
public class QuiltVersionService
{
    private readonly string _minecraftFolder;

    public QuiltVersionService(string minecraftFolder)
    {
        _minecraftFolder = minecraftFolder;
    }

    /// <summary>
    /// 获取指定Minecraft版本的所有Quilt版本
    /// </summary>
    /// <param name="mcVersion">Minecraft版本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Quilt版本列表</returns>
    public async Task<IEnumerable<MinecraftVersion>> GetAvailableVersionsAsync(string mcVersion, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var quiltEntries = await QuiltInstaller.EnumerableQuiltAsync(mcVersion);
            
            return quiltEntries.Where(entry => entry?.Loader != null)
                .Select(entry => new MinecraftVersion
                {
                    Id = $"quilt-loader-{entry.Loader.Version}_{mcVersion}",
                    Type = "quilt",
                    ReleaseTime = DateTime.Now, // Quilt条目没有发布时间信息
                    Time = DateTime.Now,
                    Url = string.Empty, // Quilt条目没有直接的URL
                    IsLatest = false, // Quilt版本通常不标记为最新
                    IsRecommended = IsRecommendedQuiltVersion(entry)
                });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"获取Quilt版本失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 下载指定的Quilt版本
    /// </summary>
    /// <param name="mcVersion">Minecraft版本</param>
    /// <param name="loaderVersion">Quilt Loader版本</param>
    /// <param name="progressCallback">进度回调</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>安装结果</returns>
    public async Task<bool> DownloadVersionAsync(string mcVersion, string loaderVersion,
        Action<double, string>? progressCallback = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 获取Quilt版本条目
            var quiltEntries = await QuiltInstaller.EnumerableQuiltAsync(mcVersion);
            var targetEntry = quiltEntries.Where(e => e?.Loader != null)
                .FirstOrDefault(e => e.Loader.Version == loaderVersion);
            
            if (targetEntry == null)
            {
                throw new ArgumentException($"未找到Quilt版本: {mcVersion}-{loaderVersion}");
            }

            // 创建安装器
            var installer = QuiltInstaller.Create(_minecraftFolder, targetEntry);
            
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
            throw new InvalidOperationException($"下载Quilt版本失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 检查是否为推荐的Quilt版本
    /// </summary>
    private static bool IsRecommendedQuiltVersion(QuiltInstallEntry entry)
    {
        // 这里可以根据需要实现更复杂的逻辑来判断是否为推荐版本
        // 例如：检查版本号、发布时间等
        return entry?.Loader?.IsStable == true;
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
}
