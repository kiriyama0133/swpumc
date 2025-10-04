using System;
using System.Threading.Tasks;
using swpumc.Services;

namespace swpumc.Services.API
{
    /// <summary>
    /// Yggdrasil认证服务实现
    /// </summary>
    public class YggdrasilService : IYggdrasilService
    {
        private readonly IHttpService _httpService;
        private const string BaseUrl = "https://littleskin.cn";

        public YggdrasilService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        /// <summary>
        /// 使用LittleSkin进行Yggdrasil认证
        /// </summary>
        public async Task<YggdrasilAuthResult?> AuthenticateAsync(string email, string password)
        {
            try
            {
                // 构建Yggdrasil认证请求
                var authRequest = new
                {
                    agent = new
                    {
                        name = "Minecraft",
                        version = 1
                    },
                    username = email,
                    password = password,
                    clientToken = Guid.NewGuid().ToString()
                };

                Console.WriteLine($"[YggdrasilService] 发送认证请求到: {BaseUrl}/api/yggdrasil/authserver/authenticate");

                // 发送认证请求
                var response = await _httpService
                    .CreateRequest()
                    .SetBaseUrl(BaseUrl)
                    .SetApi("api/yggdrasil/authserver/authenticate")
                    .SetMethod(Services.HttpMethod.POST)
                    .SetData(authRequest)
                    .ExecuteAsync<YggdrasilAuthResult>();

                if (response.IsSuccess && response.Data != null)
                {
                    Console.WriteLine($"[YggdrasilService] 认证成功，AccessToken: {response.Data.AccessToken[..20]}...");
                    Console.WriteLine($"[YggdrasilService] 用户角色: {response.Data.SelectedProfile?.Name}");
                    
                    // 直接返回结果
                    return response.Data;
                }
                else
                {
                    Console.WriteLine($"[YggdrasilService] 认证失败: {response.ErrorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[YggdrasilService] 认证异常: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 验证访问令牌是否有效
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string accessToken)
        {
            try
            {
                var response = await _httpService
                    .CreateRequest()
                    .SetBaseUrl(BaseUrl)
                    .SetApi("api/yggdrasil/authserver/validate")
                    .SetMethod(Services.HttpMethod.POST)
                    .SetData(new { accessToken })
                    .ExecuteAsync<object>();

                return response.IsSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[YggdrasilService] 验证令牌异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 刷新访问令牌
        /// </summary>
        public async Task<string?> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var response = await _httpService
                    .CreateRequest()
                    .SetBaseUrl(BaseUrl)
                    .SetApi("api/yggdrasil/authserver/refresh")
                    .SetMethod(Services.HttpMethod.POST)
                    .SetData(new { accessToken = refreshToken })
                    .ExecuteAsync<YggdrasilAuthResult>();

                if (response.IsSuccess && response.Data != null)
                {
                    Console.WriteLine($"[YggdrasilService] 令牌刷新成功");
                    return response.Data.AccessToken;
                }
                else
                {
                    Console.WriteLine($"[YggdrasilService] 令牌刷新失败: {response.ErrorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[YggdrasilService] 刷新令牌异常: {ex.Message}");
                return null;
            }
        }
    }
}
