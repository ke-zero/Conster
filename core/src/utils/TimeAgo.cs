namespace Conster.Core.utils;

using System;

public static class TimeAgo
{
    public static string Parse(DateTime dateTime, bool isUtc = true)
    {
        var now = isUtc ? DateTime.UtcNow : DateTime.Now;
        var time = now - dateTime;

        if (time.TotalSeconds <= 60)
            return "Just now";
        if (time.TotalMinutes <= 1)
            return "A few seconds ago";
        if (time.TotalMinutes <= 60)
            return $"{(int)time.TotalMinutes} minute{GetPluralSuffix(time.TotalMinutes)} ago";
        if (time.TotalHours <= 24)
            return $"{(int)time.TotalHours} hour{GetPluralSuffix(time.TotalHours)} ago";
        if (time.TotalDays <= 7)
            return $"{(int)time.TotalDays} day{GetPluralSuffix(time.TotalDays)} ago";
        if (time.TotalDays <= 30)
            return $"{(int)(time.TotalDays / 7)} week{GetPluralSuffix(time.TotalDays / 7)} ago";
        if (time.TotalDays <= 365)
            return $"{(int)(time.TotalDays / 30)} month{GetPluralSuffix(time.TotalDays / 30)} ago";
        else
            return $"{(int)(time.TotalDays / 365)} year{GetPluralSuffix(time.TotalDays / 365)} ago";
    }

    private static string GetPluralSuffix(double value)
    {
        return value > 1 ? "s" : "";
    }
}