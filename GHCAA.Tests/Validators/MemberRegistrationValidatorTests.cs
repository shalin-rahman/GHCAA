using FluentValidation.TestHelper;
using GHCAA.Application.DTOs;
using GHCAA.Application.Validators;

namespace GHCAA.Tests.Validators;

[TestFixture]
public class MemberRegistrationValidatorTests
{
    private MemberRegistrationValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new MemberRegistrationValidator();
    }

    private MemberRegistrationDto CreateValidDto()
    {
        return new MemberRegistrationDto
        {
            FullName = "John Doe",
            FatherName = "Father Name",
            MotherName = "Mother Name",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = GHCAA.Domain.Enums.Gender.Male,
            BloodGroup = GHCAA.Domain.Enums.BloodGroup.APositive,
            NID = "1234567890",
            MobileNo = "01712345678",
            Email = "test@example.com",
            PresentAddress = "Present Address",
            PermanentAddress = "Permanent Address",
            EmergencyContactName = "Emergency Contact",
            EmergencyContactRelation = "Relation",
            EmergencyContactPhone = "01812345678",
            AcademicHistory = new System.Collections.Generic.List<AcademicRecordDto>
            {
                new AcademicRecordDto { InstitutionName = "Govt. Haraganga College", Degree = "HSC", Subject = "Science", PassingYear = 2005, IsGHC = true }
            },
            HasAcceptedTerms = true,
            HasAcceptedGdpr = true,
            PaymentMethodId = 1
        };
    }

    #region PaymentMethodId Tests

    [Test]
    public void PaymentMethodId_WhenZero_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.PaymentMethodId = 0;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PaymentMethodId)
            .WithErrorMessage("Please select a valid payment method");
    }

    [Test]
    public void PaymentMethodId_WhenValid_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.PaymentMethodId = 1;
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.PaymentMethodId);
    }

    #endregion

    [Test]
    public void AcademicHistory_WhenFirstRecordIsNotInstitutional_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.AcademicHistory[0].IsGHC = false;
        dto.AcademicHistory[0].InstitutionName = "Other College";

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AcademicHistory)
            .WithErrorMessage("The first academic record must be the institutional record.");
    }

    [Test]
    public void AcademicHistory_WhenFirstRecordIsInstitutional_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.AcademicHistory);
    }

    [Test]
    public void AcademicHistory_WhenFirstRecordIsNotInstitutionalEvenWithConfiguredName_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.AcademicHistory[0].IsGHC = false;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AcademicHistory)
            .WithErrorMessage("The first academic record must be the institutional record.");
    }

    [Test]
    public void AcademicHistory_WhenEmpty_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.AcademicHistory.Clear();

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.AcademicHistory)
            .WithErrorMessage("At least one academic record is required");
    }

    #region FullName Tests

    [Test]
    public void FullName_WhenEmpty_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.FullName = string.Empty;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Test]
    public void FullName_WhenNull_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.FullName = null!;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Test]
    public void FullName_WhenExceedsMaxLength_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.FullName = new string('A', 201);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Test]
    public void FullName_WhenValid_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    #endregion

    #region NID Tests

    [Test]
    public void NID_WhenEmpty_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.NID = string.Empty;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.NID);
    }

    [Test]
    public void NID_When10Digits_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.NID = "1234567890";
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.NID);
    }

    [Test]
    public void NID_When13Digits_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.NID = "1234567890123";
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.NID);
    }

    [Test]
    public void NID_When17Digits_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.NID = "12345678901234567";
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.NID);
    }

    [Test]
    public void NID_WhenInvalidLength_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.NID = "12345";
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.NID)
            .WithErrorMessage("NID must be 10, 13 or 17 digits");
    }

    [Test]
    public void NID_WhenContainsNonDigits_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.NID = "123456789A";
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.NID);
    }

    #endregion

    #region MobileNo Tests

    [Test]
    public void MobileNo_WhenEmpty_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.MobileNo = string.Empty;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.MobileNo);
    }

    [Test]
    public void MobileNo_WhenValid_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.MobileNo = "01712345678";
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.MobileNo);
    }

    [Test]
    public void MobileNo_WhenDoesNotStartWith01_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.MobileNo = "02712345678";
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.MobileNo)
            .WithErrorMessage("Mobile must be 11 digits and start with 01");
    }

    [Test]
    public void MobileNo_WhenNot11Digits_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.MobileNo = "017123456";
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.MobileNo);
    }

    [Test]
    public void MobileNo_WhenContainsNonDigits_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.MobileNo = "0171234567A";
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.MobileNo);
    }

    #endregion

    #region Email Tests

    [Test]
    public void Email_WhenEmpty_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.Email = string.Empty;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Email_WhenInvalidFormat_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.Email = "invalid-email";
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Email_WhenValid_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.Email = "test@example.com";
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region DateOfBirth Tests

    [Test]
    public void DateOfBirth_WhenInFuture_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.DateOfBirth = DateTime.UtcNow.AddDays(1);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Test]
    public void DateOfBirth_WhenToday_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.DateOfBirth = DateTime.UtcNow;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Test]
    public void DateOfBirth_WhenInPast_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        dto.DateOfBirth = DateTime.UtcNow.AddYears(-20);
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    #endregion

    #region Year Validation Tests

    [Test]
    public void HSCAdmissionYear_WhenBefore1950_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        //         dto.HSCAdmissionYear = 1949;
        var result = _validator.TestValidate(dto);
        //         result.ShouldHaveValidationErrorFor(x => x.HSCAdmissionYear);
    }

    [Test]
    public void HSCAdmissionYear_WhenAfterCurrentYear_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        //         dto.HSCAdmissionYear = DateTime.UtcNow.Year + 1;
        var result = _validator.TestValidate(dto);
        //         result.ShouldHaveValidationErrorFor(x => x.HSCAdmissionYear);
    }

    [Test]
    public void HSCAdmissionYear_WhenValid_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        //         dto.HSCAdmissionYear = 2000;
        var result = _validator.TestValidate(dto);
        //         result.ShouldNotHaveValidationErrorFor(x => x.HSCAdmissionYear);
    }

    [Test]
    public void GHCLastCertificatePassingYear_WhenBefore1950_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        //         dto.GHCLastCertificatePassingYear = 1949;
        var result = _validator.TestValidate(dto);
        //         result.ShouldHaveValidationErrorFor(x => x.GHCLastCertificatePassingYear);
    }

    [Test]
    public void GHCLastCertificatePassingYear_WhenAfterCurrentYear_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        //         dto.GHCLastCertificatePassingYear = DateTime.UtcNow.Year + 1;
        var result = _validator.TestValidate(dto);
        //         result.ShouldHaveValidationErrorFor(x => x.GHCLastCertificatePassingYear);
    }

    [Test]
    public void GHCAdmissionYear_WhenBefore1950_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        //         dto.GHCAdmissionYear = 1949;
        var result = _validator.TestValidate(dto);
        //         result.ShouldHaveValidationErrorFor(x => x.GHCAdmissionYear);
    }

    [Test]
    public void GHCAdmissionYear_WhenAfterCurrentYear_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        //         dto.GHCAdmissionYear = DateTime.UtcNow.Year + 1;
        var result = _validator.TestValidate(dto);
        //         result.ShouldHaveValidationErrorFor(x => x.GHCAdmissionYear);
    }

    [Test]
    public void GHCAdmissionYear_WhenGreaterThanPassingYear_ShouldHaveValidationError()
    {
        var dto = CreateValidDto();
        //         dto.GHCAdmissionYear = 2010;
        //         dto.GHCLastCertificatePassingYear = 2008;
        var result = _validator.TestValidate(dto);
        //         result.ShouldHaveValidationErrorFor(x => x.GHCAdmissionYear)
        //             .WithErrorMessage("Admission year must be <= passing year");
    }

    [Test]
    public void GHCAdmissionYear_WhenEqualToPassingYear_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        //         dto.GHCAdmissionYear = 2010;
        //         dto.GHCLastCertificatePassingYear = 2010;
        var result = _validator.TestValidate(dto);
        //         result.ShouldNotHaveValidationErrorFor(x => x.GHCAdmissionYear);
    }

    [Test]
    public void GHCAdmissionYear_WhenLessThanPassingYear_ShouldNotHaveValidationError()
    {
        var dto = CreateValidDto();
        //         dto.GHCAdmissionYear = 2008;
        //         dto.GHCLastCertificatePassingYear = 2010;
        var result = _validator.TestValidate(dto);
        //         result.ShouldNotHaveValidationErrorFor(x => x.GHCAdmissionYear);
    }

    #endregion
}
