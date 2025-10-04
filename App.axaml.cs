using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using swpumc.Services;
using swpumc.Utils;
using swpumc.ViewModels;
using swpumc.Views;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using MinecraftLaunch.Utilities;
using MinecraftLaunch;

namespace swpumc;

public partial class App : Application
{
    public ServiceProvider? _serviceProvider;
    public static int MaxThreadCount = 256; // 最大线程数
    public static bool IsEnableMirror = true; // 是否启用镜像下载
    

    public override void Initialize()
    {
        DownloadManager.MaxThread = MaxThreadCount;
        DownloadManager.IsEnableMirror = IsEnableMirror;
        
        
        
        AvaloniaXamlLoader.Load(this);
        ConfigureServices();
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // 注册服务
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IAnimationService, AnimationService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IBackgroundService, BackgroundService>();
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<IHttpService, HttpService>();
        services.AddSingleton<IHttpDownloadService, HttpDownloadService>();
        services.AddSingleton<ITrayService, TrayService>();
        services.AddSingleton<ISukiDialogManager, SukiDialogManager>();
        services.AddSingleton<ISukiToastManager, SukiToastManager>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IPlayerManagementService,PlayerManagementService>();
        services.AddSingleton<swpumc.Services.API.IYggdrasilService, swpumc.Services.API.YggdrasilService>();
        services.AddSingleton<swpumc.Services.API.IAvatarApiService, swpumc.Services.API.AvatarApiService>();
        services.AddSingleton<IAvatarManagementService, AvatarManagementService>();
        services.AddSingleton<IMicrosoftAuthService, MicrosoftAuthService>();
        services.AddSingleton<IMinecraftVersionService, MinecraftVersionService>();
        services.AddSingleton<MinecraftVersionService>();
        services.AddSingleton<VersionServiceFactory>();
        services.AddSingleton<HttpClient>();
        services.AddSingleton<LauncherManager>();
        services.AddSingleton<IJavaEnvironmentService, JavaEnvironmentService>();
        services.AddSingleton<IModpackDownloadService>(provider =>
        {
            var versionServiceFactory = provider.GetRequiredService<VersionServiceFactory>();
            var configService = provider.GetRequiredService<IConfigService>();
            var minecraftFolder = versionServiceFactory.GetMinecraftFolderPath();
            var javaPath = configService.GetDefaultJavaPath();
            return ModpackDownloadServiceFactory.Create(minecraftFolder, javaPath);
        });
                // services.AddSingleton<AccountConfigManager>();
        services.AddTransient<MainWindowViewModel>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            // 初始化配置服务并加载所有配置
            var configService = _serviceProvider?.GetRequiredService<IConfigService>();
            if (configService != null)
            {
                await configService.LoadAllConfigsAsync();
            }
            
            // 初始化玩家管理服务并加载用户数据
            var playerManagementService = _serviceProvider?.GetRequiredService<IPlayerManagementService>();
            if (playerManagementService != null)
            {
                Console.WriteLine("[App] 初始化玩家管理服务，加载用户数据...");
                await playerManagementService.LoadPlayersAsync();
                Console.WriteLine($"[App] 玩家管理服务初始化完成，加载了 {playerManagementService.Players.Count} 个玩家");
                
                // 通知InfoView刷新数据
                // 这里需要在MainWindow创建后通知InfoView，暂时先记录日志
            }
            else
            {
                Console.WriteLine("[App] PlayerManagementService服务不可用");
            }
            
                    // 初始化Java环境和Minecraft游戏核心检测
                    await InitializeServicesAsync();
            
            // 初始化主题服务并设置为Dark主题
            var themeService = _serviceProvider?.GetRequiredService<IThemeService>();
            themeService?.SetTheme(true); // true = Dark theme
            
            // 应用启动时进行一次主题刷新，确保所有样式正确加载
            themeService?.RefreshThemeStyles();
            
            // 使用依赖注入创建ViewModel
            var mainWindowViewModel = _serviceProvider?.GetRequiredService<MainWindowViewModel>();
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel,
            };
            
            // 初始化托盘服务
            var trayService = _serviceProvider?.GetRequiredService<ITrayService>();
            trayService?.Initialize();
            
            desktop.MainWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    public IServiceProvider? Services => _serviceProvider;
    
            /// <summary>
            /// 初始化服务
            /// </summary>
            private async System.Threading.Tasks.Task InitializeServicesAsync()
            {
                try
                {
                    Console.WriteLine("[App] 开始初始化服务...");
                    
                    // 获取服务
                    var javaService = _serviceProvider?.GetRequiredService<IJavaEnvironmentService>();
                    var launcherManager = _serviceProvider?.GetRequiredService<LauncherManager>();
                    var configService = _serviceProvider?.GetRequiredService<IConfigService>();
                    
                    if (javaService == null || launcherManager == null || configService == null)
                    {
                        Console.WriteLine("[App] 服务不可用，跳过初始化");
                        return;
                    }
                    
                    // 初始化Java环境
                    await javaService.InitializeJavaEnvironmentsAsync(configService);
                    
                    // 初始化Minecraft核心
                    await launcherManager.InitializeMinecraftCoresAsync(configService);
                    
                    Console.WriteLine("[App] 服务初始化完成");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[App] 服务初始化失败: {ex.Message}");
                    Console.WriteLine($"[App] 错误详情: {ex}");
                }
            }
}