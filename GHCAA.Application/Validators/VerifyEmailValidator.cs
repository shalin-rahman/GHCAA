using FluentValidation;

namespace GHCAA.Application.Validators
{
    public class VerifyEmailValidator : AbstractValidator<DTOs.VerifyEmailDto>
    {
        public VerifyEmailValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.OtpCode)
                .NotEmpty()
                .Matches(@"^\d{6}$")
                .WithMessage("OTP code must be exactly 6 digits");
        }
    }
}
