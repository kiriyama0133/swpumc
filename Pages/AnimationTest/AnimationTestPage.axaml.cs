using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using swpumc.Pages.AnimationTest;
using swpumc.Services;
using swpumc.Utils;

namespace swpumc.Pages.AnimationTest;

public partial class AnimationTestPage : BasePage
{
    private readonly IAnimationService _animationService;
    private CancellationTokenSource _cancellationTokenSource;

    public AnimationTestPage()
    {
        InitializeComponent();
        
        // 从依赖注入容器获取AnimationService
        var app = App.Current as App;
        _animationService = app?.Services?.GetService<IAnimationService>() ?? new AnimationService();
        _cancellationTokenSource = new CancellationTokenSource();
        
        DataContext = new AnimationTestPageViewModel();
    }

    private async void OnFadeInClick(object? sender, RoutedEventArgs e)
    {
        if (FadeTestTarget != null)
        {
            await AnimationManager.StartAnimationAsync(FadeTestTarget, async (token) =>
            {
                await _animationService.AnimateOpacityAsync(FadeTestTarget, 0.0, 1.0, TimeSpan.FromMilliseconds(500), EasingType.CubicEaseOut, token);
            }, "淡入动画");
        }
    }

    private async void OnFadeOutClick(object? sender, RoutedEventArgs e)
    {
        if (FadeTestTarget != null)
        {
            await AnimationManager.StartAnimationAsync(FadeTestTarget, async (token) =>
            {
                await _animationService.AnimateOpacityAsync(FadeTestTarget, 1.0, 0.0, TimeSpan.FromMilliseconds(500), EasingType.CubicEaseIn, token);
            }, "淡出动画");
        }
    }

    private async void OnScaleInClick(object? sender, RoutedEventArgs e)
    {
        if (ScaleTestTarget != null)
        {
            await AnimationManager.StartAnimationAsync(ScaleTestTarget, async (token) =>
            {
                await _animationService.AnimateComponentScaleAsync(ScaleTestTarget, 1.0, 1.2, TimeSpan.FromMilliseconds(500), 2f, 0.4f, token);
            }, "缩放动画 (放大)");
        }
    }

    private async void OnScaleOutClick(object? sender, RoutedEventArgs e)
    {
        if (ScaleTestTarget != null)
        {
            await AnimationManager.StartAnimationAsync(ScaleTestTarget, async (token) =>
            {
                await _animationService.AnimateComponentScaleAsync(ScaleTestTarget, 1.2, 1.0, TimeSpan.FromMilliseconds(500), 2f, 0.4f, token);
            }, "缩放动画 (缩小)");
        }
    }

    private async void OnMoveLeftClick(object? sender, RoutedEventArgs e)
    {
        if (MoveTestTarget != null)
        {
            await AnimationManager.StartAnimationAsync(MoveTestTarget, async (token) =>
            {
                await _animationService.AnimateTranslationAsync(MoveTestTarget, 0, -100, 0, 0, TimeSpan.FromMilliseconds(500), 2f, 0.4f, token);
            }, "左移动画");
        }
    }

    private async void OnMoveRightClick(object? sender, RoutedEventArgs e)
    {
        if (MoveTestTarget != null)
        {
            await AnimationManager.StartAnimationAsync(MoveTestTarget, async (token) =>
            {
                await _animationService.AnimateTranslationAsync(MoveTestTarget, 0, 100, 0, 0, TimeSpan.FromMilliseconds(500), 2f, 0.4f, token);
            }, "右移动画");
        }
    }

    private async void OnMoveUpClick(object? sender, RoutedEventArgs e)
    {
        if (MoveTestTarget != null)
        {
            await AnimationManager.StartAnimationAsync(MoveTestTarget, async (token) =>
            {
                await _animationService.AnimateTranslationAsync(MoveTestTarget, 0, 0, 0, -50, TimeSpan.FromMilliseconds(500), 2f, 0.4f, token);
            }, "上移动画");
        }
    }

    private async void OnMoveDownClick(object? sender, RoutedEventArgs e)
    {
        if (MoveTestTarget != null)
        {
            await AnimationManager.StartAnimationAsync(MoveTestTarget, async (token) =>
            {
                await _animationService.AnimateTranslationAsync(MoveTestTarget, 0, 0, 0, 50, TimeSpan.FromMilliseconds(500), 2f, 0.4f, token);
            }, "下移动画");
        }
    }

    private async void OnRotateClick(object? sender, RoutedEventArgs e)
    {
        if (RotateTestTarget != null)
        {
            await AnimationManager.StartAnimationAsync(RotateTestTarget, async (token) =>
            {
                await _animationService.AnimateRotationAsync(RotateTestTarget, 0, 360, TimeSpan.FromMilliseconds(1500), 2f, 0.4f, token);
            }, "旋转动画 (360度)");
        }
    }

    private async void OnSpringClick(object? sender, RoutedEventArgs e)
    {
        if (SpringTestTarget != null)
        {
            await AnimationManager.StartAnimationAsync(SpringTestTarget, async (token) =>
            {
                await _animationService.AnimateComponentScaleAsync(SpringTestTarget, 1.0, 1.2, TimeSpan.FromMilliseconds(1000), 3f, 0.3f, token);
            }, "弹簧动画 (长宽变化)");
        }
    }

    private async void OnComboClick(object? sender, RoutedEventArgs e)
    {
        if (ComboTestTarget != null)
        {
            await AnimationManager.StartAnimationAsync(ComboTestTarget, async (token) =>
            {
                Console.WriteLine("[AnimationTest] 开始组合动画");
                
                // 组合动画：使用多种动画效果
                await _animationService.AnimateOpacityAsync(ComboTestTarget, 1.0, 0.0, TimeSpan.FromMilliseconds(200), EasingType.CubicEaseIn, token);
                await _animationService.AnimateComponentScaleAsync(ComboTestTarget, 0.5, 1.2, TimeSpan.FromMilliseconds(400), 2f, 0.4f, token);
                await _animationService.AnimateRotationAsync(ComboTestTarget, 180, 0, TimeSpan.FromMilliseconds(400), 2f, 0.4f, token);
                await _animationService.AnimateComponentScaleAsync(ComboTestTarget, 1.2, 1.0, TimeSpan.FromMilliseconds(200), 2f, 0.4f, token);
                await _animationService.AnimateOpacityAsync(ComboTestTarget, 0.0, 1.0, TimeSpan.FromMilliseconds(200), EasingType.CubicEaseOut, token);
                
                Console.WriteLine("[AnimationTest] 组合动画完成");
            }, "组合动画");
        }
    }

    private async void OnEasingTestClick(object? sender, RoutedEventArgs e)
    {
        if (EasingTestTarget != null && EasingComboBox != null)
        {
            await AnimationManager.StartAnimationAsync(EasingTestTarget, async (token) =>
            {
                var selectedIndex = EasingComboBox.SelectedIndex;
                var easingType = selectedIndex switch
                {
                    0 => EasingType.Linear,
                    1 => EasingType.EaseIn,
                    2 => EasingType.EaseOut,
                    3 => EasingType.EaseInOut,
                    4 => EasingType.ElasticEaseOut,
                    5 => EasingType.BounceEaseOut,
                    _ => EasingType.CubicEaseOut
                };

                await _animationService.AnimateTranslationAsync(EasingTestTarget, 0, 100, 0, 0, TimeSpan.FromMilliseconds(1000), 2f, 0.4f, token);
            }, "缓动测试动画");
        }
    }

    private async void OnSequenceClick(object? sender, RoutedEventArgs e)
    {
        if (SequenceTestTarget != null)
        {
            await AnimationManager.StartAnimationAsync(SequenceTestTarget, async (token) =>
            {
                // 连续动画序列
                await _animationService.AnimateOpacityAsync(SequenceTestTarget, 1.0, 0.0, TimeSpan.FromMilliseconds(200), EasingType.CubicEaseIn, token);
                await _animationService.AnimateComponentScaleAsync(SequenceTestTarget, 1.0, 0.5, TimeSpan.FromMilliseconds(300), 2f, 0.4f, token);
                await _animationService.AnimateRotationAsync(SequenceTestTarget, 0, 180, TimeSpan.FromMilliseconds(500), 2f, 0.4f, token);
                await _animationService.AnimateTranslationAsync(SequenceTestTarget, 0, 50, 0, 0, TimeSpan.FromMilliseconds(300), 2f, 0.4f, token);
                await _animationService.AnimateComponentScaleAsync(SequenceTestTarget, 0.5, 1.0, TimeSpan.FromMilliseconds(300), 2f, 0.4f, token);
                await _animationService.AnimateOpacityAsync(SequenceTestTarget, 0.0, 1.0, TimeSpan.FromMilliseconds(200), EasingType.CubicEaseOut, token);
            }, "序列动画");
        }
    }

    private void OnStopAllClick(object? sender, RoutedEventArgs e)
    {
        AnimationManager.CancelAllAnimations();
        Console.WriteLine($"[AnimationTest] 已停止所有动画，当前活跃动画数量: {AnimationManager.ActiveAnimationCount}");
    }

    private void OnResetAllClick(object? sender, RoutedEventArgs e)
    {
        // 重置所有测试目标
        ResetElement(FadeTestTarget);
        ResetElement(ScaleTestTarget);
        ResetElement(MoveTestTarget);
        ResetElement(RotateTestTarget);
        ResetElement(SpringTestTarget);
        ResetElement(ComboTestTarget);
        ResetElement(EasingTestTarget);
        ResetElement(SequenceTestTarget);
        Console.WriteLine("[AnimationTest] 所有元素已重置");
    }

    private void ResetElement(Control? element)
    {
        if (element != null)
        {
            element.Opacity = 1.0;
            element.RenderTransform = null;
            element.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Relative);
            element.Margin = new Thickness(0);
            element.Width = double.NaN; // 恢复自动宽度
            element.Height = double.NaN; // 恢复自动高度
            Console.WriteLine($"[AnimationTest] 重置元素: {element.GetType().Name}");
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        base.OnDetachedFromVisualTree(e);
    }
}
