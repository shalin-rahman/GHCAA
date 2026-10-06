using FluentAssertions;
using GHCAA.Application.Security;
using NUnit.Framework;

namespace GHCAA.Tests.Utils
{
    // The step-up dialog shows this to whoever is at the screen, so it must never reveal the
    // whole local part.
    [TestFixture]
    public class EmailMaskTests
    {
        [TestCase("shalin@gmail.com", "sha***@gmail.com")]
        [TestCase("habibur.rahman@example.org", "hab***********@example.org")]
        [TestCase("  shalin@gmail.com  ", "sha***@gmail.com")]
        public void Keeps_three_characters_and_the_domain(string email, string expected)
        {
            EmailMask.Mask(email).Should().Be(expected);
        }

        [TestCase("abc@x.com", "a***@x.com")]
        [TestCase("ab@x.com", "a***@x.com")]
        [TestCase("a@x.com", "a***@x.com")]
        public void Short_local_part_shows_one_character_only(string email, string expected)
        {
            EmailMask.Mask(email).Should().Be(expected);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Empty_input_gives_empty_string(string? email)
        {
            EmailMask.Mask(email).Should().BeEmpty();
        }

        [Test]
        public void Value_without_an_at_sign_is_still_masked()
        {
            EmailMask.Mask("username").Should().Be("use*****");
        }
    }
}
