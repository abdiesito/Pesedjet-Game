using System;
using System.Globalization;
using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.Utilities.Localization;

public static class PresenceFormatter
{
    private const string OnlineKey = "Presence.Online";
    private const string InMatchKey = "Presence.InMatch";
    private const string OfflineKey = "Presence.Offline";
    private const string OfflineSinceKey = "Presence.OfflineSince";

    public static string Format(PresenceStatus presence, DateTimeOffset? lastSeenAt, DateTimeOffset now)
    {
        string text = LocalizationManager.Instance[OfflineKey];

        if (presence == PresenceStatus.Online)
        {
            text = LocalizationManager.Instance[OnlineKey];
        }
        else if (presence == PresenceStatus.InMatch)
        {
            text = LocalizationManager.Instance[InMatchKey];
        }
        else if (lastSeenAt is not null)
        {
            text = string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[OfflineSinceKey],
                RelativeTimeFormatter.Format(lastSeenAt.Value, now));
        }

        return text;
    }
}
