using System.Windows;
using SteamAnalytics.App.Services;
using SteamAnalytics.App.ViewModels;

namespace SteamAnalytics.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(new SteamIdResolver(), new SteamProfileService(), new SteamAuthService());
    }
}
