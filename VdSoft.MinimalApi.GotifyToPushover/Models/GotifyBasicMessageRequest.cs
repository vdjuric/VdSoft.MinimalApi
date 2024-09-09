using System.Text.Json.Serialization;

namespace VdSoft.MinimalApi.GotifyToPushover.Models;

/// <summary>
/// NOTE: Currently, we support only the most basic properties.
/// 
/// Details: https://gotify.github.io/api-docs/#/message/createMessage
/// </summary>
public class GotifyBasicMessageRequest
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }


    [JsonPropertyName("priority")]
    public int? Priority { get; set; }
}
