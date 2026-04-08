using System.Diagnostics;
using System.Windows.Input;
using SteamAnalytics.App.Models;
using SteamAnalytics.App.Services;

namespace SteamAnalytics.App.ViewModels;

public sealed class MainViewModel : BaseViewModel
{
    private readonly ISteamIdResolver _idResolver;
    private readonly ISteamProfileService _profileService;
    private readonly ISteamAuthService _authService;

    private string _inputIdentifier = "https://steamcommunity.com/id/gaben";
    private ProfileDto _profile = ProfileDto.Empty();
    private string _status = "Готово";

    public MainViewModel(ISteamIdResolver idResolver, ISteamProfileService profileService, ISteamAuthService authService)
    {
        _idResolver = idResolver;
        _profileService = profileService;
        _authService = authService;

        CheckProfileCommand = new AsyncRelayCommand(CheckProfileAsync);
        SignInWithSteamCommand = new RelayCommand(SignInWithSteam);
    }

    public string InputIdentifier
    {
        get => _inputIdentifier;
        set
        {
            _inputIdentifier = value;
            OnPropertyChanged();
        }
    }

    public ProfileDto Profile
    {
        get => _profile;
        private set
        {
            _profile = value;
            OnPropertyChanged();
        }
    }

    public string Status
    {
        get => _status;
        private set
        {
            _status = value;
            OnPropertyChanged();
        }
    }

    public ICommand CheckProfileCommand { get; }
    public ICommand SignInWithSteamCommand { get; }

    private async Task CheckProfileAsync()
    {
        Status = "Проверяем идентификатор...";
        var steamId64 = _idResolver.ResolveToSteamId64(InputIdentifier);

        if (steamId64 is null)
        {
            Profile = ProfileDto.Error("Не удалось распознать идентификатор Steam (или не задан STEAM_API_KEY для vanity URL).");
            Status = "Ошибка";
            return;
        }

        Status = "Загружаем профиль, игры, баны, друзей и инвентарь...";
        Profile = await _profileService.GetProfileAsync(steamId64);
        Status = Profile.IsError ? "Ошибка" : "Готово";
    }

    private void SignInWithSteam()
    {
        var authUrl = _authService.BuildOpenIdUrl("https://localhost:5001/auth/steam/callback", "https://localhost:5001/");

        Process.Start(new ProcessStartInfo
        {
            FileName = authUrl,
            UseShellExecute = true
        });
    }
}
