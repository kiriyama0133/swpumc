using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace swpumc.Utils
{
    /// <summary>
    /// Markdown转控件转换器，用于将Markdown字符串转换为Avalonia控件
    /// </summary>
    public sealed class MarkdownToControlConverter : IValueConverter
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        public static readonly MarkdownToControlConverter Instance = new();


        /// <summary>
        /// 将Markdown字符串转换为Avalonia控件
        /// </summary>
        /// <param name="value">Markdown字符串</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">参数（可选，格式：maxWidth,maxHeight）</param>
        /// <param name="culture">文化信息</param>
        /// <returns>转换后的控件，失败时返回null</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 检查输入值是否为非空字符串
            if (value is not string markdownText) return null;
            if (string.IsNullOrWhiteSpace(markdownText)) return null;

            try
            {
                // 解析参数（如果有的话）
                double? maxWidth = null;
                double? maxHeight = null;

                if (parameter is string paramString && !string.IsNullOrWhiteSpace(paramString))
                {
                    var parts = paramString.Split(',');
                    if (parts.Length >= 1 && double.TryParse(parts[0], out var width))
                    {
                        maxWidth = width;
                    }
                    if (parts.Length >= 2 && double.TryParse(parts[1], out var height))
                    {
                        maxHeight = height;
                    }
                }

                // 使用AST渲染器渲染Markdown
                return MarkdownRenderer.RenderMarkdownWithAST(markdownText, maxWidth, maxHeight);
            }
            catch (Exception ex)
            {
                // 渲染失败时记录错误并返回错误信息控件
                Console.WriteLine($"[MarkdownToControlConverter] Markdown转换失败: {ex.Message}");
                
                return new TextBlock
                {
                    Text = $"Markdown渲染失败: {ex.Message}",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Foreground = Avalonia.Media.Brushes.Red
                };
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
            throw new NotSupportedException("MarkdownToControlConverter不支持反向转换");
        }
    }
}
