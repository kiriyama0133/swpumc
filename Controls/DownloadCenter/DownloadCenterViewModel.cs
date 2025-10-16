using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using swpumc.Models;
using swpumc.Services;
using SukiUI.Toasts;

namespace swpumc.Controls.DownloadCenter
{
    public partial class DownloadCenterViewModel : ObservableObject
    {
        private readonly VersionServiceFactory _versionServiceFactory;
        private readonly MinecraftVersionService _minecraftVersionService;
        private readonly ISukiToastManager _toastManager;
        private readonly LauncherManager? _launcherManager;
        private readonly IConfigService? _configService;

        // 事件：下载开始，用于关闭 dialog
        public event Action? DownloadStarted;

        // 版本列表
        [ObservableProperty] private ObservableCollection<MinecraftVersion> _minecraftVersions = new();
        [ObservableProperty] private ObservableCollection<MinecraftVersion> _optifineVersions = new();
        [ObservableProperty] private ObservableCollection<MinecraftVersion> _fabricVersions = new();
        [ObservableProperty] private ObservableCollection<MinecraftVersion> _quiltVersions = new();

        // 选中的版本
        [ObservableProperty] private MinecraftVersion? _selectedMinecraftVersion;
        [ObservableProperty] private MinecraftVersion? _selectedOptifineVersion;
        [ObservableProperty] private MinecraftVersion? _selectedFabricVersion;
        [ObservableProperty] private MinecraftVersion? _selectedQuiltVersion;

        // Tab控制
        [ObservableProperty] private ObservableCollection<VersionTabViewModel> _versionTabs = new();
        [ObservableProperty] private VersionTabViewModel? _selectedVersionTab;

        // 新的Tab选择状态
        [ObservableProperty] private bool _isOptifineSelected;
        [ObservableProperty] private bool _isFabricSelected;
        [ObservableProperty] private bool _isQuiltSelected;

        // 下载状态（仅用于控制按钮状态）
        [ObservableProperty] private bool _isDownloading;

        public DownloadCenterViewModel(VersionServiceFactory versionServiceFactory, MinecraftVersionService minecraftVersionService, ISukiToastManager toastManager, LauncherManager? launcherManager = null, IConfigService? configService = null, MinecraftVersion? preSelectedVersion = null)
        {
            _versionServiceFactory = versionServiceFactory;
            _minecraftVersionService = minecraftVersionService;
            _toastManager = toastManager;
            _launcherManager = launcherManager;
            _configService = configService;
            
            Console.WriteLine($"[DownloadCenterViewModel] 构造函数开始 - 预选择版本: {preSelectedVersion?.Id}");
            
            // 诊断服务工厂状态
            Console.WriteLine("[DownloadCenterViewModel] 诊断VersionServiceFactory状态:");
            _versionServiceFactory.Diagnose();
            
            InitializeTabs();
            
            // 如果有预选择的版本，直接设置它
            if (preSelectedVersion != null)
            {
                SelectedMinecraftVersion = preSelectedVersion;
                Console.WriteLine($"[DownloadCenterViewModel] 设置预选择版本: {preSelectedVersion.Id}");
            }
            
            _ = LoadVersionsAsync(); // 使用 _ = 来避免警告
        }

        /// <summary>
        /// 初始化Tab页面
        /// </summary>
        private void InitializeTabs()
        {
            VersionTabs.Clear();
            VersionTabs.Add(new VersionTabViewModel { Header = "Optifine", Content = "Optifine" });
            VersionTabs.Add(new VersionTabViewModel { Header = "Fabric", Content = "Fabric" });
            VersionTabs.Add(new VersionTabViewModel { Header = "Quilt", Content = "Quilt" });
            
            SelectedVersionTab = VersionTabs.FirstOrDefault();
            // 默认选中第一个Tab
            if (SelectedVersionTab?.Content == "Optifine")
                IsOptifineSelected = true;
            else if (SelectedVersionTab?.Content == "Fabric")
                IsFabricSelected = true;
            else if (SelectedVersionTab?.Content == "Quilt")
                IsQuiltSelected = true;
        }

        /// <summary>
        /// 加载所有版本
        /// </summary>
        private async Task LoadVersionsAsync()
        {
            try
            {
                Console.WriteLine($"[DownloadCenterViewModel] LoadVersionsAsync开始 - 当前选中版本: {SelectedMinecraftVersion?.Id}");
                
                // 加载Minecraft版本
                Console.WriteLine("[DownloadCenterViewModel] 开始加载Minecraft版本...");
                var minecraftVersions = await _minecraftVersionService.GetAllVersionsAsync();
                MinecraftVersions.Clear();
                foreach (var version in minecraftVersions)
                {
                    MinecraftVersions.Add(version);
                }
                Console.WriteLine($"[DownloadCenterViewModel] 加载了{MinecraftVersions.Count}个Minecraft版本");

                // 如果有选中的Minecraft版本，加载对应的模组版本
                if (SelectedMinecraftVersion != null)
                {
                    Console.WriteLine($"[DownloadCenterViewModel] 开始加载模组版本，基于版本: {SelectedMinecraftVersion.Id}");
                    
                    // 加载Optifine版本
                    await LoadOptifineVersionsAsync();
                    
                    // 加载Fabric版本
                    await LoadFabricVersionsAsync();
                    
                    // 加载Quilt版本
                    await LoadQuiltVersionsAsync();
                }
                else
                {
                    Console.WriteLine("[DownloadCenterViewModel] 没有选中的Minecraft版本，跳过模组版本加载");
                }

                Console.WriteLine("[DownloadCenterViewModel] LoadVersionsAsync完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DownloadCenterViewModel] LoadVersionsAsync异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载Optifine版本
        /// </summary>
        private async Task LoadOptifineVersionsAsync()
        {
            try
            {
                Console.WriteLine($"[DownloadCenterViewModel] 开始加载Optifine版本，基于Minecraft版本: {SelectedMinecraftVersion?.Id}");
                
                if (SelectedMinecraftVersion == null) 
                {
                    Console.WriteLine("[DownloadCenterViewModel] SelectedMinecraftVersion为null，跳过Optifine版本加载");
                    return;
                }

                Console.WriteLine("[DownloadCenterViewModel] 尝试从VersionServiceFactory获取optifine服务");
                var optifineService = _versionServiceFactory.GetService("optifine");
                Console.WriteLine("[DownloadCenterViewModel] 成功获取optifine服务");
                
                var parameters = new Dictionary<string, object>
                {
                    ["mcVersion"] = SelectedMinecraftVersion.Id
                };

                Console.WriteLine($"[DownloadCenterViewModel] 调用Optifine服务获取版本，参数: mcVersion={SelectedMinecraftVersion.Id}");
                var versions = await optifineService.GetAvailableVersionsAsync(parameters);
                
                OptifineVersions.Clear();
                foreach (var version in versions)
                {
                    OptifineVersions.Add(version);
                }
                
                Console.WriteLine($"[DownloadCenterViewModel] 加载了{OptifineVersions.Count}个Optifine版本");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DownloadCenterViewModel] 加载Optifine版本失败: {ex.Message}");
                Console.WriteLine($"[DownloadCenterViewModel] 异常详情: {ex}");
                // 显示错误信息给用户
                _toastManager.CreateToast()
                    .WithTitle("加载失败")
                    .WithContent($"加载Optifine版本失败: {ex.Message}")
                    .OfType(Avalonia.Controls.Notifications.NotificationType.Error)
                    .Dismiss().After(TimeSpan.FromSeconds(5))
                    .Queue();
            }
        }

        /// <summary>
        /// 加载Fabric版本
        /// </summary>
        private async Task LoadFabricVersionsAsync()
        {
            try
            {
                Console.WriteLine($"[DownloadCenterViewModel] 开始加载Fabric版本，基于Minecraft版本: {SelectedMinecraftVersion?.Id}");
                
                if (SelectedMinecraftVersion == null) 
                {
                    Console.WriteLine("[DownloadCenterViewModel] SelectedMinecraftVersion为null，跳过Fabric版本加载");
                    return;
                }

                Console.WriteLine("[DownloadCenterViewModel] 尝试从VersionServiceFactory获取fabric服务");
                var fabricService = _versionServiceFactory.GetService("fabric");
                Console.WriteLine("[DownloadCenterViewModel] 成功获取fabric服务");
                
                var parameters = new Dictionary<string, object>
                {
                    ["mcVersion"] = SelectedMinecraftVersion.Id
                };

                Console.WriteLine($"[DownloadCenterViewModel] 调用Fabric服务获取版本，参数: mcVersion={SelectedMinecraftVersion.Id}");
                var versions = await fabricService.GetAvailableVersionsAsync(parameters);
                
                FabricVersions.Clear();
                foreach (var version in versions)
                {
                    FabricVersions.Add(version);
                }
                
                Console.WriteLine($"[DownloadCenterViewModel] 加载了{FabricVersions.Count}个Fabric版本");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DownloadCenterViewModel] 加载Fabric版本失败: {ex.Message}");
                Console.WriteLine($"[DownloadCenterViewModel] 异常详情: {ex}");
                // 显示错误信息给用户
                _toastManager.CreateToast()
                    .WithTitle("加载失败")
                    .WithContent($"加载Fabric版本失败: {ex.Message}")
                    .OfType(Avalonia.Controls.Notifications.NotificationType.Error)
                    .Dismiss().After(TimeSpan.FromSeconds(5))
                    .Queue();
            }
        }

        /// <summary>
        /// 加载Quilt版本
        /// </summary>
        private async Task LoadQuiltVersionsAsync()
        {
            try
            {
                Console.WriteLine($"[DownloadCenterViewModel] 开始加载Quilt版本，基于Minecraft版本: {SelectedMinecraftVersion?.Id}");
                
                if (SelectedMinecraftVersion == null) 
                {
                    Console.WriteLine("[DownloadCenterViewModel] SelectedMinecraftVersion为null，跳过Quilt版本加载");
                    return;
                }

                var quiltService = _versionServiceFactory.GetService("quilt");
                var parameters = new Dictionary<string, object>
                {
                    ["mcVersion"] = SelectedMinecraftVersion.Id
                };

                Console.WriteLine($"[DownloadCenterViewModel] 调用Quilt服务获取版本，参数: mcVersion={SelectedMinecraftVersion.Id}");
                var versions = await quiltService.GetAvailableVersionsAsync(parameters);
                
                QuiltVersions.Clear();
                foreach (var version in versions)
                {
                    QuiltVersions.Add(version);
                }
                
                Console.WriteLine($"[DownloadCenterViewModel] 加载了{QuiltVersions.Count}个Quilt版本");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DownloadCenterViewModel] 加载Quilt版本失败: {ex.Message}");
                // 显示错误信息给用户
                _toastManager.CreateToast()
                    .WithTitle("加载失败")
                    .WithContent($"加载Quilt版本失败: {ex.Message}")
                    .OfType(Avalonia.Controls.Notifications.NotificationType.Error)
                    .Dismiss().After(TimeSpan.FromSeconds(5))
                    .Queue();
            }
        }

        /// <summary>
        /// 当选中Minecraft版本时，重新加载对应的模组加载器版本
        /// </summary>
        partial void OnSelectedMinecraftVersionChanged(MinecraftVersion? value)
        {
            if (value != null)
            {
                _ = LoadOptifineVersionsAsync();
                _ = LoadFabricVersionsAsync();
                _ = LoadQuiltVersionsAsync();
            }
        }

        /// <summary>
        /// 当选中Tab时更新选择状态
        /// </summary>
        partial void OnSelectedVersionTabChanged(VersionTabViewModel? value)
        {
            IsOptifineSelected = value?.Content == "Optifine";
            IsFabricSelected = value?.Content == "Fabric";
            IsQuiltSelected = value?.Content == "Quilt";
        }

        /// <summary>
        /// 是否可以下载
        /// </summary>
        public bool CanDownload => SelectedMinecraftVersion != null && 
                                   !IsDownloading;

        /// <summary>
        /// 获取当前选中的模组版本
        /// </summary>
        private MinecraftVersion? GetSelectedModVersion()
        {
            return SelectedVersionTab?.Content switch
            {
                "Optifine" => SelectedOptifineVersion,
                "Fabric" => SelectedFabricVersion,
                "Quilt" => SelectedQuiltVersion,
                _ => null
            };
        }

        /// <summary>
        /// 下载命令
        /// </summary>
        [RelayCommand]
        private void SelectOptifine()
        {
            IsOptifineSelected = true;
            IsFabricSelected = false;
            IsQuiltSelected = false;
        }

        [RelayCommand]
        private void SelectFabric()
        {
            IsOptifineSelected = false;
            IsFabricSelected = true;
            IsQuiltSelected = false;
        }

        [RelayCommand]
        private void SelectQuilt()
        {
            IsOptifineSelected = false;
            IsFabricSelected = false;
            IsQuiltSelected = true;
        }

        [RelayCommand]
        private async Task DownloadAsync()
        {
            if (!CanDownload) return;

            // 创建 Loading Toast
            var progressToast = _toastManager.CreateToast()
                .WithTitle("正在下载...")
                .WithLoadingState(true)
                .WithContent("正在下载Minecraft版本，请稍候...")
                .Queue();

            try
            {
                IsDownloading = true;

                // 触发下载开始事件，关闭 dialog
                DownloadStarted?.Invoke();

                var modVersion = GetSelectedModVersion();
                
                // 检查是否选择了模组加载器且选择了具体版本
                if (modVersion == null)
                {
                    // 没有选择具体版本，下载原版Minecraft
                    Console.WriteLine("[DownloadCenterViewModel] 没有选择具体版本，下载原版Minecraft");
                    await DownloadVanillaVersionAsync(progressToast);
                    return;
                }
                
                // 确定版本类型
                string versionType;
                if (IsOptifineSelected)
                    versionType = "optifine";
                else if (IsFabricSelected)
                    versionType = "fabric";
                else if (IsQuiltSelected)
                    versionType = "quilt";
                else
                {
                    // 显示错误 Toast
                    _toastManager.Dismiss(progressToast);
                    _toastManager.CreateToast()
                        .WithTitle("下载失败")
                        .WithContent("请先选择一个模组加载器")
                        .OfType(Avalonia.Controls.Notifications.NotificationType.Warning)
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();
                    return;
                }
                
                Console.WriteLine($"[DownloadCenterViewModel] 选择的版本类型: {versionType}");
                var versionService = _versionServiceFactory.GetService(versionType);

                var parameters = new Dictionary<string, object>
                {
                    ["mcVersion"] = SelectedMinecraftVersion!.Id
                };
                
                Console.WriteLine($"[DownloadCenterViewModel] 下载参数: mcVersion={SelectedMinecraftVersion.Id}");

                string versionId = modVersion.Id;
                Console.WriteLine($"[DownloadCenterViewModel] 使用选择的模组版本: {versionId}");

                Console.WriteLine($"[DownloadCenterViewModel] 开始执行下载: {versionId}");
                // 执行下载
                var result = await versionService.DownloadVersionAsync(
                    versionId,
                    parameters,
                    (progress, status) =>
                    {
                        // Loading Toast 会自动显示加载动画，不需要手动更新
                        // 可以在这里添加状态文本更新逻辑
                    },
                    System.Threading.CancellationToken.None);

                if (result)
                {
                    // 更新 Toast 为成功状态
                    _toastManager.Dismiss(progressToast);
                    _toastManager.CreateToast()
                        .WithTitle("下载完成！")
                        .WithContent("Minecraft 版本下载成功")
                        .OfType(Avalonia.Controls.Notifications.NotificationType.Success)
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();
                    
                    // 刷新游戏核心
                    await RefreshGameCoresAsync();
                }
                else
                {
                    // 更新 Toast 为失败状态
                    _toastManager.Dismiss(progressToast);
                    _toastManager.CreateToast()
                        .WithTitle("下载失败！")
                        .WithContent("Minecraft 版本下载失败，请重试")
                        .OfType(Avalonia.Controls.Notifications.NotificationType.Error)
                        .Dismiss().After(TimeSpan.FromSeconds(5))
                        .Queue();
                }
            }
            catch (Exception ex)
            {
                // 更新 Toast 为异常状态
                _toastManager.Dismiss(progressToast);
                _toastManager.CreateToast()
                    .WithTitle("下载异常！")
                    .WithContent($"下载过程中发生错误: {ex.Message}")
                    .OfType(Avalonia.Controls.Notifications.NotificationType.Error)
                    .Dismiss().After(TimeSpan.FromSeconds(5))
                    .Queue();
            }
            finally
            {
                IsDownloading = false;
            }
        }
        
        /// <summary>
        /// 下载原版Minecraft
        /// </summary>
        private async Task DownloadVanillaVersionAsync(ISukiToast progressToast)
        {
            Console.WriteLine($"[DownloadCenterViewModel] 开始下载原版Minecraft: {SelectedMinecraftVersion!.Id}");
            
            var vanillaService = _versionServiceFactory.GetService("vanilla");
            var parameters = new Dictionary<string, object>
            {
                ["mcVersion"] = SelectedMinecraftVersion.Id
            };
            
            var result = await vanillaService.DownloadVersionAsync(
                SelectedMinecraftVersion.Id,
                parameters,
                (progress, status) =>
                {
                    // Loading Toast 会自动显示加载动画，不需要手动更新
                    // 可以在这里添加状态文本更新逻辑
                },
                System.Threading.CancellationToken.None);

            if (result)
            {
                // 更新 Toast 为成功状态
                _toastManager.Dismiss(progressToast);
                _toastManager.CreateToast()
                    .WithTitle("下载完成！")
                    .WithContent("原版Minecraft下载成功")
                    .OfType(Avalonia.Controls.Notifications.NotificationType.Success)
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .Queue();
            }
            else
            {
                // 更新 Toast 为失败状态
                _toastManager.Dismiss(progressToast);
                _toastManager.CreateToast()
                    .WithTitle("下载失败！")
                    .WithContent("原版Minecraft下载失败，请重试")
                    .OfType(Avalonia.Controls.Notifications.NotificationType.Error)
                    .Dismiss().After(TimeSpan.FromSeconds(5))
                    .Queue();
            }
        }
        
        /// <summary>
        /// 刷新游戏核心
        /// </summary>
        private async Task RefreshGameCoresAsync()
        {
            try
            {
                if (_launcherManager == null || _configService == null)
                {
                    Console.WriteLine("[DownloadCenterViewModel] LauncherManager 或 ConfigService 不可用，跳过刷新游戏核心");
                    return;
                }
                
                Console.WriteLine("[DownloadCenterViewModel] 开始刷新游戏核心...");
                
                // 重新扫描游戏核心
                var scanResult = await _launcherManager.ScanGameCoresAsync(_launcherManager.GameDirectory);
                if (scanResult.Success)
                {
                    // 转换为MinecraftCoreInfo格式并保存到全局配置
                    var minecraftCores = scanResult.GameCores.Select(core => new Models.MinecraftCoreInfo
                    {
                        Id = core.Id,
                        DisplayName = core.DisplayName,
                        Type = core.Type,
                        Source = core.Source,
                        MainClass = core.MainClass,
                        Assets = core.Assets,
                        JavaVersion = core.JavaVersion,
                        ForgeVersion = core.ForgeVersion,
                        FabricVersion = core.FabricVersion,
                        QuiltVersion = core.QuiltVersion,
                        LastDetected = DateTime.Now,
                        IsValid = true
                    }).ToList();
                    
                    await _configService.UpdateMinecraftCoresAsync(minecraftCores);
                    Console.WriteLine($"[DownloadCenterViewModel] 游戏核心刷新完成，发现 {minecraftCores.Count} 个核心");
                }
                else
                {
                    Console.WriteLine("[DownloadCenterViewModel] 扫描游戏核心失败");
                    foreach (var issue in scanResult.Issues)
                    {
                        Console.WriteLine($"[DownloadCenterViewModel] 扫描问题: {issue}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DownloadCenterViewModel] 刷新游戏核心失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 版本Tab视图模型
    /// </summary>
    public class VersionTabViewModel
    {
        public string Header { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}