using SteamAnalytics.App.Models;

namespace SteamAnalytics.App.Services;

public sealed class SteamProfileService : ISteamProfileService
{
    public ProfileDto GetProfile(string steamId64)
    {
        // TODO: заменить на реальные вызовы Steam Web API.
        return new ProfileDto
        {
            PersonaName = "Demo User",
            SteamId64 = steamId64,
            ProfileUrl = $"https://steamcommunity.com/profiles/{steamId64}",
            BanSummary = "VAC: 0, Game Bans: 0, Community: clean",
            TopGames =
            [
                new GamePlaytimeDto { Name = "Counter-Strike 2", Hours = 512 },
                new GamePlaytimeDto { Name = "Dota 2", Hours = 430 },
                new GamePlaytimeDto { Name = "PUBG: BATTLEGROUNDS", Hours = 120 }
            ]
        };
    }
}
