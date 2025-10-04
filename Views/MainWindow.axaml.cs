using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using swpumc.Controls;
using swpumc.Services;
using swpumc.ViewModels;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace swpumc.Views
{
    public partial class MainWindow : SukiWindow
    {
        private readonly IConfigService _configService;

        public MainWindow()
        {
            InitializeComponent();
            
            // 获取服务
            _configService = ((App)Application.Current!).Services?.GetService(typeof(IConfigService)) as IConfigService;
            
            // 设置拖拽处理器
            SetupDragHandlers();
            
            // 设置启动动画
            SetupStartupAnimation();
        }

        private void SetupDragHandlers()
        {
            // 设置窗口移动拖拽功能
            var contentBorder = this.FindControl<Border>("ContentBorder");
            if (contentBorder != null)
            {
                contentBorder.PointerPressed += OnBorderPointerPressed;
            }
        }

        private void OnBorderPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                this.BeginMoveDrag(e);
            }
        }

        private void SetupStartupAnimation()
        {
            // 启动动画 - 暂时注释掉
            // _animationService.StartupAnimation(this);
        }
    }
}