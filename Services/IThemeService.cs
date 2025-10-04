using System;

namespace swpumc.Services
{
    public interface IThemeService
    {
        /// <summary>
        /// 当前是否为深色主题
        /// </summary>
        bool IsDarkTheme { get; }
        
        /// <summary>
        /// 主题切换事件
        /// </summary>
        event EventHandler<bool> ThemeChanged;
        
        /// <summary>
        /// 切换主题
        /// </summary>
        void ToggleTheme();
        
        /// <summary>
        /// 设置主题
        /// </summary>
        /// <param name="isDark">是否为深色主题</param>
        void SetTheme(bool isDark);
        
        /// <summary>
        /// 获取当前主题名称
        /// </summary>
        /// <returns>主题名称</returns>
        string GetCurrentThemeName();
        
        /// <summary>
        /// 强制刷新主题样式，解决Dialog按钮样式初始化问题
        /// </summary>
        void RefreshThemeStyles();
    }
}
