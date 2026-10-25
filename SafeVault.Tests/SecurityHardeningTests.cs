
using NUnit.Framework;
using SafeVault;

namespace SafeVault.Tests
{
    public class SecurityHardeningTests
    {
        private readonly AuthorizationService _authorizationService = new();

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        [TestCase("ab")]
        [TestCase("user<script>")]
        [TestCase("user name")]
        public void IsValidUsername_InvalidInput_ReturnsFalse(string? username)
        {
            Assert.That(
                InputValidator.IsValidUsername(username),
                Is.False
            );
        }

        [TestCase("ahmad_123", true)]
        [TestCase("Ahmad123", true)]
        [TestCase("user name", false)]
        [TestCase("user<script>", false)]
        public void IsValidUsername_ChecksAllowedCharacters(
            string username,
            bool expected)
        {
            Assert.That(
                InputValidator.IsValidUsername(username),
                Is.EqualTo(expected)
            );
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("not-an-email")]
        [TestCase("user@@example.com")]
        [TestCase("user@example")]
        public void IsValidEmail_InvalidInput_ReturnsFalse(string? email)
        {
            Assert.That(
                InputValidator.IsValidEmail(email),
                Is.False
            );
        }

        [Test]
        public void Encode_ScriptPayload_EncodesHtmlCharacters()
        {
            string input = "<script>alert('XSS')</script>";

            string result = HtmlEncoderHelper.Encode(input);

            Assert.That(result, Does.Contain("&lt;script&gt;"));
            Assert.That(result, Does.Not.Contain("<script>"));
            Assert.That(result, Does.Not.Contain("</script>"));
        }

        [Test]
        public void UnknownRole_CannotAccessAdminDashboard()
        {
            var user = new AuthenticatedUser(
                100,
                "testuser",
                "manager"
            );

            Assert.That(
                _authorizationService.CanAccessAdminDashboard(user),
                Is.False
            );
        }

        [Test]
        public void UnknownRole_CannotAccessUserDashboard()
        {
            var user = new AuthenticatedUser(
                100,
                "testuser",
                "manager"
            );

            Assert.That(
                _authorizationService.CanAccessUserDashboard(user),
                Is.False
            );
        }

        [Test]
        public void Admin_CanAccessBothDashboards()
        {
            var admin = new AuthenticatedUser(
                101,
                "testadmin",
                "admin"
            );

            Assert.That(
                _authorizationService.CanAccessAdminDashboard(admin),
                Is.True
            );

            Assert.That(
                _authorizationService.CanAccessUserDashboard(admin),
                Is.True
            );
        }

        [Test]
        public void UnauthenticatedUser_CannotAccessEitherDashboard()
        {
            Assert.That(
                _authorizationService.CanAccessAdminDashboard(null),
                Is.False
            );

            Assert.That(
                _authorizationService.CanAccessUserDashboard(null),
                Is.False
            );
        }
    }
}
