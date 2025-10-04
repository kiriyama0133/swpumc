using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Material.Icons;
using swpumc.Models;
using swpumc.Pages;

namespace swpumc.Services
{
    public class NavigationService : INavigationService
    {
        private readonly List<NavigationPage> _availablePages;
        private NavigationPage? _activePage;
        
        public NavigationService()
        {
            _availablePages = new List<NavigationPage>();
            DiscoverPages();
        }
        
        public NavigationPage? ActivePage => _activePage;
        
        public event Action<Type>? NavigationRequested;
        
        public async Task NavigateToAsync(Type pageType)
        {
            var page = _availablePages.FirstOrDefault(p => p.PageType == pageType);
            if (page != null)
            {
                _activePage = page;
                NavigationRequested?.Invoke(pageType);
            }
            await Task.CompletedTask;
        }
        
        public async Task NavigateToAsync<T>() where T : class
        {
            await NavigateToAsync(typeof(T));
        }
        
        public IEnumerable<NavigationPage> GetAvailablePages()
        {
            return _availablePages.Where(p => p.IsVisible).OrderBy(p => p.Index);
        }
        
        private void DiscoverPages()
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            // 查找所有实现了 BaseViewModel 接口的类
            var viewModelTypes = assembly.GetTypes()
                .Where(t => t.GetInterfaces().Any(i => i.Name == "BaseViewModel"))
                .ToList();
            
            foreach (var viewModelType in viewModelTypes)
            {
                // 查找对应的页面类型
                var pageType = FindCorrespondingPageType(viewModelType);
                if (pageType != null)
                {
                    var page = CreateNavigationPage(viewModelType, pageType);
                    if (page != null)
                    {
                        _availablePages.Add(page);
                    }
                }
            }
        }
        
        private Type? FindCorrespondingPageType(Type viewModelType)
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            // 尝试找到对应的页面类型
            var pageTypeName = viewModelType.Name.Replace("ViewModel", "");
            var pageType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name == pageTypeName && 
                                 t.IsSubclassOf(typeof(BasePage)));
            
            return pageType;
        }
        
        private NavigationPage? CreateNavigationPage(Type viewModelType, Type pageType)
        {
            try
            {
                // 尝试从 ViewModel 获取导航信息
                var titleProperty = viewModelType.GetProperty("Title");
                var descriptionProperty = viewModelType.GetProperty("Description");
                var iconProperty = viewModelType.GetProperty("Icon");
                var indexProperty = viewModelType.GetProperty("Index");
                
                var title = titleProperty?.GetValue(null)?.ToString() ?? pageType.Name.Replace("Page", "");
                var description = descriptionProperty?.GetValue(null)?.ToString() ?? $"Navigate to {title}";
                var icon = iconProperty?.GetValue(null) as MaterialIconKind? ?? MaterialIconKind.Home;
                var index = indexProperty?.GetValue(null) as int? ?? 0;
                
                return new NavigationPage(title, description, icon, pageType, viewModelType, index);
            }
            catch
            {
                // 如果无法获取属性，使用默认值
                var title = pageType.Name.Replace("Page", "");
                return new NavigationPage(title, $"Navigate to {title}", MaterialIconKind.Home, pageType, viewModelType);
            }
        }
    }
}
