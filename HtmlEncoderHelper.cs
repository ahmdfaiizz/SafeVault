using System.Text.Encodings.Web;

namespace SafeVault
{
    public static class HtmlEncoderHelper
    {
        public static string Encode(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            return HtmlEncoder.Default.Encode(input);
        }
    }
}