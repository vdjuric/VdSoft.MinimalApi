// **************************************************************
// Licensed under the MIT license.
// Original author: https://vladimir.dev
// **************************************************************

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using VdSoft.MinimalApi.GotifyToPushover.Models;

namespace VdSoft.MinimalApi.GotifyToPushover;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers <paramref name="endpoint"/> simple Gotify adapter which translates very basic Gotify JSON request
    /// to Pushover JSON request and forwards it to public Pushover API (https://api.pushover.net/1/messages.json)
    /// 
    /// You must also provide <paramref name="options"/> parameter where you must define mandatory property
    /// <seealso cref="GotifyToPushoverOptions.GetPushoverUserByToken"/>, which returns Pushover user for given token.
    /// 
    /// For Pushover token we are forwarding Gotify token by default but you can override this in <seealso cref="GotifyToPushoverOptions.GotifyTokenToPushoverToken"/>.
    /// 
    /// Registration example:
    /// 
    ///    using VdSoft.MinimalApi.GotifyToPushover;
    ///    //...
    ///    var builder = WebApplication.CreateBuilder(args);
    ///    //...
    ///    //var app = builder.Build();
    /// 
    ///    app.MapGotifyToPushover(new GotifyToPushoverOptions()
    ///    {
    ///        //required
    ///        GetPushoverUserByToken = token => "YOUR_PUSHOVER_USER",
    ///        
    ///        //optional configuration
    ///        GotifyPriorityToPushoverPriority = priority => 0, //translates Gotify priority to fixed Pushover priority 0
    ///        CanContinueAsync = async (httpContext, gotifyToken, req, cancellationToken) => {
    ///            //you can check request before forwarding it to Pushover API
    ///            if (gotifyToken == "EXPECTED_GOTIFY_TOKEN")
    ///            {
    ///                return (true, null); //allow request
    ///            }
    ///    
    ///            await Task.Delay(Random.Shared.Next(100, 500), cancellationToken);
    ///            return (false, "Denied"); //deny request
    ///        }
    ///    }).RequireHost("api.your-server.example"); //RequireHost is also optional
    /// 
    /// This example would register Gotify endpoint http(s)://api.your-server.example/gotify-to-pushover/message
    /// 
    /// </summary>
    public static IEndpointConventionBuilder MapGotifyToPushover(this IEndpointRouteBuilder endpoints,
        string endpoint,
        GotifyToPushoverOptions options
    ) => endpoints.MapPost(endpoint, (HttpContext httpContext, GotifyBasicMessageRequest req, CancellationToken cancellationToken) =>
            Adapter.GotifyToPushoverHandlerAsync(
                httpContext,
                req,
                options,
                cancellationToken
            )
        )
        .Accepts<GotifyBasicMessageRequest>(Constants.ContentTypeJson)
        .Produces<GotifyBasicMessageResponse>(200, Constants.ContentTypeJson)
        .Produces<GotifyErrorResponse>(400, Constants.ContentTypeJson)
        .Produces<GotifyErrorResponse>(401, Constants.ContentTypeJson)
        .Produces<GotifyErrorResponse>(403, Constants.ContentTypeJson)
        .Produces<GotifyErrorResponse>(500, Constants.ContentTypeJson);

    /// <summary>
    /// Registers endpoint with default name "/gotify-to-pushover/message"
    /// 
    /// <seealso cref="MapGotifyToPushover(IEndpointRouteBuilder, GotifyToPushoverOptions)"/>.
    /// </summary>
    public static IEndpointConventionBuilder MapGotifyToPushover(this IEndpointRouteBuilder endpoints,
        GotifyToPushoverOptions options
    ) => endpoints.MapGotifyToPushover(Constants.DefaultEndpointName, options);

    /// <summary>
    /// Registers endpoint with default name "/gotify-to-pushover/message" with minimal configuration.
    /// 
    /// <seealso cref="MapGotifyToPushover(IEndpointRouteBuilder, GotifyToPushoverOptions)"/> for more configuration options.
    /// </summary>
    public static IEndpointConventionBuilder MapGotifyToPushover(this IEndpointRouteBuilder endpoints,
        Func<string, string> getPushoverUserByToken
    ) => endpoints.MapGotifyToPushover(Constants.DefaultEndpointName, GotifyToPushoverOptions.CreateBasic(getPushoverUserByToken));

    /// <summary>
    /// Registers <paramref name="endpoint"/> endpoint with minimal configuration.
    /// 
    /// <seealso cref="MapGotifyToPushover(IEndpointRouteBuilder, GotifyToPushoverOptions)"/> for more configuration options.
    /// </summary>
    public static IEndpointConventionBuilder MapGotifyToPushover(this IEndpointRouteBuilder endpoints,
        string endpoint, Func<string, string> getPushoverUserByToken
    ) => endpoints.MapGotifyToPushover(endpoint, GotifyToPushoverOptions.CreateBasic(getPushoverUserByToken));
}
