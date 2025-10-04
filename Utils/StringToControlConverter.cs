using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace swpumc.Utils
{
    /// <summary>
    /// 字符串转控件转换器，用于将字符串转换为简单的文本控件
    /// </summary>
    public sealed class StringToControlConverter : IValueConverter
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        public static readonly StringToControlConverter Instance = new();

        /// <summary>
        /// 将字符串转换为文本控件
        /// </summary>
        /// <param name="value">字符串值</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">参数</param>
        /// <param name="culture">文化信息</param>
        /// <returns>转换后的控件，失败时返回null</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 检查输入值是否为非空字符串
            if (value is not string text) return null;
            if (string.IsNullOrWhiteSpace(text)) return null;

            try
            {
                // 创建文本块控件
                var textBlock = new TextBlock
                {
                    Text = text,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                };

                return textBlock;
            }
            catch (Exception ex)
            {
                // 创建失败时记录错误并返回null
                Console.WriteLine($"[StringToControlConverter] 控件创建失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 不支持反向转换
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">参数</param>
        /// <param name="culture">文化信息</param>
        /// <returns>抛出异常</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("StringToControlConverter不支持反向转换");
        }
    }
}
