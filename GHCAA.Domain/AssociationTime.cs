using System.Globalization;

namespace GHCAA.Domain;

// The Association keeps Bangladesh time. There is no org setting for the zone yet (37.14o).
// Linux knows the zone as "Asia/Dhaka" and older Windows as "Bangladesh Standard Time"; if neither
// is installed, Bangladesh has had no daylight saving since 2009, so a fixed +6 hours is right.
public static class AssociationTime
{
    public static DateTime FromUtc(DateTime utc)
    {
        try { return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.FindSystemTimeZoneById("Asia/Dhaka")); }
        catch (TimeZoneNotFoundException)
        {
            try { return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time")); }
            catch (TimeZoneNotFoundException) { return utc.AddHours(6); }
        }
    }

    public static string FormatReceiptTimestamp(DateTime utc) =>
        FromUtc(utc).ToString("dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
}
