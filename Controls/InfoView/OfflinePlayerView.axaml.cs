using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using swpumc.Controls.InfoView;
using swpumc.Models;
using swpumc.Services;
using System;
using System.Threading.Tasks;

namespace swpumc.Controls.InfoView;

public partial class OfflinePlayerView : UserControl
{
    public OfflinePlayerView()
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
            DataContext = new OfflinePlayerViewModel();
        }
    }
    
    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            // 通过依赖注入获取服务
            var app = Application.Current as App;
            var playerManagementService = app?.Services?.GetService(typeof(IPlayerManagementService)) as IPlayerManagementService;
            
            if (playerManagementService != null)
            {
                DataContext = new OfflinePlayerViewModel();
                
                // 订阅ViewModel的切换验证方式事件
                if (DataContext is OfflinePlayerViewModel viewModel)
                {
                    viewModel.OnSwitchAuthRequested += () => OnSwitchAuthRequested?.Invoke();
                    viewModel.RefreshData();
                }
            }
            else
            {
                DataContext = new OfflinePlayerViewModel();
            }
        }
        catch (Exception)
        {
            // 创建一个默认的ViewModel，避免控件崩溃
            DataContext = new OfflinePlayerViewModel();
        }
        
        // 移除事件处理器，避免重复调用
        this.Loaded -= OnLoaded;
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
            if (DataContext is OfflinePlayerViewModel viewModel)
            {
                await viewModel.LoadPlayersAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OfflinePlayerView] 加载玩家数据时出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 刷新显示数据
    /// </summary>
    public void RefreshData()
    {
        try
        {
            if (DataContext is OfflinePlayerViewModel viewModel)
            {
                viewModel.RefreshData();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OfflinePlayerView] 刷新数据时出错: {ex.Message}");
        }
    }
}
