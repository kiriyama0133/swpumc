using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using swpumc.Models;
using swpumc.Services;

namespace swpumc.Controls.InfoView
{
    public partial class InfoViewModel : ObservableObject
    {
        private readonly IPlayerManagementService? _playerManagementService;
        private readonly IAvatarManagementService? _avatarManagementService;
        private bool _isUpdatingFromService = false;

        [ObservableProperty]
        private ObservableCollection<PlayerInfo> _availablePlayers = new();

        [ObservableProperty]
        private PlayerInfo? _selectedPlayer;

        [ObservableProperty]
        private bool _isLoading = false;

        // 独立的UI绑定属性
        [ObservableProperty]
        private string _displayNickname = "";
        
        [ObservableProperty]
        private string _displayEmail = "";
        
        [ObservableProperty]
        private string _displayAvatarPath = "";

        [ObservableProperty]
        private Avalonia.Media.Imaging.Bitmap? _displayAvatarBitmap;
        
        [ObservableProperty]
        private string _displayLoginStatus = "";
        
        [ObservableProperty]
        private object? _displayLoginStatusColor;
        
        [ObservableProperty]
        private string _displayLastSignAt = "";

        public bool HasSelectedPlayer => SelectedPlayer != null;

        public InfoViewModel(IPlayerManagementService? playerManagementService, IAvatarManagementService? avatarManagementService)
        {
            _playerManagementService = playerManagementService!;
            _avatarManagementService = avatarManagementService!;
            
            if (_playerManagementService != null)
            {
                // 移除重复的事件订阅，避免循环调用
                // _playerManagementService.OnSelectedPlayerChanged += OnPlayerSelectionChanged;
                
                // 延迟同步数据，等待PlayerManagementService加载完成
                // SyncDataFromService(); // 暂时注释掉，等待外部调用
            }
            else
            {
            }
        }

        /// <summary>
        /// 手动触发数据同步（供外部调用）
        /// </summary>
        public void RefreshData()
        {
            SyncDataFromService();
        }

        /// <summary>
        /// 加载玩家列表
        /// </summary>
        public async Task LoadPlayersAsync()
        {
            try
            {
                if (_playerManagementService == null)
                {
                    return;
                }
                
                // 刷新服务数据
                await _playerManagementService.LoadPlayersAsync();
                
                // 同步数据到UI
                SyncDataFromService();
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 从服务同步数据到UI
        /// </summary>
        private void SyncDataFromService()
        {
            try
            {
                if (_playerManagementService == null) 
                {
                    return;
                }
                
                AvailablePlayers.Clear();
                foreach (var player in _playerManagementService.Players)
                {
                    // 只显示第三方用户，过滤掉离线用户
                    if (IsThirdPartyUser(player.UserAccount))
                    {
                        AvailablePlayers.Add(player);
                    }
                }

                // 设置默认选中的用户（如果没有选中用户且有可用用户）
                if (SelectedPlayer == null && AvailablePlayers.Count > 0)
                {
                    _isUpdatingFromService = true;
                    try
                    {
                        SelectedPlayer = AvailablePlayers[0];
                    }
                    finally
                    {
                        _isUpdatingFromService = false;
                    }
                }
                
                IsLoading = _playerManagementService.IsLoading;

                // 更新显示属性
                UpdateDisplayProperties();

                // 触发HasSelectedPlayer属性变更通知
                OnPropertyChanged(nameof(HasSelectedPlayer));
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 加载头像为位图
        /// </summary>
        private async Task LoadAvatarBitmapAsync(string? avatarPath)
        {
            try
            {
                if (string.IsNullOrEmpty(avatarPath) || !File.Exists(avatarPath))
                {
                    // 尝试使用头像管理服务获取默认头像
                    if (_avatarManagementService != null)
                    {
                        DisplayAvatarBitmap = await _avatarManagementService.GetDefaultAvatarBitmapAsync();
                    }
                    else
                    {
                        DisplayAvatarBitmap = null;
                    }
                    return;
                }

                Console.WriteLine($"[InfoViewModel] 开始加载头像位图: {avatarPath}");
                
                // 在后台线程加载位图
                var bitmap = await Task.Run(() =>
                {
                    try
                    {
                        using var stream = File.OpenRead(avatarPath);
                        return new Avalonia.Media.Imaging.Bitmap(stream);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[InfoViewModel] 加载位图失败: {ex.Message}");
                        return null;
                    }
                });

                if (bitmap != null)
                {
                    DisplayAvatarBitmap = bitmap;
                    Console.WriteLine($"[InfoViewModel] 头像位图加载成功");
                }
                else
                {
                    DisplayAvatarBitmap = null;
                    Console.WriteLine($"[InfoViewModel] 头像位图加载失败");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InfoViewModel] 加载头像位图异常: {ex.Message}");
                DisplayAvatarBitmap = null;
            }
        }

        /// <summary>
        /// 更新显示属性
        /// </summary>
        private void UpdateDisplayProperties()
        {
            if (SelectedPlayer != null)
            {
                DisplayNickname = SelectedPlayer.Nickname ?? "";
                DisplayEmail = SelectedPlayer.Email ?? "";
                DisplayAvatarPath = SelectedPlayer.AvatarPath ?? "";
                DisplayLoginStatus = SelectedPlayer.LoginStatus ?? "";
                DisplayLoginStatusColor = SelectedPlayer.LoginStatusColor;
                DisplayLastSignAt = SelectedPlayer.LastSignAt ?? "";
                
                Console.WriteLine($"[InfoViewModel] 更新显示属性: {DisplayNickname}, 头像: {DisplayAvatarPath}");
                Console.WriteLine($"[InfoViewModel] 触发属性变更通知: DisplayAvatarPath");
                
                // 强制触发属性变更通知
                OnPropertyChanged(nameof(DisplayAvatarPath));
                
                // 异步加载头像位图
                if (!string.IsNullOrEmpty(DisplayAvatarPath))
                {
                    _ = Task.Run(async () => await LoadAvatarBitmapAsync(DisplayAvatarPath));
                }
                else
                {
                    DisplayAvatarBitmap = null;
                }
            }
            else
            {
                DisplayNickname = "";
                DisplayEmail = "";
                DisplayAvatarPath = "";
                DisplayLoginStatus = "";
                DisplayLoginStatusColor = null;
                DisplayLastSignAt = "";
                DisplayAvatarBitmap = null;
            }
        }

        partial void OnSelectedPlayerChanged(PlayerInfo? value)
        {
            // 防止重复处理
            if (_isUpdatingFromService) return;
            
            // 简化处理，只更新显示属性，不更新服务
            Console.WriteLine($"[InfoViewModel] 玩家选择变更: {value?.Nickname ?? "无"}");
            
            // 更新显示属性
            UpdateDisplayProperties();
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

        // 清理资源
        ~InfoViewModel()
        {
        }
    }
}