using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using swpumc.Services;

namespace swpumc.Services
{
    /// <summary>
    /// 认证HTTP委托处理器
    /// 自动在请求头中添加access_token进行鉴权
    /// </summary>
    public class AuthenticationHttpHandler : DelegatingHandler
    {
        private readonly IConfigService _configService;
        private const string AuthorizationHeader = "Authorization";
        private const string BearerPrefix = "Bearer ";

        public AuthenticationHttpHandler(IConfigService configService) : base(new HttpClientHandler())
        {
            _configService = configService;
        }

        public AuthenticationHttpHandler(HttpMessageHandler innerHandler, IConfigService configService) 
            : base(innerHandler)
        {
            _configService = configService;
        }

        /// <summary>
        /// 处理HTTP请求，自动添加认证头
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                // 获取当前用户的access token
                var accessToken = GetCurrentUserAccessToken();
                
                // 只有在有有效token且不是登录相关请求时才添加认证头
                if (!string.IsNullOrEmpty(accessToken) && !IsLoginRequest(request))
                {
                    // 添加Authorization头
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    Console.WriteLine($"[AuthenticationHttpHandler] 已添加认证头到请求: {request.RequestUri}");
                }
                else if (IsLoginRequest(request))
                {
                    Console.WriteLine($"[AuthenticationHttpHandler] 登录请求，跳过认证头添加: {request.RequestUri}");
                }
                else
                {
                    Console.WriteLine($"[AuthenticationHttpHandler] 未找到access token，跳过认证头添加: {request.RequestUri}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthenticationHttpHandler] 添加认证头时发生异常: {ex.Message}");
            }

            // 继续处理请求
            return await base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// 判断是否为登录相关请求
        /// </summary>
        private bool IsLoginRequest(HttpRequestMessage request)
        {
            var uri = request.RequestUri?.ToString().ToLower() ?? "";
            return uri.Contains("authenticate") || uri.Contains("login") || uri.Contains("auth");
        }

        /// <summary>
        /// 获取当前用户的access token
        /// </summary>
        private string? GetCurrentUserAccessToken()
        {
            try
            {
                var currentUser = _configService.GetCurrentUserAccount();
                return currentUser?.AccessToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthenticationHttpHandler] 获取用户token失败: {ex.Message}");
                return null;
            }
        }
    }

    /// <summary>
    /// 认证HTTP委托处理器工厂
    /// </summary>
    public static class AuthenticationHttpHandlerFactory
    {
        /// <summary>
        /// 创建带认证的HttpClient
        /// </summary>
        /// <param name="configService">配置服务</param>
        /// <param name="baseAddress">基础地址（可选）</param>
        /// <returns>配置了认证的HttpClient</returns>
        public static HttpClient CreateAuthenticatedHttpClient(IConfigService configService, string? baseAddress = null)
        {
            var handler = new AuthenticationHttpHandler(configService);
            var httpClient = new HttpClient(handler);
            
            if (!string.IsNullOrEmpty(baseAddress))
            {
                httpClient.BaseAddress = new Uri(baseAddress);
            }

            // 设置默认超时
            httpClient.Timeout = TimeSpan.FromMinutes(5);
            
            Console.WriteLine($"[AuthenticationHttpHandlerFactory] 创建认证HttpClient: {baseAddress ?? "无基础地址"}");
            return httpClient;
        }

        /// <summary>
        /// 创建带认证的HttpClient，使用现有的HttpMessageHandler
        /// </summary>
        /// <param name="innerHandler">内部处理器</param>
        /// <param name="configService">配置服务</param>
        /// <param name="baseAddress">基础地址（可选）</param>
        /// <returns>配置了认证的HttpClient</returns>
        public static HttpClient CreateAuthenticatedHttpClient(HttpMessageHandler innerHandler, IConfigService configService, string? baseAddress = null)
        {
            var handler = new AuthenticationHttpHandler(innerHandler, configService);
            var httpClient = new HttpClient(handler);
            
            if (!string.IsNullOrEmpty(baseAddress))
            {
                httpClient.BaseAddress = new Uri(baseAddress);
            }

            // 设置默认超时
            httpClient.Timeout = TimeSpan.FromMinutes(5);
            
            Console.WriteLine($"[AuthenticationHttpHandlerFactory] 创建认证HttpClient（使用现有处理器）: {baseAddress ?? "无基础地址"}");
            return httpClient;
        }
    }
}
