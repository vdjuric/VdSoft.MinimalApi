using System.Text.Json.Serialization;

namespace VdSoft.MinimalApi.GotifyToPushover.Models;

internal class GotifyErrorResponse : GotifyBaseResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("errorCode")]
    public int? ErrorCode { get; set; }

    [JsonPropertyName("errorDescription")]
    public string? ErrorDescription { get; set; }
}
