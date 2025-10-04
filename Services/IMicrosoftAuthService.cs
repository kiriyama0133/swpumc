using System;
using System.Threading.Tasks;
using swpumc.Models;

namespace swpumc.Services
{
    /// <summary>
    /// 微软认证服务接口
    /// </summary>
    public interface IMicrosoftAuthService
    {
        /// <summary>
        /// 开始设备流认证
        /// </summary>
        /// <param name="onDeviceCodeReceived">设备代码接收回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>认证结果</returns>
        Task<MicrosoftAuthResult> StartDeviceFlowAsync(Action<DeviceCodeResponse> onDeviceCodeReceived, System.Threading.CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 刷新访问令牌
        /// </summary>
        /// <param name="refreshToken">刷新令牌</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>认证结果</returns>
        Task<MicrosoftAuthResult> RefreshTokenAsync(string refreshToken, System.Threading.CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// 微软认证结果
    /// </summary>
    public class MicrosoftAuthResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string PlayerUuid { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

}
