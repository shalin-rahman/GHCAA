using FluentValidation;
using GHCAA.Application.DTOs;
using System;
using System.Linq;

namespace GHCAA.Application.Validators
{
    public class MemberRegistrationValidator : AbstractValidator<MemberRegistrationDto>
    {
        public MemberRegistrationValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.FatherName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.MotherName).NotEmpty().MaximumLength(200);

            RuleFor(x => x.NID)
                .NotEmpty()
                .Must(nid => nid != null && (nid.Length == 10 || nid.Length == 13 || nid.Length == 17))
                .WithMessage("NID must be 10, 13 or 17 digits")
                .Matches(@"^\d+$").WithMessage("NID must contain only digits");

            RuleFor(x => x.MobileNo).NotEmpty().Matches(@"^01\d{9}$").WithMessage("Mobile must be 11 digits and start with 01");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);

            // S8.2: Age gate — must be at least 13 and at most 120 years old.
            RuleFor(x => x.DateOfBirth)
                .NotEmpty()
                .GreaterThan(DateTime.UtcNow.Date.AddYears(-120)).WithMessage("Date of birth is too far in the past")
                .LessThan(DateTime.UtcNow.Date.AddYears(-13)).WithMessage("Applicant must be at least 13 years old");

            RuleFor(x => x.PresentAddress).NotEmpty().MaximumLength(500);
            RuleFor(x => x.PermanentAddress).NotEmpty().MaximumLength(500);

            RuleFor(x => x.EmergencyContactName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.EmergencyContactRelation).NotEmpty().MaximumLength(100);
            RuleFor(x => x.EmergencyContactPhone).NotEmpty().Matches(@"^01\d{9}$")
                .WithMessage("Emergency contact phone must be 11 digits and start with 01");

            RuleFor(x => x.PaymentMethodId).GreaterThan(0).WithMessage("Please select a valid payment method");

            RuleFor(x => x.AcademicHistory).NotEmpty().WithMessage("At least one academic record is required");
            RuleFor(x => x.AcademicHistory)
                .Must(history => history != null && history.Count > 0 && history[0].IsOrgProfile)
                .WithMessage("The first academic record must be the institutional record.");
            RuleForEach(x => x.AcademicHistory).ChildRules(academic =>
            {
                academic.RuleFor(a => a.InstitutionName).NotEmpty().MaximumLength(250);
                academic.RuleFor(a => a.Degree).NotEmpty().MaximumLength(100);
                academic.RuleFor(a => a.Subject).MaximumLength(100);
                academic.RuleFor(a => a.Result).MaximumLength(100);
                academic.RuleFor(a => a.PassingYear).NotNull().GreaterThan(1950).LessThanOrEqualTo(DateTime.UtcNow.Year);
                // S8.2: AdmissionYear must precede PassingYear when provided.
                academic.RuleFor(a => a.PassingYear)
                    .GreaterThanOrEqualTo(a => a.AdmissionYear!.Value)
                    .WithMessage("Passing year must be on or after admission year")
                    .When(a => a.AdmissionYear.HasValue);
            });
        }
    }
}
