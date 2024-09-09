namespace VdSoft.MinimalApi.GotifyToPushover.Helpers;

public static class PushoverHelper
{
    public static string TruncateLongTitle(this string pushoverTitle)
    {
        if (pushoverTitle.Length > Constants.PushoverTitleMaxLength)
        {
            return pushoverTitle[..Constants.PushoverTitleMaxLength];
        }

        return pushoverTitle;
    }

    public static string TruncateLongMessage(this string pushoverMessage)
    {
        if (pushoverMessage.Length > Constants.PushoverMessageMaxLength)
        {
            return pushoverMessage[..Constants.PushoverMessageMaxLength];
        }

        return pushoverMessage;
    }
}
