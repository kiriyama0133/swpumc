using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Threading;
using System.Threading.Tasks;
using swpumc.Utils.animation;

namespace swpumc.Utils
{
    /// <summary>
    /// 动画服务实现类，提供统一的动画创建和管理功能
    /// </summary>
    public class AnimationService : IAnimationService
    {
        /// <summary>
        /// 创建组件尺寸变化动画（使用SecondOrder物理模型）
        /// </summary>
        public async Task AnimateComponentSizeAsync(
            Control control, 
            double fromWidth, double toWidth,
            double fromHeight, double toHeight,
            TimeSpan duration,
            float frequency = 2f,
            float damping = 0.4f,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var startTime = DateTime.Now;
                var secondOrder = new SecondOrder(new Vec2((float)fromWidth, (float)fromHeight), frequency, damping);
                
                while (DateTime.Now - startTime < duration && !cancellationToken.IsCancellationRequested)
                {
                    var elapsed = DateTime.Now - startTime;
                    var progress = Math.Min((float)elapsed.TotalMilliseconds / (float)duration.TotalMilliseconds, 1.0f);
                    
                    // 使用SecondOrder系统计算当前尺寸
                    var targetSize = new Vec2((float)toWidth, (float)toHeight);
                    var currentSize = secondOrder.Update(0.016f, targetSize); // 60FPS
                    
                    // 应用尺寸变化
                    control.Width = currentSize.X;
                    control.Height = currentSize.Y;
                    
                    await Task.Delay(16, cancellationToken); // 约60FPS
                }
                
                // 确保最终尺寸
                control.Width = toWidth;
                control.Height = toHeight;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[AnimationService] 组件尺寸动画被取消");
            }
        }

        /// <summary>
        /// 创建窗口尺寸变化动画（使用高频率自定义循环）
        /// </summary>
        public async Task AnimateWindowSizeAsync(
            Window window,
            double fromWidth, double toWidth,
            double fromHeight, double toHeight,
            TimeSpan duration,
            float frequency = 2f,
            float damping = 0.4f,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Console.WriteLine($"[AnimationService] 开始窗口尺寸动画: {fromWidth}x{fromHeight} -> {toWidth}x{toHeight}");
                
                // 设置初始尺寸
                window.Width = fromWidth;
                window.Height = fromHeight;
                
                var startTime = DateTime.Now;
                var totalDuration = duration.TotalMilliseconds;
                
                // 使用120FPS的高频率更新
                while (DateTime.Now - startTime < duration && !cancellationToken.IsCancellationRequested)
                {
                    var elapsed = DateTime.Now - startTime;
                    var progress = Math.Min(elapsed.TotalMilliseconds / totalDuration, 1.0);
                    
                    // 使用CubicEaseOut缓动函数计算当前进度
                    var easedProgress = 1 - Math.Pow(1 - progress, 3);
                    
                    // 计算当前尺寸
                    var currentWidth = fromWidth + (toWidth - fromWidth) * easedProgress;
                    var currentHeight = fromHeight + (toHeight - fromHeight) * easedProgress;
                    
                    // 应用尺寸变化
                    window.Width = currentWidth;
                    window.Height = currentHeight;
                    
                    // 120FPS更新频率 (约8.33ms间隔)
                    await Task.Delay(1, cancellationToken);
                }
                
                // 确保最终尺寸
                window.Width = toWidth;
                window.Height = toHeight;
                
                Console.WriteLine("[AnimationService] 窗口尺寸动画完成");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[AnimationService] 窗口尺寸动画被取消");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnimationService] 窗口尺寸动画出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建组件缩放动画（使用RenderTransform）
        /// </summary>
        public async Task AnimateComponentScaleAsync(
            Visual control,
            double fromScale, double toScale,
            TimeSpan duration,
            float frequency = 2f,
            float damping = 0.4f,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 设置缩放中心点
                control.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                control.RenderTransform = new ScaleTransform(fromScale, fromScale);
                
                var startTime = DateTime.Now;
                var secondOrder = new SecondOrder(new Vec2((float)fromScale, (float)fromScale), frequency, damping);
                
                while (DateTime.Now - startTime < duration && !cancellationToken.IsCancellationRequested)
                {
                    var elapsed = DateTime.Now - startTime;
                    var progress = Math.Min((float)elapsed.TotalMilliseconds / (float)duration.TotalMilliseconds, 1.0f);
                    
                    // 使用SecondOrder系统计算当前缩放
                    var targetScale = new Vec2((float)toScale, (float)toScale);
                    var currentScale = secondOrder.Update(0.016f, targetScale); // 60FPS
                    
                    // 应用缩放变换
                    control.RenderTransform = new ScaleTransform(currentScale.X, currentScale.Y);
                    
                    await Task.Delay(16, cancellationToken); // 约60FPS
                }
                
                // 确保最终缩放
                control.RenderTransform = new ScaleTransform(toScale, toScale);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[AnimationService] 组件缩放动画被取消");
            }
        }

        /// <summary>
        /// 创建透明度动画（使用自定义缓动函数）
        /// </summary>
        public async Task AnimateOpacityAsync(
            Visual control,
            double fromOpacity, double toOpacity,
            TimeSpan duration,
            EasingType easingType = EasingType.CubicEaseOut,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var startTime = DateTime.Now;
                
                while (DateTime.Now - startTime < duration && !cancellationToken.IsCancellationRequested)
                {
                    var elapsed = DateTime.Now - startTime;
                    var progress = Math.Min(elapsed.TotalMilliseconds / duration.TotalMilliseconds, 1.0);
                    
                    // 使用自定义缓动函数计算当前透明度
                    var easedProgress = CalculateEasedProgress((float)progress, easingType);
                    var currentOpacity = fromOpacity + (toOpacity - fromOpacity) * easedProgress;
                    
                    // 应用透明度
                    control.Opacity = currentOpacity;
                    
                    await Task.Delay(16, cancellationToken); // 约60FPS
                }
                
                // 确保最终透明度
                control.Opacity = toOpacity;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[AnimationService] 透明度动画被取消");
            }
        }

        /// <summary>
        /// 创建位移动画（使用SecondOrder物理模型）
        /// </summary>
        public async Task AnimateTranslationAsync(
            Visual control,
            double fromX, double toX,
            double fromY, double toY,
            TimeSpan duration,
            float frequency = 2f,
            float damping = 0.4f,
            CancellationToken cancellationToken = default)
        {
            try
            {
                control.RenderTransform = new TranslateTransform(fromX, fromY);
                
                var startTime = DateTime.Now;
                var secondOrder = new SecondOrder(new Vec2((float)fromX, (float)fromY), frequency, damping);
                
                while (DateTime.Now - startTime < duration && !cancellationToken.IsCancellationRequested)
                {
                    var elapsed = DateTime.Now - startTime;
                    var progress = Math.Min((float)elapsed.TotalMilliseconds / (float)duration.TotalMilliseconds, 1.0f);
                    
                    // 使用SecondOrder系统计算当前位置
                    var targetPosition = new Vec2((float)toX, (float)toY);
                    var currentPosition = secondOrder.Update(0.016f, targetPosition); // 60FPS
                    
                    // 应用位移变换
                    control.RenderTransform = new TranslateTransform(currentPosition.X, currentPosition.Y);
                    
                    await Task.Delay(16, cancellationToken); // 约60FPS
                }
                
                // 确保最终位置
                control.RenderTransform = new TranslateTransform(toX, toY);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[AnimationService] 位移动画被取消");
            }
        }

        /// <summary>
        /// 创建旋转动画（使用SecondOrder物理模型）
        /// </summary>
        public async Task AnimateRotationAsync(
            Visual control,
            double fromAngle, double toAngle,
            TimeSpan duration,
            float frequency = 2f,
            float damping = 0.4f,
            CancellationToken cancellationToken = default)
        {
            try
            {
                control.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                control.RenderTransform = new RotateTransform(fromAngle);
                
                var startTime = DateTime.Now;
                var secondOrder = new SecondOrder(new Vec2((float)fromAngle, 0), frequency, damping);
                
                while (DateTime.Now - startTime < duration && !cancellationToken.IsCancellationRequested)
                {
                    var elapsed = DateTime.Now - startTime;
                    var progress = Math.Min((float)elapsed.TotalMilliseconds / (float)duration.TotalMilliseconds, 1.0f);
                    
                    // 使用SecondOrder系统计算当前角度
                    var targetAngle = new Vec2((float)toAngle, 0);
                    var currentAngle = secondOrder.Update(0.016f, targetAngle); // 60FPS
                    
                    // 应用旋转变换
                    control.RenderTransform = new RotateTransform(currentAngle.X);
                    
                    await Task.Delay(16, cancellationToken); // 约60FPS
                }
                
                // 确保最终角度
                control.RenderTransform = new RotateTransform(toAngle);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[AnimationService] 旋转动画被取消");
            }
        }

        /// <summary>
        /// 计算缓动进度
        /// </summary>
        private float CalculateEasedProgress(float progress, EasingType easingType)
        {
            return easingType switch
            {
                EasingType.Linear => progress,
                EasingType.CubicEaseIn => Easings.EaseInCubic(progress),
                EasingType.CubicEaseOut => Easings.EaseOutCubic(progress),
                EasingType.EaseInQuint => Easings.EaseInQuint(progress),
                EasingType.EaseOutQuint => Easings.EaseOutQuint(progress),
                EasingType.EaseInSin => Easings.EaseInSin(progress),
                EasingType.EaseOutSin => Easings.EaseOutSin(progress),
                _ => Easings.EaseOutCubic(progress)
            };
        }

    }
}
