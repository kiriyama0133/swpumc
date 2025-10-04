using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using swpumc.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MinecraftLaunch.Launch;
using MinecraftLaunch.Base.Models;

namespace swpumc.Controls.GameLauncher
{
    public partial class GameLauncher : UserControl, INotifyPropertyChanged
    {
        private LauncherManager? _launcherManager;
        private IJavaEnvironmentService? _javaService;
        private IConfigService? _configService;
        private IPlayerManagementService? _playerManagementService;
        
        private ObservableCollection<GameCoreInfo> _gameCores = new();
        private GameCoreInfo? _selectedGameCore;
        private int _minMemorySize = 1024;
        private int _maxMemorySize = 4096;
        private int _width = 1280;
        private int _height = 720;
        private bool _isFullScreen = false;
        private string _serverAddress = "";
        private string _javaPath = "";
        private string _customJvmArgs = "";
        private string _customGameArgs = "";
        private bool _canLaunch = false;
        
        // Java环境相关属性
        private ObservableCollection<JavaEnvironmentDisplayInfo> _javaEnvironments = new();
        private JavaEnvironmentDisplayInfo? _selectedJavaEnvironment;
        public new event PropertyChangedEventHandler? PropertyChanged;
        
        // Tab Control相关属性
        private ObservableCollection<LaunchConfigTabViewModel> _launchConfigTabs = new();
        private LaunchConfigTabViewModel? _selectedLaunchConfigTab;
        
        
        // 游戏退出相关
        private readonly List<string> _errorLogs = new();
        private bool _hasError = false;
        
        // 游戏退出事件
        public event EventHandler<GameExitEventArgs>? GameExited;

        public GameLauncher()
        {
            InitializeComponent();
            
            
            // 设置DataContext为自己，以支持数据绑定
            DataContext = this;
            
            // 延迟获取服务，避免在构造函数中抛出异常
            this.Loaded += OnLoaded;
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
                    Console.WriteLine("[GameLauncher] Service provider not available");
                    return;
                }
                
                _launcherManager = serviceProvider.GetService<LauncherManager>();
                _javaService = serviceProvider.GetService<IJavaEnvironmentService>();
                _configService = serviceProvider.GetService<IConfigService>();
                _playerManagementService = serviceProvider.GetService<IPlayerManagementService>();
                
                if (_launcherManager == null || _javaService == null || _configService == null || _playerManagementService == null)
                {
                    Console.WriteLine("[GameLauncher] Some services are not available");
                    return;
                }
                
                // 初始化数据
                InitializeAsync();
                
                // 加载启动配置
                LoadLaunchConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLauncher] Failed to initialize: {ex.Message}");
            }
        }

        public ObservableCollection<GameCoreInfo> GameCores
        {
            get => _gameCores;
            set => SetProperty(ref _gameCores, value);
        }

        public GameCoreInfo? SelectedGameCore
        {
            get => _selectedGameCore;
            set
            {
                if (SetProperty(ref _selectedGameCore, value))
                {
                    OnPropertyChanged(nameof(HasSelectedCore));
                    UpdateCanLaunch();
                    if (value != null)
                    {
                        _configService?.UpdateLastUsedCore(value.Id);
                    }
                }
            }
        }

        public bool HasSelectedCore => SelectedGameCore != null;

        public int MinMemorySize
        {
            get => _minMemorySize;
            set
            {
                if (SetProperty(ref _minMemorySize, value))
                {
                    UpdateCanLaunch();
                    _configService?.UpdateMemoryConfig(value, MaxMemorySize);
                }
            }
        }

        public int MaxMemorySize
        {
            get => _maxMemorySize;
            set
            {
                if (SetProperty(ref _maxMemorySize, value))
                {
                    UpdateCanLaunch();
                    _configService?.UpdateMemoryConfig(MinMemorySize, value);
                }
            }
        }

        public int GameWidth
        {
            get => _width;
            set
            {
                if (SetProperty(ref _width, value))
                {
                    UpdateCanLaunch();
                    _configService?.UpdateWindowConfig(value, GameHeight, IsFullScreen);
                }
            }
        }

        public int GameHeight
        {
            get => _height;
            set
            {
                if (SetProperty(ref _height, value))
                {
                    UpdateCanLaunch();
                    _configService?.UpdateWindowConfig(GameWidth, value, IsFullScreen);
                }
            }
        }

        public bool IsFullScreen
        {
            get => _isFullScreen;
            set
            {
                if (SetProperty(ref _isFullScreen, value))
                {
                    UpdateCanLaunch();
                    _configService?.UpdateWindowConfig(GameWidth, GameHeight, value);
                }
            }
        }

        public string ServerAddress
        {
            get => _serverAddress;
            set
            {
                if (SetProperty(ref _serverAddress, value))
                {
                    UpdateCanLaunch();
                    _configService?.UpdateServerConfig(value);
                }
            }
        }

        public string JavaPath
        {
            get => _javaPath;
            set
            {
                if (SetProperty(ref _javaPath, value))
                {
                    OnPropertyChanged(nameof(HasSelectedJava));
                    UpdateCanLaunch();
                    var javaEnvironments = JavaEnvironments.Select(j => j.JavaPath).ToList();
                    _configService?.UpdateJavaConfig(value, javaEnvironments);
                }
            }
        }

        public string CustomJvmArgs
        {
            get => _customJvmArgs;
            set
            {
                if (SetProperty(ref _customJvmArgs, value))
                {
                    UpdateCanLaunch();
                    _configService?.UpdateAdvancedConfig(value, CustomGameArgs);
                }
            }
        }

        public string CustomGameArgs
        {
            get => _customGameArgs;
            set
            {
                if (SetProperty(ref _customGameArgs, value))
                {
                    UpdateCanLaunch();
                    _configService?.UpdateAdvancedConfig(CustomJvmArgs, value);
                }
            }
        }

        public bool CanLaunch
        {
            get => _canLaunch;
            set => SetProperty(ref _canLaunch, value);
        }
        
        // Java环境相关属性
        public ObservableCollection<JavaEnvironmentDisplayInfo> JavaEnvironments
        {
            get => _javaEnvironments;
            set => SetProperty(ref _javaEnvironments, value);
        }
        
        public JavaEnvironmentDisplayInfo? SelectedJavaEnvironment
        {
            get => _selectedJavaEnvironment;
            set
            {
                if (SetProperty(ref _selectedJavaEnvironment, value))
                {
                    if (value != null && _javaService != null)
                    {
                        // 使用JavaEnvironmentService获取可执行文件路径
                        JavaPath = _javaService.GetJavaExecutablePath(value.JavaPath);
                    }
                    OnPropertyChanged(nameof(HasSelectedJava));
                    UpdateCanLaunch();
                }
            }
        }
        
        public bool HasSelectedJava => !string.IsNullOrEmpty(JavaPath);
        
        // Tab Control属性
        public ObservableCollection<LaunchConfigTabViewModel> LaunchConfigTabs
        {
            get => _launchConfigTabs;
            set => SetProperty(ref _launchConfigTabs, value);
        }
        
        public LaunchConfigTabViewModel? SelectedLaunchConfigTab
        {
            get => _selectedLaunchConfigTab;
            set => SetProperty(ref _selectedLaunchConfigTab, value);
        }

        private async void InitializeAsync()
        {
            try
            {
                // 从全局配置加载游戏核心
                await LoadGameCoresFromConfigAsync();
                
                // 加载Java环境
                await LoadJavaEnvironmentsAsync();
                
                // 初始化Tab Control
                InitializeLaunchConfigTabs();
                
                // 触发所有属性的变更通知，确保UI正确显示
                OnPropertyChanged(nameof(MinMemorySize));
                OnPropertyChanged(nameof(MaxMemorySize));
                OnPropertyChanged(nameof(GameWidth));
                OnPropertyChanged(nameof(GameHeight));
                OnPropertyChanged(nameof(JavaPath));
                OnPropertyChanged(nameof(IsFullScreen));
                OnPropertyChanged(nameof(ServerAddress));
                
                UpdateCanLaunch();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLauncher] 初始化失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 初始化启动配置Tab
        /// </summary>
        private void InitializeLaunchConfigTabs()
        {
            LaunchConfigTabs.Clear();
            
            // 添加内存配置Tab
            LaunchConfigTabs.Add(new LaunchConfigTabViewModel("内存配置", "MemoryConfig"));
            
            // 添加窗口配置Tab
            LaunchConfigTabs.Add(new LaunchConfigTabViewModel("窗口配置", "WindowConfig"));
            
            // 添加服务器配置Tab
            LaunchConfigTabs.Add(new LaunchConfigTabViewModel("服务器配置", "ServerConfig"));
            
            // 添加Java配置Tab
            LaunchConfigTabs.Add(new LaunchConfigTabViewModel("Java配置", "JavaConfig"));
            
            // 添加高级设置Tab
            LaunchConfigTabs.Add(new LaunchConfigTabViewModel("高级设置", "AdvancedConfig"));
            
            // 设置默认选中的Tab
            if (LaunchConfigTabs.Count > 0)
            {
                SelectedLaunchConfigTab = LaunchConfigTabs[0];
            }
        }

        private async Task LoadGameCoresAsync()
        {
            try
            {
                if (_launcherManager == null) return;
                
                var scanResult = await _launcherManager.ScanGameCoresAsync(_launcherManager.GameDirectory);
                if (scanResult.Success)
                {
                    GameCores.Clear();
                    foreach (var core in scanResult.GameCores)
                    {
                        GameCores.Add(core);
                    }
                    
                    // 自动选择第一个核心
                    if (GameCores.Count > 0)
                    {
                        SelectedGameCore = GameCores.First();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLauncher] 加载游戏核心失败: {ex.Message}");
            }
        }
        
        private async Task LoadJavaEnvironmentsAsync()
        {
            try
            {
                if (_javaService == null) return;
                
                var javaEnvironments = _javaService.JavaInstallations;
                if (javaEnvironments?.Any() == true)
                {
                    JavaEnvironments.Clear();
                    foreach (var java in javaEnvironments)
                    {
                        JavaEnvironments.Add(new JavaEnvironmentDisplayInfo
                        {
                            JavaPath = java.JavaHome,
                            Version = java.Version,
                            Vendor = java.Vendor,
                            Architecture = java.Architecture,
                            IsDefault = java.IsDefault
                        });
                    }
                    
                    // 自动选择默认Java环境
                    var defaultJava = JavaEnvironments.FirstOrDefault(j => j.IsDefault);
                    if (defaultJava != null)
                    {
                        SelectedJavaEnvironment = defaultJava;
                    }
                    else if (JavaEnvironments.Count > 0)
                    {
                        SelectedJavaEnvironment = JavaEnvironments[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLauncher] 加载Java环境失败: {ex.Message}");
            }
        }

        public async Task LoadGameCoresFromConfigAsync()
        {
            try
            {
                if (_configService == null) return;
                
                StatusText.Text = "正在从配置加载游戏核心...";
                RefreshButton.IsEnabled = false;
                
                // 从全局配置获取Minecraft核心
                var minecraftCores = _configService.GetMinecraftCores();
                if (minecraftCores.Count > 0)
                {
                    GameCores.Clear();
                    foreach (var core in minecraftCores)
                    {
                        var gameCoreInfo = new GameCoreInfo
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
                            QuiltVersion = core.QuiltVersion
                        };
                        GameCores.Add(gameCoreInfo);
                    }
                    
                    // 不自动选择，让LoadLaunchConfig来处理
                    StatusText.Text = $"从配置加载了 {GameCores.Count} 个游戏核心";
                }
                else
                {
                    StatusText.Text = "配置中未找到游戏核心，请点击刷新按钮扫描游戏核心";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLauncher] 从配置加载游戏核心失败: {ex.Message}");
                StatusText.Text = "加载游戏核心失败";
            }
            finally
            {
                RefreshButton.IsEnabled = true;
            }
        }

        private async Task SetDefaultJavaPathFromConfigAsync()
        {
            try
            {
                if (_configService == null || _launcherManager == null) return;
                
                // 从全局配置获取Java环境列表
                var javaEnvironments = _configService.GetJavaEnvironments();
                if (javaEnvironments.Count > 0)
                {
                    // 优先选择标记为默认的Java环境
                    var defaultJava = javaEnvironments.FirstOrDefault(j => j.IsDefault);
                    if (defaultJava != null)
                    {
                        // 使用LauncherManager的SetJavaPath方法确保Java路径正确
                        _launcherManager.SetJavaPath(defaultJava.JavaPath);
                        JavaPath = _launcherManager.JavaPath;
                    }
                    else
                    {
                        // 如果没有标记为默认的，选择第一个
                        _launcherManager.SetJavaPath(javaEnvironments.First().JavaPath);
                        JavaPath = _launcherManager.JavaPath;
                    }
                }
                else
                {
                    // 如果配置中没有Java环境，尝试从默认路径获取
                    var defaultJavaPath = _configService.GetDefaultJavaPath();
                    if (!string.IsNullOrEmpty(defaultJavaPath))
                    {
                        JavaPath = defaultJavaPath;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLauncher] 从配置设置默认Java路径失败: {ex.Message}");
            }
        }


        private void UpdateCanLaunch()
        {
            CanLaunch = SelectedGameCore != null && !string.IsNullOrEmpty(JavaPath);
        }
        

        private async void OnRefreshClicked(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (_launcherManager == null || _configService == null) return;
                
                StatusText.Text = "正在重新扫描游戏核心...";
                RefreshButton.IsEnabled = false;
                
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
                    
                    // 重新从配置加载
                    await LoadGameCoresFromConfigAsync();
                }
                else
                {
                    StatusText.Text = "扫描游戏核心失败";
                    foreach (var issue in scanResult.Issues)
                    {
                        Console.WriteLine($"[GameLauncher] 扫描问题: {issue}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLauncher] 刷新游戏核心失败: {ex.Message}");
                StatusText.Text = "刷新失败";
            }
            finally
            {
                RefreshButton.IsEnabled = true;
            }
        }
        
        private async void OnRefreshJavaClicked(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (_javaService != null && _configService != null)
                {
                    await _javaService.InitializeJavaEnvironmentsAsync(_configService);
                    await LoadJavaEnvironmentsAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLauncher] 刷新Java环境失败: {ex.Message}");
            }
        }

        private async void OnSelectJavaClicked(object? sender, RoutedEventArgs e)
        {
            try
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel?.StorageProvider is not { } storageProvider)
                    return;

                var file = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "选择Java可执行文件",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("Java可执行文件")
                        {
                            Patterns = new[] { "*.exe" }
                        },
                        new FilePickerFileType("所有文件")
                        {
                            Patterns = new[] { "*" }
                        }
                    }
                });

                if (file.Count > 0)
                {
                    JavaPath = file[0].Path.LocalPath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLauncher] 选择Java路径失败: {ex.Message}");
            }
        }


        private async void OnLaunchClicked(object? sender, RoutedEventArgs e)
        {
            if (SelectedGameCore == null || _launcherManager == null || _playerManagementService == null) return;

            LaunchButton.IsEnabled = false;
            StatusText.Text = "正在启动游戏...";
            LaunchProgressBar.IsVisible = true;

            try
            {
                // 设置启动器配置
                // 根据用户选择的模式获取玩家信息
                var selectedPlayer = _playerManagementService.SelectedPlayer;
                if (selectedPlayer?.UserAccount != null)
                {
                    var userAccount = selectedPlayer.UserAccount;
                    
                    // 检查用户类型和角色选择
                    if (userAccount.Profiles?.Any() == true)
                    {
                        // 第三方验证用户（LittleSkin等）- 使用第一个角色
                        var selectedProfile = userAccount.Profiles.FirstOrDefault();
                        if (selectedProfile != null)
                        {
                            _launcherManager.AccountName = selectedProfile.Name;
                        }
                        else
                        {
                            _launcherManager.AccountName = userAccount.Nickname;
                        }
                    }
                    else if (!string.IsNullOrEmpty(userAccount.AccessToken))
                    {
                        // 微软用户或其他在线用户 - 使用昵称
                        _launcherManager.AccountName = userAccount.Nickname;
                    }
                    else
                    {
                        // 离线用户 - 使用昵称或邮箱
                        _launcherManager.AccountName = !string.IsNullOrEmpty(userAccount.Nickname) ? userAccount.Nickname : userAccount.Email;
                    }
                }
                else
                {
                    // 没有选中玩家，使用默认名称
                    _launcherManager.AccountName = "Player";
                }
                _launcherManager.GameCoreId = SelectedGameCore.Id;
                _launcherManager.MaxMemorySize = MaxMemorySize;
                _launcherManager.MinMemorySize = MinMemorySize;
                _launcherManager.Width = GameWidth;
                _launcherManager.Height = GameHeight;
                _launcherManager.IsFullScreen = IsFullScreen;
                _launcherManager.ServerAddress = ServerAddress;
                _launcherManager.JavaPath = JavaPath;
                _launcherManager.CustomJvmArgs = CustomJvmArgs.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
                _launcherManager.CustomGameArgs = CustomGameArgs.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

                // 设置游戏核心ID
                _launcherManager.GameCoreId = SelectedGameCore.Id;
                
                // 使用MinecraftLaunch内置的依赖验证和下载功能
                StatusText.Text = "正在验证和下载游戏依赖...";
                
                try
                {
                    // 获取MinecraftEntry对象
                    var minecraftEntry = await _launcherManager.GetMinecraftEntryAsync(SelectedGameCore.Id);
                    if (minecraftEntry == null)
                    {
                        StatusText.Text = "无法获取游戏核心信息";
                        LaunchButton.IsEnabled = true;
                        LaunchProgressBar.IsVisible = false;
                        return;
                    }
                    
                    // 使用MinecraftLaunch的依赖验证和下载功能
                    var result = await _launcherManager.VerifyAndDownloadGameCoreDependenciesAsync(minecraftEntry);
                    if (!result)
                    {
                        StatusText.Text = "游戏核心依赖验证失败";
                        LaunchButton.IsEnabled = true;
                        LaunchProgressBar.IsVisible = false;
                        return;
                    }
                    
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"验证游戏核心时出错: {ex.Message}";
                    LaunchButton.IsEnabled = true;
                    LaunchProgressBar.IsVisible = false;
                    Console.WriteLine($"[GameLauncher] 验证游戏核心时出错: {ex.Message}");
                    return;
                }

                // 启动游戏
                var minecraftProcess = await _launcherManager.LaunchGameAsync();
                
                StatusText.Text = "游戏启动中，正在监控日志...";
                LaunchProgressBar.IsVisible = false;
                
                if (minecraftProcess?.Process == null)
                {
                    StatusText.Text = "游戏启动失败：无法获取进程信息";
                    LaunchButton.IsEnabled = true;
                    return;
                }
                
                
                // 使用MinecraftLaunch内置的日志监控功能
                minecraftProcess.Started += OnGameStarted;
                minecraftProcess.Exited += OnGameExited;
                minecraftProcess.OutputLogReceived += OnLogReceived;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"启动失败: {ex.Message}";
                LaunchProgressBar.IsVisible = false;
                LaunchButton.IsEnabled = true;
                Console.WriteLine($"[GameLauncher] 启动游戏失败: {ex}");
            }
        }

        private void OnGameStarted(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusText.Text = "游戏启动成功！";
            });
        }


        private void OnGameExited(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                StatusText.Text = "游戏已退出";
                LaunchButton.IsEnabled = true;
                
                // 判断是否为正常退出
                bool isNormalExit = !_hasError;
                string? errorMessage = null;
                string? errorDetails = null;
                
                if (_hasError && _errorLogs.Count > 0)
                {
                    errorMessage = "游戏异常退出";
                    errorDetails = string.Join("\n", _errorLogs);
                }
                
                // 触发游戏退出事件
                var eventArgs = new GameExitEventArgs(isNormalExit, 0, errorMessage, errorDetails);
                GameExited?.Invoke(this, eventArgs);
                
                // 重置错误状态
                _hasError = false;
                _errorLogs.Clear();
            });
        }

        private void OnLogReceived(object? sender, LogReceivedEventArgs e)
        {
            // 处理游戏日志
            var logEntry = e.Data;
            var logType = logEntry.LogLevel.ToString();
            var logContent = logEntry.Log;
            
            // 只检测真正的致命错误，忽略游戏内的正常错误日志
            if (logType == "Fatal" || 
                (logType == "Error" && IsCriticalError(logContent)) ||
                (logType == "Exception" && IsCriticalException(logContent)))
            {
                _hasError = true;
                _errorLogs.Add($"[{logType}] {logContent}");
            }
        }
        
        /// <summary>
        /// 判断是否为关键错误（会导致游戏崩溃的错误）
        /// </summary>
        private bool IsCriticalError(string logContent)
        {
            // 检测会导致游戏无法启动或崩溃的关键错误
            return logContent.Contains("Failed to start") ||
                   logContent.Contains("Could not find or load main class") ||
                   logContent.Contains("Unable to launch") ||
                   logContent.Contains("Game crashed") ||
                   logContent.Contains("OutOfMemoryError") ||
                   logContent.Contains("StackOverflowError") ||
                   logContent.Contains("IllegalAccessError") ||
                   logContent.Contains("UnsupportedClassVersionError") ||
                   logContent.Contains("NoClassDefFoundError");
        }
        
        /// <summary>
        /// 判断是否为关键异常（会导致游戏崩溃的异常）
        /// </summary>
        private bool IsCriticalException(string logContent)
        {
            // 检测会导致游戏崩溃的关键异常
            return logContent.Contains("Exception in thread \"main\"") ||
                   logContent.Contains("Exception in thread \"Server thread\"") ||
                   logContent.Contains("OutOfMemoryError") ||
                   logContent.Contains("StackOverflowError") ||
                   logContent.Contains("IllegalAccessError") ||
                   logContent.Contains("UnsupportedClassVersionError") ||
                   logContent.Contains("NoClassDefFoundError");
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 加载启动配置
        /// </summary>
        private void LoadLaunchConfig()
        {
            try
            {
                if (_configService == null) return;

                var launchConfig = _configService.LaunchConfig;
                if (launchConfig == null) return;

                // 直接设置属性值，不触发保存
                _minMemorySize = launchConfig.LaunchSettings.MemoryConfig.MinMemorySize;
                _maxMemorySize = launchConfig.LaunchSettings.MemoryConfig.MaxMemorySize;
                _width = launchConfig.LaunchSettings.WindowConfig.GameWidth;
                _height = launchConfig.LaunchSettings.WindowConfig.GameHeight;
                _isFullScreen = launchConfig.LaunchSettings.WindowConfig.IsFullScreen;
                _serverAddress = launchConfig.LaunchSettings.ServerConfig.ServerAddress;
                _javaPath = launchConfig.LaunchSettings.JavaConfig.JavaPath;
                _customJvmArgs = launchConfig.LaunchSettings.AdvancedConfig.CustomJvmArgs;
                _customGameArgs = launchConfig.LaunchSettings.AdvancedConfig.CustomGameArgs;

                // 触发属性变更通知
                OnPropertyChanged(nameof(MinMemorySize));
                OnPropertyChanged(nameof(MaxMemorySize));
                OnPropertyChanged(nameof(GameWidth));
                OnPropertyChanged(nameof(GameHeight));
                OnPropertyChanged(nameof(IsFullScreen));
                OnPropertyChanged(nameof(ServerAddress));
                OnPropertyChanged(nameof(JavaPath));
                OnPropertyChanged(nameof(CustomJvmArgs));
                OnPropertyChanged(nameof(CustomGameArgs));
                OnPropertyChanged(nameof(HasSelectedJava));

                // 加载最后使用的核心
                if (!string.IsNullOrEmpty(launchConfig.LastUsedCore))
                {
                    var core = GameCores.FirstOrDefault(c => c.Id == launchConfig.LastUsedCore);
                    if (core != null)
                    {
                        _selectedGameCore = core;
                        OnPropertyChanged(nameof(SelectedGameCore));
                        OnPropertyChanged(nameof(HasSelectedCore));
                    }
                }
                else if (GameCores.Count > 0)
                {
                    // 如果没有保存的核心，选择第一个
                    _selectedGameCore = GameCores.FirstOrDefault();
                    OnPropertyChanged(nameof(SelectedGameCore));
                    OnPropertyChanged(nameof(HasSelectedCore));
                }

                UpdateCanLaunch();
                Console.WriteLine("[GameLauncher] 启动配置加载完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameLauncher] 加载启动配置失败: {ex.Message}");
            }
        }

    }
    
    /// <summary>
    /// Java环境显示信息
    /// </summary>
    public class JavaEnvironmentDisplayInfo
    {
        public string JavaPath { get; set; } = "";
        public string Version { get; set; } = "";
        public string Vendor { get; set; } = "";
        public string Architecture { get; set; } = "";
        public bool IsDefault { get; set; }
        
        public string DisplayName => $"{Vendor} {Version} ({Architecture})" + (IsDefault ? " [默认]" : "");
    }
    
    /// <summary>
    /// 游戏退出事件参数
    /// </summary>
    public class GameExitEventArgs : EventArgs
    {
        public bool IsNormalExit { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetails { get; set; }
        public int ExitCode { get; set; }
        
        public GameExitEventArgs(bool isNormalExit, int exitCode = 0, string? errorMessage = null, string? errorDetails = null)
        {
            IsNormalExit = isNormalExit;
            ExitCode = exitCode;
            ErrorMessage = errorMessage;
            ErrorDetails = errorDetails;
        }
    }
}
