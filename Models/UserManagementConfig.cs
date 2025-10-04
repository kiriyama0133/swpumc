using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace swpumc.Models
{
    /// <summary>
    /// 用户管理配置
    /// </summary>
    public class UserManagementConfig
    {
        [JsonPropertyName("exclude")]
        public List<string> Exclude { get; set; } = new List<string>
        {
            "**/bin",
            "**/bower_components",
            "**/jspm_packages",
            "**/node_modules",
            "**/obj",
            "**/platforms"
        };

        [JsonPropertyName("baseUrl")]
        public string BaseUrl { get; set; } = "https://littleskin.cn/api/yggdrasil";

        [JsonPropertyName("currentUser")]
        public string CurrentUser { get; set; } = string.Empty;

        [JsonPropertyName("users")]
        public List<UserAccount> Users { get; set; } = new List<UserAccount>();
    }

    /// <summary>
    /// 用户账户信息
    /// </summary>
    public class UserAccount
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("uid")]
        public int Uid { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = string.Empty;

        [JsonPropertyName("avatar")]
        public int Avatar { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("permission")]
        public int Permission { get; set; }

        [JsonPropertyName("last_sign_at")]
        public string LastSignAt { get; set; } = string.Empty;

        [JsonPropertyName("register_at")]
        public string RegisterAt { get; set; } = string.Empty;

        [JsonPropertyName("verified")]
        public bool Verified { get; set; }

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("clientToken")]
        public string ClientToken { get; set; } = string.Empty;

        [JsonPropertyName("profiles")]
        public List<PlayerProfile> Profiles { get; set; } = new List<PlayerProfile>();

        [JsonPropertyName("wardrobe")]
        public Wardrobe Wardrobe { get; set; } = new Wardrobe();

        // 头像路径属性，用于UI绑定
        [JsonIgnore]
        public string? AvatarPath { get; set; }
    }

    /// <summary>
    /// 玩家角色信息
    /// </summary>
    public class PlayerProfile
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("legacy")]
        public bool Legacy { get; set; }

        [JsonPropertyName("textures")]
        public PlayerTextures Textures { get; set; } = new PlayerTextures();

        [JsonPropertyName("lastModified")]
        public string LastModified { get; set; } = string.Empty;
    }

    /// <summary>
    /// 玩家材质信息
    /// </summary>
    public class PlayerTextures
    {
        [JsonPropertyName("skin")]
        public TextureInfo? Skin { get; set; }

        [JsonPropertyName("cape")]
        public TextureInfo? Cape { get; set; }
    }

    /// <summary>
    /// 材质信息
    /// </summary>
    public class TextureInfo
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("metadata")]
        public TextureMetadata Metadata { get; set; } = new TextureMetadata();
    }

    /// <summary>
    /// 材质元数据
    /// </summary>
    public class TextureMetadata
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "default";
    }

    /// <summary>
    /// 衣柜信息
    /// </summary>
    public class Wardrobe
    {
        [JsonPropertyName("skins")]
        public List<WardrobeItem> Skins { get; set; } = new List<WardrobeItem>();

        [JsonPropertyName("capes")]
        public List<WardrobeItem> Capes { get; set; } = new List<WardrobeItem>();
    }

    /// <summary>
    /// 衣柜物品
    /// </summary>
    public class WardrobeItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("hash")]
        public string Hash { get; set; } = string.Empty;

        [JsonPropertyName("uploaded_at")]
        public string UploadedAt { get; set; } = string.Empty;

        [JsonPropertyName("is_public")]
        public bool IsPublic { get; set; }
    }
}
