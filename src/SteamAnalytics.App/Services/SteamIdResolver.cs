using System.Text.RegularExpressions;

namespace SteamAnalytics.App.Services;

public sealed class SteamIdResolver : ISteamIdResolver
{
    private static readonly Regex SteamId64Regex = new(@"^\d{17}$", RegexOptions.Compiled);
    private static readonly Regex ProfilesUrlRegex = new(@"steamcommunity\.com\/profiles\/(\d{17})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IdUrlRegex = new(@"steamcommunity\.com\/id\/([A-Za-z0-9_-]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string? ResolveToSteamId64(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        input = input.Trim();

        if (SteamId64Regex.IsMatch(input))
        {
            return input;
        }

        var profileMatch = ProfilesUrlRegex.Match(input);
        if (profileMatch.Success)
        {
            return profileMatch.Groups[1].Value;
        }

        var vanityMatch = IdUrlRegex.Match(input);
        if (vanityMatch.Success)
        {
            // TODO: вызвать ISteamUser/ResolveVanityURL через Steam Web API.
            return null;
        }

        return null;
    }
}
