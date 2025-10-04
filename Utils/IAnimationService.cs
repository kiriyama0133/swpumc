using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace swpumc.Utils
{
    /// <summary>
    /// 动画服务接口，定义动画管理功能
    /// </summary>
    public interface IAnimationService
    {
        /// <summary>
        /// 创建组件尺寸变化动画（使用SecondOrder物理模型）
        /// </summary>
        /// <param name="control">目标控件</param>
        /// <param name="fromWidth">起始宽度</param>
        /// <param name="toWidth">结束宽度</param>
        /// <param name="fromHeight">起始高度</param>
        /// <param name="toHeight">结束高度</param>
        /// <param name="duration">动画持续时间</param>
        /// <param name="frequency">物理系统频率</param>
        /// <param name="damping">阻尼系数</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task AnimateComponentSizeAsync(
            Control control, 
            double fromWidth, double toWidth,
            double fromHeight, double toHeight,
            TimeSpan duration,
            float frequency = 2f,
            float damping = 0.4f,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 创建窗口尺寸变化动画（使用SecondOrder物理模型）
        /// </summary>
        /// <param name="window">目标窗口</param>
        /// <param name="fromWidth">起始宽度</param>
        /// <param name="toWidth">结束宽度</param>
        /// <param name="fromHeight">起始高度</param>
        /// <param name="toHeight">结束高度</param>
        /// <param name="duration">动画持续时间</param>
        /// <param name="frequency">物理系统频率</param>
        /// <param name="damping">阻尼系数</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task AnimateWindowSizeAsync(
            Window window,
            double fromWidth, double toWidth,
            double fromHeight, double toHeight,
            TimeSpan duration,
            float frequency = 2f,
            float damping = 0.4f,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 创建组件缩放动画（使用RenderTransform）
        /// </summary>
        /// <param name="control">目标控件</param>
        /// <param name="fromScale">起始缩放值</param>
        /// <param name="toScale">结束缩放值</param>
        /// <param name="duration">动画持续时间</param>
        /// <param name="frequency">物理系统频率</param>
        /// <param name="damping">阻尼系数</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task AnimateComponentScaleAsync(
            Visual control,
            double fromScale, double toScale,
            TimeSpan duration,
            float frequency = 2f,
            float damping = 0.4f,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 创建透明度动画（使用自定义缓动函数）
        /// </summary>
        /// <param name="control">目标控件</param>
        /// <param name="fromOpacity">起始透明度</param>
        /// <param name="toOpacity">结束透明度</param>
        /// <param name="duration">动画持续时间</param>
        /// <param name="easingType">缓动类型</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task AnimateOpacityAsync(
            Visual control,
            double fromOpacity, double toOpacity,
            TimeSpan duration,
            EasingType easingType = EasingType.CubicEaseOut,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 创建位移动画（使用SecondOrder物理模型）
        /// </summary>
        /// <param name="control">目标控件</param>
        /// <param name="fromX">起始X坐标</param>
        /// <param name="toX">结束X坐标</param>
        /// <param name="fromY">起始Y坐标</param>
        /// <param name="toY">结束Y坐标</param>
        /// <param name="duration">动画持续时间</param>
        /// <param name="frequency">物理系统频率</param>
        /// <param name="damping">阻尼系数</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task AnimateTranslationAsync(
            Visual control,
            double fromX, double toX,
            double fromY, double toY,
            TimeSpan duration,
            float frequency = 2f,
            float damping = 0.4f,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 创建旋转动画（使用SecondOrder物理模型）
        /// </summary>
        /// <param name="control">目标控件</param>
        /// <param name="fromAngle">起始角度</param>
        /// <param name="toAngle">结束角度</param>
        /// <param name="duration">动画持续时间</param>
        /// <param name="frequency">物理系统频率</param>
        /// <param name="damping">阻尼系数</param>
        /// <param name="cancellationToken">取消令牌</param>
        Task AnimateRotationAsync(
            Visual control,
            double fromAngle, double toAngle,
            TimeSpan duration,
            float frequency = 2f,
            float damping = 0.4f,
            CancellationToken cancellationToken = default);
    }
}