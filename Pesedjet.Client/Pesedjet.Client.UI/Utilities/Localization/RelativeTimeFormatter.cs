using System;
using System.Globalization;

namespace Pesedjet.Client.UI.Utilities.Localization;

public static class RelativeTimeFormatter
{
    private const int DaysPerMonth = 30;
    private const string MinutesAgoKey = "Common.MinutesAgo";
    private const string HoursAgoKey = "Common.HoursAgo";
    private const string YesterdayKey = "Common.Yesterday";
    private const string DaysAgoKey = "Common.DaysAgo";
    private const string MonthAgoKey = "Common.MonthAgo";
    private const string MonthsAgoKey = "Common.MonthsAgo";

    public static string Format(DateTimeOffset moment, DateTimeOffset now)
    {
        TimeSpan elapsed = now - moment;
        string text;

        if (elapsed < TimeSpan.FromHours(1))
        {
            text = FormatAmount(MinutesAgoKey, Math.Max(1, (int)elapsed.TotalMinutes));
        }
        else if (elapsed < TimeSpan.FromDays(1))
        {
            text = FormatAmount(HoursAgoKey, (int)elapsed.TotalHours);
        }
        else if (elapsed < TimeSpan.FromDays(2))
        {
            text = LocalizationManager.Instance[YesterdayKey];
        }
        else if (elapsed < TimeSpan.FromDays(DaysPerMonth))
        {
            text = FormatAmount(DaysAgoKey, (int)elapsed.TotalDays);
        }
        else
        {
            int months = (int)(elapsed.TotalDays / DaysPerMonth);

            text = months == 1
                ? LocalizationManager.Instance[MonthAgoKey]
                : FormatAmount(MonthsAgoKey, months);
        }

        return text;
    }

    private static string FormatAmount(string key, int amount)
    {
        return string.Format(CultureInfo.CurrentCulture, LocalizationManager.Instance[key], amount);
    }
}
