using System.Net.Http;
using System.Windows;
using SteamAnalytics.App.Services;
using SteamAnalytics.App.ViewModels;

namespace SteamAnalytics.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var httpClient = new HttpClient();
        DataContext = new MainViewModel(
            new SteamIdResolver(httpClient),
            new SteamProfileService(httpClient),
            new SteamAuthService());
    }
}
