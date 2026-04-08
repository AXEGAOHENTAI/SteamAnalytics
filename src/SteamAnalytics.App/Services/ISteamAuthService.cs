namespace SteamAnalytics.App.Services;

public interface ISteamAuthService
{
    string BuildOpenIdUrl(string returnTo, string realm);
}
