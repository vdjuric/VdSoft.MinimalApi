using System.Text.Json.Serialization;

namespace VdSoft.MinimalApi.GotifyToPushover.Models;

internal class PushoverRequest
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }
}
