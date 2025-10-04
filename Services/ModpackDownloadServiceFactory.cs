using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace swpumc.Services
{
    /// <summary>
    /// 整合包下载服务工厂
    /// 负责创建和管理整合包下载服务实例
    /// </summary>
    public class ModpackDownloadServiceFactory
    {
        private static IModpackDownloadService? _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// 获取整合包下载服务实例（单例模式）
        /// </summary>
        /// <param name="minecraftFolder">Minecraft文件夹路径</param>
        /// <param name="javaPath">Java可执行文件路径</param>
        /// <returns>整合包下载服务实例</returns>
        public static IModpackDownloadService GetInstance(string minecraftFolder, string javaPath)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ModpackDownloadService(minecraftFolder, javaPath);
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// 创建新的整合包下载服务实例
        /// </summary>
        /// <param name="minecraftFolder">Minecraft文件夹路径</param>
        /// <param name="javaPath">Java可执行文件路径</param>
        /// <returns>新的整合包下载服务实例</returns>
        public static IModpackDownloadService Create(string minecraftFolder, string javaPath)
        {
            return new ModpackDownloadService(minecraftFolder, javaPath);
        }

        /// <summary>
        /// 重置单例实例（主要用于测试）
        /// </summary>
        public static void ResetInstance()
        {
            lock (_lock)
            {
                _instance = null;
            }
        }
    }
}
