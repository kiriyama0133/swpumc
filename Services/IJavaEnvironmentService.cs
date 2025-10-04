using System.Collections.Generic;
using System.Threading.Tasks;

namespace swpumc.Services
{
    /// <summary>
    /// Java环境检测服务接口
    /// </summary>
    public interface IJavaEnvironmentService
    {
        /// <summary>
        /// 获取所有检测到的Java安装
        /// </summary>
        IReadOnlyList<JavaInstallation> JavaInstallations { get; }

        /// <summary>
        /// 初始化Java环境并保存到全局配置
        /// </summary>
        Task InitializeJavaEnvironmentsAsync(IConfigService configService);

        /// <summary>
        /// 检测所有Java环境
        /// </summary>
        Task<List<JavaInstallation>> DetectJavaEnvironmentsAsync();

        /// <summary>
        /// 获取Java可执行文件路径
        /// </summary>
        string GetJavaExecutablePath(string javaHome);
    }
}