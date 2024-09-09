namespace VdSoft.MinimalApi.GotifyToPushover;

internal static class Constants
{
    public const int PushoverTitleMaxLength = 250;
    public const int PushoverMessageMaxLength = 1024;
    public const string DefaultLoggerName = "GotifyToPushover";
    public const string DefaultEndpointName = "/gotify-to-pushover/message";
    public const string ContentTypeJson = "application/json";
    public const string PushoverMessagesApi = "https://api.pushover.net/1/messages.json";
}
