using System.Text.RegularExpressions;

namespace BiliExtract.Extensions;

public static class StringExtensions
{
    public static bool IsLegalIpv4Address(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return false;
        }

        Regex pattern = new(@"^(\d{1,3}\.){3}\d{1,3}$");
        if (!pattern.IsMatch(str))
        {
            return false;
        }

        string[] parts = str.Split('.');
        foreach (var part in parts)
        {
            if (!int.TryParse(part, out int value) || value < 0 || value > 255)
            {
                return false;
            }
        }

        return true;
    }
}
