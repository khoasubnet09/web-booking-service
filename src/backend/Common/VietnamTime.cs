namespace ServiceBooking.Api.Common;

public static class VietnamTime
{
    public static TimeZoneInfo Zone { get; } = ResolveZone();

    public static DateOnly Today =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Zone).DateTime);

    public static DateTimeOffset ToUtc(DateOnly date, TimeOnly time)
    {
        var localDateTime = date.ToDateTime(time, DateTimeKind.Unspecified);
        var offset = Zone.GetUtcOffset(localDateTime);

        return new DateTimeOffset(localDateTime, offset).ToUniversalTime();
    }

    public static DateTimeOffset ToVietnamTime(DateTimeOffset utcTime)
    {
        return TimeZoneInfo.ConvertTime(utcTime, Zone);
    }

    private static TimeZoneInfo ResolveZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
    }
}
