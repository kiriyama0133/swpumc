using System;
using Avalonia;
using Avalonia.Styling;
using SukiUI;

namespace swpumc.Services
{
    public class ThemeService : IThemeService
    {
        private readonly SukiTheme _sukiTheme;
        private bool _isDarkTheme = false;
        
        public bool IsDarkTheme 
        { 
            get => _isDarkTheme; 
            private set 
            {
                if (_isDarkTheme != value)
                {
                    _isDarkTheme = value;
                    ThemeChanged?.Invoke(this, value);
                }
            }
        }
        
        public event EventHandler<bool>? ThemeChanged;
        
        public ThemeService()
        {
            _sukiTheme = SukiTheme.GetInstance();
            
            // 订阅 SukiUI 主题变化事件
            _sukiTheme.OnBaseThemeChanged += OnSukiThemeChanged;
            
            // 初始化当前主题状态
            _isDarkTheme = _sukiTheme.ActiveBaseTheme == ThemeVariant.Dark;
        }
        
        private void OnSukiThemeChanged(ThemeVariant variant)
        {
            IsDarkTheme = variant == ThemeVariant.Dark;
        }
        
        public void ToggleTheme()
        {
            _sukiTheme.SwitchBaseTheme();
        }
        
        public void SetTheme(bool isDark)
        {
            var targetVariant = isDark ? ThemeVariant.Dark : ThemeVariant.Light;
            _sukiTheme.ChangeBaseTheme(targetVariant);
        }
        
        public string GetCurrentThemeName()
        {
            return IsDarkTheme ? "Dark" : "Light";
        }
        
        public void RefreshThemeStyles()
        {
            try
            {
                var currentTheme = _sukiTheme.ActiveBaseTheme;
                
                // 临时切换到相反主题，然后切换回原主题，强制触发样式刷新
                var oppositeTheme = currentTheme == ThemeVariant.Dark 
                    ? ThemeVariant.Light 
                    : ThemeVariant.Dark;
                
                _sukiTheme.ChangeBaseTheme(oppositeTheme);
                _sukiTheme.ChangeBaseTheme(currentTheme);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ThemeService] 主题刷新异常: {ex.Message}");
            }
        }
    }
}