using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FluentAssertions;
using GHCAA.Application.DTOs;

namespace GHCAA.Tests.Validators;

// TODO 84.50. MVC refuses to bind a record whose primary-constructor parameter carries a validation
// attribute through the property: target, and the request fails with a 500. AttestReadinessRequest
// did this, so POST /elections/{id}/readiness/attest always failed. The attribute goes on the parameter.
[TestFixture]
[Category("FR-39")]
public class RecordDtoValidationMetadataTests
{
    private static bool IsRecord(Type t) => t.GetMethod("<Clone>$") != null;

    [Test]
    public void NoRecordDto_PutsValidationOnAConstructorParameterProperty()
    {
        var offenders = typeof(AttestReadinessRequest).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && IsRecord(t))
            .SelectMany(t => t.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => t.GetProperty(p.Name!, BindingFlags.Public | BindingFlags.Instance))
                .Where(prop => prop != null && prop.GetCustomAttributes<ValidationAttribute>(true).Any())
                .Select(prop => $"{t.Name}.{prop!.Name}"))
            .Distinct()
            .ToList();

        offenders.Should().BeEmpty();
    }

    [Test]
    public void AttestReadinessRequest_ConditionIsRequired()
    {
        typeof(AttestReadinessRequest).GetConstructors().Single().GetParameters().Single()
            .GetCustomAttribute<RequiredAttribute>().Should().NotBeNull();
    }
}
