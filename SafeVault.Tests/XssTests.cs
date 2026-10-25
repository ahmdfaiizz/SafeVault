using NUnit.Framework;
using SafeVault;

namespace SafeVault.Tests
{
    public class XssTests
    {
        [Test]
        public void Encode_EncodesMaliciousScript()
        {
            // Arrange: prepare malicious input
            string input = "<script>alert('XSS')</script>";

            // Act: encode the input
            string result = HtmlEncoderHelper.Encode(input);

            // Assert: the script tag should not remain literal
            Assert.That(result, Does.Not.Contain("<script>"));
            Assert.That(result, Does.Not.Contain("</script>"));

            // The encoded output should differ from the input
            Assert.That(result, Is.Not.EqualTo(input));
        }

        [Test]
        public void Encode_PreservesOrdinaryText()
        {
            // Arrange
            string input = "Hello Ahmad";

            // Act
            string result = HtmlEncoderHelper.Encode(input);

            // Assert
            Assert.That(result, Is.EqualTo(input));
        }
    }
}