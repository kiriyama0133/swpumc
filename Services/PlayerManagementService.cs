using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using swpumc.Models;
using swpumc.Services.API;

namespace swpumc.Services
{
    /// <summary>
    /// 玩家管理服务实现
    /// </summary>
    public class PlayerManagementService : IPlayerManagementService
    {
        private readonly IConfigService _configService;
        private readonly IAvatarManagementService _avatarManagementService;
        private PlayerInfo? _selectedPlayer;
        private bool _isLoading = false;

        public ObservableCollection<PlayerInfo> Players { get; } = new();
        public bool IsLoading => _isLoading;

        public PlayerInfo? SelectedPlayer
        {
            get => _selectedPlayer;
            set
            {
                if (_selectedPlayer != value)
                {
                    _selectedPlayer = value;
                    OnSelectedPlayerChanged?.Invoke(this, value);
                }
            }
        }

        public bool HasSelectedPlayer => SelectedPlayer != null;

        public event EventHandler<PlayerInfo?>? OnSelectedPlayerChanged;

        public PlayerManagementService(IConfigService configService, IAvatarManagementService avatarManagementService)
        {
            _configService = configService;
            _avatarManagementService = avatarManagementService;
        }

        /// <summary>
        /// 加载玩家列表
        /// </summary>
        public async Task LoadPlayersAsync()
        {
            try
            {
                _isLoading = true;
                Players.Clear();

                var userManagementConfig = _configService.GetUserManagementConfig();
                
                if (userManagementConfig.Users?.Any() == true)
                {
                    foreach (var user in userManagementConfig.Users)
                    {
                        // 加载所有用户（包括第三方用户和离线用户）
                        var playerInfo = new PlayerInfo
                        {
                            UserAccount = user
                        };
                        // 只设置基本信息，头像路径延迟获取
                        playerInfo.LoginStatus = "未登录";
                        playerInfo.LoginStatusColor = Brushes.Gray as Brush;
                        Players.Add(playerInfo);
                    }

                    // 选择当前用户对应的玩家，如果没有则选择第一个玩家
                    if (Players.Count > 0)
                    {
                        var currentUserAccount = _configService.GetCurrentUserAccount();
                        var currentPlayer = Players.FirstOrDefault(p => p.UserAccount?.Email == currentUserAccount?.Email);
                        
                        if (currentPlayer != null)
                        {
                            // 选择当前用户对应的玩家
                            SelectedPlayer = currentPlayer;
                        }
                        else if (SelectedPlayer == null)
                        {
                            // 如果没有找到当前用户对应的玩家，选择第一个玩家
                            SelectedPlayer = Players[0];
                        }
                        
                        // 只在选中玩家时获取头像路径
                        if (SelectedPlayer != null)
                        {
                            SelectedPlayer.AvatarPath = GetPlayerAvatarPathInternal(SelectedPlayer.UserAccount);
                            SelectedPlayer.LoginStatus = GetPlayerLoginStatusInternal(SelectedPlayer.UserAccount);
                            SelectedPlayer.LoginStatusColor = GetPlayerLoginStatusColorInternal(SelectedPlayer.UserAccount);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// 刷新指定玩家的头像
        /// </summary>
        public async Task RefreshPlayerAvatarAsync(PlayerInfo player)
        {
            try
            {
                await _avatarManagementService.RefreshPlayerAvatarAsync(player);
                
                // 更新头像路径和状态
                player.AvatarPath = _avatarManagementService.GetPlayerAvatarPath(player);
                player.LoginStatus = GetPlayerLoginStatusInternal(player.UserAccount);
                player.LoginStatusColor = GetPlayerLoginStatusColorInternal(player.UserAccount);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 刷新当前选中玩家的头像
        /// </summary>
        public async Task RefreshCurrentPlayerAvatarAsync()
        {
            if (SelectedPlayer != null)
            {
                await RefreshPlayerAvatarAsync(SelectedPlayer);
            }
        }

        /// <summary>
        /// 获取玩家头像路径
        /// </summary>
        public string? GetPlayerAvatarPath(PlayerInfo player)
        {
            return _avatarManagementService.GetPlayerAvatarPath(player);
        }

        /// <summary>
        /// 获取玩家登录状态
        /// </summary>
        public string GetPlayerLoginStatus(PlayerInfo player)
        {
            return GetPlayerLoginStatusInternal(player.UserAccount);
        }

        /// <summary>
        /// 获取玩家登录状态颜色
        /// </summary>
        public Brush GetPlayerLoginStatusColor(PlayerInfo player)
        {
            return GetPlayerLoginStatusColorInternal(player.UserAccount);
        }

        /// <summary>
        /// 获取玩家头像位图
        /// </summary>
        public async Task<Avalonia.Media.Imaging.Bitmap?> GetPlayerAvatarBitmapAsync(PlayerInfo player)
        {
            return await _avatarManagementService.GetPlayerAvatarBitmapAsync(player);
        }





        /// <summary>
        /// 判断是否为第三方用户（非离线用户）
        /// </summary>
        private bool IsThirdPartyUser(UserAccount user)
        {
            // 离线用户的特征：
            // 1. 邮箱为空或为 "offline"
            // 2. 邮箱以 "@local" 结尾
            // 3. 没有 accessToken 或 clientToken
            
            if (string.IsNullOrEmpty(user.Email) || 
                user.Email == "offline" || 
                user.Email.EndsWith("@local"))
            {
                return false;
            }
            
            // 第三方用户必须有有效的访问令牌
            if (string.IsNullOrEmpty(user.AccessToken) && string.IsNullOrEmpty(user.ClientToken))
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// 保存离线用户信息到配置文件
        /// </summary>
        public async Task<bool> SaveOfflineUserAsync(string playerName)
        {
            try
            {
                // 创建离线用户账户信息
                var userAccount = new UserAccount
                {
                    Email = $"offline_{playerName}@local",
                    Uid = 0,
                    Nickname = playerName,
                    Avatar = 0,
                    Score = 0,
                    Permission = 1,
                    LastSignAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    RegisterAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Verified = true,
                    AccessToken = "",
                    ClientToken = ""
                };

                // 添加或更新用户账户
                await _configService.AddOrUpdateUserAccountAsync(userAccount);
                
                // 设置为当前用户
                await _configService.SetCurrentUserAsync(userAccount.Email);
                
                // 重新加载玩家列表以更新SelectedPlayer
                await LoadPlayersAsync();
                
                Console.WriteLine($"[PlayerManagementService] 离线用户创建成功: {playerName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayerManagementService] 创建离线用户失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 保存微软用户信息到配置文件
        /// </summary>
        public async Task<bool> SaveMicrosoftUserAsync(string playerName, string playerUuid, string accessToken, string refreshToken, DateTime expiresAt)
        {
            try
            {
                // 创建微软用户账户信息
                var userAccount = new UserAccount
                {
                    Email = $"microsoft_{playerName}@microsoft.com",
                    Uid = 0,
                    Nickname = playerName,
                    Avatar = 0,
                    Score = 0,
                    Permission = 1,
                    LastSignAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    RegisterAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Verified = true,
                    AccessToken = accessToken,
                    ClientToken = refreshToken
                };

                // 添加或更新用户账户
                await _configService.AddOrUpdateUserAccountAsync(userAccount);
                
                // 设置为当前用户
                await _configService.SetCurrentUserAsync(userAccount.Email);
                
                Console.WriteLine($"[PlayerManagementService] 微软用户创建成功: {playerName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayerManagementService] 创建微软用户失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 保存用户信息到配置文件
        /// </summary>
        public async Task<bool> SaveUserInfoAsync(YggdrasilAuthResult authResult, string email)
        {
            try
            {
                // 创建用户账户信息
                var userAccount = new UserAccount
                {
                    Email = authResult.User?.Email ?? email,
                    Uid = int.TryParse(authResult.User?.Id, out var uid) ? uid : 0,
                    Nickname = authResult.SelectedProfile?.Name ?? "Unknown",
                    Avatar = 0,
                    Score = 0,
                    Permission = 1,
                    LastSignAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    RegisterAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Verified = true,
                    AccessToken = authResult.AccessToken ?? "",
                    ClientToken = authResult.ClientToken ?? ""
                };

                // 添加角色信息
                if (authResult.SelectedProfile != null)
                {
                    userAccount.Profiles.Add(new PlayerProfile
                    {
                        Id = authResult.SelectedProfile.Id,
                        Name = authResult.SelectedProfile.Name,
                        Legacy = false,
                        LastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                // 添加或更新用户账户
                await _configService.AddOrUpdateUserAccountAsync(userAccount);
                
                // 设置为当前用户
                await _configService.SetCurrentUserAsync(userAccount.Email);
                
                
                // 异步下载用户头像（不阻塞登录流程）
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _avatarManagementService.DownloadUserAvatarsAsync(userAccount);
                    }
                    catch (Exception ex)
                    {
                    }
                });
                
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        public UserModel? GetCurrentUser()
        {
            try
            {
                var currentUserAccount = _configService.GetCurrentUserAccount();
                if (currentUserAccount == null)
                    return null;

                // 转换为UserModel以保持兼容性
                return new UserModel
                {
                    Uid = currentUserAccount.Uid,
                    Email = currentUserAccount.Email,
                    Nickname = currentUserAccount.Nickname,
                    Avatar = currentUserAccount.Avatar,
                    Score = currentUserAccount.Score,
                    Permission = currentUserAccount.Permission,
                    LastSignAt = currentUserAccount.LastSignAt,
                    RegisterAt = currentUserAccount.RegisterAt,
                    Verified = currentUserAccount.Verified
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 清除用户信息
        /// </summary>
        public async Task<bool> ClearUserInfoAsync()
        {
            try
            {
                var currentUserAccount = _configService.GetCurrentUserAccount();
                if (currentUserAccount != null)
                {
                    await _configService.RemoveUserAccountAsync(currentUserAccount.Email);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 检查用户是否已登录
        /// </summary>
        public bool IsUserLoggedIn()
        {
            try
            {
                var user = GetCurrentUser();
                return user != null && !string.IsNullOrEmpty(user.Email) && user.Verified;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region 私有方法

        private string? GetPlayerAvatarPathInternal(UserAccount user)
        {
            if (user == null) return null;
            
            var avatarDir = _avatarManagementService.GetAvatarDirectory(user.Nickname);
            
            // 按优先级查找头像文件
            var possiblePaths = new[]
            {
                Path.Combine(avatarDir, $"{user.Nickname}_user_avatar.png"),  // 用户头像
                Path.Combine(avatarDir, $"{user.Nickname}_avatar_2d.png"),     // 2D头像
                Path.Combine(avatarDir, $"{user.Nickname}_avatar_3d.png"),    // 3D头像
                Path.Combine(avatarDir, "avatar.png")                          // 默认头像
            };
            
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    // 尝试转换为相对路径
                    var relativePath = Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, path);
                    
                    // 尝试不同的路径格式
                    var normalizedPath = relativePath.Replace("\\", "/");
                    
                    // 直接使用文件路径，不使用file://前缀
                    return path;
                }
            }
            
            return null;
        }

        private string GetPlayerLoginStatusInternal(UserAccount user)
        {
            if (user == null) return "未选择玩家";

            var avatarPath = GetPlayerAvatarPathInternal(user);
            if (!string.IsNullOrEmpty(avatarPath))
            {
                return "已登录";
            }
            else
            {
                return "头像下载中...";
            }
        }

        private Brush GetPlayerLoginStatusColorInternal(UserAccount user)
        {
            if (user == null) return Brushes.Gray as Brush;

            var avatarPath = GetPlayerAvatarPathInternal(user);
            if (!string.IsNullOrEmpty(avatarPath))
            {
                return Brushes.Green as Brush;
            }
            else
            {
                return Brushes.Orange as Brush;
            }
        }

        #endregion
    }
}
