using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using GHCAA.Application.DTOs;

namespace GHCAA.Tests.Validators;

// TODO 84.36. The registration and verify-email rules moved from unused FluentValidation validators
// onto the DTOs. They apply to create and update requests, not to stored rows.
[TestFixture]
[Category("FR-01")]
public class MemberDtoValidationTests
{
    private static List<ValidationResult> Validate(object dto)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), results, true);
        return results;
    }

    private static T Fill<T>(T dto) where T : BaseMemberDto
    {
        dto.FullName = "Rahim Uddin";
        dto.FatherName = "Karim Uddin";
        dto.MotherName = "Amina Begum";
        dto.DateOfBirth = DateTime.UtcNow.Date.AddYears(-30);
        dto.NID = "1234567890";
        dto.MobileNo = "01712345678";
        dto.Email = "rahim@example.com";
        dto.PresentAddress = "Dhaka";
        dto.PermanentAddress = "Dhaka";
        dto.EmergencyContactName = "Karim Uddin";
        dto.EmergencyContactRelation = "Father";
        dto.EmergencyContactPhone = "01812345678";
        dto.AcademicHistory = new List<AcademicRecordDto>
        {
            new() { InstitutionName = "GHC", Degree = "HSC", Subject = "Science", PassingYear = 2010, IsOrgProfile = true }
        };
        return dto;
    }

    private static MemberRegistrationDto Registration() =>
        Fill(new MemberRegistrationDto { HasAcceptedTerms = true, HasAcceptedGdpr = true, PaymentMethodId = 1 });

    [Test]
    public void Registration_ValidRequest_HasNoErrors()
    {
        Validate(Registration()).Should().BeEmpty();
    }

    [TestCase("1234567890")]
    [TestCase("1234567890123")]
    [TestCase("12345678901234567")]
    public void Nid_OfAllowedLength_IsAccepted(string nid)
    {
        var dto = Registration();
        dto.NID = nid;
        Validate(dto).Should().BeEmpty();
    }

    [TestCase("123456")]
    [TestCase("123456789012")]
    [TestCase("12345abcde")]
    public void Nid_OfOtherLengthOrNotDigits_IsRejected(string nid)
    {
        var dto = Registration();
        dto.NID = nid;
        Validate(dto).Should().ContainSingle(r => r.MemberNames.Contains(nameof(BaseMemberDto.NID)));
    }

    [Test]
    public void DateOfBirth_UnderThirteen_IsRejected()
    {
        var dto = Registration();
        dto.DateOfBirth = DateTime.UtcNow.Date.AddYears(-12);
        Validate(dto).Should().ContainSingle(r => r.ErrorMessage!.Contains("at least 13"));
    }

    [Test]
    public void DateOfBirth_ThirteenToday_IsAccepted()
    {
        var dto = Registration();
        dto.DateOfBirth = DateTime.UtcNow.Date.AddYears(-13);
        Validate(dto).Should().BeEmpty();
    }

    [Test]
    public void Registration_NoAcademicRecord_IsRejected()
    {
        var dto = Registration();
        dto.AcademicHistory.Clear();
        Validate(dto).Should().ContainSingle(r => r.MemberNames.Contains(nameof(BaseMemberDto.AcademicHistory)));
    }

    [Test]
    public void DateOfBirth_OverOneHundredTwentyYearsAgo_IsRejected()
    {
        var dto = Registration();
        dto.DateOfBirth = DateTime.UtcNow.Date.AddYears(-121);
        Validate(dto).Should().ContainSingle(r => r.ErrorMessage!.Contains("too far in the past"));
    }

    [Test]
    public void ProfileUpdate_GetsTheSameAgeAndNidRules()
    {
        var dto = Fill(new UpdateProfileDto());
        dto.DateOfBirth = DateTime.UtcNow.Date.AddYears(-5);
        dto.NID = "123";
        var results = Validate(dto);
        results.Should().Contain(r => r.MemberNames.Contains(nameof(BaseMemberDto.NID)));

        dto.NID = "1234567890";
        Validate(dto).Should().ContainSingle(r => r.MemberNames.Contains(nameof(BaseMemberDto.DateOfBirth)));
    }

    [Test]
    public void Registration_FirstAcademicRecordNotInstitutional_IsRejected()
    {
        var dto = Registration();
        dto.AcademicHistory[0].IsOrgProfile = false;
        Validate(dto).Should().ContainSingle(r => r.MemberNames.Contains(nameof(BaseMemberDto.AcademicHistory)));
    }

    [TestCase(1950)]
    [TestCase(9999)]
    public void AcademicRecord_PassingYearOutOfRange_IsRejected(int year)
    {
        var record = new AcademicRecordDto { InstitutionName = "GHC", Degree = "HSC", Subject = "Science", PassingYear = year };
        Validate(record).Should().ContainSingle(r => r.MemberNames.Contains(nameof(AcademicRecordDto.PassingYear)));
    }

    [Test]
    public void AcademicRecord_PassingYearNextYear_IsRejected()
    {
        var record = new AcademicRecordDto { InstitutionName = "GHC", Degree = "HSC", Subject = "Science", PassingYear = DateTime.UtcNow.Year + 1 };
        Validate(record).Should().ContainSingle(r => r.MemberNames.Contains(nameof(AcademicRecordDto.PassingYear)));
    }

    [Test]
    public void VerifyEmail_ValidRequest_HasNoErrors()
    {
        Validate(new VerifyEmailDto { Email = "a@example.com", OtpCode = "123456" }).Should().BeEmpty();
    }

    [TestCase("12345")]
    [TestCase("1234567")]
    [TestCase("12a456")]
    public void VerifyEmail_OtpNotSixDigits_IsRejected(string code)
    {
        Validate(new VerifyEmailDto { Email = "a@example.com", OtpCode = code })
            .Should().ContainSingle(r => r.MemberNames.Contains(nameof(VerifyEmailDto.OtpCode)));
    }

    [Test]
    public void VerifyEmail_BadEmail_IsRejected()
    {
        Validate(new VerifyEmailDto { Email = "not-an-email", OtpCode = "123456" })
            .Should().ContainSingle(r => r.MemberNames.Contains(nameof(VerifyEmailDto.Email)));
    }
}
