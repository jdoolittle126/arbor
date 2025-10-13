using System;
using System.Text.RegularExpressions;

namespace Arbor.Core.TreeBuilding;

internal static class WildcardMatcher
{
    public static bool IsMatch(string value, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }

        var regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";

        return Regex.IsMatch(
            value,
            regexPattern,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}
