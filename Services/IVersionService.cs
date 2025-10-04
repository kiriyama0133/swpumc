using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using swpumc.Models;

namespace swpumc.Services;

/// <summary>
/// 版本服务接口
/// 定义版本查找和下载的统一接口
/// </summary>
public interface IVersionService
{
    /// <summary>
    /// 获取可用的版本列表
    /// </summary>
    /// <param name="parameters">版本参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>版本列表</returns>
    Task<IEnumerable<MinecraftVersion>> GetAvailableVersionsAsync(Dictionary<string, object> parameters, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 下载指定版本
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <param name="parameters">版本参数</param>
    /// <param name="progressCallback">进度回调</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>下载结果</returns>
    Task<bool> DownloadVersionAsync(string versionId, Dictionary<string, object> parameters,
        Action<double, string>? progressCallback = null, 
        CancellationToken cancellationToken = default);
}
