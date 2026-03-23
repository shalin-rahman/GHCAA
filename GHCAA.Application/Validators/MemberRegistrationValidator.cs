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
            RuleFor(x => x.NID).NotEmpty().Length(6, 20).WithMessage("NID must be between 6 and 20 characters");
            RuleFor(x => x.MobileNo).NotEmpty().Matches(@"^01\d{9}$").WithMessage("Mobile must be 11 digits and start with 01");
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.DateOfBirth).LessThan(DateTime.UtcNow).WithMessage("Invalid DOB");
            RuleFor(x => x.PaymentMethodId).GreaterThan(0).WithMessage("Please select a valid payment method");
        }
    }
}
