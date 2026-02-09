GHCAA.Application\Validators\MemberRegistrationValidator.cs
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
            RuleFor(x => x.NID).NotEmpty().Matches(@"^\d{10}|\d{13}|\d{17}$").WithMessage("NID must be 10, 13 or 17 digits");
            RuleFor(x => x.MobileNo).NotEmpty().Matches(@"^01\d{9}$").WithMessage("Mobile must be 11 digits and start with 01");
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.DateOfBirth).LessThan(DateTime.UtcNow).WithMessage("Invalid DOB");
            RuleFor(x => x.HSCAdmissionYear).InclusiveBetween(1950, DateTime.UtcNow.Year);
            RuleFor(x => x.GHCLastCertificatePassingYear).InclusiveBetween(1950, DateTime.UtcNow.Year);
            RuleFor(x => x.GHCAdmissionYear).InclusiveBetween(1950, DateTime.UtcNow.Year)
                .LessThanOrEqualTo(x => x.GHCLastCertificatePassingYear).WithMessage("Admission year must be <= passing year");
            // more rules can be added per SRS
        }
    }
}