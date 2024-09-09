# Minimal API extensions and utilities for ASP.NET Core

## VdSoft.MinimalApi.GotifyToPushover
Implements a minimal API endpoint adapter that converts Gotify notification requests into [Pushover](https://pushover.net) requests and calls the [Pushover public API](https://pushover.net/api).

Project doesn't have any third-party dependencies.

Motivation: [Proxmox Virtual Environment](https://www.proxmox.com) offers Gotify notifications, the iOS client for receiving notifications is unfortunately not officially supported. For this reason, I implemented simple minimal API endpoint for .NET Core that listens for Gotify notifications and translates them on the fly into [Pushover](https://pushover.net) requests.

Note: The current implementation is minimal and supports only my use case with Proxmox (and other use cases where Gotify JSON requests are sent). You can adapt the code for your own use case, and if possible, you're welcome to submit a PR.
