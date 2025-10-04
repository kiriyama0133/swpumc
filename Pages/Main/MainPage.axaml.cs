using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using swpumc.Services;
using swpumc.Controls.InfoView;
using swpumc.Controls.Loginform;
using swpumc.Controls.GameLauncher;
using swpumc.Models;
using System;
using System.Linq;
using SukiUI;
using SukiUI.Enums;
using SukiUI.Toasts;
using SukiUI.Dialogs;

namespace swpumc.Pages.Main;

public partial class MainPage : BasePage
{
    private readonly IPlayerManagementService? _playerManagementService;
    
    // 缓存控件引用，避免重复查找
    private ScrollViewer? _gameLauncher;
    private StackPanel? _userInfoPanel;
    private SkinLoginForm? _loginForm;
    private InfoView? _infoViewControl;
    private OfflinePlayerView? _offlinePlayerViewControl;
    private Controls.GameLauncher.GameLauncher? _gameLauncherControl;
    
    public MainPage()
    {
        InitializeComponent();
        
        // 获取服务
        var app = Application.Current as App;
        _playerManagementService = app?.Services?.GetService(typeof(IPlayerManagementService)) as IPlayerManagementService;
        
        // 延迟初始化界面状态，避免在构造函数中访问可能未完全初始化的服务
        this.Loaded += OnLoaded;
    }
    
    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // 移除事件处理器，避免重复调用
        this.Loaded -= OnLoaded;
        
        // 初始化控件引用
        InitializeControls();
        
        // 初始化界面状态
        InitializeUIState();
        
        // 初始化GameLauncher数据将在控件加载后自动进行
    }
    
    /// <summary>
    /// 初始化控件引用
    /// </summary>
    private void InitializeControls()
    {
        _gameLauncher = this.FindControl<ScrollViewer>("GameLauncherScrollViewer");
        _userInfoPanel = this.FindControl<StackPanel>("UserInfoPanel");
        _loginForm = this.FindControl<SkinLoginForm>("FullScreenLoginForm");
        _infoViewControl = this.FindControl<InfoView>("InfoViewControl");
        _offlinePlayerViewControl = this.FindControl<OfflinePlayerView>("OfflinePlayerViewControl");
        _gameLauncherControl = this.FindControl<Controls.GameLauncher.GameLauncher>("GameLauncherControl");
        
        // 订阅登录成功事件
        if (_loginForm != null)
        {
            _loginForm.OnLoginSuccess += OnLoginSuccess;
        }
        
        // 订阅OfflinePlayerView的切换验证方式事件
        if (_offlinePlayerViewControl != null)
        {
            _offlinePlayerViewControl.OnSwitchAuthRequested += OnSwitchAuthRequested;
        }
        
        // 订阅GameLauncher的游戏退出事件
        if (_gameLauncherControl != null)
        {
            _gameLauncherControl.GameExited += OnGameExited;
        }
    }
    
    /// <summary>
    /// 显示登录界面
    /// </summary>
    private void ShowLoginInterface()
    {
        if (_loginForm != null) _loginForm.IsVisible = true;
        if (_gameLauncher != null) _gameLauncher.IsVisible = false;
        if (_userInfoPanel != null) _userInfoPanel.IsVisible = false;
    }
    
    /// <summary>
    /// 显示主界面
    /// </summary>
    private void ShowMainInterface()
    {
        if (_loginForm != null) _loginForm.IsVisible = false;
        if (_gameLauncher != null) _gameLauncher.IsVisible = true;
        if (_userInfoPanel != null) _userInfoPanel.IsVisible = true;
        
        // 根据用户类型显示不同的用户信息控件
        if (_playerManagementService?.SelectedPlayer?.UserAccount != null)
        {
            var userAccount = _playerManagementService.SelectedPlayer.UserAccount;
            
            // 判断是否为离线用户
            if (userAccount.Email?.EndsWith("@local") == true || 
                string.IsNullOrEmpty(userAccount.Email) ||
                userAccount.Email == "offline")
            {
                // 显示离线玩家控件
                if (_infoViewControl != null) _infoViewControl.IsVisible = false;
                if (_offlinePlayerViewControl != null) _offlinePlayerViewControl.IsVisible = true;
            }
            else
            {
                // 显示第三方用户控件
                if (_infoViewControl != null) _infoViewControl.IsVisible = true;
                if (_offlinePlayerViewControl != null) _offlinePlayerViewControl.IsVisible = false;
            }
        }
        else
        {
            // 默认显示离线玩家控件
            if (_infoViewControl != null) _infoViewControl.IsVisible = false;
            if (_offlinePlayerViewControl != null) _offlinePlayerViewControl.IsVisible = true;
        }
        
    }
    
    /// <summary>
    /// 初始化界面状态
    /// </summary>
    private void InitializeUIState()
    {
        try
        {
            
            // 检查是否有有效的用户配置
            if (_playerManagementService != null)
            {
                var hasValidUser = _playerManagementService.Players?.Any() == true && 
                                 _playerManagementService.SelectedPlayer != null &&
                                 HasValidAuthentication(_playerManagementService.SelectedPlayer);
                
                
                if (hasValidUser)
                {
                    ShowMainInterface();
                }
                else
                {
                    ShowLoginInterface();
                }
            }
            else
            {
                ShowMainInterface();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MainPage] 初始化界面状态时出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 检查用户是否有有效的验证方式
    /// </summary>
    /// <param name="player">玩家信息</param>
    /// <returns>是否有有效验证</returns>
    private bool HasValidAuthentication(PlayerInfo player)
    {
        if (player?.UserAccount == null) return false;
        
        var userAccount = player.UserAccount;
        
        // 检查是否有有效的访问令牌（在线验证）
        if (!string.IsNullOrEmpty(userAccount.AccessToken))
        {
            return true;
        }
        
        // 检查是否有第三方验证角色（第三方验证）
        if (userAccount.Profiles?.Any() == true)
        {
            return true;
        }
        
        // 检查是否已验证（离线验证）
        if (userAccount.Verified)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 处理切换验证方式请求
    /// </summary>
    public void OnSwitchAuthRequested()
    {
        try
        {
            
            // 清理当前用户配置，强制显示登录界面
            ClearCurrentUserConfiguration();
            
            // 显示登录界面
            ShowLoginInterface();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MainPage] 切换验证方式时出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 处理游戏退出事件
    /// </summary>
    private void OnGameExited(object? sender, GameExitEventArgs e)
    {
        try
        {
            if (e.IsNormalExit)
            {
                // 正常退出，显示Toast
                var app = Application.Current as App;
                var toastManager = app?.Services?.GetService(typeof(ISukiToastManager)) as ISukiToastManager;
                
                if (toastManager != null)
                {
                    toastManager.CreateToast()
                        .WithTitle("游戏已退出")
                        .WithContent("欢迎下次游玩！")
                        .OfType(NotificationType.Success)
                        .Dismiss().ByClicking()
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();
                }
            }
            else
            {
                // 异常退出，显示错误Dialog
                ShowGameErrorDialog(e.ErrorMessage ?? "游戏异常退出", e.ErrorDetails ?? "未知错误");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MainPage] 处理游戏退出事件失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 显示游戏错误Dialog
    /// </summary>
    private async void ShowGameErrorDialog(string title, string details)
    {
        try
        {
            var app = Application.Current as App;
            var dialogManager = app?.Services?.GetService(typeof(ISukiDialogManager)) as ISukiDialogManager;
            
            if (dialogManager != null)
            {
                dialogManager.CreateDialog()
                    .WithTitle(title)
                    .WithContent($"游戏启动失败，请检查以下信息：\n\n{details}")
                    .WithActionButton("确定", _ => { }, true, "Accent", "Accent")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MainPage] 显示游戏错误Dialog失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 清理当前用户配置
    /// </summary>
    private void ClearCurrentUserConfiguration()
    {
        try
        {
            if (_playerManagementService != null)
            {
                // 清除选中的玩家
                _playerManagementService.SelectedPlayer = null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MainPage] 清理用户配置时出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 处理登录成功事件
    /// </summary>
    public async void OnLoginSuccess()
    {
        try
        {
            
            // 刷新用户数据
            if (_playerManagementService != null)
            {
                await _playerManagementService.LoadPlayersAsync();
                
                // 自动选择最新登录的用户（最后一个用户）
                if (_playerManagementService.Players?.Any() == true)
                {
                    var latestPlayer = _playerManagementService.Players.LastOrDefault();
                    if (latestPlayer != null)
                    {
                        _playerManagementService.SelectedPlayer = latestPlayer;
                    }
                }
                
                // 强制刷新用户信息控件
                if (_infoViewControl != null)
                {
                    await _infoViewControl.LoadPlayersAsync();
                    _infoViewControl.RefreshData();
                }
                
                if (_offlinePlayerViewControl != null)
                {
                    await _offlinePlayerViewControl.LoadPlayersAsync();
                    _offlinePlayerViewControl.RefreshData();
                }
                
                // 刷新游戏核心列表
                if (_gameLauncherControl != null)
                {
                    Console.WriteLine("[MainPage] 登录成功，刷新游戏核心列表...");
                    await _gameLauncherControl.LoadGameCoresFromConfigAsync();
                }
                
                // 检查是否有有效的用户配置
                var hasValidUser = _playerManagementService.Players?.Any() == true && 
                                 _playerManagementService.SelectedPlayer != null &&
                                 HasValidAuthentication(_playerManagementService.SelectedPlayer);
                
                if (hasValidUser)
                {
                    ShowMainInterface();
                }
                else
                {
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MainPage] 处理登录成功时出错: {ex.Message}");
        }
    }
}