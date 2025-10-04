using System.Threading.Tasks;
using swpumc.Models;
using swpumc.Services.API;

namespace swpumc.Services
{
    /// <summary>
    /// 用户信息管理服务接口
    /// </summary>
    public interface IUserInfoService
    {
        /// <summary>
        /// 保存用户信息到配置文件
        /// </summary>
        /// <param name="authResult">认证结果</param>
        /// <param name="email">用户邮箱</param>
        /// <returns>是否保存成功</returns>
        Task<bool> SaveUserInfoAsync(YggdrasilAuthResult authResult, string email);

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        /// <returns>用户信息</returns>
        UserModel? GetCurrentUser();

        /// <summary>
        /// 清除用户信息
        /// </summary>
        Task<bool> ClearUserInfoAsync();

        /// <summary>
        /// 检查用户是否已登录
        /// </summary>
        /// <returns>是否已登录</returns>
        bool IsUserLoggedIn();
    }
}
