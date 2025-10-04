using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using swpumc.Controls.Loginform;
using swpumc.Views;
using swpumc.ViewModels;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using swpumc.Services;
using swpumc.Services.API;
using System;

namespace swpumc.Controls.Loginform;

public partial class SkinLoginForm : UserControl
{
        /// <summary>
        /// 登录成功事件
        /// </summary>
        public event Action? OnLoginSuccess;
        
        public SkinLoginForm()
        {
            InitializeComponent();
            
            // 延迟初始化，避免在构造函数中创建复杂的服务依赖
            this.Loaded += OnLoaded;
            
            // 添加额外的延迟初始化，确保MainWindow完全加载
            this.AttachedToVisualTree += OnAttachedToVisualTree;
        }
        
        private void OnAttachedToVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
        {
            this.AttachedToVisualTree -= OnAttachedToVisualTree;
            
            // 再次延迟初始化，确保MainWindow完全加载
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    InitializeViewModel();
                });
            });
        }
        
        private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // 移除事件处理器，避免重复调用
            this.Loaded -= OnLoaded;
            
            // 延迟初始化，确保MainWindow完全初始化后再获取ToastManager
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                InitializeViewModel();
            });
        }
        
        private void InitializeViewModel()
        {
            try
            {
                // 通过依赖注入获取服务
                var app = Application.Current as App;
                var serviceProvider = app?.Services;
                
                var dialogManager = serviceProvider?.GetService(typeof(ISukiDialogManager)) as ISukiDialogManager;
                var toastManager = serviceProvider?.GetService(typeof(ISukiToastManager)) as ISukiToastManager;
                var yggdrasilService = serviceProvider?.GetService(typeof(IYggdrasilService)) as IYggdrasilService;
                var playerManagementService = serviceProvider?.GetService(typeof(IPlayerManagementService)) as IPlayerManagementService;
                var microsoftAuthService = serviceProvider?.GetService(typeof(IMicrosoftAuthService)) as IMicrosoftAuthService;
                
                Console.WriteLine($"[SkinLoginForm] 依赖注入ToastManager: {toastManager?.GetType().Name} (HashCode: {toastManager?.GetHashCode()})");
                
                // 直接使用依赖注入的ToastManager，它应该是单例
                var correctToastManager = toastManager ?? new SukiToastManager();
                
                Console.WriteLine($"[SkinLoginForm] 使用的ToastManager: {correctToastManager?.GetType().Name} (HashCode: {correctToastManager?.GetHashCode()})");
                
                var avatarManagementService = app?.Services?.GetService(typeof(IAvatarManagementService)) as IAvatarManagementService;
                
                var viewModel = new SkinLoginFormViewModel(
                    dialogManager ?? new SukiDialogManager(), 
                    correctToastManager,
                    yggdrasilService ?? new YggdrasilService(new HttpService()),
                    playerManagementService ?? new PlayerManagementService(new ConfigService(), avatarManagementService ?? new AvatarManagementService(new AvatarApiService(new HttpService(), new ConfigService(), new HttpDownloadService()))),
                    microsoftAuthService ?? new MicrosoftAuthService(new System.Net.Http.HttpClient())
                );
                
                // 订阅登录成功事件
                viewModel.OnLoginSuccess += OnLoginSuccessInternal;
                
                DataContext = viewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SkinLoginForm] Failed to initialize: {ex.Message}");
                // 创建一个基本的ViewModel作为后备
                DataContext = new SkinLoginFormViewModel(
                    new SukiDialogManager(), 
                    new SukiToastManager(),
                    new YggdrasilService(new HttpService()),
                    new PlayerManagementService(new ConfigService(), new AvatarManagementService(new AvatarApiService(new HttpService(), new ConfigService(), new HttpDownloadService()))),
                    new MicrosoftAuthService(new System.Net.Http.HttpClient())
                );
            }
        }

        private void OnLoginSuccessInternal()
        {
            // 登录成功后隐藏或销毁登录表单
            this.IsVisible = false;
            Console.WriteLine("[SkinLoginForm] 登录成功，隐藏登录表单");
            
            // 触发公开事件
            OnLoginSuccess?.Invoke();
        }
}