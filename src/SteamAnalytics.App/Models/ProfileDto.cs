namespace SteamAnalytics.App.Models;

public sealed class ProfileDto
{
    public string PersonaName { get; init; } = "—";
    public string SteamId64 { get; init; } = "—";
    public string ProfileUrl { get; init; } = "—";
    public string BanSummary { get; init; } = "Нет данных";
    public IReadOnlyList<GamePlaytimeDto> TopGames { get; init; } = Array.Empty<GamePlaytimeDto>();

    public static ProfileDto Empty() => new();

    public static ProfileDto Error(string message) => new()
    {
        PersonaName = "Ошибка",
        BanSummary = message
    };
}

public sealed class GamePlaytimeDto
{
    public string Name { get; init; } = string.Empty;
    public int Hours { get; init; }
}
