using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using swpumc.Models;
using swpumc.Services.API;

namespace swpumc.Services
{
    /// <summary>
    /// 头像管理服务实现
    /// </summary>
    public class AvatarManagementService : IAvatarManagementService
    {
        private readonly IAvatarApiService _avatarApiService;

        public AvatarManagementService(IAvatarApiService avatarApiService)
        {
            _avatarApiService = avatarApiService;
        }

        /// <summary>
        /// 获取玩家头像位图
        /// </summary>
        public async Task<Bitmap?> GetPlayerAvatarBitmapAsync(PlayerInfo player)
        {
            try
            {
                var avatarPath = GetPlayerAvatarPath(player);
                if (string.IsNullOrEmpty(avatarPath) || !File.Exists(avatarPath))
                {
                    return await GetDefaultAvatarBitmapAsync();
                }

                // 在后台线程加载位图
                return await Task.Run(() =>
                {
                    try
                    {
                        using var stream = File.OpenRead(avatarPath);
                        return new Bitmap(stream);
                    }
                    catch
                    {
                        return null;
                    }
                });
            }
            catch
            {
                return await GetDefaultAvatarBitmapAsync();
            }
        }

        /// <summary>
        /// 获取玩家头像路径
        /// </summary>
        public string? GetPlayerAvatarPath(PlayerInfo player)
        {
            if (player?.UserAccount == null) return null;
            
            var avatarDir = GetAvatarDirectory(player.UserAccount.Nickname);
            
            // 按优先级查找头像文件
            var possiblePaths = new[]
            {
                Path.Combine(avatarDir, $"{player.UserAccount.Nickname}_user_avatar.png"),  // 用户头像
                Path.Combine(avatarDir, $"{player.UserAccount.Nickname}_avatar_2d.png"),     // 2D头像
                Path.Combine(avatarDir, $"{player.UserAccount.Nickname}_avatar_3d.png"),    // 3D头像
                Path.Combine(avatarDir, "avatar.png")                          // 默认头像
            };
            
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            
            return null;
        }

        /// <summary>
        /// 刷新指定玩家的头像
        /// </summary>
        public async Task RefreshPlayerAvatarAsync(PlayerInfo player)
        {
            try
            {
                // 检查头像是否已存在
                var existingPath = GetPlayerAvatarPath(player);
                if (!string.IsNullOrEmpty(existingPath))
                {
                    return;
                }

                // 下载头像
                await DownloadUserAvatarsAsync(player.UserAccount);
            }
            catch
            {
                // 忽略错误，使用默认头像
            }
        }

        /// <summary>
        /// 下载用户头像
        /// </summary>
        public async Task DownloadUserAvatarsAsync(UserAccount userAccount)
        {
            try
            {
                // 创建头像保存目录
                var avatarDir = GetAvatarDirectory(userAccount.Nickname);
                if (!Directory.Exists(avatarDir))
                {
                    Directory.CreateDirectory(avatarDir);
                }

                var tasks = new List<Task<bool>>();

                // 下载用户头像 (用于显示)
                var userAvatarPath = Path.Combine(avatarDir, $"{userAccount.Nickname}_user_avatar.png");
                tasks.Add(DownloadUserAvatarAsync(userAccount.Uid.ToString(), userAvatarPath));

                // 只下载第一个角色的2D和3D头像
                if (userAccount.Profiles?.Any() == true)
                {
                    var firstProfile = userAccount.Profiles.First();
                    var playerAvatar2D = Path.Combine(avatarDir, $"{firstProfile.Name}_avatar_2d.png");
                    var playerAvatar3D = Path.Combine(avatarDir, $"{firstProfile.Name}_avatar_3d.png");
                    
                    tasks.Add(DownloadPlayerAvatarAsync(firstProfile.Name, playerAvatar2D, false));
                    tasks.Add(DownloadPlayerAvatarAsync(firstProfile.Name, playerAvatar3D, true));
                }

                // 等待所有下载完成
                await Task.WhenAll(tasks);
            }
            catch
            {
                // 忽略错误
            }
        }

        /// <summary>
        /// 获取默认头像位图
        /// </summary>
        public async Task<Bitmap?> GetDefaultAvatarBitmapAsync()
        {
            try
            {
                var defaultAvatarPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "IMG", "Avatars", "default.png");
                if (File.Exists(defaultAvatarPath))
                {
                    return await Task.Run(() =>
                    {
                        try
                        {
                            using var stream = File.OpenRead(defaultAvatarPath);
                            return new Bitmap(stream);
                        }
                        catch
                        {
                            return null;
                        }
                    });
                }
            }
            catch
            {
                // 忽略错误
            }
            
            return null;
        }

        #region 私有方法

        /// <summary>
        /// 获取用户头像保存目录
        /// </summary>
        public string GetAvatarDirectory(string nickname)
        {
            // 先检查当前目录
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            
#if DEBUG
            // Debug模式：优先查找Release目录的头像文件
            var releaseDir = Path.Combine(currentDir.Replace("Debug", "Release"), "Assets", "Avatars", nickname);
            if (Directory.Exists(releaseDir))
            {
                return releaseDir;
            }
            
            // 如果Release目录不存在，使用Debug目录
            var debugDir = Path.Combine(currentDir, "Assets", "Avatars", nickname);
            return debugDir;
#else
            // Release模式：使用当前目录
            return Path.Combine(currentDir, "Assets", "Avatars", nickname);
#endif
        }

        /// <summary>
        /// 下载用户头像到本地文件
        /// </summary>
        private async Task<bool> DownloadUserAvatarAsync(string uid, string savePath)
        {
            try
            {
                // 创建临时UserAccount对象
                var userAccount = new UserAccount { Uid = int.Parse(uid) };
                
                // 获取用户头像数据
                var avatarData = await _avatarApiService.GetUserAvatarDataAsync(userAccount);
                if (avatarData == null)
                {
                    return false;
                }

                // 保存到文件
                await File.WriteAllBytesAsync(savePath, avatarData);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 下载玩家头像到本地文件
        /// </summary>
        private async Task<bool> DownloadPlayerAvatarAsync(string playerName, string savePath, bool is3D)
        {
            try
            {
                // 获取头像数据
                byte[]? avatarData;
                if (is3D)
                {
                    avatarData = await _avatarApiService.GetPlayer3DAvatarDataAsync(playerName);
                }
                else
                {
                    avatarData = await _avatarApiService.GetPlayer2DAvatarDataAsync(playerName);
                }

                if (avatarData == null)
                {
                    return false;
                }

                // 保存到文件
                await File.WriteAllBytesAsync(savePath, avatarData);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
