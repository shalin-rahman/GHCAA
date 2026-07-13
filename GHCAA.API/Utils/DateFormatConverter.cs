using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace GHCAA.API.Utils
{
    public class DateFormatConverter : JsonConverter<DateTime>
    {
        private readonly string _format = "dd-MM-yyyy";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateStr = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateStr)) return default;

            if (DateTime.TryParseExact(dateStr, _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            // Fallback to standard parsing if dd-MM-yyyy fails
            return DateTime.Parse(dateStr);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // ISO-8601 so browser Date parsing (and Angular's DatePipe) is unambiguous;
            // dd-MM-yyyy on the wire was invalid input to `new Date(...)` client-side.
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture));
        }
    }

    public class NullableDateFormatConverter : JsonConverter<DateTime?>
    {
        private readonly string _format = "dd-MM-yyyy";

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateStr = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateStr)) return null;

            if (DateTime.TryParseExact(dateStr, _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            return DateTime.Parse(dateStr);
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
