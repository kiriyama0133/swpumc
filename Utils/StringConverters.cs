using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia;

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
        
        /// <summary>
        /// 布尔值到背景色的转换器
        /// </summary>
        public static readonly BooleanToBackgroundConverter BooleanToBackgroundConverter = new();
        
        /// <summary>
        /// 布尔值到前景色的转换器
        /// </summary>
        public static readonly BooleanToForegroundConverter BooleanToForegroundConverter = new();
        
        /// <summary>
        /// 布尔值到边框厚度的转换器
        /// </summary>
        public static readonly BooleanToBorderThicknessConverter BooleanToBorderThicknessConverter = new();
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

    /// <summary>
    /// 布尔值到背景色的转换器
    /// </summary>
    public sealed class BooleanToBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return isSelected ? 
                    new SolidColorBrush(Color.FromRgb(33, 150, 243)) : // 选中时的蓝色背景
                    new SolidColorBrush(Color.FromRgb(45, 45, 45));    // 未选中时的深色背景
            }
            return new SolidColorBrush(Color.FromRgb(45, 45, 45));
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("BooleanToBackgroundConverter不支持反向转换");
        }
    }

    /// <summary>
    /// 布尔值到前景色的转换器
    /// </summary>
    public sealed class BooleanToForegroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return isSelected ? 
                    Brushes.White : // 选中时的白色前景
                    new SolidColorBrush(Color.FromRgb(200, 200, 200)); // 未选中时的浅色前景
            }
            return new SolidColorBrush(Color.FromRgb(200, 200, 200));
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("BooleanToForegroundConverter不支持反向转换");
        }
    }

    /// <summary>
    /// 布尔值到边框厚度的转换器
    /// </summary>
    public sealed class BooleanToBorderThicknessConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return isSelected ? 
                    new Thickness(0) : // 选中时无边框
                    new Thickness(1);  // 未选中时有边框
            }
            return new Thickness(1);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("BooleanToBorderThicknessConverter不支持反向转换");
        }
    }
}
