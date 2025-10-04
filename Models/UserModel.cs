using System.Text.Json.Serialization;

namespace swpumc.Models
{
    public class UserModel
    {
        [JsonPropertyName("uid")]
        public int Uid { get; set; } = 0;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = string.Empty;

        [JsonPropertyName("avatar")]
        public int Avatar { get; set; } = 0;

        [JsonPropertyName("score")]
        public int Score { get; set; } = 0;

        [JsonPropertyName("permission")]
        public int Permission { get; set; } = 0;

        [JsonPropertyName("last_sign_at")]
        public string LastSignAt { get; set; } = string.Empty;

        [JsonPropertyName("register_at")]
        public string RegisterAt { get; set; } = string.Empty;

        [JsonPropertyName("verified")]
        public bool Verified { get; set; } = false;
    }
}
