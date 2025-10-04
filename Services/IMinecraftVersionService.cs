using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using swpumc.Models;

namespace swpumc.Services;

public interface IMinecraftVersionService
{
    /// <summary>
    /// 获取所有可用的Minecraft版本
    /// </summary>
    Task<IEnumerable<MinecraftVersion>> GetAllVersionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取正式发布版本
    /// </summary>
    Task<IEnumerable<MinecraftVersion>> GetReleaseVersionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取快照版本
    /// </summary>
    Task<IEnumerable<MinecraftVersion>> GetSnapshotVersionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取最新版本
    /// </summary>
    Task<MinecraftVersion?> GetLatestVersionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据版本ID获取版本信息
    /// </summary>
    Task<MinecraftVersion?> GetVersionByIdAsync(string versionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 刷新版本列表
    /// </summary>
    Task RefreshVersionsAsync(CancellationToken cancellationToken = default);
}
