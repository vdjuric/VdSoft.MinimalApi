using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using VdSoft.MinimalApi.GotifyToPushover.Helpers;
using VdSoft.MinimalApi.GotifyToPushover.Models;

namespace VdSoft.MinimalApi.GotifyToPushover;

internal static class Adapter
{
    internal static async Task<IResult> GotifyToPushoverHandlerAsync(
        HttpContext httpContext,
        GotifyBasicMessageRequest req,
        GotifyToPushoverOptions options,
        CancellationToken cancellationToken
    )
    {
        if (options.GetPushoverUserByToken is null)
        {
            throw new ArgumentException($"{nameof(GotifyToPushoverOptions)}.{nameof(GotifyToPushoverOptions.GetPushoverUserByToken)}");
        }

        try
        {
            if (!TryGetToken(httpContext, out var gotifyToken) || gotifyToken is null)
            {
                return CreateGotifyError(401, "Unauthorized", "Missing Gotify token in request query parameters and headers");
            }

            var (canContinue, denyReason) = await options.CanContinueAsync(httpContext, gotifyToken, req, cancellationToken);
            if (!canContinue)
            {
                if (options.LogDenied)
                {
                    httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(options.LoggerName)
                        .LogWarning("Gotify to Pushover denied for {@req}: {reason}", req, denyReason);
                }

                return CreateGotifyError(403, "Forbidden", denyReason ?? "Denied");
            }

            string pushoverToken = options.GotifyTokenToPushoverToken?.Invoke(gotifyToken) ?? gotifyToken;
            var pushoverUser = options.GetPushoverUserByToken(pushoverToken);
            var pushoverPriority = options.GotifyPriorityToPushoverPriority?.Invoke(req.Priority) ?? req.Priority;
            var pushoverTitle = options.GotifyTitleToPushoverTitle?.Invoke(req.Title) ?? req.Title;
            var pushoverMessage = options.GotifyMessageToPushoverTitle?.Invoke(req.Message) ?? req.Message;

            HttpClient? client = null;

            try
            {
                if (options.GetPushoverClient is null)
                {
                    var factory = httpContext.RequestServices.GetService<IHttpClientFactory>();
                    client = factory?.CreateClient("PushoverClient") ?? new HttpClient();
                }
                else
                {
                    client = options.GetPushoverClient();
                }

                var pushoverReq = new PushoverRequest()
                {
                    Token = pushoverToken,
                    User = pushoverUser,
                    Priority = pushoverPriority,
                    Title = options.AutoTrimLongTitle ? pushoverTitle?.TruncateLongTitle() : pushoverTitle,
                    Message = options.AutoTrimLongMessage ? pushoverMessage?.TruncateLongMessage() : pushoverMessage,
                };

                return await PushoverMessagePostAsync(httpContext, req, options, client, pushoverReq, cancellationToken);
            }
            finally
            {
                client?.Dispose();
            }
        }
        catch (Exception ex)
        {
            if (options.LogException)
            {
                httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(options.LoggerName)
                    .LogError(ex, "Unhandled exception for {@req}", req);
            }

            return CreateGotifyError(500, "Internal Server Error", "Exception while handling request");
        }
    }

    private static async Task<IResult> PushoverMessagePostAsync(
        HttpContext httpContext,
        GotifyBasicMessageRequest req,
        GotifyToPushoverOptions options,
        HttpClient client, PushoverRequest pushoverReq,
        CancellationToken cancellationToken)
    {
        var res = await client.PostAsJsonAsync(Constants.PushoverMessagesApi, pushoverReq, cancellationToken);

        if (res.StatusCode == HttpStatusCode.OK)
        {
            if (options.LogPushoverSuccess)
            {
                httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(options.LoggerName)
                    .LogInformation("Pushover returned success for {@pushoverReq}, original Gotify request was {@req}", pushoverReq, req);
            }

            return Results.Ok(GotifyBasicMessageResponse.Empty);
        }

        var rawResponse = await res.Content.ReadAsStringAsync(cancellationToken);

        if (options.LogPushoverFailure)
        {
            httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(options.LoggerName)
                .LogWarning("Pushover returned {statusCode} for {@pushoverReq}, original Gotify request was {@req}: {rawResponse}", res.StatusCode, pushoverReq, req, rawResponse);
        }

        var statusCode = res.StatusCode switch
        {
            HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => (int)res.StatusCode,
            _ => 500 //for everything else we return for now 500 with original status available in GotifyErrorResponse.Error
        };

        return CreateGotifyError(statusCode, $"Pushover API returned HTTP status {(int)res.StatusCode}", rawResponse);
    }

    private static IResult CreateGotifyError(int statusCode, string error, string description)
        => Results.Json(
            new GotifyErrorResponse()
            {
                ErrorCode = statusCode,
                Error = error,
                ErrorDescription = description
            },
            contentType: Constants.ContentTypeJson,
            statusCode: statusCode
        );

    private static bool TryGetToken(HttpContext httpContext, out string? token)
    {
        if (httpContext.Request.Query.TryGetValue("token", out var requestToken))
        {
            token = requestToken.ToString();
            return true;
        }

        if (httpContext.Request.Headers.TryGetValue("X-Gotify-Key", out var headerToken))
        {
            token = headerToken.ToString();
            return true;
        }

        if (httpContext.Request.Headers.Authorization.Count > 0 &&
            httpContext.Request.Headers.Authorization[0]?.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase) == true
        )
        {
            token = (httpContext.Request.Headers.Authorization[0] ?? throw new Exception("Authorization header is null"))[7..].Trim();
            return true;
        }

        token = null;
        return false;
    }
}
