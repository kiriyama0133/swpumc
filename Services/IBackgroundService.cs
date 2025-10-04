using System;

namespace swpumc.Services
{
    public interface IBackgroundService
    {
        /// <summary>
        /// 当前背景图片路径
        /// </summary>
        string CurrentBackgroundPath { get; }
        
        /// <summary>
        /// 背景图片变化事件
        /// </summary>
        event EventHandler<string>? BackgroundChanged;
        
        /// <summary>
        /// 随机选择背景图片
        /// </summary>
        void SelectRandomBackground();
        
        /// <summary>
        /// 获取所有可用的背景图片
        /// </summary>
        string[] GetAvailableBackgrounds();
    }
}
