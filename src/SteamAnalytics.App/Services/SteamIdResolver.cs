using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SteamAnalytics.App.Services;

public sealed class SteamIdResolver : ISteamIdResolver
{
    private static readonly Regex SteamId64Regex = new(@"^\d{17}$", RegexOptions.Compiled);
    private static readonly Regex ProfilesUrlRegex = new(@"steamcommunity\.com\/profiles\/(\d{17})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IdUrlRegex = new(@"steamcommunity\.com\/id\/([A-Za-z0-9_-]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly HttpClient _httpClient;

    public SteamIdResolver(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

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
            return ResolveVanity(vanityMatch.Groups[1].Value);
        }

        return null;
    }

    private string? ResolveVanity(string vanity)
    {
        var apiKey = Environment.GetEnvironmentVariable("STEAM_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var url = "https://api.steampowered.com/ISteamUser/ResolveVanityURL/v1/"
                  + $"?key={Uri.EscapeDataString(apiKey)}&vanityurl={Uri.EscapeDataString(vanity)}";

        using var response = _httpClient.GetAsync(url).GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        using var doc = JsonDocument.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        var result = doc.RootElement.GetProperty("response");
        var success = result.TryGetProperty("success", out var successNode) && successNode.GetInt32() == 1;

        return success && result.TryGetProperty("steamid", out var steamIdNode)
            ? steamIdNode.GetString()
            : null;
    }
}
