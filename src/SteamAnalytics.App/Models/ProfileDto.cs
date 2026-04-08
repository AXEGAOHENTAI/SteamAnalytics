namespace SteamAnalytics.App.Models;

public sealed class ProfileDto
{
    public string PersonaName { get; init; } = "—";
    public string SteamId64 { get; init; } = "—";
    public string ProfileUrl { get; init; } = "—";
    public string AvatarUrl { get; init; } = "";
    public string BanSummary { get; init; } = "Нет данных";
    public bool IsError { get; init; }
    public string StatusMessage { get; init; } = "";

    public IReadOnlyList<GamePlaytimeDto> TopGames { get; init; } = Array.Empty<GamePlaytimeDto>();
    public IReadOnlyList<FriendDto> Friends { get; init; } = Array.Empty<FriendDto>();
    public IReadOnlyList<InventoryItemDto> InventoryItems { get; init; } = Array.Empty<InventoryItemDto>();

    public static ProfileDto Empty() => new();

    public static ProfileDto Error(string message) => new()
    {
        PersonaName = "Ошибка",
        BanSummary = message,
        IsError = true,
        StatusMessage = message
    };
}

public sealed class GamePlaytimeDto
{
    public string Name { get; init; } = string.Empty;
    public int Hours { get; init; }
    public int AppId { get; init; }
}

public sealed class FriendDto
{
    public string SteamId64 { get; init; } = string.Empty;
    public string PersonaName { get; init; } = string.Empty;
}

public sealed class InventoryItemDto
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public int Amount { get; init; }
}
