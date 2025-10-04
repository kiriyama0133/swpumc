using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;

namespace swpumc.Services
{
    public class TrayService : ITrayService
    {
        private IClassicDesktopStyleApplicationLifetime? _lifetime;
        private Window? _mainWindow;
        private TrayIcon? _trayIcon;

        public void Initialize()
        {
            _lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            _mainWindow = _lifetime?.MainWindow;

            if (_mainWindow == null) return;

            // 创建托盘图标
            try
            {
                var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "IMG", "minecraft_light-32x32.ico");
                Console.WriteLine($"[TrayService] 尝试加载托盘图标: {iconPath}");
                Console.WriteLine($"[TrayService] 文件是否存在: {System.IO.File.Exists(iconPath)}");
                
                if (System.IO.File.Exists(iconPath))
                {
                    _trayIcon = new TrayIcon
                    {
                        Icon = new WindowIcon(iconPath),
                        ToolTipText = "SWPU Minecraft Launcher",
                        IsVisible = true
                    };
                    Console.WriteLine($"[TrayService] 托盘图标加载成功");
                }
                else
                {
                    throw new FileNotFoundException($"图标文件不存在: {iconPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrayService] 无法加载托盘图标: {ex.Message}");
                Console.WriteLine($"[TrayService] 使用默认托盘图标");
                // 使用默认图标
                _trayIcon = new TrayIcon
                {
                    ToolTipText = "SWPU Minecraft Launcher",
                    IsVisible = true
                };
            }

            // 创建托盘菜单
            var contextMenu = new NativeMenu();
            
            var showItem = new NativeMenuItem("显示窗口");
            showItem.Click += (s, e) => ShowWindow();
            contextMenu.Add(showItem);

            var hideItem = new NativeMenuItem("隐藏到托盘");
            hideItem.Click += (s, e) => HideWindow();
            contextMenu.Add(hideItem);

            contextMenu.Add(new NativeMenuItemSeparator());

            var exitItem = new NativeMenuItem("退出");
            exitItem.Click += (s, e) => Exit();
            contextMenu.Add(exitItem);

            _trayIcon.Menu = contextMenu;

            // 托盘图标点击事件
            _trayIcon.Clicked += (s, e) => ShowWindow();

            // 窗口状态变化事件
            _mainWindow.PropertyChanged += OnWindowPropertyChanged;
        }

        public void Show()
        {
            _trayIcon!.IsVisible = true;
        }

        public void Hide()
        {
            _trayIcon!.IsVisible = false;
        }

        public void ShowWindow()
        {
            if (_mainWindow == null) return;

            Dispatcher.UIThread.Post(() =>
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
            });
        }

        public void HideWindow()
        {
            if (_mainWindow == null) return;

            Dispatcher.UIThread.Post(() =>
            {
                _mainWindow.Hide();
            });
        }

        public void Exit()
        {
            _lifetime?.Shutdown();
        }

        private void OnWindowPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Window.WindowStateProperty && _mainWindow?.WindowState == WindowState.Minimized)
            {
                HideWindow();
            }
        }
    }
}
