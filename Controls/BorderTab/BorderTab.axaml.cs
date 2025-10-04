using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Animation;
using Avalonia.Styling;
using System;
using System.Threading.Tasks;
using swpumc.Services;

namespace swpumc;

public partial class BorderTab : UserControl
{
    public event EventHandler? MinimizeClicked;
    public event EventHandler? CloseClicked;
    
    public BorderTab()
    {
        InitializeComponent();
    }
    
    private async void OnMinimizeClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("[BorderTab] OnMinimizeClick 被调用");
        
        // 获取父窗口 - 手动遍历控件树
        var window = GetParentWindow(this);
        if (window == null) 
        {
            Console.WriteLine("[BorderTab] 无法获取父窗口");
            return;
        }
        
        Console.WriteLine($"[BorderTab] 找到父窗口: {window.GetType().Name}");
        
        // 创建透明度动画
        var animation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.5),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters = { new Avalonia.Styling.Setter(Window.OpacityProperty, 1.0) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters = { new Avalonia.Styling.Setter(Window.OpacityProperty, 0.0) }
                }
            }
        };
        
        // 启动动画
        await animation.RunAsync(window);
        
        // 动画完成后隐藏到托盘
        var app = Application.Current as App;
        var trayService = app?.Services?.GetService(typeof(ITrayService)) as ITrayService;
        if (trayService != null)
        {
            Console.WriteLine("[BorderTab] 调用托盘服务隐藏窗口");
            trayService.HideWindow();
        }
        else
        {
            Console.WriteLine("[BorderTab] 托盘服务不可用，直接隐藏窗口");
            window.Hide();
        }
    }
    
    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("[BorderTab] OnCloseClick 被调用");
        
        // 获取父窗口并关闭 - 手动遍历控件树
        var window = GetParentWindow(this);
        if (window != null)
        {
            Console.WriteLine($"[BorderTab] 关闭窗口: {window.GetType().Name}");
            window.Close();
        }
        else
        {
            Console.WriteLine("[BorderTab] 无法获取父窗口，无法关闭");
        }
    }
    
    /// <summary>
    /// 手动遍历控件树获取父窗口
    /// </summary>
    private Window? GetParentWindow(Control control)
    {
        var parent = control.Parent;
        while (parent != null)
        {
            if (parent is Window window)
            {
                return window;
            }
            parent = parent.Parent;
        }
        return null;
    }
}