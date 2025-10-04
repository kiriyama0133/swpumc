using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace swpumc.Utils
{
    /// <summary>
    /// 字符串转换器集合，提供常用的字符串比较和转换功能
    /// </summary>
    public static class StringConverters
    {
        /// <summary>
        /// 字符串相等比较转换器
        /// </summary>
        public static readonly StringEqualityConverter Equal = new();
    }

    /// <summary>
    /// 字符串相等比较转换器
    /// </summary>
    public sealed class StringEqualityConverter : IValueConverter
    {
        /// <summary>
        /// 将字符串与参数进行比较，返回是否相等
        /// </summary>
        /// <param name="value">要比较的字符串值</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">比较参数</param>
        /// <param name="culture">文化信息</param>
        /// <returns>如果字符串与参数相等则返回true，否则返回false</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null && parameter == null)
                return true;
            
            if (value == null || parameter == null)
                return false;

            var stringValue = value.ToString();
            var stringParameter = parameter.ToString();

            return string.Equals(stringValue, stringParameter, StringComparison.OrdinalIgnoreCase);
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
            throw new NotSupportedException("StringEqualityConverter不支持反向转换");
        }
    }
}
