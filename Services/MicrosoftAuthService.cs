using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using swpumc.Services;
using swpumc.Models;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace swpumc.Services
{
    /// <summary>
    /// 微软认证服务实现
    /// </summary>
    public class MicrosoftAuthService : IMicrosoftAuthService
    {
        private readonly string _clientId;
        private readonly HttpClient _httpClient;
        private readonly string[] _scopes = { "XboxLive.signin", "offline_access", "openid", "profile", "email" };

        public MicrosoftAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // 使用 Microsoft 官方示例客户端 ID
            _clientId = "04b07795-8ddb-461a-bbee-02f9e1bf7b46";
            Console.WriteLine($"[MicrosoftAuthService] 初始化，ClientId: {_clientId}");
        }

        public async Task<MicrosoftAuthResult> StartDeviceFlowAsync(Action<DeviceCodeResponse> onDeviceCodeReceived, CancellationToken cancellationToken = default)
        {
            try
            {
                Console.WriteLine("[MicrosoftAuthService] 开始设备流认证");

                // 1. 获取设备代码
                var deviceCodeResponse = await GetDeviceCodeAsync(cancellationToken);
                Console.WriteLine($"[MicrosoftAuthService] 设备代码: {deviceCodeResponse.UserCode}");
                Console.WriteLine($"[MicrosoftAuthService] 验证URL: {deviceCodeResponse.VerificationUrl}");

                // 通知UI显示设备代码
                onDeviceCodeReceived?.Invoke(deviceCodeResponse);

                // 2. 轮询获取访问令牌
                var tokenResponse = await PollForTokenAsync(deviceCodeResponse, cancellationToken);
                if (tokenResponse == null)
                {
                    return new MicrosoftAuthResult
                    {
                        Success = false,
                        ErrorMessage = "获取访问令牌超时"
                    };
                }

                // 3. 获取 Minecraft 访问令牌和用户信息
                var minecraftResult = await GetMinecraftTokenAsync(tokenResponse.AccessToken, cancellationToken);
                if (!minecraftResult.Success)
                {
                    return minecraftResult;
                }

                // 4. 获取 Minecraft 用户档案
                var profileResult = await GetMinecraftProfileAsync(minecraftResult.AccessToken, cancellationToken);
                if (!profileResult.Success)
                {
                    return profileResult;
                }

                return new MicrosoftAuthResult
                {
                    Success = true,
                    AccessToken = minecraftResult.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    PlayerName = profileResult.PlayerName,
                    PlayerUuid = profileResult.PlayerUuid,
                    ExpiresAt = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MicrosoftAuthService] 设备流认证失败: {ex.Message}");
                return new MicrosoftAuthResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<MicrosoftAuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                Console.WriteLine("[MicrosoftAuthService] 刷新访问令牌");

                var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://login.live.com/oauth20_token.srf");
                var formData = new List<KeyValuePair<string, string>>
                {
                    new("client_id", _clientId),
                    new("refresh_token", refreshToken),
                    new("grant_type", "refresh_token")
                };
                request.Content = new FormUrlEncodedContent(formData);

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var tokenResponse = JsonSerializer.Deserialize<OAuth2TokenResponse>(json);

                if (tokenResponse?.AccessToken == null)
                {
                    return new MicrosoftAuthResult
                    {
                        Success = false,
                        ErrorMessage = "刷新令牌失败"
                    };
                }

                // 获取新的 Minecraft 访问令牌
                var minecraftResult = await GetMinecraftTokenAsync(tokenResponse.AccessToken, cancellationToken);
                if (!minecraftResult.Success)
                {
                    return minecraftResult;
                }

                // 获取 Minecraft 用户档案
                var profileResult = await GetMinecraftProfileAsync(minecraftResult.AccessToken, cancellationToken);
                if (!profileResult.Success)
                {
                    return profileResult;
                }

                return new MicrosoftAuthResult
                {
                    Success = true,
                    AccessToken = minecraftResult.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    PlayerName = profileResult.PlayerName,
                    PlayerUuid = profileResult.PlayerUuid,
                    ExpiresAt = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MicrosoftAuthService] 刷新令牌失败: {ex.Message}");
                return new MicrosoftAuthResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private async Task<DeviceCodeResponse> GetDeviceCodeAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"[MicrosoftAuthService] 请求设备代码，ClientId: {_clientId}");
                Console.WriteLine($"[MicrosoftAuthService] 请求范围: {string.Join(" ", _scopes)}");
                
                var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode");
                var formData = new List<KeyValuePair<string, string>>
                {
                    new("client_id", _clientId),
                    new("tenant", "/consumers"),
                    new("scope", string.Join(" ", _scopes))
                };
                request.Content = new FormUrlEncodedContent(formData);
                
                // 设置正确的Content-Type
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                
                Console.WriteLine($"[MicrosoftAuthService] 响应状态: {response.StatusCode}");
                Console.WriteLine($"[MicrosoftAuthService] 响应内容: {json}");
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"HTTP错误: {response.StatusCode} - {json}");
                }
                
                var deviceCodeResponse = JsonSerializer.Deserialize<DeviceCodeResponse>(json);
                if (deviceCodeResponse == null)
                {
                    throw new InvalidOperationException("设备代码响应反序列化失败");
                }
                
                Console.WriteLine($"[MicrosoftAuthService] 设备代码解析成功: {deviceCodeResponse.UserCode}");
                return deviceCodeResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MicrosoftAuthService] 获取设备代码失败: {ex.Message}");
                throw;
            }
        }

        private async Task<OAuth2TokenResponse?> PollForTokenAsync(DeviceCodeResponse deviceCode, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var timeout = TimeSpan.FromSeconds(deviceCode.ExpiresIn);

            while (stopwatch.Elapsed < timeout)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://login.microsoftonline.com/consumers/oauth2/v2.0/token");
                var formData = new List<KeyValuePair<string, string>>
                {
                    new("grant_type", "urn:ietf:params:oauth:grant-type:device_code"),
                    new("client_id", _clientId),
                    new("device_code", deviceCode.DeviceCode)
                };
                request.Content = new FormUrlEncodedContent(formData);

                try
                {
                    var response = await _httpClient.SendAsync(request, cancellationToken);
                    var json = await response.Content.ReadAsStringAsync(cancellationToken);
                    var jsonDoc = JsonDocument.Parse(json);

                    if (jsonDoc.RootElement.TryGetProperty("error", out var error))
                    {
                        var errorDescription = jsonDoc.RootElement.TryGetProperty("error_description", out var desc) ? desc.GetString() : "未知错误";
                        Console.WriteLine($"[MicrosoftAuthService] 轮询错误: {error.GetString()} - {errorDescription}");
                        
                        if (error.GetString() == "authorization_pending")
                        {
                            await Task.Delay(TimeSpan.FromSeconds(deviceCode.Interval), cancellationToken);
                            continue;
                        }
                        
                        return null;
                    }

                    if (jsonDoc.RootElement.TryGetProperty("access_token", out var accessToken))
                    {
                        Console.WriteLine("[MicrosoftAuthService] 成功获取访问令牌");
                        return JsonSerializer.Deserialize<OAuth2TokenResponse>(json);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MicrosoftAuthService] 轮询异常: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(deviceCode.Interval), cancellationToken);
            }

            return null;
        }

        private async Task<MicrosoftAuthResult> GetMinecraftTokenAsync(string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                // 1. 获取 Xbox Live 令牌
                var xblToken = await GetXBLTokenAsync(accessToken, cancellationToken);
                if (xblToken == null)
                {
                    return new MicrosoftAuthResult { Success = false, ErrorMessage = "获取 Xbox Live 令牌失败" };
                }

                // 2. 获取 Xbox Security Token Service 令牌
                var xstsToken = await GetXSTSTokenAsync(xblToken, cancellationToken);
                if (xstsToken == null)
                {
                    return new MicrosoftAuthResult { Success = false, ErrorMessage = "获取 Xbox STS 令牌失败" };
                }

                // 3. 获取 Minecraft 访问令牌
                var minecraftToken = await GetMinecraftAccessTokenAsync(xblToken, xstsToken, cancellationToken);
                if (minecraftToken == null)
                {
                    return new MicrosoftAuthResult { Success = false, ErrorMessage = "获取 Minecraft 访问令牌失败" };
                }

                return new MicrosoftAuthResult
                {
                    Success = true,
                    AccessToken = minecraftToken
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MicrosoftAuthService] 获取 Minecraft 令牌失败: {ex.Message}");
                return new MicrosoftAuthResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        private async Task<MicrosoftAuthResult> GetMinecraftProfileAsync(string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, "https://api.minecraftservices.com/minecraft/profile");
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var profile = JsonSerializer.Deserialize<MinecraftProfile>(json);

                if (profile?.Name == null || profile.Id == null)
                {
                    return new MicrosoftAuthResult { Success = false, ErrorMessage = "获取 Minecraft 用户档案失败" };
                }

                return new MicrosoftAuthResult
                {
                    Success = true,
                    PlayerName = profile.Name,
                    PlayerUuid = profile.Id
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MicrosoftAuthService] 获取 Minecraft 档案失败: {ex.Message}");
                return new MicrosoftAuthResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        private async Task<string?> GetXBLTokenAsync(string accessToken, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://user.auth.xboxlive.com/user/authenticate");
            var payload = new
            {
                Properties = new
                {
                    AuthMethod = "RPS",
                    SiteName = "user.auth.xboxlive.com",
                    RpsTicket = $"d={accessToken}"
                },
                RelyingParty = "http://auth.xboxlive.com",
                TokenType = "JWT"
            };

            request.Content = JsonContent.Create(payload);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonDoc = JsonDocument.Parse(json);

            return jsonDoc.RootElement.TryGetProperty("Token", out var token) ? token.GetString() : null;
        }

        private async Task<string?> GetXSTSTokenAsync(string xblToken, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://xsts.auth.xboxlive.com/xsts/authorize");
            var payload = new
            {
                Properties = new
                {
                    SandboxId = "RETAIL",
                    UserTokens = new[] { xblToken }
                },
                RelyingParty = "rp://api.minecraftservices.com/",
                TokenType = "JWT"
            };

            request.Content = JsonContent.Create(payload);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonDoc = JsonDocument.Parse(json);

            return jsonDoc.RootElement.TryGetProperty("Token", out var token) ? token.GetString() : null;
        }

        private async Task<string?> GetMinecraftAccessTokenAsync(string xblToken, string xstsToken, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://api.minecraftservices.com/authentication/login_with_xbox");
            
            // 从 XBL 令牌中提取 uhs
            var xblJson = JsonDocument.Parse(await GetXBLTokenJsonAsync(xblToken, cancellationToken));
            var uhs = xblJson.RootElement
                .GetProperty("DisplayClaims")
                .GetProperty("xui")[0]
                .GetProperty("uhs")
                .GetString();

            var payload = new { identityToken = $"XBL3.0 x={uhs};{xstsToken}" };
            request.Content = JsonContent.Create(payload);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonDoc = JsonDocument.Parse(json);

            return jsonDoc.RootElement.TryGetProperty("access_token", out var token) ? token.GetString() : null;
        }

        private async Task<string> GetXBLTokenJsonAsync(string xblToken, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://user.auth.xboxlive.com/user/authenticate");
            var payload = new
            {
                Properties = new
                {
                    AuthMethod = "RPS",
                    SiteName = "user.auth.xboxlive.com",
                    RpsTicket = $"d={xblToken}"
                },
                RelyingParty = "http://auth.xboxlive.com",
                TokenType = "JWT"
            };

            request.Content = JsonContent.Create(payload);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }

    // 辅助类
    public class MinecraftProfile
    {
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }
}
