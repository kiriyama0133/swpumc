using System;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using swpumc.Services;

namespace swpumc.Pages.DownloadCenter;

public partial class DownloadCenterPage : BasePage
{
    private static DownloadCenterPageViewModel? _cachedViewModel;
    
    public DownloadCenterPage()
    {
        Console.WriteLine($"[DownloadCenterPage] 构造函数开始 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        
        InitializeComponent();
        Console.WriteLine($"[DownloadCenterPage] InitializeComponent完成 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        
        // 使用缓存的ViewModel或创建新的
        if (_cachedViewModel == null)
        {
            Console.WriteLine($"[DownloadCenterPage] 创建新的ViewModel - 时间: {DateTime.Now:HH:mm:ss.fff}");
            var app = App.Current as App;
            var versionService = app?.Services?.GetService<IMinecraftVersionService>();
            _cachedViewModel = new DownloadCenterPageViewModel(versionService!);
            Console.WriteLine($"[DownloadCenterPage] ViewModel创建完成 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        }
        else
        {
            Console.WriteLine($"[DownloadCenterPage] 使用缓存的ViewModel - 时间: {DateTime.Now:HH:mm:ss.fff}");
        }
        
        DataContext = _cachedViewModel;
        Console.WriteLine($"[DownloadCenterPage] DataContext设置完成 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine($"[DownloadCenterPage] 构造函数结束 - 时间: {DateTime.Now:HH:mm:ss.fff}");
    }
}
