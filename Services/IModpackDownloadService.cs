using System;
using System.Threading;
using System.Threading.Tasks;

namespace swpumc.Services
{
    /// <summary>
    /// 整合包下载服务接口
    /// 定义整合包下载和安装的统一接口
    /// </summary>
    public interface IModpackDownloadService
    {
        /// <summary>
        /// 下载并安装CurseForge整合包
        /// </summary>
        /// <param name="modpackPath">整合包文件路径</param>
        /// <param name="progressCallback">进度回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>安装结果</returns>
        Task<ModpackInstallResult> InstallCurseforgeModpackAsync(string modpackPath,
            Action<double, string>? progressCallback = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 下载并安装Modrinth整合包
        /// </summary>
        /// <param name="modpackPath">整合包文件路径</param>
        /// <param name="progressCallback">进度回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>安装结果</returns>
        Task<ModpackInstallResult> InstallModrinthModpackAsync(string modpackPath,
            Action<double, string>? progressCallback = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 下载并安装MCBBS整合包
        /// </summary>
        /// <param name="modpackPath">整合包文件路径</param>
        /// <param name="progressCallback">进度回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>安装结果</returns>
        Task<ModpackInstallResult> InstallMcbbsModpackAsync(string modpackPath,
            Action<double, string>? progressCallback = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 验证整合包文件格式
        /// </summary>
        /// <param name="modpackPath">整合包文件路径</param>
        /// <returns>验证结果</returns>
        Task<ModpackValidationResult> ValidateModpackAsync(string modpackPath);

        /// <summary>
        /// 带Toast进度显示的CurseForge整合包安装
        /// </summary>
        /// <param name="modpackPath">整合包文件路径</param>
        /// <param name="toastManager">Toast管理器</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>安装结果</returns>
        Task<ModpackInstallResult> InstallCurseforgeModpackWithToastAsync(string modpackPath,
            SukiUI.Toasts.ISukiToastManager toastManager,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 带Toast进度显示的Modrinth整合包安装
        /// </summary>
        /// <param name="modpackPath">整合包文件路径</param>
        /// <param name="toastManager">Toast管理器</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>安装结果</returns>
        Task<ModpackInstallResult> InstallModrinthModpackWithToastAsync(string modpackPath,
            SukiUI.Toasts.ISukiToastManager toastManager,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 带Toast进度显示的MCBBS整合包安装
        /// </summary>
        /// <param name="modpackPath">整合包文件路径</param>
        /// <param name="toastManager">Toast管理器</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>安装结果</returns>
        Task<ModpackInstallResult> InstallMcbbsModpackWithToastAsync(string modpackPath,
            SukiUI.Toasts.ISukiToastManager toastManager,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// 整合包安装结果
    /// </summary>
    public class ModpackInstallResult
    {
        /// <summary>
        /// 是否安装成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 游戏核心ID
        /// </summary>
        public string? GameCoreId { get; set; }

        /// <summary>
        /// 模组数量
        /// </summary>
        public int ModCount { get; set; }

        /// <summary>
        /// 资源包数量
        /// </summary>
        public int ResourcepackCount { get; set; }

        /// <summary>
        /// 光影包数量
        /// </summary>
        public int ShaderpackCount { get; set; }

        /// <summary>
        /// 配置文件数量
        /// </summary>
        public int ConfigCount { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 整合包验证结果
    /// </summary>
    public class ModpackValidationResult
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 整合包类型
        /// </summary>
        public ModpackType Type { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// 整合包类型
    /// </summary>
    public enum ModpackType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown,

        /// <summary>
        /// CurseForge整合包
        /// </summary>
        CurseForge,

        /// <summary>
        /// Modrinth整合包
        /// </summary>
        Modrinth,

        /// <summary>
        /// MCBBS整合包
        /// </summary>
        MCBBS
    }
}
