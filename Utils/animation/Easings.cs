using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace swpumc.Utils.animation;

/// <summary>
/// Easings - 缓动函数库
/// 
/// 提供各种动画缓动效果，用于创建自然流畅的动画。
/// 所有函数接受0.0-1.0的输入值，返回0.0-1.0的输出值。
/// 
/// 缓动类型说明：
/// - EaseIn: 缓入效果，开始时慢，结束时快
/// - EaseOut: 缓出效果，开始时快，结束时慢
/// - 函数名后缀表示缓动曲线类型（Quint、Cubic、Sin等）
/// 
/// 使用场景：
/// - UI动画的缓动效果
/// - 元素移动、缩放、透明度变化
/// - 菜单展开/收起动画
/// - 按钮点击反馈
/// </summary>
public class Easings
{
    public static float EaseInQuint(float x)
    {
        return x * x * x * x * x;
    }

    public static float EaseOutQuint(float x)
    {
        //if (1 - (float)Math.Pow(1 - x, 5) >= 0.975f) return 1f;
        return 1 - (float)Math.Pow(1 - x, 5);
    }

    public static float EaseOutSin(float x)
    {
        return (float)Math.Sin((x * Math.PI) / 2);
    }

    public static float EaseInSin(float x)
    {
        return 1 - (float)Math.Cos((x * Math.PI) / 2);
    }

    public static float EaseOutCubic(float x)
    {
        return 1 - (float)Math.Pow(1 - x, 3);
    }

    public static float EaseInCubic(float x)
    {
        return x * x * x;
    }
}
