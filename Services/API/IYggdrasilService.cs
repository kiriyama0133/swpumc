using System;
using System.Threading.Tasks;

namespace swpumc.Services.API
{
    /// <summary>
    /// Yggdrasil认证服务接口
    /// </summary>
    public interface IYggdrasilService
    {
        /// <summary>
        /// 使用LittleSkin进行Yggdrasil认证
        /// </summary>
        /// <param name="email">用户邮箱</param>
        /// <param name="password">用户密码</param>
        /// <returns>认证结果，包含用户信息和访问令牌</returns>
        Task<YggdrasilAuthResult?> AuthenticateAsync(string email, string password);

        /// <summary>
        /// 验证访问令牌是否有效
        /// </summary>
        /// <param name="accessToken">访问令牌</param>
        /// <returns>是否有效</returns>
        Task<bool> ValidateTokenAsync(string accessToken);

        /// <summary>
        /// 刷新访问令牌
        /// </summary>
        /// <param name="refreshToken">刷新令牌</param>
        /// <returns>新的访问令牌</returns>
        Task<string?> RefreshTokenAsync(string refreshToken);
    }

    /// <summary>
    /// Yggdrasil认证结果
    /// </summary>
    public class YggdrasilAuthResult
    {
        public string AccessToken { get; set; } = string.Empty;
        public string ClientToken { get; set; } = string.Empty;
        public YggdrasilProfile[] AvailableProfiles { get; set; } = Array.Empty<YggdrasilProfile>();
        public YggdrasilProfile? SelectedProfile { get; set; }
        public YggdrasilUser? User { get; set; }
    }

    /// <summary>
    /// Yggdrasil用户角色
    /// </summary>
    public class YggdrasilProfile
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Legacy { get; set; }
    }

    /// <summary>
    /// Yggdrasil用户信息
    /// </summary>
    public class YggdrasilUser
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
