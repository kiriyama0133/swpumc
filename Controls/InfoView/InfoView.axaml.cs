using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using swpumc.Controls.InfoView;
using swpumc.Models;
using swpumc.Services;
using System;
using System.Threading.Tasks;

namespace swpumc.Controls.InfoView;

public partial class InfoView : UserControl
{
    public InfoView()
    {
        try
        {
            InitializeComponent();
            
            // 延迟初始化DataContext，避免在构造函数中访问服务
            this.Loaded += OnLoaded;
        }
        catch (Exception)
        {
            // 创建一个简单的DataContext，避免崩溃
            DataContext = new InfoViewModel(null!, null!);
        }
    }
    
    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            // 通过依赖注入获取服务
            var app = Application.Current as App;
            var playerManagementService = app?.Services?.GetService(typeof(IPlayerManagementService)) as IPlayerManagementService;
            var avatarManagementService = app?.Services?.GetService(typeof(IAvatarManagementService)) as IAvatarManagementService;
            
            if (playerManagementService != null)
            {
                DataContext = new InfoViewModel(playerManagementService, avatarManagementService);
                
                // 立即触发数据同步
                if (DataContext is InfoViewModel viewModel)
                {
                    viewModel.RefreshData();
                }
            }
            else
            {
                DataContext = new InfoViewModel(null!, null!);
            }
        }
        catch (Exception)
        {
            // 创建一个默认的ViewModel，避免控件崩溃
            DataContext = new InfoViewModel(null!, null!);
        }
        
        // 移除事件处理器，避免重复调用
        this.Loaded -= OnLoaded;
    }
    
    private void OnRoleSelectionButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // 切换用户列表弹出菜单的可见性
        var popup = this.FindControl<Border>("UserListPopup");
        if (popup != null)
        {
            popup.IsVisible = !popup.IsVisible;
        }
    }
    
    private void OnUserItemClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is PlayerInfo playerInfo)
        {
            // 设置选中的玩家
            if (DataContext is InfoViewModel viewModel)
            {
                viewModel.SelectedPlayer = playerInfo;
            }
            
            // 隐藏弹出菜单
            var popup = this.FindControl<Border>("UserListPopup");
            if (popup != null)
            {
                popup.IsVisible = false;
            }
        }
    }
    
    private void OnSwitchAuthButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            Console.WriteLine("[InfoView] 用户点击切换验证方式按钮");
            
            // 触发切换验证方式事件
            OnSwitchAuthRequested?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[InfoView] 切换验证方式时出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 切换验证方式请求事件
    /// </summary>
    public event Action? OnSwitchAuthRequested;
    
    /// <summary>
    /// 重新加载玩家数据
    /// </summary>
    public async Task LoadPlayersAsync()
    {
        try
        {
            if (DataContext is InfoViewModel viewModel)
            {
                await viewModel.LoadPlayersAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[InfoView] 加载玩家数据时出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 刷新显示数据
    /// </summary>
    public void RefreshData()
    {
        try
        {
            if (DataContext is InfoViewModel viewModel)
            {
                viewModel.RefreshData();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[InfoView] 刷新数据时出错: {ex.Message}");
        }
    }
}