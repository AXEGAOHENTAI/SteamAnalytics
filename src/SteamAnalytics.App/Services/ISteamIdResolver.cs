namespace SteamAnalytics.App.Services;

public interface ISteamIdResolver
{
    string? ResolveToSteamId64(string input);
}
