using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace swpumc.Utils
{
    /// <summary>
    /// 动画管理器，提供统一的动画取消和状态管理
    /// </summary>
    public class AnimationManager
    {
        private static readonly ConcurrentDictionary<Control, CancellationTokenSource> _activeAnimations = new();
        private static readonly object _lock = new object();

        /// <summary>
        /// 开始动画并注册到管理器
        /// </summary>
        public static async Task StartAnimationAsync(Control control, Func<CancellationToken, Task> animationAction, string animationName = "")
        {
            // 取消该控件的现有动画
            CancelAnimation(control);

            // 创建新的取消令牌
            var cts = new CancellationTokenSource();
            _activeAnimations[control] = cts;

            try
            {
                Console.WriteLine($"[AnimationManager] 开始动画: {animationName} for {control.GetType().Name}");
                await animationAction(cts.Token);
                Console.WriteLine($"[AnimationManager] 动画完成: {animationName} for {control.GetType().Name}");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[AnimationManager] 动画被取消: {animationName} for {control.GetType().Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnimationManager] 动画出错: {animationName} for {control.GetType().Name}, 错误: {ex.Message}");
            }
            finally
            {
                // 清理
                _activeAnimations.TryRemove(control, out _);
                cts?.Dispose();
            }
        }

        /// <summary>
        /// 取消指定控件的动画
        /// </summary>
        public static void CancelAnimation(Control control)
        {
            if (_activeAnimations.TryGetValue(control, out var cts))
            {
                Console.WriteLine($"[AnimationManager] 取消动画 for {control.GetType().Name}");
                cts.Cancel();
                _activeAnimations.TryRemove(control, out _);
                cts.Dispose();
            }
        }

        /// <summary>
        /// 取消所有动画
        /// </summary>
        public static void CancelAllAnimations()
        {
            Console.WriteLine($"[AnimationManager] 取消所有动画，当前活跃动画数量: {_activeAnimations.Count}");
            
            var animations = new List<KeyValuePair<Control, CancellationTokenSource>>(_activeAnimations);
            foreach (var kvp in animations)
            {
                kvp.Value.Cancel();
                kvp.Value.Dispose();
            }
            _activeAnimations.Clear();
        }

        /// <summary>
        /// 检查控件是否有活跃的动画
        /// </summary>
        public static bool HasActiveAnimation(Control control)
        {
            return _activeAnimations.ContainsKey(control);
        }

        /// <summary>
        /// 获取当前活跃动画数量
        /// </summary>
        public static int ActiveAnimationCount => _activeAnimations.Count;

        /// <summary>
        /// 获取所有活跃动画的控件
        /// </summary>
        public static IEnumerable<Control> GetActiveAnimationControls()
        {
            return _activeAnimations.Keys;
        }
    }
}
