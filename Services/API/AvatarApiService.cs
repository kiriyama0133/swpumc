using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using swpumc.Services;
using swpumc.Models;

namespace swpumc.Services.API
{
    /// <summary>
    /// 头像和材质预览图API服务实现
    /// </summary>
    public class AvatarApiService : IAvatarApiService
    {
        private readonly IHttpService _httpService;
        private readonly IConfigService _configService;
        private readonly IHttpDownloadService _downloadService;

        public AvatarApiService(IHttpService httpService, IConfigService configService, IHttpDownloadService downloadService)
        {
            _httpService = httpService;
            _configService = configService;
            _downloadService = downloadService;
        }

        /// <summary>
        /// 获取API基础URL
        /// </summary>
        private string GetBaseUrl()
        {
            var userManagementConfig = _configService.GetUserManagementConfig();
            // 尝试使用LittleSkin的皮肤API端点
            // 根据Minecraft皮肤站的常见做法，头像API可能在根路径下
            return "https://littleskin.cn";
        }

        /// <summary>
        /// 根据角色名获取头像
        /// GET /avatar/player/{name}
        /// </summary>
        public async Task<byte[]?> GetPlayerAvatarAsync(string playerName, int? size = null, bool is3D = false, bool isPng = false)
        {
            try
            {
                var baseUrl = GetBaseUrl();
                var apiPath = $"/avatar/player/{Uri.EscapeDataString(playerName)}";
                
                var request = _httpService.CreateRequest()
                    .SetBaseUrl(baseUrl)
                    .SetApi(apiPath)
                    .SetMethod(Services.HttpMethod.GET);

                // 添加查询参数
                if (size.HasValue)
                    request = request.SetQueryParam("size", size.Value.ToString());
                if (is3D)
                    request = request.SetQueryParam("3d", "1");
                if (isPng)
                    request = request.SetQueryParam("png", "1");

                var response = await request.ExecuteBytesAsync();

                if (response.IsSuccess)
                {
                    Console.WriteLine($"[AvatarApiService] 成功获取角色头像: {playerName}");
                    return response.Data;
                }
                else
                {
                    Console.WriteLine($"[AvatarApiService] 获取角色头像失败: {response.ErrorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AvatarApiService] 获取角色头像异常: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 根据用户UID获取头像
        /// GET /avatar/user/{uid}
        /// </summary>
        public async Task<byte[]?> GetUserAvatarAsync(int uid, int? size = null, bool is3D = false, bool isPng = false)
        {
            try
            {
                var baseUrl = GetBaseUrl();
                var apiPath = $"/avatar/user/{uid}";
                
                var request = _httpService.CreateRequest()
                    .SetBaseUrl(baseUrl)
                    .SetApi(apiPath)
                    .SetMethod(Services.HttpMethod.GET);

                // 添加查询参数
                if (size.HasValue)
                    request = request.SetQueryParam("size", size.Value.ToString());
                if (is3D)
                    request = request.SetQueryParam("3d", "1");
                if (isPng)
                    request = request.SetQueryParam("png", "1");

                var response = await request.ExecuteBytesAsync();

                if (response.IsSuccess)
                {
                    Console.WriteLine($"[AvatarApiService] 成功获取用户头像: UID {uid}");
                    return response.Data;
                }
                else
                {
                    Console.WriteLine($"[AvatarApiService] 获取用户头像失败: {response.ErrorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AvatarApiService] 获取用户头像异常: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 根据材质TID生成头像
        /// GET /avatar/{tid}
        /// </summary>
        public async Task<byte[]?> GetTextureAvatarAsync(int tid, int? size = null, bool is3D = false, bool isPng = false)
        {
            try
            {
                var baseUrl = GetBaseUrl();
                var apiPath = $"avatar/{tid}";
                
                var request = _httpService.CreateRequest()
                    .SetBaseUrl(baseUrl)
                    .SetApi(apiPath)
                    .SetMethod(Services.HttpMethod.GET);

                // 添加查询参数
                if (size.HasValue)
                    request = request.SetQueryParam("size", size.Value.ToString());
                if (is3D)
                    request = request.SetQueryParam("3d", "1");
                if (isPng)
                    request = request.SetQueryParam("png", "1");

                var response = await request.ExecuteBytesAsync();

                if (response.IsSuccess)
                {
                    Console.WriteLine($"[AvatarApiService] 成功获取材质头像: TID {tid}");
                    return response.Data;
                }
                else
                {
                    Console.WriteLine($"[AvatarApiService] 获取材质头像失败: {response.ErrorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AvatarApiService] 获取材质头像异常: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 根据材质文件hash生成头像
        /// GET /avatar/hash/{hash}
        /// </summary>
        public async Task<byte[]?> GetTextureAvatarByHashAsync(string hash, int? size = null, bool is3D = false, bool isPng = false)
        {
            try
            {
                var baseUrl = GetBaseUrl();
                var apiPath = $"avatar/hash/{Uri.EscapeDataString(hash)}";
                
                var request = _httpService.CreateRequest()
                    .SetBaseUrl(baseUrl)
                    .SetApi(apiPath)
                    .SetMethod(Services.HttpMethod.GET);

                // 添加查询参数
                if (size.HasValue)
                    request = request.SetQueryParam("size", size.Value.ToString());
                if (is3D)
                    request = request.SetQueryParam("3d", "1");
                if (isPng)
                    request = request.SetQueryParam("png", "1");

                var response = await request.ExecuteBytesAsync();

                if (response.IsSuccess)
                {
                    Console.WriteLine($"[AvatarApiService] 成功获取材质头像: Hash {hash}");
                    return response.Data;
                }
                else
                {
                    Console.WriteLine($"[AvatarApiService] 获取材质头像失败: {response.ErrorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AvatarApiService] 获取材质头像异常: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 根据材质TID生成材质预览图
        /// GET /preview/{tid}
        /// </summary>
        public async Task<byte[]?> GetTexturePreviewAsync(int tid, bool isPng = false)
        {
            try
            {
                var baseUrl = GetBaseUrl();
                var apiPath = $"preview/{tid}";
                
                var request = _httpService.CreateRequest()
                    .SetBaseUrl(baseUrl)
                    .SetApi(apiPath)
                    .SetMethod(Services.HttpMethod.GET);

                // 添加查询参数
                if (isPng)
                    request = request.SetQueryParam("png", "1");

                var response = await request.ExecuteBytesAsync();

                if (response.IsSuccess)
                {
                    Console.WriteLine($"[AvatarApiService] 成功获取材质预览图: TID {tid}");
                    return response.Data;
                }
                else
                {
                    Console.WriteLine($"[AvatarApiService] 获取材质预览图失败: {response.ErrorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AvatarApiService] 获取材质预览图异常: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 根据材质文件hash生成材质预览图
        /// GET /preview/hash/{hash}
        /// </summary>
        public async Task<byte[]?> GetTexturePreviewByHashAsync(string hash, bool isPng = false)
        {
            try
            {
                var baseUrl = GetBaseUrl();
                var apiPath = $"preview/hash/{Uri.EscapeDataString(hash)}";
                
                var request = _httpService.CreateRequest()
                    .SetBaseUrl(baseUrl)
                    .SetApi(apiPath)
                    .SetMethod(Services.HttpMethod.GET);

                // 添加查询参数
                if (isPng)
                    request = request.SetQueryParam("png", "1");

                var response = await request.ExecuteBytesAsync();

                if (response.IsSuccess)
                {
                    Console.WriteLine($"[AvatarApiService] 成功获取材质预览图: Hash {hash}");
                    return response.Data;
                }
                else
                {
                    Console.WriteLine($"[AvatarApiService] 获取材质预览图失败: {response.ErrorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AvatarApiService] 获取材质预览图异常: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 下载玩家头像到本地文件
        /// </summary>
        /// <param name="playerName">玩家名称</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="size">头像大小</param>
        /// <param name="is3D">是否3D</param>
        /// <param name="isPng">是否PNG格式</param>
        /// <returns>是否成功</returns>
        public async Task<bool> DownloadPlayerAvatarAsync(string playerName, string savePath, int? size = null, bool is3D = false, bool isPng = false)
        {
            try
            {
                Console.WriteLine($"[AvatarApiService] 开始下载玩家头像: {playerName}");
                
                // 构建下载URL
                var baseUrl = GetBaseUrl();
                var apiPath = $"/avatar/player/{Uri.EscapeDataString(playerName)}";
                var url = $"{baseUrl}{apiPath}";
                
                // 添加查询参数
                var queryParams = new List<string>();
                if (size.HasValue)
                    queryParams.Add($"size={size.Value}");
                if (is3D)
                    queryParams.Add("3d=1");
                if (isPng)
                    queryParams.Add("png=1");
                
                if (queryParams.Count > 0)
                {
                    url += "?" + string.Join("&", queryParams);
                }

                Console.WriteLine($"[AvatarApiService] 下载URL: {url}");
                
                // 使用HttpDownloadService下载
                await _downloadService.DownloadFileAsync(url, savePath);
                
                Console.WriteLine($"[AvatarApiService] 玩家头像下载成功: {savePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AvatarApiService] 下载玩家头像失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 下载用户头像到本地文件
        /// </summary>
        /// <param name="uid">用户UID</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="size">头像大小</param>
        /// <param name="is3D">是否3D</param>
        /// <param name="isPng">是否PNG格式</param>
        /// <returns>是否成功</returns>
        public async Task<bool> DownloadUserAvatarAsync(int uid, string savePath, int? size = null, bool is3D = false, bool isPng = false)
        {
            try
            {
                Console.WriteLine($"[AvatarApiService] 开始下载用户头像: UID {uid}");
                
                // 构建下载URL
                var baseUrl = GetBaseUrl();
                var apiPath = $"/avatar/user/{uid}";
                var url = $"{baseUrl}{apiPath}";
                
                // 添加查询参数
                var queryParams = new List<string>();
                if (size.HasValue)
                    queryParams.Add($"size={size.Value}");
                if (is3D)
                    queryParams.Add("3d=1");
                if (isPng)
                    queryParams.Add("png=1");
                
                if (queryParams.Count > 0)
                {
                    url += "?" + string.Join("&", queryParams);
                }

                Console.WriteLine($"[AvatarApiService] 下载URL: {url}");
                
                // 使用HttpDownloadService下载
                await _downloadService.DownloadFileAsync(url, savePath);
                
                Console.WriteLine($"[AvatarApiService] 用户头像下载成功: {savePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AvatarApiService] 下载用户头像失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取用户头像数据（不涉及文件操作）
        /// </summary>
        public async Task<byte[]?> GetUserAvatarDataAsync(UserAccount userAccount)
        {
            if (userAccount == null) return null;
            
            Console.WriteLine($"[AvatarApiService] 获取用户头像数据: {userAccount.Nickname}");
            return await GetUserAvatarAsync(userAccount.Uid, 64, false, true);
        }

        /// <summary>
        /// 获取玩家2D头像数据（不涉及文件操作）
        /// </summary>
        public async Task<byte[]?> GetPlayer2DAvatarDataAsync(string playerName)
        {
            Console.WriteLine($"[AvatarApiService] 获取玩家2D头像数据: {playerName}");
            return await GetPlayerAvatarAsync(playerName, 64, false, true);
        }

        /// <summary>
        /// 获取玩家3D头像数据（不涉及文件操作）
        /// </summary>
        public async Task<byte[]?> GetPlayer3DAvatarDataAsync(string playerName)
        {
            Console.WriteLine($"[AvatarApiService] 获取玩家3D头像数据: {playerName}");
            return await GetPlayerAvatarAsync(playerName, 64, true, true);
        }
    }
}
