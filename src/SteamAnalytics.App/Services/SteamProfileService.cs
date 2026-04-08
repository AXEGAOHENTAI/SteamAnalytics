using System.Net.Http;
using System.Text.Json;
using SteamAnalytics.App.Models;

namespace SteamAnalytics.App.Services;

public sealed class SteamProfileService : ISteamProfileService
{
    private readonly HttpClient _httpClient;

    public SteamProfileService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<ProfileDto> GetProfileAsync(string steamId64)
    {
        var apiKey = Environment.GetEnvironmentVariable("STEAM_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ProfileDto.Error("Не задан STEAM_API_KEY (переменная окружения).");
        }

        try
        {
            var summary = await GetPlayerSummaryAsync(apiKey, steamId64);
            var banSummary = await GetPlayerBansAsync(apiKey, steamId64);
            var games = await GetOwnedGamesAsync(apiKey, steamId64);
            var friends = await GetFriendsAsync(apiKey, steamId64);
            var inventory = await GetInventoryAsync(steamId64, 730, 2);

            return new ProfileDto
            {
                PersonaName = summary.PersonaName,
                SteamId64 = steamId64,
                ProfileUrl = summary.ProfileUrl,
                AvatarUrl = summary.AvatarUrl,
                BanSummary = banSummary,
                TopGames = games,
                Friends = friends,
                InventoryItems = inventory,
                StatusMessage = "Данные загружены из Steam Web API."
            };
        }
        catch (Exception ex)
        {
            return ProfileDto.Error($"Ошибка загрузки Steam API: {ex.Message}");
        }
    }

    private async Task<(string PersonaName, string ProfileUrl, string AvatarUrl)> GetPlayerSummaryAsync(string apiKey, string steamId64)
    {
        var url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={Uri.EscapeDataString(apiKey)}&steamids={Uri.EscapeDataString(steamId64)}";
        using var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var player = doc.RootElement
            .GetProperty("response")
            .GetProperty("players")[0];

        var personaName = player.TryGetProperty("personaname", out var p) ? p.GetString() ?? "Unknown" : "Unknown";
        var profileUrl = player.TryGetProperty("profileurl", out var u) ? u.GetString() ?? "" : "";
        var avatarUrl = player.TryGetProperty("avatarfull", out var a) ? a.GetString() ?? "" : "";

        return (personaName, profileUrl, avatarUrl);
    }

    private async Task<string> GetPlayerBansAsync(string apiKey, string steamId64)
    {
        var url = $"https://api.steampowered.com/ISteamUser/GetPlayerBans/v1/?key={Uri.EscapeDataString(apiKey)}&steamids={Uri.EscapeDataString(steamId64)}";
        using var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var player = doc.RootElement.GetProperty("players")[0];

        var vacBans = player.GetProperty("NumberOfVACBans").GetInt32();
        var gameBans = player.GetProperty("NumberOfGameBans").GetInt32();
        var communityBanned = player.GetProperty("CommunityBanned").GetBoolean();

        return $"VAC: {vacBans}, Game Bans: {gameBans}, Community: {(communityBanned ? "banned" : "clean")}";
    }

    private async Task<IReadOnlyList<GamePlaytimeDto>> GetOwnedGamesAsync(string apiKey, string steamId64)
    {
        var url = "https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/"
                  + $"?key={Uri.EscapeDataString(apiKey)}"
                  + $"&steamid={Uri.EscapeDataString(steamId64)}"
                  + "&include_appinfo=1&include_played_free_games=1";

        using var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (!doc.RootElement.GetProperty("response").TryGetProperty("games", out var gamesElement))
        {
            return Array.Empty<GamePlaytimeDto>();
        }

        var games = new List<GamePlaytimeDto>();
        foreach (var game in gamesElement.EnumerateArray())
        {
            var minutes = game.TryGetProperty("playtime_forever", out var playtime) ? playtime.GetInt32() : 0;
            games.Add(new GamePlaytimeDto
            {
                AppId = game.TryGetProperty("appid", out var appIdValue) ? appIdValue.GetInt32() : 0,
                Name = game.TryGetProperty("name", out var nameValue) ? nameValue.GetString() ?? "Unknown" : "Unknown",
                Hours = (int)Math.Round(minutes / 60.0)
            });
        }

        return games.OrderByDescending(g => g.Hours).Take(25).ToList();
    }

    private async Task<IReadOnlyList<FriendDto>> GetFriendsAsync(string apiKey, string steamId64)
    {
        var friendsUrl = "https://api.steampowered.com/ISteamUser/GetFriendList/v1/"
                         + $"?key={Uri.EscapeDataString(apiKey)}"
                         + $"&steamid={Uri.EscapeDataString(steamId64)}"
                         + "&relationship=friend";

        using var friendsResponse = await _httpClient.GetAsync(friendsUrl);
        if (!friendsResponse.IsSuccessStatusCode)
        {
            return Array.Empty<FriendDto>();
        }

        using var friendsDoc = JsonDocument.Parse(await friendsResponse.Content.ReadAsStringAsync());
        if (!friendsDoc.RootElement.TryGetProperty("friendslist", out var friendsList)
            || !friendsList.TryGetProperty("friends", out var friendsArray))
        {
            return Array.Empty<FriendDto>();
        }

        var ids = friendsArray.EnumerateArray()
            .Select(x => x.GetProperty("steamid").GetString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Take(50)
            .ToArray();

        if (ids.Length == 0)
        {
            return Array.Empty<FriendDto>();
        }

        var summariesUrl = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/"
                           + $"?key={Uri.EscapeDataString(apiKey)}"
                           + $"&steamids={Uri.EscapeDataString(string.Join(',', ids!))}";

        using var summariesResponse = await _httpClient.GetAsync(summariesUrl);
        summariesResponse.EnsureSuccessStatusCode();

        using var summariesDoc = JsonDocument.Parse(await summariesResponse.Content.ReadAsStringAsync());
        var players = summariesDoc.RootElement.GetProperty("response").GetProperty("players");

        return players.EnumerateArray()
            .Select(p => new FriendDto
            {
                SteamId64 = p.TryGetProperty("steamid", out var idNode) ? idNode.GetString() ?? "" : "",
                PersonaName = p.TryGetProperty("personaname", out var nameNode) ? nameNode.GetString() ?? "Unknown" : "Unknown"
            })
            .OrderBy(x => x.PersonaName)
            .ToList();
    }

    private async Task<IReadOnlyList<InventoryItemDto>> GetInventoryAsync(string steamId64, int appId, int contextId)
    {
        var url = $"https://steamcommunity.com/inventory/{steamId64}/{appId}/{contextId}?l=english&count=200";
        using var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<InventoryItemDto>();
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (!doc.RootElement.TryGetProperty("descriptions", out var descriptions))
        {
            return Array.Empty<InventoryItemDto>();
        }

        var descriptionMap = descriptions.EnumerateArray().ToDictionary(
            d => $"{d.GetProperty("classid").GetString()}_{d.GetProperty("instanceid").GetString()}",
            d => new
            {
                Name = d.TryGetProperty("name", out var nameNode) ? nameNode.GetString() ?? "Unknown item" : "Unknown item",
                Type = d.TryGetProperty("type", out var typeNode) ? typeNode.GetString() ?? "Unknown" : "Unknown"
            });

        if (!doc.RootElement.TryGetProperty("assets", out var assets))
        {
            return Array.Empty<InventoryItemDto>();
        }

        return assets.EnumerateArray()
            .Select(a =>
            {
                var key = $"{a.GetProperty("classid").GetString()}_{a.GetProperty("instanceid").GetString()}";
                var meta = descriptionMap.TryGetValue(key, out var v)
                    ? v
                    : new { Name = "Unknown item", Type = "Unknown" };

                return new InventoryItemDto
                {
                    Name = meta.Name,
                    Type = meta.Type,
                    Amount = a.TryGetProperty("amount", out var amountNode) ? int.Parse(amountNode.GetString() ?? "1") : 1
                };
            })
            .GroupBy(x => new { x.Name, x.Type })
            .Select(g => new InventoryItemDto { Name = g.Key.Name, Type = g.Key.Type, Amount = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Amount)
            .Take(50)
            .ToList();
    }
}
