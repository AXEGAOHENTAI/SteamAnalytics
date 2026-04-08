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

    public MainViewModel(ISteamIdResolver idResolver, ISteamProfileService profileService, ISteamAuthService authService)
    {
        _idResolver = idResolver;
        _profileService = profileService;
        _authService = authService;

        CheckProfileCommand = new RelayCommand(CheckProfile);
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

    public ICommand CheckProfileCommand { get; }
    public ICommand SignInWithSteamCommand { get; }

    private void CheckProfile()
    {
        var steamId64 = _idResolver.ResolveToSteamId64(InputIdentifier);
        Profile = steamId64 is null
            ? ProfileDto.Error("Не удалось распознать идентификатор Steam.")
            : _profileService.GetProfile(steamId64);
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
