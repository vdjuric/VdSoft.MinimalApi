using Microsoft.AspNetCore.Http;
using VdSoft.MinimalApi.GotifyToPushover.Models;

namespace VdSoft.MinimalApi.GotifyToPushover;

/// <summary>
/// This class allows you to override default behavior.
/// 
/// The only mandatory property is <seealso cref="GetPushoverUserByToken"/>, where you have to define Pushover user by given token.
/// 
/// Example:
/// 
///    app.MapGotifyToPushover(new GotifyToPushoverOptions()
///    {
///        GetPushoverUserByToken = token => "YOUR_PUSHOVER_USER",
///        //...
///    });
/// 
/// </summary>
public class GotifyToPushoverOptions
{
    //adapters
    public required Func<string, string> GetPushoverUserByToken { get; set; } //mandatory
    public Func<string, string>? GotifyTokenToPushoverToken { get; set; }
    public Func<int?, int>? GotifyPriorityToPushoverPriority { get; set; }
    public Func<string?, string>? GotifyTitleToPushoverTitle { get; set; }
    public Func<string?, string>? GotifyMessageToPushoverTitle { get; set; }

    public bool AutoTrimLongTitle { get; set; } = true;
    public bool AutoTrimLongMessage { get; set; } = true;

    //logging
    public string LoggerName { get; set; } = Constants.DefaultLoggerName;
    public bool LogPushoverSuccess { get; set; }
    public bool LogPushoverFailure { get; set; } = true;
    public bool LogDenied { get; set; } = true;
    public bool LogException { get; set; } = true;

    //infrastructure & security
    public Func<HttpClient>? GetPushoverClient { get; set; }

    private static ValueTask<(bool canContinue, string? denyReason)> AllowAll(
        HttpContext httpContext, 
        string gotifyToken,
        GotifyBasicMessageRequest req,
        CancellationToken cancellationToken
    ) => ValueTask.FromResult<(bool canContinue, string? denyReason)>((true, null));

    /// <summary>
    /// You can use this property to filter Gotify requests or validate Gotify token before we forward request to Pushover.
    /// </summary>
    public Func<HttpContext, string, GotifyBasicMessageRequest, CancellationToken, ValueTask<(bool canContinue, string? denyReason)>> CanContinueAsync { get; set; } = AllowAll;

    public static GotifyToPushoverOptions CreateBasic(Func<string, string> getPushoverUserByToken)
        => new GotifyToPushoverOptions()
        {
            GetPushoverUserByToken = getPushoverUserByToken
        };
}
