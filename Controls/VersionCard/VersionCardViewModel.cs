using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using swpumc.Models;
using swpumc.Services;

namespace swpumc.Controls.VersionCard;

public partial class VersionCardViewModel : ObservableObject
{
    [ObservableProperty] private MinecraftVersion _version;
    [ObservableProperty] private bool _isDownloading;
    [ObservableProperty] private double _downloadProgress;
    [ObservableProperty] private string _downloadStatus = string.Empty;

    private readonly VersionServiceFactory _versionServiceFactory;
    private readonly ISukiDialogManager? _dialogManager;
    private readonly ISukiToastManager? _toastManager;

    public VersionCardViewModel(MinecraftVersion version, VersionServiceFactory versionServiceFactory)
    {
        _version = version;
        _versionServiceFactory = versionServiceFactory;
        
        // 获取服务
        var serviceProvider = (Avalonia.Application.Current as App)?.Services;
        _dialogManager = serviceProvider?.GetService<ISukiDialogManager>();
        _toastManager = serviceProvider?.GetService<ISukiToastManager>();
    }

    // 代理属性到MinecraftVersion
    public string DisplayName => Version?.DisplayName ?? string.Empty;
    public string TypeDisplayName => Version?.TypeDisplayName ?? string.Empty;
    public string TimeAgo => Version?.TimeAgo ?? string.Empty;
    public string Type => Version?.Type ?? string.Empty;
    public bool IsLatest => Version?.IsLatest ?? false;
    public bool IsRecommended => Version?.IsRecommended ?? false;

    [RelayCommand]
    private async Task DownloadAsync()
    {
        if (IsDownloading) return;

        // 显示下载中心dialog
        if (_dialogManager != null)
        {
            var downloadCenter = new DownloadCenter.DownloadCenter(Version);
            _dialogManager.CreateDialog()
                .WithContent(downloadCenter)
                .WithActionButton("取消", dialog => dialog.Dismiss())
                .TryShow();
        }
        else
        {
            // 如果dialog管理器不可用，直接开始下载
            await StartDownloadDirectlyAsync();
        }
    }

    private async Task StartDownloadDirectlyAsync()
    {
        // 显示下载进度toast
        if (_toastManager != null)
        {
            var toast = _toastManager.CreateToast()
                .WithTitle($"正在下载 {DisplayName}")
                .WithContent("准备开始下载...")
                .WithLoadingState(true)
                .Dismiss().ByClicking()
                .Queue();

            // 开始下载
            await StartDownloadAsync(toast);
        }
        else
        {
            // 如果没有toast管理器，直接执行下载逻辑
            await StartDownloadWithoutToastAsync();
        }
    }

    private async Task StartDownloadAsync(ISukiToast toast)
    {
        try
        {
            IsDownloading = true;
            DownloadProgress = 0;
            DownloadStatus = "准备下载...";

            // 更新toast内容
            UpdateToastContent(toast, "准备下载...", 0);

            // 获取版本服务
            var versionService = _versionServiceFactory.GetService(Version.Type);
            
            // 准备参数
            var parameters = new Dictionary<string, object>
            {
                ["mcVersion"] = ExtractMcVersion(Version.Id)
            };

            // 根据版本类型添加特定参数
            if (Version.Type == "forge" || Version.Type == "neoforge")
            {
                parameters["isNeoforge"] = Version.Type == "neoforge";
            }

            // 执行下载
            var result = await versionService.DownloadVersionAsync(
                Version.Id, 
                parameters, 
                (progress, status) =>
                {
                    DownloadProgress = progress;
                    DownloadStatus = status;
                    
                    // 更新toast进度
                    UpdateToastContent(toast, status, progress);
                },
                CancellationToken.None);

            if (result)
            {
                DownloadStatus = "下载完成！";
                UpdateToastContent(toast, "下载完成！", 100);
                
                // 3秒后自动关闭toast
                await Task.Delay(3000);
                _toastManager?.Dismiss(toast);
            }
            else
            {
                DownloadStatus = "下载失败！";
                UpdateToastContent(toast, "下载失败！", 0);
            }
        }
        catch (Exception ex)
        {
            DownloadStatus = $"下载失败: {ex.Message}";
            UpdateToastContent(toast, $"下载失败: {ex.Message}", 0);
        }
        finally
        {
            IsDownloading = false;
        }
    }

    private async Task StartDownloadWithoutToastAsync()
    {
        try
        {
            IsDownloading = true;
            DownloadProgress = 0;
            DownloadStatus = "准备下载...";

            // 获取版本服务
            var versionService = _versionServiceFactory.GetService(Version.Type);
            
            // 准备参数
            var parameters = new Dictionary<string, object>
            {
                ["mcVersion"] = ExtractMcVersion(Version.Id)
            };

            // 根据版本类型添加特定参数
            if (Version.Type == "forge" || Version.Type == "neoforge")
            {
                parameters["isNeoforge"] = Version.Type == "neoforge";
            }

            // 执行下载
            var result = await versionService.DownloadVersionAsync(
                Version.Id, 
                parameters, 
                (progress, status) =>
                {
                    DownloadProgress = progress;
                    DownloadStatus = status;
                },
                CancellationToken.None);

            if (result)
            {
                DownloadStatus = "下载完成！";
            }
            else
            {
                DownloadStatus = "下载失败！";
            }
        }
        catch (Exception ex)
        {
            DownloadStatus = $"下载失败: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
        }
    }

    private void UpdateToastContent(ISukiToast toast, string status, double progress)
    {
        // 这里需要根据SukiUI的toast API来更新内容
        // 由于toast内容更新可能需要特殊处理，这里先记录状态
        DownloadStatus = status;
        DownloadProgress = progress;
    }

    /// <summary>
    /// 从版本ID中提取Minecraft版本
    /// </summary>
    private string ExtractMcVersion(string versionId)
    {
        // 根据不同的版本类型提取Minecraft版本
        return Version.Type switch
        {
            "vanilla" => versionId,
            "forge" or "neoforge" => versionId.Split('-')[0],
            "fabric" => versionId.Split('_')[1],
            "quilt" => versionId.Split('_')[1],
            "optifine" => versionId.Split('-')[0],
            _ => versionId
        };
    }
}
