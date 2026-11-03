using FluentAssertions;
using GHCAA.Domain.Models;

namespace GHCAA.Tests.Utils
{
    [TestFixture]
    [Category("FR-05")]
    public class BatchLabelTests
    {
        private static AcademicRecord Rec(int id, string degree, string subject, int year, bool org = true) => new()
        {
            Id = id,
            InstitutionName = "College",
            Degree = degree,
            Subject = subject,
            PassingYear = year,
            IsOrgProfile = org
        };

        [Test]
        public void BatchLabel_ManyOrgRecords_ListsEachEarliestFirst()
        {
            var records = new[]
            {
                Rec(1, "MSc", "English", 1996),
                Rec(2, "HSC", "Science", 1980),
                Rec(3, "BA", "Pass", 1991),
            };

            AcademicRecord.BatchLabel(records).Should().Be("HSC (Science) 1980 / BA (Pass) 1991 / MSc (English) 1996");
        }

        [Test]
        public void BatchLabel_IgnoresRecordsFromOtherInstitutions()
        {
            var records = new[] { Rec(1, "HSC", "Science", 2001), Rec(2, "PhD", "Physics", 2010, org: false) };

            AcademicRecord.BatchLabel(records).Should().Be("HSC (Science) 2001");
        }

        [Test]
        public void BatchLabel_DropsPlaceholderSubjectAndMissingYear()
        {
            var records = new[] { Rec(1, "BSc", "None", 1982), Rec(2, "MA", "N/A", 0) };

            AcademicRecord.BatchLabel(records).Should().Be("BSc 1982 / MA");
        }

        [Test]
        public void BatchLabel_NullOrBlankFields_AreSkippedWithoutThrowing()
        {
            var records = new AcademicRecord?[]
            {
                null,
                new() { Id = 1, InstitutionName = "College", Degree = null!, Subject = null!, PassingYear = 1990, IsOrgProfile = true },
                new() { Id = 2, InstitutionName = "College", Degree = "  ", Subject = "Science", PassingYear = 0, IsOrgProfile = true },
                new() { Id = 3, InstitutionName = "College", Degree = "", Subject = "", PassingYear = 0, IsOrgProfile = true },
            };

            AcademicRecord.BatchLabel(records!).Should().Be("1990 / Science");
            AcademicRecord.EarliestOrgRecord(records!)!.Id.Should().Be(1);
        }

        [Test]
        public void BatchLabel_NoOrgRecords_ReturnsNull()
        {
            AcademicRecord.BatchLabel(null).Should().BeNull();
            AcademicRecord.BatchLabel(Array.Empty<AcademicRecord>()).Should().BeNull();
            AcademicRecord.BatchLabel(new[] { Rec(1, "HSC", "Science", 2001, org: false) }).Should().BeNull();
        }

        [Test]
        public void EarliestOrgRecord_PicksLowestYear_AndPutsMissingYearLast()
        {
            var records = new[] { Rec(1, "MA", "History", 0), Rec(2, "BSc", "Physics", 1985), Rec(3, "HSC", "Science", 1981) };

            AcademicRecord.EarliestOrgRecord(records)!.Id.Should().Be(3);
        }
    }
}
