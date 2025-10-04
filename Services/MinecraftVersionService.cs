using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MinecraftLaunch.Components.Installer;
using swpumc.Models;

namespace swpumc.Services;

public class MinecraftVersionService : IMinecraftVersionService
{
    private readonly IConfigService _configService;
    private readonly List<MinecraftVersion> _cachedVersions = new();
    private DateTime _lastRefreshTime = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public MinecraftVersionService(IConfigService configService)
    {
        _configService = configService;
    }

    public async Task<IEnumerable<MinecraftVersion>> GetAllVersionsAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[MinecraftVersionService] GetAllVersionsAsync开始 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        await EnsureVersionsLoadedAsync(cancellationToken);
        Console.WriteLine($"[MinecraftVersionService] GetAllVersionsAsync完成，返回{_cachedVersions.Count}个版本 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        return _cachedVersions.ToList();
    }

    public async Task<IEnumerable<MinecraftVersion>> GetReleaseVersionsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureVersionsLoadedAsync(cancellationToken);
        return _cachedVersions
            .Where(v => v.Type == "release")
            .OrderByDescending(v => v.ReleaseTime)
            .ToList();
    }

    public async Task<IEnumerable<MinecraftVersion>> GetSnapshotVersionsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureVersionsLoadedAsync(cancellationToken);
        return _cachedVersions
            .Where(v => v.Type == "snapshot")
            .OrderByDescending(v => v.ReleaseTime)
            .ToList();
    }

    public async Task<MinecraftVersion?> GetLatestVersionAsync(CancellationToken cancellationToken = default)
    {
        await EnsureVersionsLoadedAsync(cancellationToken);
        return _cachedVersions
            .Where(v => v.Type == "release")
            .OrderByDescending(v => v.ReleaseTime)
            .FirstOrDefault();
    }

    public async Task<MinecraftVersion?> GetVersionByIdAsync(string versionId, CancellationToken cancellationToken = default)
    {
        await EnsureVersionsLoadedAsync(cancellationToken);
        return _cachedVersions.FirstOrDefault(v => v.Id == versionId);
    }

    public async Task RefreshVersionsAsync(CancellationToken cancellationToken = default)
    {
        _lastRefreshTime = DateTime.MinValue;
        _cachedVersions.Clear();
        await EnsureVersionsLoadedAsync(cancellationToken);
    }

    private async Task EnsureVersionsLoadedAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"[MinecraftVersionService] EnsureVersionsLoadedAsync开始 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        
        // 首先尝试从全局配置加载缓存
        if (!_cachedVersions.Any())
        {
            Console.WriteLine($"[MinecraftVersionService] 从配置加载缓存 - 时间: {DateTime.Now:HH:mm:ss.fff}");
            await LoadVersionsFromConfigAsync();
        }

        // 检查缓存是否过期
        var appSettings = _configService.AppSettings;
        var isCacheExpired = DateTime.Now - appSettings.LastVersionUpdate > TimeSpan.FromHours(appSettings.VersionCacheExpirationHours);
        Console.WriteLine($"[MinecraftVersionService] 缓存检查: 有缓存={_cachedVersions.Any()}, 过期={isCacheExpired} - 时间: {DateTime.Now:HH:mm:ss.fff}");
        
        if (!_cachedVersions.Any() || isCacheExpired)
        {
            try
            {
                Console.WriteLine($"[MinecraftVersionService] 开始网络请求获取版本 - 时间: {DateTime.Now:HH:mm:ss.fff}");
                // 使用MinecraftLaunch库获取真实版本数据
                var versionEntries = await VanillaInstaller.EnumerableMinecraftAsync(cancellationToken);
                Console.WriteLine($"[MinecraftVersionService] 网络请求完成，获得{versionEntries.Count()}个版本 - 时间: {DateTime.Now:HH:mm:ss.fff}");
                
                _cachedVersions.Clear();
                var versionInfos = new List<MinecraftVersionInfo>();
                
                foreach (var entry in versionEntries)
                {
                    var version = new MinecraftVersion
                    {
                        Id = entry.Id,
                        Type = entry.Type,
                        ReleaseTime = entry.ReleaseTime,
                        Time = entry.Time,
                        Url = entry.Url,
                        IsLatest = false,
                        IsRecommended = false
                    };
                    _cachedVersions.Add(version);
                    
                    // 同时创建配置信息
                    versionInfos.Add(new MinecraftVersionInfo
                    {
                        Id = entry.Id,
                        Type = entry.Type,
                        ReleaseTime = entry.ReleaseTime,
                        Time = entry.Time,
                        Url = entry.Url,
                        IsLatest = false,
                        IsRecommended = false,
                        CachedAt = DateTime.Now
                    });
                }
                
                // 设置最新版本标记
                if (_cachedVersions.Any())
                {
                    var latestRelease = _cachedVersions
                        .Where(v => v.Type == "release")
                        .OrderByDescending(v => v.ReleaseTime)
                        .FirstOrDefault();
                    
                    if (latestRelease != null)
                    {
                        latestRelease.IsLatest = true;
                        latestRelease.IsRecommended = true;
                        
                        // 更新配置中的标记
                        var configLatest = versionInfos.FirstOrDefault(v => v.Id == latestRelease.Id);
                        if (configLatest != null)
                        {
                            configLatest.IsLatest = true;
                            configLatest.IsRecommended = true;
                        }
                    }
                }
                
                // 保存到全局配置
                appSettings.CachedMinecraftVersions = versionInfos;
                appSettings.LastVersionUpdate = DateTime.Now;
                await _configService.SaveAppSettingsAsync();
                
                _lastRefreshTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取Minecraft版本失败: {ex.Message}");
                throw;
            }
        }
    }

    private async Task LoadVersionsFromConfigAsync()
    {
        try
        {
            var appSettings = _configService.AppSettings;
            if (appSettings.CachedMinecraftVersions?.Any() == true)
            {
                _cachedVersions.Clear();
                foreach (var cachedVersion in appSettings.CachedMinecraftVersions)
                {
                    var version = new MinecraftVersion
                    {
                        Id = cachedVersion.Id,
                        Type = cachedVersion.Type,
                        ReleaseTime = cachedVersion.ReleaseTime,
                        Time = cachedVersion.Time,
                        Url = cachedVersion.Url,
                        IsLatest = cachedVersion.IsLatest,
                        IsRecommended = cachedVersion.IsRecommended
                    };
                    _cachedVersions.Add(version);
                }
                _lastRefreshTime = appSettings.LastVersionUpdate;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"从配置加载版本缓存失败: {ex.Message}");
        }
    }

}
