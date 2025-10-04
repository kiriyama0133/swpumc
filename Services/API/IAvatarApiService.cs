using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using swpumc.Models;

namespace swpumc.Services.API
{
    /// <summary>
    /// 头像和材质预览图API服务接口
    /// </summary>
    public interface IAvatarApiService
    {
        /// <summary>
        /// 根据角色名获取头像
        /// GET /avatar/player/{name}
        /// </summary>
        /// <param name="playerName">角色名</param>
        /// <param name="size">头像大小（可选）</param>
        /// <param name="is3D">是否生成3D效果头像（可选）</param>
        /// <param name="isPng">是否返回PNG格式（可选，默认WebP）</param>
        /// <returns>头像图片数据</returns>
        Task<byte[]?> GetPlayerAvatarAsync(string playerName, int? size = null, bool is3D = false, bool isPng = false);

        /// <summary>
        /// 根据用户UID获取头像
        /// GET /avatar/user/{uid}
        /// </summary>
        /// <param name="uid">用户UID</param>
        /// <param name="size">头像大小（可选）</param>
        /// <param name="is3D">是否生成3D效果头像（可选）</param>
        /// <param name="isPng">是否返回PNG格式（可选，默认WebP）</param>
        /// <returns>头像图片数据</returns>
        Task<byte[]?> GetUserAvatarAsync(int uid, int? size = null, bool is3D = false, bool isPng = false);

        /// <summary>
        /// 根据材质TID生成头像
        /// GET /avatar/{tid}
        /// </summary>
        /// <param name="tid">材质TID</param>
        /// <param name="size">头像大小（可选）</param>
        /// <param name="is3D">是否生成3D效果头像（可选）</param>
        /// <param name="isPng">是否返回PNG格式（可选，默认WebP）</param>
        /// <returns>头像图片数据</returns>
        Task<byte[]?> GetTextureAvatarAsync(int tid, int? size = null, bool is3D = false, bool isPng = false);

        /// <summary>
        /// 根据材质文件hash生成头像
        /// GET /avatar/hash/{hash}
        /// </summary>
        /// <param name="hash">材质文件hash值</param>
        /// <param name="size">头像大小（可选）</param>
        /// <param name="is3D">是否生成3D效果头像（可选）</param>
        /// <param name="isPng">是否返回PNG格式（可选，默认WebP）</param>
        /// <returns>头像图片数据</returns>
        Task<byte[]?> GetTextureAvatarByHashAsync(string hash, int? size = null, bool is3D = false, bool isPng = false);

        /// <summary>
        /// 根据材质TID生成材质预览图
        /// GET /preview/{tid}
        /// </summary>
        /// <param name="tid">材质TID</param>
        /// <param name="isPng">是否返回PNG格式（可选，默认WebP）</param>
        /// <returns>材质预览图数据</returns>
        Task<byte[]?> GetTexturePreviewAsync(int tid, bool isPng = false);

        /// <summary>
        /// 根据材质文件hash生成材质预览图
        /// GET /preview/hash/{hash}
        /// </summary>
        /// <param name="hash">材质文件hash值</param>
        /// <param name="isPng">是否返回PNG格式（可选，默认WebP）</param>
        /// <returns>材质预览图数据</returns>
        Task<byte[]?> GetTexturePreviewByHashAsync(string hash, bool isPng = false);

        /// <summary>
        /// 下载玩家头像到本地文件
        /// </summary>
        /// <param name="playerName">玩家名称</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="size">头像大小</param>
        /// <param name="is3D">是否3D</param>
        /// <param name="isPng">是否PNG格式</param>
        /// <returns>是否成功</returns>
        Task<bool> DownloadPlayerAvatarAsync(string playerName, string savePath, int? size = null, bool is3D = false, bool isPng = false);

        /// <summary>
        /// 下载用户头像到本地文件
        /// </summary>
        /// <param name="uid">用户UID</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="size">头像大小</param>
        /// <param name="is3D">是否3D</param>
        /// <param name="isPng">是否PNG格式</param>
        /// <returns>是否成功</returns>
        Task<bool> DownloadUserAvatarAsync(int uid, string savePath, int? size = null, bool is3D = false, bool isPng = false);

        /// <summary>
        /// 获取用户头像数据（不涉及文件操作）
        /// </summary>
        Task<byte[]?> GetUserAvatarDataAsync(UserAccount userAccount);

        /// <summary>
        /// 获取玩家2D头像数据（不涉及文件操作）
        /// </summary>
        Task<byte[]?> GetPlayer2DAvatarDataAsync(string playerName);

        /// <summary>
        /// 获取玩家3D头像数据（不涉及文件操作）
        /// </summary>
        Task<byte[]?> GetPlayer3DAvatarDataAsync(string playerName);
    }
}
