using BiliExtract.Lib;
using BiliExtract.Lib.Settings;
using System.Threading.Tasks;
using System.Windows;

namespace BiliExtract.Views.Windows.Settings;

public partial class AdbSettingsWindow
{
    private readonly AdbSettings _adbSettings = IoCContainer.Resolve<AdbSettings>();

    private bool _isRefreshing;

    public AdbSettingsWindow()
    {
        InitializeComponent();

        return;
    }

    private async void AdbSettingsWindow_IsVisibleChangedAsync(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsVisible)
        {
            await RefreshAsync();
        }
        return;
    }

    private Task RefreshAsync()
    {
        _isRefreshing = true;

        _automaticStartAdbServerToggleSwitch.IsChecked = _adbSettings.Data.AutoStartServerIfNotStarted;
        _automaticStartAdbServerToggleSwitch.Visibility = Visibility.Visible;
        _startAdbServerOnStartupToggleSwitch.IsChecked = _adbSettings.Data.StartServerOnStartup;
        _startAdbServerOnStartupToggleSwitch.Visibility = Visibility.Visible;
        _killAdbServerOnExitToggleSwitch.IsChecked = _adbSettings.Data.KillServerOnExit;
        _killAdbServerOnExitToggleSwitch.Visibility = Visibility.Visible;

        _isRefreshing = false;
        return Task.CompletedTask;
    }

    private void AutomaticStartAdbServerToggleSwitch_Click(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        var state = _automaticStartAdbServerToggleSwitch.IsChecked;
        if (state is null)
        {
            return;
        }

        _adbSettings.Data.AutoStartServerIfNotStarted = state.Value;
        _adbSettings.SynchronizeData();

        return;
    }

    private void KillAdbServerOnExitToggleSwitch_Click(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        var state = _killAdbServerOnExitToggleSwitch.IsChecked;
        if (state is null)
        {
            return;
        }

        _adbSettings.Data.KillServerOnExit = state.Value;
        _adbSettings.SynchronizeData();

        return;
    }

    private void StartAdbServerOnStartupToggleSwitch_Click(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        var state = _startAdbServerOnStartupToggleSwitch.IsChecked;
        if (state is null)
        {
            return;
        }

        _adbSettings.Data.StartServerOnStartup = state.Value;
        _adbSettings.SynchronizeData();

        return;
    }
}
