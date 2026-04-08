using SteamAnalytics.App.Models;

namespace SteamAnalytics.App.Services;

public interface ISteamProfileService
{
    Task<ProfileDto> GetProfileAsync(string steamId64);
}
