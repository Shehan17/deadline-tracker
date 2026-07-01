namespace DeadlineTracker.Utilities;

public static class DeadlineTimeFormatter
{
    public static string FormatRemainingTime(DateTimeOffset deadlineAt, DateTimeOffset currentTime)
    {
        if (deadlineAt <= currentTime)
            return "Overdue";

        TimeSpan remaining = deadlineAt - currentTime;

        if (remaining.TotalDays >= 1)
            return $"{remaining.Days}d {remaining.Hours}h {remaining.Minutes}m {remaining.Seconds}s";

        if (remaining.TotalHours >= 1)
            return $"{remaining.Hours}h {remaining.Minutes}m {remaining.Seconds}s";

        if (remaining.TotalMinutes >= 1)
            return $"{remaining.Minutes}m {remaining.Seconds}s";

        return $"{remaining.Seconds}s";
    }
}
