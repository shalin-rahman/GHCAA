using System;
using System.Text.Json;
using FluentAssertions;
using GHCAA.API.Utils;
using NUnit.Framework;

namespace GHCAA.Tests.Utils
{
    /// <summary>
    /// TODO 27.7 — these two converters are registered in the global JsonSerializerOptions
    /// (GHCAA.API/Program.cs), so they govern every DateTime crossing the wire, in both
    /// directions, for every endpoint. The contract pinned here is the one settled in 29F.3:
    ///   * WRITE = ISO-8601 ("yyyy-MM-ddTHH:mm:ss.fff") — unambiguous to `new Date(...)` and DatePipe.
    ///   * READ  = dd-MM-yyyy accepted first (legacy clients / form input), ISO accepted as fallback.
    /// A regression back to dd-MM-yyyy on the write side breaks web and mobile simultaneously
    /// and is silent at compile time, which is why these tests exist.
    /// </summary>
    [TestFixture]
    public class DateFormatConverterTests
    {
        private JsonSerializerOptions _options = null!;

        [SetUp]
        public void Setup()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new DateFormatConverter());
            _options.Converters.Add(new NullableDateFormatConverter());
        }

        private string Write<T>(T value) => JsonSerializer.Serialize(value, _options);
        private T Read<T>(string json) => JsonSerializer.Deserialize<T>(json, _options)!;

        // ---------- DateFormatConverter: Write ----------

        [Test]
        public void Write_EmitsIso8601_NotDdMmYyyy()
        {
            var value = new DateTime(2026, 8, 22, 14, 30, 45, 123);

            Write(value).Should().Be("\"2026-08-22T14:30:45.123\"");
        }

        [Test]
        public void Write_PreservesTimeComponent()
        {
            // The wire format carries time, so a timestamp must not be truncated to a date.
            Write(new DateTime(2026, 1, 2, 23, 59, 59)).Should().Be("\"2026-01-02T23:59:59.000\"");
        }

        [Test]
        public void Write_MidnightStillEmitsFullIsoShape()
        {
            Write(new DateTime(2026, 1, 2)).Should().Be("\"2026-01-02T00:00:00.000\"");
        }

        // ---------- DateFormatConverter: Read ----------

        [Test]
        public void Read_AcceptsLegacyDdMmYyyy()
        {
            Read<DateTime>("\"22-08-2026\"").Should().Be(new DateTime(2026, 8, 22));
        }

        [Test]
        public void Read_PrefersDayFirst_WhenInputIsAmbiguous()
        {
            // "02-03-2026" is valid as both dd-MM and MM-dd. dd-MM-yyyy is tried first,
            // so this must be 2 March, never 3 February. Changing this silently shifts dates.
            Read<DateTime>("\"02-03-2026\"").Should().Be(new DateTime(2026, 3, 2));
        }

        [Test]
        public void Read_AcceptsIso8601DateOnly()
        {
            Read<DateTime>("\"2026-08-22\"").Should().Be(new DateTime(2026, 8, 22));
        }

        [Test]
        public void Read_AcceptsIso8601WithTime_AndKeepsTheTime()
        {
            Read<DateTime>("\"2026-08-22T14:30:45\"").Should().Be(new DateTime(2026, 8, 22, 14, 30, 45));
        }

        [Test]
        public void Read_RoundTripsWhatWriteProduced()
        {
            var original = new DateTime(2026, 8, 22, 14, 30, 45, 123);

            Read<DateTime>(Write(original)).Should().Be(original);
        }

        [Test]
        public void Read_EmptyString_YieldsDefault()
        {
            // Documented current behaviour: empty is swallowed into default(DateTime) rather
            // than rejected. Callers that care must validate before deserializing.
            Read<DateTime>("\"\"").Should().Be(default(DateTime));
        }

        [Test]
        public void Read_WhitespaceOnly_YieldsDefault()
        {
            Read<DateTime>("\"   \"").Should().Be(default(DateTime));
        }

        [Test]
        public void Read_MalformedString_Throws_RatherThanSilentlyDefaulting()
        {
            // The important half of this assertion is that it does NOT return default(DateTime):
            // a silent 01-01-0001 in a payload is far worse than a 400.
            Action act = () => Read<DateTime>("\"not-a-date\"");

            act.Should().Throw<FormatException>();
        }

        [Test]
        public void Read_ImpossibleCalendarDate_Throws()
        {
            Action act = () => Read<DateTime>("\"32-13-2026\"");

            act.Should().Throw<FormatException>();
        }

        // ---------- NullableDateFormatConverter ----------

        [Test]
        public void NullableWrite_Null_EmitsJsonNull()
        {
            Write<DateTime?>(null).Should().Be("null");
        }

        [Test]
        public void NullableWrite_Value_EmitsSameIsoShapeAsNonNullable()
        {
            var value = new DateTime(2026, 8, 22, 14, 30, 45, 123);

            Write<DateTime?>(value).Should().Be(Write(value));
        }

        [Test]
        public void NullableRead_JsonNull_YieldsNull()
        {
            Read<DateTime?>("null").Should().BeNull();
        }

        [Test]
        public void NullableRead_EmptyString_YieldsNull_NotDefaultDate()
        {
            // Divergence from the non-nullable converter, and the correct behaviour here:
            // an absent optional date must stay absent, not become 01-01-0001.
            Read<DateTime?>("\"\"").Should().BeNull();
        }

        [Test]
        public void NullableRead_AcceptsLegacyDdMmYyyy()
        {
            Read<DateTime?>("\"22-08-2026\"").Should().Be(new DateTime(2026, 8, 22));
        }

        [Test]
        public void NullableRead_RoundTripsWhatWriteProduced()
        {
            DateTime? original = new DateTime(2026, 8, 22, 14, 30, 45, 123);

            Read<DateTime?>(Write(original)).Should().Be(original);
        }

        [Test]
        public void NullableRead_MalformedString_Throws()
        {
            Action act = () => Read<DateTime?>("\"not-a-date\"");

            act.Should().Throw<FormatException>();
        }

        // ---------- Cross-converter consistency ----------

        [Test]
        public void BothConverters_ProduceIdenticalWireFormat_ForTheSameInstant()
        {
            var value = new DateTime(2026, 12, 31, 1, 2, 3, 4);

            Write(value).Should().Be(Write<DateTime?>(value));
        }
    }
}
