using System;
using System.Text.Json.Serialization;

namespace swpumc.Models
{
    /// <summary>
    /// 登录请求模型
    /// </summary>
    public class LoginRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// 登录响应模型
    /// </summary>
    public class LoginResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// 刷新Token响应模型
    /// </summary>
    public class RefreshTokenResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// 认证状态模型
    /// </summary>
    public class AuthState
    {
        public bool IsAuthenticated { get; set; } = false;
        public string Token { get; set; } = string.Empty;
        public DateTime TokenExpiry { get; set; } = DateTime.MinValue;
        public UserModel? User { get; set; }
    }
}
