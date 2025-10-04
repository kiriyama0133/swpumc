using System;
using System.Text.Json.Serialization;

namespace swpumc.Models;

/// <summary>
/// OAuth2 Token Response
/// </summary>
public record OAuth2TokenResponse
{
    [JsonPropertyName("foci")]
    public string? Foci { get; set; }
    
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
    
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
}

/// <summary>
/// Device Code Response
/// </summary>
public record DeviceCodeResponse
{
    [JsonPropertyName("interval")]
    public int Interval { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonPropertyName("user_code")]
    public string? UserCode { get; set; }
    
    [JsonPropertyName("device_code")]
    public string? DeviceCode { get; set; }
    
    [JsonPropertyName("verification_uri")]
    public string? VerificationUrl { get; set; }
}

/// <summary>
/// Microsoft Account Model
/// </summary>
public record MicrosoftAccount
{
    public string Name { get; set; } = string.Empty;
    public Guid Uuid { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime LastRefreshTime { get; set; }
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// XBL Properties
/// </summary>
public record XBLProperties
{
    public required string SiteName { get; init; }
    public required string RpsTicket { get; init; }
    public required string AuthMethod { get; init; }
}

/// <summary>
/// XSTS Properties
/// </summary>
public record XSTSProperties
{
    public required string SandboxId { get; init; }
    public required string[] UserTokens { get; init; }
}

/// <summary>
/// XBL Token Payload
/// </summary>
public record XBLTokenPayload
{
    public required string TokenType { get; init; }
    public required string RelyingParty { get; init; }
    public required XBLProperties Properties { get; init; }
}

/// <summary>
/// XSTS Token Payload
/// </summary>
public record XSTSTokenPayload
{
    public required string TokenType { get; init; }
    public required string RelyingParty { get; init; }
    public required XSTSProperties Properties { get; init; }
}

/// <summary>
/// Minecraft Payload
/// </summary>
public record MinecraftPayload
{
    public required string IdentityToken { get; init; }
}
