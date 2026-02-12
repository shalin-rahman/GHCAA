using FluentValidation.TestHelper;
using GHCAA.Application.DTOs;
using GHCAA.Application.Validators;
using NUnit.Framework;

namespace GHCAA.Tests.Validators
{
    [TestFixture]
    public class VerifyEmailValidatorTests
    {
        private VerifyEmailValidator _validator = null!;

        [SetUp]
        public void Setup()
        {
            _validator = new VerifyEmailValidator();
        }

        [Test]
        public void Email_WhenEmpty_ShouldHaveValidationError()
        {
            var dto = new VerifyEmailDto { Email = "", OtpCode = "123456" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Test]
        public void Email_WhenInvalidFormat_ShouldHaveValidationError()
        {
            var dto = new VerifyEmailDto { Email = "invalid-email", OtpCode = "123456" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Test]
        public void Email_WhenValid_ShouldNotHaveValidationError()
        {
            var dto = new VerifyEmailDto { Email = "test@example.com", OtpCode = "123456" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        [Test]
        public void OtpCode_WhenEmpty_ShouldHaveValidationError()
        {
            var dto = new VerifyEmailDto { Email = "test@example.com", OtpCode = "" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.OtpCode);
        }

        [Test]
        public void OtpCode_WhenNot6Digits_ShouldHaveValidationError()
        {
            var dto = new VerifyEmailDto { Email = "test@example.com", OtpCode = "12345" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.OtpCode);
        }

        [Test]
        public void OtpCode_WhenContainsNonDigits_ShouldHaveValidationError()
        {
            var dto = new VerifyEmailDto { Email = "test@example.com", OtpCode = "12345a" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.OtpCode);
        }

        [Test]
        public void OtpCode_When6Digits_ShouldNotHaveValidationError()
        {
            var dto = new VerifyEmailDto { Email = "test@example.com", OtpCode = "123456" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.OtpCode);
        }

        [Test]
        public void ValidDto_ShouldPassValidation()
        {
            var dto = new VerifyEmailDto { Email = "test@example.com", OtpCode = "123456" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
