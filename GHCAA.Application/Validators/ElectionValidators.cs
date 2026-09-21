using FluentValidation;
using GHCAA.Application.DTOs;

namespace GHCAA.Application.Validators;

public sealed class CreateElectionValidator : AbstractValidator<CreateElectionDto>
{
    public CreateElectionValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ECPeriodId).GreaterThan(0);
        RuleFor(x => x.NominationOpensOn).NotEqual(default(DateTime));
        RuleFor(x => x.NominationClosesOn).GreaterThan(x => x.NominationOpensOn);
        RuleFor(x => x.PollingOpensOn).GreaterThan(x => x.NominationClosesOn);
        RuleFor(x => x.PollingClosesOn).GreaterThan(x => x.PollingOpensOn);
    }
}

public sealed class NominationValidator : AbstractValidator<NominationDto>
{
    public NominationValidator()
    {
        RuleFor(x => x.ElectionSeatId).GreaterThan(0);
        RuleFor(x => x.CandidateMemberId).GreaterThan(0);
        RuleFor(x => x.ProposerMemberId).GreaterThan(0);
        RuleFor(x => x.SeconderMemberId).GreaterThan(0);
        RuleFor(x => x.Statement).NotEmpty().MaximumLength(4000);
        RuleFor(x => x).Must(x => x.CandidateMemberId != x.ProposerMemberId && x.CandidateMemberId != x.SeconderMemberId)
            .WithMessage("The candidate cannot propose or second their own nomination.");
    }
}

public sealed class ElectionSeatRequestValidator : AbstractValidator<ElectionSeatRequestDto>
{
    public ElectionSeatRequestValidator()
    {
        RuleFor(x => x.Position).IsInEnum();
        RuleFor(x => x.SeatCount).InclusiveBetween(1, 20);
    }
}

public sealed class CastVoteValidator : AbstractValidator<CastVoteDto>
{
    public CastVoteValidator()
    {
        RuleFor(x => x.ElectionSeatId).GreaterThan(0);
        RuleFor(x => x.NominationId).GreaterThan(0);
        RuleFor(x => x.SerialNumber).MaximumLength(80);
    }
}
