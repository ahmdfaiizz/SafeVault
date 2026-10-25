
using System.Text.RegularExpressions;

namespace SafeVault
{
    public static class InputValidator
    {
        public static bool IsValidUsername(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            if (username.Length < 3 || username.Length > 20)
            {
                return false;
            }

            return Regex.IsMatch(
                username,
                @"\A[a-zA-Z0-9_]+\z",
                RegexOptions.CultureInvariant
            );
        }

        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            if (email.Length > 254)
            {
                return false;
            }

            return Regex.IsMatch(
                email,
                @"\A[^@\s]+@[^@\s]+\.[^@\s]+\z",
                RegexOptions.CultureInvariant
            );
        }
    }
}
