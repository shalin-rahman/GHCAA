using FluentValidation;
using GHCAA.Application.DTOs;
using System;

namespace GHCAA.Application.Validators
{
    public class MemberRegistrationValidator : AbstractValidator<MemberRegistrationDto>
    {
        public MemberRegistrationValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.NID)
                .NotEmpty()
                .Must(nid => nid != null && (nid.Length == 10 || nid.Length == 13 || nid.Length == 17))
                .WithMessage("NID must be 10, 13 or 17 digits")
                .Matches(@"^\d+$").WithMessage("NID must contain only digits");
            RuleFor(x => x.MobileNo).NotEmpty().Matches(@"^01\d{9}$").WithMessage("Mobile must be 11 digits and start with 01");
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateTime.UtcNow.Date).WithMessage("Invalid DOB");
            RuleFor(x => x.PaymentMethodId).GreaterThan(0).WithMessage("Please select a valid payment method");
        }
    }
}
