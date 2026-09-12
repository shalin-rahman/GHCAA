using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;
using GHCAA.Domain;

namespace GHCAA.API.Utils
{
    public class DateFormatConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateStr = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateStr)) return default;

            return ParseValue(dateStr);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // ISO-8601 so browser Date parsing (and Angular's DatePipe) is unambiguous;
            // dd-MM-yyyy on the wire was invalid input to `new Date(...)` client-side.
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture));
        }

        internal static DateTime ParseValue(string value)
        {
            var displayFormats = new[]
            {
                Constants.Localization.DayMonthYearDateFormat,
                Constants.Localization.UsDateFormat
            };

            if (DateTime.TryParseExact(
                    value,
                    displayFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
            {
                return date;
            }

            return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }
    }

    public class NullableDateFormatConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateStr = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateStr)) return null;

            return DateFormatConverter.ParseValue(dateStr);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture));
            else
                writer.WriteNullValue();
        }
    }
}
