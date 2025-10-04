using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using swpumc.Models;

namespace swpumc.Services
{
    /// <summary>
    /// 头像管理服务接口
    /// </summary>
    public interface IAvatarManagementService
    {
        /// <summary>
        /// 获取玩家头像位图
        /// </summary>
        Task<Bitmap?> GetPlayerAvatarBitmapAsync(PlayerInfo player);

        /// <summary>
        /// 获取玩家头像路径
        /// </summary>
        string? GetPlayerAvatarPath(PlayerInfo player);

        /// <summary>
        /// 刷新指定玩家的头像
        /// </summary>
        Task RefreshPlayerAvatarAsync(PlayerInfo player);

        /// <summary>
        /// 下载用户头像
        /// </summary>
        Task DownloadUserAvatarsAsync(UserAccount userAccount);

        /// <summary>
        /// 获取默认头像位图
        /// </summary>
        Task<Bitmap?> GetDefaultAvatarBitmapAsync();

        /// <summary>
        /// 获取用户头像保存目录
        /// </summary>
        string GetAvatarDirectory(string nickname);
    }
}
