using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using GHCAA.Application.DTOs;

namespace GHCAA.Tests.Validators;

// TODO 84.56. The job DTO had no rules, so an empty title or an unbounded description was accepted.
[TestFixture]
[Category("FR-54")]
public class JobDtoValidationTests
{
    private static List<string> FailedMembers(CreateJobDto dto)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), results, true);
        return results.SelectMany(r => r.MemberNames).ToList();
    }

    // Shaped like the mobile form: no Requirements, empty email as the web form sends.
    private static CreateJobDto Valid() => new()
    {
        Title = "Backend Engineer",
        CompanyName = "Acme",
        Location = "",
        Description = "",
        ApplicationEmail = "",
    };

    [Test]
    public void MinimalPosting_IsValid()
    {
        FailedMembers(Valid()).Should().BeEmpty();
    }

    // TryValidateObject does not add MVC's implicit [Required] for non-nullable strings, so
    // MinimalPosting_IsValid alone missed the 400 the real binder gave the mobile form.
    [TestCase(nameof(CreateJobDto.Location))]
    [TestCase(nameof(CreateJobDto.Description))]
    [TestCase(nameof(CreateJobDto.Requirements))]
    public void OptionalTextField_IsNullable_SoMvcDoesNotRequireIt(string member)
    {
        var prop = typeof(CreateJobDto).GetProperty(member)!;
        new System.Reflection.NullabilityInfoContext().Create(prop).WriteState
            .Should().Be(System.Reflection.NullabilityState.Nullable);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Title_IsRequired(string? title)
    {
        var dto = Valid();
        dto.Title = title!;
        FailedMembers(dto).Should().Contain(nameof(CreateJobDto.Title));
    }

    [TestCase(null)]
    [TestCase("")]
    public void CompanyName_IsRequired(string? company)
    {
        var dto = Valid();
        dto.CompanyName = company!;
        FailedMembers(dto).Should().Contain(nameof(CreateJobDto.CompanyName));
    }

    [TestCase(nameof(CreateJobDto.Title), 200)]
    [TestCase(nameof(CreateJobDto.CompanyName), 200)]
    [TestCase(nameof(CreateJobDto.Location), 200)]
    [TestCase(nameof(CreateJobDto.Description), 4000)]
    [TestCase(nameof(CreateJobDto.Requirements), 4000)]
    [TestCase(nameof(CreateJobDto.ApplicationEmail), 256)]
    [TestCase(nameof(CreateJobDto.ApplicationLink), 500)]
    public void Field_RejectsOneCharOverItsLimit_AndAcceptsTheLimit(string member, int max)
    {
        var prop = typeof(CreateJobDto).GetProperty(member)!;

        var atLimit = Valid();
        prop.SetValue(atLimit, new string('a', max));
        FailedMembers(atLimit).Should().NotContain(member);

        var over = Valid();
        prop.SetValue(over, new string('a', max + 1));
        FailedMembers(over).Should().Contain(member);
    }
}
