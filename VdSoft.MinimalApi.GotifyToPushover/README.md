This simple project provides a minimal API endpoint adapter for ASP.NET Core that transforms [Gotify notification requests](https://gotify.net/) into [Pushover-compatible requests](https://pushover.net) and forwards them to the [Pushover public API](https://pushover.net/api).
The project has no external library dependencies.

### Sending test message with cURL

```bash
curl -v -X POST "https://api.your-server.example/gotify-to-pushover/message" \
  -H "X-Gotify-Key: SECRET_VALUE" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test message",
    "message": "Just a test :)"
  }'
```

This sends a Gotify request that is converted and forwarded to Pushover.

---

A minimal Gotify-to-Pushover API endpoint is available as a NuGet package that you can easily include in your ASP.NET Core project: [VdSoft.MinimalApi.GotifyToPushover](https://www.nuget.org/packages/VdSoft.MinimalApi.GotifyToPushover/)
```xml
<PackageReference Include="VdSoft.MinimalApi.GotifyToPushover" Version="1.0.0" />
```

Alternatively, you can include the source code directly and adapt it to your needs.

### Motivation
[Proxmox Virtual Environment](https://www.proxmox.com) offers Gotify notifications, but the iOS client for receiving notifications is unfortunately not officially supported. For this reason, I implemented simple minimal API endpoint for .NET Core that listens for Gotify notifications and translates them on the fly into [Pushover](https://pushover.net) requests.

> [!NOTE]
> The current implementation is minimal and supports only my use case with Proxmox (and other use cases where Gotify JSON requests are sent). You can adapt the code for your own use case, and if possible, you're welcome to submit a PR.

### ASP.NET Core, registration example

```csharp
using VdSoft.MinimalApi.GotifyToPushover;
//...
var builder = WebApplication.CreateBuilder(args);
//...
var app = builder.Build();

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
