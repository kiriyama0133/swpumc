using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media;
using swpumc.Models;
using swpumc.Services.API;

namespace swpumc.Services
{
    /// <summary>
    /// 玩家管理服务接口
    /// </summary>
    public interface IPlayerManagementService
    {
        /// <summary>
        /// 玩家选择变化事件
        /// </summary>
        event EventHandler<PlayerInfo?>? OnSelectedPlayerChanged;
        /// <summary>
        /// 获取所有玩家列表
        /// </summary>
        ObservableCollection<PlayerInfo> Players { get; }

        /// <summary>
        /// 当前选中的玩家
        /// </summary>
        PlayerInfo? SelectedPlayer { get; set; }

        /// <summary>
        /// 是否有选中的玩家
        /// </summary>
        bool HasSelectedPlayer { get; }

        /// <summary>
        /// 是否正在加载
        /// </summary>
        bool IsLoading { get; }

        /// <summary>
        /// 加载玩家列表
        /// </summary>
        Task LoadPlayersAsync();

        /// <summary>
        /// 刷新指定玩家的头像
        /// </summary>
        /// <param name="player">玩家信息</param>
        Task RefreshPlayerAvatarAsync(PlayerInfo player);

        /// <summary>
        /// 刷新当前选中玩家的头像
        /// </summary>
        Task RefreshCurrentPlayerAvatarAsync();

        /// <summary>
        /// 获取玩家头像路径
        /// </summary>
        /// <param name="player">玩家信息</param>
        /// <returns>头像文件路径，如果不存在则返回null</returns>
        string? GetPlayerAvatarPath(PlayerInfo player);

        /// <summary>
        /// 获取玩家登录状态
        /// </summary>
        /// <param name="player">玩家信息</param>
        /// <returns>登录状态文本</returns>
        string GetPlayerLoginStatus(PlayerInfo player);

        /// <summary>
        /// 获取玩家登录状态颜色
        /// </summary>
        /// <param name="player">玩家信息</param>
        /// <returns>状态颜色</returns>
        Brush GetPlayerLoginStatusColor(PlayerInfo player);

        /// <summary>
        /// 获取玩家头像位图
        /// </summary>
        /// <param name="player">玩家信息</param>
        /// <returns>头像位图</returns>
        Task<Avalonia.Media.Imaging.Bitmap?> GetPlayerAvatarBitmapAsync(PlayerInfo player);

        /// <summary>
        /// 保存离线用户信息到配置文件
        /// </summary>
        Task<bool> SaveOfflineUserAsync(string playerName);

        /// <summary>
        /// 保存用户信息到配置文件
        /// </summary>
        Task<bool> SaveUserInfoAsync(YggdrasilAuthResult authResult, string email);

        /// <summary>
        /// 保存微软用户信息到配置文件
        /// </summary>
        Task<bool> SaveMicrosoftUserAsync(string playerName, string playerUuid, string accessToken, string refreshToken, DateTime expiresAt);

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        UserModel? GetCurrentUser();

        /// <summary>
        /// 清除用户信息
        /// </summary>
        Task<bool> ClearUserInfoAsync();

        /// <summary>
        /// 检查用户是否已登录
        /// </summary>
        bool IsUserLoggedIn();
    }
}
