using System;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using swpumc.Services;
using swpumc.Models;
using SukiUI.Toasts;

namespace swpumc.Controls.DownloadCenter
{
    public partial class DownloadCenter : UserControl
    {
        private DownloadCenterViewModel? _viewModel;
        private MinecraftVersion? _selectedMinecraftVersion;

        public DownloadCenter()
        {
            InitializeComponent();
            
            // 设置DataContext为自己，以支持数据绑定
            DataContext = this;
            
            // 延迟获取服务，避免在构造函数中抛出异常
            this.Loaded += OnLoaded;
        }

        public DownloadCenter(MinecraftVersion selectedMinecraftVersion) : this()
        {
            _selectedMinecraftVersion = selectedMinecraftVersion;
        }

        private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // 移除事件处理器，避免重复调用
            this.Loaded -= OnLoaded;
            
            try
            {
                // 获取服务
                var serviceProvider = (Avalonia.Application.Current as App)?.Services;
                if (serviceProvider == null)
                {
                    Console.WriteLine("[DownloadCenter] Service provider not available");
                    return;
                }
                
                var minecraftVersionService = serviceProvider.GetService<MinecraftVersionService>();
                
                if (minecraftVersionService == null)
                {
                    Console.WriteLine("[DownloadCenter] MinecraftVersionService is not available");
                    return;
                }
                
                // 获取配置服务
                var configService = serviceProvider.GetService<IConfigService>();
                if (configService == null)
                {
                    Console.WriteLine("[DownloadCenter] IConfigService is not available");
                    return;
                }
                
                // 获取Java环境服务
                var javaEnvironmentService = serviceProvider.GetService<IJavaEnvironmentService>();
                if (javaEnvironmentService == null)
                {
                    Console.WriteLine("[DownloadCenter] JavaEnvironmentService not available");
                    return;
                }
                
                // 创建VersionServiceFactory，使用配置服务
                var versionServiceFactory = new VersionServiceFactory(configService, javaEnvironmentService, minecraftVersionService);
                
                // 获取 Toast 管理器
                var toastManager = serviceProvider.GetService<ISukiToastManager>();
                if (toastManager == null)
                {
                    Console.WriteLine("[DownloadCenter] ToastManager not available");
                    return;
                }
                
                // 获取 LauncherManager
                var launcherManager = serviceProvider.GetService<LauncherManager>();
                
                // 创建ViewModel，传入预选择的Minecraft版本
                _viewModel = new DownloadCenterViewModel(versionServiceFactory, minecraftVersionService, toastManager, launcherManager, configService, _selectedMinecraftVersion);
                
                // 订阅下载开始事件，关闭 dialog
                _viewModel.DownloadStarted += OnDownloadStarted;
                
                DataContext = _viewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DownloadCenter] Error initializing: {ex.Message}");
            }
        }

        private void OnDownloadStarted()
        {
            // 关闭 dialog
            var parent = this.Parent;
            while (parent != null)
            {
                if (parent is SukiUI.Dialogs.ISukiDialog dialog)
                {
                    dialog.Dismiss();
                    break;
                }
                parent = parent.Parent;
            }
        }
    }
}
