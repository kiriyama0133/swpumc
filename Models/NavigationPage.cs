using System;
using Material.Icons;

namespace swpumc.Models
{
    public class NavigationPage
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public MaterialIconKind Icon { get; set; } = MaterialIconKind.Home;
        public Type PageType { get; set; } = typeof(object);
        public Type ViewModelType { get; set; } = typeof(object);
        public int Index { get; set; } = 0;
        public bool IsVisible { get; set; } = true;
        
        public NavigationPage()
        {
        }
        
        public NavigationPage(string title, string description, MaterialIconKind icon, Type pageType, Type viewModelType, int index = 0)
        {
            Title = title;
            Description = description;
            Icon = icon;
            PageType = pageType;
            ViewModelType = viewModelType;
            Index = index;
        }
    }
}
