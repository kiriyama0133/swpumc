using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using swpumc.ViewModels;
using swpumc.Models;

namespace swpumc
{
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            // 如果是 NavigationPage，直接创建对应的页面
            if (param is NavigationPage navigationPage)
            {
                try
                {
                    return (Control)Activator.CreateInstance(navigationPage.PageType)!;
                }
                catch
                {
                    return new TextBlock { Text = "Failed to create: " + navigationPage.PageType.Name };
                }
            }

            // 如果是 ViewModel，尝试找到对应的 View
            if (param is ViewModelBase)
            {
                var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
                var type = Type.GetType(name);

                if (type != null)
                {
                    return (Control)Activator.CreateInstance(type)!;
                }

                return new TextBlock { Text = "Not Found: " + name };
            }

            return new TextBlock { Text = "Unknown type: " + param.GetType().Name };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase || data is NavigationPage;
        }
    }
}
