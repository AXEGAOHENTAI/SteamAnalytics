using SteamAnalytics.App.Models;

namespace SteamAnalytics.App.Services;

public interface ISteamProfileService
{
    ProfileDto GetProfile(string steamId64);
}
