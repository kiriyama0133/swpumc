using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace swpumc.Models
{
    /// <summary>
    /// 玩家信息模型
    /// </summary>
    public partial class PlayerInfo : ObservableObject
    {
        /// <summary>
        /// 用户账户信息
        /// </summary>
        public UserAccount UserAccount { get; set; } = new();

        /// <summary>
        /// 头像路径
        /// </summary>
        [ObservableProperty]
        private string? _avatarPath;

        /// <summary>
        /// 登录状态文本
        /// </summary>
        [ObservableProperty]
        private string _loginStatus = "未登录";

        /// <summary>
        /// 登录状态颜色
        /// </summary>
        [ObservableProperty]
        private Brush _loginStatusColor = Brushes.Gray as Brush;

        /// <summary>
        /// 玩家昵称
        /// </summary>
        public string Nickname => UserAccount.Nickname;

        /// <summary>
        /// 玩家邮箱
        /// </summary>
        public string Email => UserAccount.Email;

        /// <summary>
        /// 玩家UID
        /// </summary>
        public int Uid => UserAccount.Uid;

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public string LastSignAt => UserAccount.LastSignAt;

        /// <summary>
        /// 验证状态
        /// </summary>
        public bool Verified => UserAccount.Verified;
    }
}
