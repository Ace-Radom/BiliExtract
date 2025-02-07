using BiliExtract.Controls.Extensions;
using BiliExtract.Extensions;
using BiliExtract.Lib;
using BiliExtract.Lib.Extensions;
using BiliExtract.Lib.Settings;
using BiliExtract.Managers;
using BiliExtract.Views.Windows.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.IO;

namespace BiliExtract.Views.Pages;

public partial class SettingsPage
{
    private readonly ApplicationSettings _settings = IoCContainer.Resolve<ApplicationSettings>();
    private readonly RichLogViewStyleManager _styleManager = IoCContainer.Resolve<RichLogViewStyleManager>();
    private readonly ThemeManager _themeManager = IoCContainer.Resolve<ThemeManager>();

    private bool _isRefreshing;

    public SettingsPage()
    {
        InitializeComponent();

        IsVisibleChanged += SettingsPage_IsVisibleChangedAsync;
        _themeManager.ThemeApplied += ThemeManager_ThemeApplied;

        return;
    }

    private async void SettingsPage_IsVisibleChangedAsync(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsVisible)
        {
            await RefreshAsync();
        }
        return;
    }

    private void ThemeManager_ThemeApplied(object? sender, EventArgs e)
    {
        if (!_isRefreshing)
        {
            UpdateAccentColorPicker();
        }
        return;
    }

    private Task RefreshAsync()
    {
        _isRefreshing = true;

        _themeComboBox.SetItems(Enum.GetValues<Theme>(), _settings.Data.Theme, t => t.GetDisplayName());
        _themeComboBox.Visibility = Visibility.Visible;

        _accentColorComboBox.SetItems(Enum.GetValues<AccentColorSource>(), _settings.Data.AccentColorSource, t => t.GetDisplayName());
        _accentColorComboBox.Visibility = Visibility.Visible;
        UpdateAccentColorPicker();

        _richLogViewStyleComboBox.SetItems(Enum.GetValues<RichLogViewStyleSource>(), _settings.Data.RichLogViewStyleSource, t => t.GetDisplayName());
        _richLogViewStyleComboBox.Visibility = Visibility.Visible;
        if (_settings.Data.RichLogViewStyleSource == RichLogViewStyleSource.Custom)
        {
            _richLogViewStyleButton.Visibility = Visibility.Visible;
        }
        else
        {
            _richLogViewStyleButton.Visibility = Visibility.Collapsed;
        }

        _useBuiltInAdbToggleSwitch.IsChecked = _settings.Data.UseBuiltInAdb;
        _useBuiltInAdbToggleSwitch.Visibility = Visibility.Visible;
        if (_settings.Data.UseBuiltInAdb)
        {
            _externalAdbPathCardControl.Visibility = Visibility.Collapsed;
        }
        else
        {
            _externalAdbPathCardControl.Visibility = Visibility.Visible;
            _externalAdbPathTextBox.Text = _settings.Data.ExternalAdbPath;
            _externalAdbPathTextBox.Visibility = Visibility.Visible;
        }

        _useBuiltInFFmpegToggleSwitch.IsChecked = _settings.Data.UseBuiltInFFmpeg;
        _useBuiltInFFmpegToggleSwitch.Visibility = Visibility.Visible;
        if (_settings.Data.UseBuiltInFFmpeg)
        {
            _useBuiltInFFmpegCardControl.Margin = new(0, 0, 0, 24);
            _externalFFmpegPathCardControl.Visibility = Visibility.Collapsed;
        }
        else
        {
            _useBuiltInFFmpegCardControl.Margin = new(0, 0, 0, 8);
            _externalFFmpegPathCardControl.Margin = new(0, 0, 0, 24);
            _externalFFmpegPathCardControl.Visibility = Visibility.Visible;
            _externalFFmpegPathTextBox.Text = _settings.Data.ExternalFFmpegPath;
            _externalFFmpegPathTextBox.Visibility = Visibility.Visible;
        }

        _autoTempCleanupIntervalNumberBox.Value = _settings.Data.AutoTempCleanupIntervalMin;
        _autoTempCleanupIntervalNumberBox.Visibility = Visibility.Visible;
        _tempStorageUsageRefreshIntervalNumberBox.Value = _settings.Data.TempFolderStorageUsageRefreshIntervalMin;
        _tempStorageUsageRefreshIntervalNumberBox.Visibility = Visibility.Visible;

        _minLogLevelComboBox.SetItems(Enum.GetValues<LogLevel>(), _settings.Data.MinLogLevel, t => t.GetDisplayName());
        _minLogLevelComboBox.Visibility = Visibility.Visible;
        if (Log.GlobalLogger.IsLoggingToFile)
        {
            _currentLogFileTextBlock.Text = Log.GlobalLogger.LogFileName;
            _currentLogFileCardControl.Visibility = Visibility.Visible;
        }
        else
        {
            _currentLogFileCardControl.Visibility = Visibility.Collapsed;
        }

        _isRefreshing = false;
        return Task.CompletedTask;
    }

    private void UpdateAccentColorPicker()
    {
        _accentColorPicker.Visibility = _settings.Data.AccentColorSource == AccentColorSource.Custom ? Visibility.Visible : Visibility.Collapsed;
        _accentColorPicker.SelectedColor = _themeManager.GetAccentColor().ToColor();
        return;
    }

    private void AccentColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (!_accentColorComboBox.TryGetSelectedItem(out AccentColorSource state))
        {
            return;
        }

        _settings.Data.AccentColorSource = state;
        _settings.SynchronizeData();

        UpdateAccentColorPicker();
        _themeManager.Apply();

        return;
    }

    private void AccentColorPicker_ColorChangedDelayed(object sender, EventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.AccentColorSource != AccentColorSource.Custom)
        {
            return;
        }

        _settings.Data.AccentColor = _accentColorPicker.SelectedColor.ToRGBColor();
        _settings.SynchronizeData();

        _themeManager.Apply();

        return;
    }

    private void AdvancedAdbSettingsCardAction_Click(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        var window = new AdbSettingsWindow { Owner = Window.GetWindow(this) };
        window.ShowDialog();

        return;
    }

    private void AutoTempCleanupIntervalNumberBox_ValueChanged(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        var value = (int)(_autoTempCleanupIntervalNumberBox.Value ?? 1);
        _settings.Data.AutoTempCleanupIntervalMin = value;
        _settings.SynchronizeData();

        return;
    }

    private void CurrentLogFileCardControl_Click(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        var text = _currentLogFileTextBlock.Text;
        _currentLogFileTextBlock.Text = (text == Log.GlobalLogger.LogFileName) ? Log.GlobalLogger.LogPath : Log.GlobalLogger.LogFileName;

        return;
    }

    private void ExternalAdbPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.UseBuiltInAdb)
        {
            return;
        }

        var text = _externalAdbPathTextBox.Text;
        if (text is null)
        {
            return;
        }

        if (!Path.Exists(text) || !Path.IsPathRooted(text))
        {
            _externalAdbPathTextBox.SetErrorBorderStyle();
            return;
        }

        _externalAdbPathTextBox.SetNormalBorderStyle();
        _settings.Data.ExternalAdbPath = text;
        _settings.SynchronizeData();

        return;
    }

    private void ExternalFFmpegPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.UseBuiltInFFmpeg)
        {
            return;
        }

        var text = _externalFFmpegPathTextBox.Text;
        if (text is null)
        {
            return;
        }

        if (!Path.Exists(text) || !Path.IsPathRooted(text))
        {
            _externalFFmpegPathTextBox.SetErrorBorderStyle();
            return;
        }

        _externalFFmpegPathTextBox.SetNormalBorderStyle();
        _settings.Data.ExternalFFmpegPath = text;
        _settings.SynchronizeData();

        return;
    }

    private void MinLogLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (!_minLogLevelComboBox.TryGetSelectedItem(out LogLevel state))
        {
            return;
        }

        _settings.Data.MinLogLevel = state;
        _settings.SynchronizeData();

        return;
    }

    private void RichLogViewStyleButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        var window = new LogPageStyleSettingsWindow { Owner = Window.GetWindow(this) };
        window.ShowDialog();

        return;
    }

    private void RichLogViewStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (!_richLogViewStyleComboBox.TryGetSelectedItem(out RichLogViewStyleSource state))
        {
            return;
        }

        _settings.Data.RichLogViewStyleSource = state;
        _settings.SynchronizeData();

        if (state == RichLogViewStyleSource.Custom)
        {
            _richLogViewStyleButton.Visibility = Visibility.Visible;
        }
        else
        {
            _richLogViewStyleButton.Visibility = Visibility.Collapsed;
        }

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void TempStorageUsageRefreshIntervalNumberBox_ValueChanged(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        var value = (int)(_tempStorageUsageRefreshIntervalNumberBox.Value ?? 1);
        _settings.Data.TempFolderStorageUsageRefreshIntervalMin = value;
        _settings.SynchronizeData();

        return;
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (!_themeComboBox.TryGetSelectedItem(out Theme state))
        {
            return;
        }

        _settings.Data.Theme = state;
        _settings.SynchronizeData();
        _themeManager.Apply();

        return;
    }

    private void UseBuiltInAdbToggleSwitch_Click(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        var state = _useBuiltInAdbToggleSwitch.IsChecked;
        if (state is null)
        {
            return;
        }

        _settings.Data.UseBuiltInAdb = state.Value;
        _settings.SynchronizeData();
        _ = RefreshAsync();

        return;
    }

    private void UseBuiltInFFmpegToggleSwitch_Click(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        var state = _useBuiltInFFmpegToggleSwitch.IsChecked;
        if (state is null)
        {
            return;
        }

        _settings.Data.UseBuiltInFFmpeg = state.Value;
        _settings.SynchronizeData();
        _ = RefreshAsync();

        return;
    }
}
