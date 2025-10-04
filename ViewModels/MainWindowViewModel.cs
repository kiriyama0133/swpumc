using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using swpumc.Models;
using swpumc.Services;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using Avalonia.Input;

namespace swpumc.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IThemeService _themeService;
        private readonly INavigationService _navigationService;
        private readonly IBackgroundService _backgroundService;
        private readonly IHttpService _httpService;
        private readonly ISukiDialogManager _dialogManager;
        private readonly ISukiToastManager _toastManager = null!;
        
        // Dialog管理器
        public ISukiDialogManager DialogManager => _dialogManager;
        
        // Toast管理器
        public ISukiToastManager ToastManager => _toastManager;
        
        public string Greeting { get; } = "Welcome to Avalonia!";
        
        [ObservableProperty]
        private bool _isDarkTheme = false;
        
        [ObservableProperty]
        private string _themeButtonText = "Switch to Dark";
        
        [ObservableProperty]
        private NavigationPage? _activePage;
        
        [ObservableProperty]
        private string _backgroundPath = string.Empty;
        
        // 导航页面列表
        public IAvaloniaReadOnlyList<NavigationPage> NavigationPages { get; }
        
        public MainWindowViewModel(IThemeService themeService, INavigationService navigationService, IBackgroundService backgroundService, IHttpService httpService, ISukiDialogManager dialogManager, ISukiToastManager toastManager)
        {
            _themeService = themeService;
            _navigationService = navigationService;
            _backgroundService = backgroundService;
            _httpService = httpService;
            _dialogManager = dialogManager;
            _toastManager = toastManager;
            
            Console.WriteLine($"[MainWindowViewModel] ToastManager初始化: {_toastManager?.GetType().Name} (HashCode: {_toastManager?.GetHashCode()})");
            
            _isDarkTheme = _themeService.IsDarkTheme;
            UpdateThemeButtonText();
            
            // 初始化导航页面列表
            NavigationPages = new AvaloniaList<NavigationPage>(_navigationService.GetAvailablePages());
            
            // 订阅主题变化事件
            _themeService.ThemeChanged += OnThemeChanged;
            
            // 订阅导航变化事件
            _navigationService.NavigationRequested += OnNavigationRequested;
            
            // 订阅背景变化事件
            _backgroundService.BackgroundChanged += OnBackgroundChanged;
            
            // 初始化背景
            BackgroundPath = _backgroundService.CurrentBackgroundPath;
            Console.WriteLine($"[MainWindowViewModel] Initial background path: {BackgroundPath}");
            
            // 设置默认页面
            if (NavigationPages.Any())
            {
                ActivePage = NavigationPages.First();
            }
        }
        
        private void OnThemeChanged(object? sender, bool isDark)
        {
            IsDarkTheme = isDark;
            UpdateThemeButtonText();
        }
        
        private void OnNavigationRequested(Type pageType)
        {
            var page = NavigationPages.FirstOrDefault(p => p.PageType == pageType);
            if (page != null)
            {
                ActivePage = page;
            }
        }
        
        private void OnBackgroundChanged(object? sender, string backgroundPath)
        {
            Console.WriteLine($"[MainWindowViewModel] Background changed to: {backgroundPath}");
            BackgroundPath = backgroundPath;
        }
        
        private void UpdateThemeButtonText()
        {
            ThemeButtonText = IsDarkTheme ? "Switch to Light" : "Switch to Dark";
        }
        
        [RelayCommand]
        private void ToggleTheme()
        {
            _themeService.ToggleTheme();
        }
        
        public void OnDragOver(Avalonia.Input.DragEventArgs e)
        {
            Console.WriteLine("[MainWindowViewModel] OnDragOver 被调用");
            
            if (e.Data.Contains(Avalonia.Input.DataFormats.Files))
            {
                e.DragEffects = Avalonia.Input.DragDropEffects.Copy;
                e.Handled = true;
                Console.WriteLine("[MainWindowViewModel] 设置拖拽效果为 Copy");
            }
            else
            {
                e.DragEffects = Avalonia.Input.DragDropEffects.None;
                e.Handled = true;
                Console.WriteLine("[MainWindowViewModel] 设置拖拽效果为 None");
            }
        }
        
        
        public async Task OnDrop(Avalonia.Input.DragEventArgs e)
        {
            Console.WriteLine("[MainWindowViewModel] OnDrop 被调用");
            
            if (!e.Data.Contains(Avalonia.Input.DataFormats.Files))
            {
                Console.WriteLine("[MainWindowViewModel] 没有检测到文件数据");
                return;
            }
            
            try
            {
                var files = DataObjectExtensions.GetFiles(e.Data);
                if (files != null && files.Any())
                {
                    foreach (var file in files)
                    {
                        var filePath = file.Path.LocalPath;
                        Console.WriteLine($"[MainWindowViewModel] 检测到文件: {filePath}");
                        
                        // 这里可以调用 ModpackDownloadService 来处理文件
                        // 暂时只显示 Toast 消息
                        _toastManager?.CreateToast()
                            .WithTitle("文件检测")
                            .WithContent($"检测到文件: {System.IO.Path.GetFileName(filePath)}")
                            .Queue();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MainWindowViewModel] 处理文件拖拽失败: {ex.Message}");
                _toastManager?.CreateToast()
                    .WithTitle("错误")
                    .WithContent($"处理文件时发生错误: {ex.Message}")
                    .Queue();
            }
        }
        
        public void OnDragLeave(Avalonia.Input.DragEventArgs e)
        {
            Console.WriteLine("[MainWindowViewModel] OnDragLeave 被调用");
        }
        
        [RelayCommand]
        private async Task NavigateToPage(NavigationPage page)
        {
            if (page != null)
            {
                await _navigationService.NavigateToAsync(page.PageType);
            }
        }
        
        [RelayCommand]
        private async Task TestHttpService()
        {
            Console.WriteLine("[MainWindowViewModel] 开始测试HTTP服务...");
            
            try
            {
                // 测试GET请求
                var response = await _httpService.CreateRequest()
                    .SetBaseUrl("https://jsonplaceholder.typicode.com")
                    .SetApi("/posts/1")
                    .SetMethod(HttpMethod.GET)
                    .ExecuteAsync<object>();

                if (response.IsSuccess)
                {
                    Console.WriteLine($"[MainWindowViewModel] HTTP测试成功! 状态码: {response.StatusCode}");
                    Console.WriteLine($"[MainWindowViewModel] 响应数据: {System.Text.Json.JsonSerializer.Serialize(response.Data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}");
                }
                else
                {
                    Console.WriteLine($"[MainWindowViewModel] HTTP测试失败: {response.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MainWindowViewModel] HTTP测试异常: {ex.Message}");
            }
        }

    }
}
