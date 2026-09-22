using System.Collections.Generic;
using GHCAA.Application.DTOs;
using GHCAA.Infrastructure.Services;
using NUnit.Framework;

namespace GHCAA.Tests.Services
{
    [TestFixture]
    public class CredentialCodeGeneratorTests
    {
        [Test]
        public void Create_GeneratesUniqueTenCharacterCodesAcrossTenThousandIssuances()
        {
            var codes = new HashSet<string>();
            for (var i = 0; i < 10_000; i++)
            {
                var code = CredentialCodeGenerator.Create();
                Assert.That(code, Has.Length.EqualTo(10));
                Assert.That(code, Does.Match("^[A-HJ-NP-Z2-9]{10}$"));
                Assert.That(codes.Add(code), Is.True);
            }
        }

        [Test]
        public void VerificationDto_ContainsOnlyPublicVerificationFields()
        {
            var names = new HashSet<string>(
                typeof(CredentialVerificationDto).GetProperties().Select(x => x.Name));

            Assert.That(names, Is.EquivalentTo(new[]
            {
                "Valid", "MemberName", "MembershipType", "IssuedOn", "Status"
            }));
        }
    }
}
