using System.Runtime.CompilerServices;

namespace SimpleScan.Domain.Common;

internal static class Guard
{
    public static string NotWhiteSpace(string value, [CallerMemberName] string parameterName = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }

    public static int Positive(int value, [CallerMemberName] string parameterName = "")
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value, parameterName);
        return value;
    }

    public static DateTime Utc(DateTime value, [CallerMemberName] string parameterName = "")
    {
        if (value.Kind == DateTimeKind.Local)
        {
            throw new ArgumentException("DateTime must be UTC or unspecified.", parameterName);
        }

        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
