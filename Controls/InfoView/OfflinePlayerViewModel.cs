using Avalonia;
using Avalonia.Platform; // Added for AssetLoader
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using swpumc.Models;
using swpumc.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace swpumc.Controls.InfoView
{
    public partial class OfflinePlayerViewModel : ObservableObject
    {
        private readonly IPlayerManagementService _playerManagementService = null!;
        private readonly IAvatarManagementService? _avatarManagementService;

        [ObservableProperty]
        private string _displayNickname = "离线玩家";

        [ObservableProperty]
        private string _displayEmail = "offline";

        [ObservableProperty]
        private string _displayLastSignAt = "从未登录";

        [ObservableProperty]
        private string _displayAvatarPath = "avares://swpumc/Assets/IMG/Avatars/default.png";

        [ObservableProperty]
        private Avalonia.Media.Imaging.Bitmap? _displayAvatarBitmap;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private ObservableCollection<PlayerInfo> _availablePlayers = new();

        public event Action? OnSwitchAuthRequested;

        public OfflinePlayerViewModel()
        {
            // 通过依赖注入获取服务
            var app = Application.Current as App;
            _playerManagementService = app?.Services?.GetService(typeof(IPlayerManagementService)) as IPlayerManagementService;
            _avatarManagementService = app?.Services?.GetService(typeof(IAvatarManagementService)) as IAvatarManagementService;
            // 注意：IPlayerManagementService 可能不实现 INotifyPropertyChanged
            // 我们通过其他方式监听变化
            
            // 加载默认头像
            _ = LoadDefaultAvatarAsync();
            
            RefreshData(); // Initial data load
        }

        [RelayCommand]
        private void SwitchAuth()
        {
            OnSwitchAuthRequested?.Invoke();
        }

        public async Task LoadPlayersAsync()
        {
            await _playerManagementService.LoadPlayersAsync();
            RefreshData();
        }

        public void RefreshData()
        {
            try
            {
                // 同步数据
                SyncDataFromService();

                // 更新显示属性
                UpdateDisplayProperties();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OfflinePlayerViewModel] 刷新数据时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 从服务同步数据
        /// </summary>
        private void SyncDataFromService()
        {
            try
            {
                if (_playerManagementService != null)
                {
                    // 同步可用玩家列表
                    var players = _playerManagementService.Players?.ToList() ?? new List<PlayerInfo>();
                    AvailablePlayers.Clear();
                    foreach (var player in players)
                    {
                        AvailablePlayers.Add(player);
                    }

                    // 同步选中玩家
                    if (_playerManagementService.SelectedPlayer != null)
                    {
                        Console.WriteLine($"[OfflinePlayerViewModel] 同步选中玩家: {_playerManagementService.SelectedPlayer.UserAccount?.Email ?? "未知"}");
                        SelectedPlayer = _playerManagementService.SelectedPlayer;
                    }
                    else if (AvailablePlayers.Any())
                    {
                        Console.WriteLine($"[OfflinePlayerViewModel] 使用第一个可用玩家: {AvailablePlayers[0].UserAccount?.Email ?? "未知"}");
                        SelectedPlayer = AvailablePlayers[0];
                    }

                    IsLoading = _playerManagementService.IsLoading;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OfflinePlayerViewModel] 同步数据时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新显示属性
        /// </summary>
        private void UpdateDisplayProperties()
        {
            try
            {
                if (SelectedPlayer?.UserAccount != null)
                {
                    var userAccount = SelectedPlayer.UserAccount;
                    
                    // 确保是离线玩家
                    if (userAccount.Email?.EndsWith("@local") == true || 
                        string.IsNullOrEmpty(userAccount.Email) ||
                        userAccount.Email == "offline")
                    {
                        // 更新显示属性
                        DisplayNickname = userAccount.Nickname ?? "离线玩家";
                        DisplayEmail = userAccount.Email ?? "";
                        DisplayLastSignAt = !string.IsNullOrEmpty(userAccount.LastSignAt) 
                            ? userAccount.LastSignAt 
                            : "从未登录";
                        
                        // 设置头像路径 - 使用PlayerManagementService管理的路径
                        DisplayAvatarPath = SelectedPlayer.AvatarPath ?? "";
                        
                        // 异步加载头像位图
                        if (!string.IsNullOrEmpty(DisplayAvatarPath))
                        {
                            _ = Task.Run(async () => await LoadAvatarBitmapAsync(DisplayAvatarPath));
                        }
                        else
                        {
                            // 如果没有头像路径，保持默认头像（不覆盖DisplayAvatarBitmap）
                            // 不设置 DisplayAvatarBitmap = null，保持默认头像
                        }
                        
                    }
                    else
                    {
                        // 如果不是离线玩家，显示默认信息
                        DisplayNickname = "离线玩家";
                        DisplayEmail = "offline";
                        DisplayLastSignAt = "从未登录";
                        DisplayAvatarPath = "";
                        // 不设置 DisplayAvatarBitmap = null，保持默认头像
                    }
                }
                else
                {
                    // 如果没有选中玩家，则显示默认离线玩家信息
                    DisplayNickname = "离线玩家";
                    DisplayEmail = "offline";
                    DisplayLastSignAt = "从未登录";
                    DisplayAvatarPath = "";
                    DisplayAvatarBitmap = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OfflinePlayerViewModel] 更新显示属性时出错: {ex.Message}");
            }
        }


        /// <summary>
        /// 加载默认头像
        /// </summary>
        private async Task LoadDefaultAvatarAsync()
        {
            try
            {
                if (_avatarManagementService != null)
                {
                    DisplayAvatarBitmap = await _avatarManagementService.GetDefaultAvatarBitmapAsync();
                }
                else
                {
                    // 使用实际文件路径加载默认头像
                    var defaultAvatarPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "IMG", "Avatars", "default.png");
                    
                    if (File.Exists(defaultAvatarPath))
                    {
                        using var stream = File.OpenRead(defaultAvatarPath);
                        var bitmap = new Avalonia.Media.Imaging.Bitmap(stream);
                        DisplayAvatarBitmap = bitmap;
                    }
                    else
                    {
                        DisplayAvatarBitmap = null;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAvatarBitmap = null;
            }
        }

        /// <summary>
        /// 加载头像为位图
        /// </summary>
        private async Task LoadAvatarBitmapAsync(string? avatarPath)
        {
            try
            {
                if (string.IsNullOrEmpty(avatarPath))
                {
                    DisplayAvatarBitmap = null;
                    return;
                }


                // 在后台线程加载位图
                var bitmap = await Task.Run(() =>
                {
                    try
                    {
                        // 检查文件是否存在
                        if (!File.Exists(avatarPath))
                        {
                            return null;
                        }

                        using var stream = File.OpenRead(avatarPath);
                        return new Avalonia.Media.Imaging.Bitmap(stream);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                });

                if (bitmap != null)
                {
                    DisplayAvatarBitmap = bitmap;
                }
                else
                {
                    DisplayAvatarBitmap = null;
                }
            }
            catch (Exception ex)
            {
                DisplayAvatarBitmap = null;
            }
        }

        private PlayerInfo? _selectedPlayer;
        public PlayerInfo? SelectedPlayer
        {
            get => _selectedPlayer;
            set => SetProperty(ref _selectedPlayer, value);
        }

        public bool HasSelectedPlayer => SelectedPlayer != null;
    }
}


