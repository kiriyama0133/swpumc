using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace swpumc.Utils.animation;

/// <summary>
/// 二阶系统物理模型类
/// 实现了一个二阶微分方程的数值求解器，用于模拟弹簧-阻尼-质量系统
/// 常用于UI动画中的平滑过渡效果，如弹性动画、缓动动画等
/// 
/// 物理模型：m * d²y/dt² + c * dy/dt + k * y = k * x
/// 其中：m是质量，c是阻尼系数，k是弹簧刚度，x是输入，y是输出
/// </summary>
public class SecondOrder
{
    /// <summary>
    /// 前一个输入值，用于计算输入速度
    /// </summary>
    private Vec2 xp;

    /// <summary>
    /// 当前输出位置
    /// </summary>
    private Vec2 y;

    /// <summary>
    /// 当前输出速度
    /// </summary>
    private Vec2 yd;

    /// <summary>
    /// 系统参数k1：与阻尼系数相关的参数
    /// k1 = ζ / (π * f)，其中ζ是阻尼比，f是自然频率
    /// </summary>
    private float k1;

    /// <summary>
    /// 系统参数k2：与质量相关的参数
    /// k2 = 1 / ((2π * f)²)，控制系统的响应速度
    /// </summary>
    private float k2;

    /// <summary>
    /// 系统参数k3：与输入速度相关的参数
    /// k3 = r * ζ / (2π * f)，其中r是输入速度权重
    /// </summary>
    private float k3;

    /// <summary>
    /// 构造函数，初始化二阶系统
    /// </summary>
    /// <param name="x0">初始位置</param>
    /// <param name="f">自然频率（Hz），控制系统的响应速度，值越大响应越快</param>
    /// <param name="z">阻尼比，控制系统的振荡程度，0-1之间，1为临界阻尼</param>
    /// <param name="r">输入速度权重，控制输入速度对输出的影响程度</param>
    public SecondOrder(Vec2 x0, float f = 2f, float z = 0.4f, float r = 0.1f)
    {
        // 计算系统参数
        k1 = (float)(z / (Math.PI * f));  // 阻尼相关参数
        k2 = (float)(1 / ((2 * Math.PI * f) * (2 * Math.PI * f)));  // 质量相关参数
        k3 = (float)(r * z / (2 * Math.PI * f));  // 输入速度相关参数

        // 初始化状态变量
        xp = x0;  // 前一个输入位置
        y = x0;   // 当前输出位置
        yd = new Vec2(0, 0);  // 当前输出速度（初始为0）
    }

    /// <summary>
    /// 设置系统参数，可以在运行时动态调整系统特性
    /// </summary>
    /// <param name="f">自然频率（Hz）</param>
    /// <param name="z">阻尼比</param>
    /// <param name="r">输入速度权重</param>
    public void SetValues(float f = 2f, float z = 0.4f, float r = 0.1f)
    {
        // 重新计算系统参数
        k1 = (float)(z / (Math.PI * f));
        k2 = (float)(1 / ((2 * Math.PI * f) * (2 * Math.PI * f)));
        k3 = (float)(r * z / (2 * Math.PI * f));
    }

    /// <summary>
    /// 更新系统状态，计算下一时刻的输出
    /// 使用数值积分方法求解二阶微分方程
    /// </summary>
    /// <param name="T">时间步长（秒）</param>
    /// <param name="x">当前输入位置</param>
    /// <param name="xd">输入速度（可选，如果不提供则自动计算）</param>
    /// <returns>更新后的输出位置</returns>
    public Vec2 Update(float T, Vec2 x, Vec2? xd = null)
    {

        // 如果没有提供输入速度，则根据位置变化计算
        if (xd != null)
        {
            xd = (x - xp) / new Vec2(T, T);  // 计算输入速度：速度 = 位置变化 / 时间
            xp = x;  // 更新前一个输入位置
        }

        // 计算稳定的k2值，确保数值积分的稳定性
        // 这是防止系统在大的时间步长下变得不稳定的重要措施
        float k2_stable = (float)Math.Max(k2, Math.Max(T * T / 2 + T * k1 / 2, T * k1));

        // 使用欧拉积分方法更新位置和速度
        // 位置更新：y = y + T * yd
        y = y + new Vec2(T, T) * yd;

        // 速度更新：yd = yd + T * 加速度
        // 加速度 = (x + k3 * xd - y - k1 * yd) / k2_stable
        yd = yd + T * (x + new Vec2(k3, k3) * xd - y - (k1 * yd)) / k2_stable;

        // 限制小数位数，避免浮点数精度问题
        y.X = Mathf.LimitDecimalPoints(y.X, 1);
        y.Y = Mathf.LimitDecimalPoints(y.Y, 1);

        return y;
    }
}