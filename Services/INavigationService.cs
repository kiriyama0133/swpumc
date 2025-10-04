using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using swpumc.Models;

namespace swpumc.Services
{
    public interface INavigationService
    {
        /// <summary>
        /// 导航到指定页面
        /// </summary>
        /// <param name="pageType">页面类型</param>
        Task NavigateToAsync(Type pageType);
        
        /// <summary>
        /// 导航到指定页面
        /// </summary>
        /// <typeparam name="T">页面类型</typeparam>
        Task NavigateToAsync<T>() where T : class;
        
        /// <summary>
        /// 获取所有可用的页面
        /// </summary>
        IEnumerable<NavigationPage> GetAvailablePages();
        
        /// <summary>
        /// 当前活动页面
        /// </summary>
        NavigationPage? ActivePage { get; }
        
        /// <summary>
        /// 页面导航请求事件
        /// </summary>
        event Action<Type> NavigationRequested;
    }
}
