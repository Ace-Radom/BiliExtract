using BiliExtract.Extensions;
using BiliExtract.Lib;
using BiliExtract.Lib.Settings;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BiliExtract.Views.Windows.Settings;

public partial class AdbSettingsWindow
{
    private readonly AdbSettings _adbSettings = IoCContainer.Resolve<AdbSettings>();

    private bool _isRefreshing;

    public AdbSettingsWindow()
    {
        InitializeComponent();

        IsVisibleChanged += AdbSettingsWindow_IsVisibleChangedAsync;

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

        _adbServerAddressIpTextBox.Text = _adbSettings.Data.ServerIp;
        _adbServerAddressPortTextBox.Text = _adbSettings.Data.ServerPort.ToString();
        _adbServerAddressStackPanel.Visibility = Visibility.Visible;
        _wirelessDeviceDefaultIpIpTextBox.Text = _adbSettings.Data.WirelessDeviceDefaultIp;
        _wirelessDeviceDefaultIpStackPanel.Visibility = Visibility.Visible;

        _isRefreshing = false;
        return Task.CompletedTask;
    }

    private void AdbServerHostIpTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (e.Key is Key.Enter or Key.Space)
        {
            e.Handled = true;
        }

        return;
    }

    private void AdbServerHostIpTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (!_adbServerAddressIpTextBox.Text.IsLegalIpv4Address())
        {
            _adbServerAddressIpTextBox.SetErrorBorderStyle();
            return;
        }
            
        _adbServerAddressIpTextBox.SetNormalBorderStyle();
        _adbSettings.Data.ServerIp = _adbServerAddressIpTextBox.Text;
        _adbSettings.SynchronizeData();

        return;
    }

    private void AdbServerHostPortTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (e.Key is Key.Enter or Key.Space)
        {
            e.Handled = true;
        }

        return;
    }

    private void AdbServerHostPortTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (!int.TryParse(e.Text, out _))
        {
            e.Handled = true;
        }

        return;
    }

    private void AdbServerHostPortTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (!int.TryParse(_adbServerAddressPortTextBox.Text, out int value) || value <= 0)
        {
            _adbServerAddressPortTextBox.SetErrorBorderStyle();
            return;
        }
            
        _adbServerAddressPortTextBox.SetNormalBorderStyle();
        _adbServerAddressPortTextBox.Text = value.ToString();
        _adbSettings.Data.ServerPort = value;
        _adbSettings.SynchronizeData();

        return;
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

    private void WirelessDeviceDefaultIpIpTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (e.Key is Key.Enter or Key.Space)
        {
            e.Handled = true;
        }

        return;
    }

    private void WirelessDeviceDefaultIpIpTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        var text = _wirelessDeviceDefaultIpIpTextBox.Text;
        if (!string.IsNullOrEmpty(text) && !text.IsLegalIpv4Address())
        {
            _wirelessDeviceDefaultIpIpTextBox.SetErrorBorderStyle();
            return;
        }

        _wirelessDeviceDefaultIpIpTextBox.SetNormalBorderStyle();
        _adbSettings.Data.WirelessDeviceDefaultIp = string.IsNullOrEmpty(text) ? null : text;
        _adbSettings.SynchronizeData();

        return;
    }
}
