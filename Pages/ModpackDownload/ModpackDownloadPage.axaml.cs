using System;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using swpumc.Services;

namespace swpumc.Pages.ModpackDownload;

public partial class ModpackDownloadPage : BasePage
{
    private static ModpackDownloadPageViewModel? _cachedViewModel;
    
    public ModpackDownloadPage()
    {
        Console.WriteLine($"[ModpackDownloadPage] 构造函数开始 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        
        InitializeComponent();
        Console.WriteLine($"[ModpackDownloadPage] InitializeComponent完成 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        
        // 使用缓存的ViewModel或创建新的
        if (_cachedViewModel == null)
        {
            Console.WriteLine($"[ModpackDownloadPage] 创建新的ViewModel - 时间: {DateTime.Now:HH:mm:ss.fff}");
            var app = App.Current as App;
            var modpackService = app?.Services?.GetService<IModpackDownloadService>();
            var configService = app?.Services?.GetService<IConfigService>();
            var versionServiceFactory = app?.Services?.GetService<VersionServiceFactory>();
            _cachedViewModel = new ModpackDownloadPageViewModel(modpackService!, configService!, versionServiceFactory!);
            Console.WriteLine($"[ModpackDownloadPage] ViewModel创建完成 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        }
        else
        {
            Console.WriteLine($"[ModpackDownloadPage] 使用缓存的ViewModel - 时间: {DateTime.Now:HH:mm:ss.fff}");
        }
        
        DataContext = _cachedViewModel;
        Console.WriteLine($"[ModpackDownloadPage] DataContext设置完成 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        Console.WriteLine($"[ModpackDownloadPage] 构造函数结束 - 时间: {DateTime.Now:HH:mm:ss.fff}");
    }
}
