Implements a minimal API endpoint adapter that converts [Gotify notification](https://gotify.net/) requests into [Pushover](https://pushover.net) requests and calls the [Pushover public API](https://pushover.net/api).

Project doesn't have any third-party dependencies.

Motivation: [Proxmox Virtual Environment](https://www.proxmox.com) offers Gotify notifications, but the iOS client for receiving notifications is unfortunately not officially supported. For this reason, I implemented simple minimal API endpoint for .NET Core that listens for Gotify notifications and translates them on the fly into [Pushover](https://pushover.net) requests.

Note: The current implementation is minimal and supports only my use case with Proxmox (and other use cases where Gotify JSON requests are sent). You can adapt the code for your own use case, and if possible, you're welcome to submit a PR.

### ASP.NET Core, registration example

```csharp
using VdSoft.MinimalApi.GotifyToPushover;
//...
var builder = WebApplication.CreateBuilder(args);
//...
//var app = builder.Build();

app.MapGotifyToPushover(new GotifyToPushoverOptions()
{
    //required
    GetPushoverUserByToken = token => "YOUR_PUSHOVER_USER",
    
    //optional configuration
    GotifyPriorityToPushoverPriority = priority => 0, //translates Gotify priority to fixed Pushover priority 0
    CanContinueAsync = async (httpContext, gotifyToken, req, cancellationToken) => {
        //you can check request before forwarding it to Pushover API
        if (gotifyToken == "EXPECTED_GOTIFY_TOKEN")
        {
            return (true, null); //allow request
        }

        await Task.Delay(Random.Shared.Next(100, 500), cancellationToken);
        return (false, "Denied"); //deny request
    }
}).RequireHost("api.your-server.example"); //RequireHost is also optional

```
