using BiliExtract.Controls.Extensions;
using BiliExtract.Extensions;
using BiliExtract.Lib;
using BiliExtract.Lib.Extensions;
using BiliExtract.Lib.Settings;
using BiliExtract.Managers;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BiliExtract.Views.Windows.Settings;

public partial class LogPageStyleSettingsWindow
{
    private readonly ApplicationSettings _settings = IoCContainer.Resolve<ApplicationSettings>();
    private readonly RichLogViewStyleManager _styleManager = IoCContainer.Resolve<RichLogViewStyleManager>();
    private readonly TextStyleSettings _styleSettings = IoCContainer.Resolve<TextStyleSettings>();
    private readonly ThemeManager _themeManager = IoCContainer.Resolve<ThemeManager>();

    private bool _isRefreshing;
    private RichLogViewStyle _cachedStyle;
    private Theme _targetTheme;

    public LogPageStyleSettingsWindow()
    {
        InitializeComponent();

        _targetTheme = _themeManager.IsDarkMode() ? Theme.Dark : Theme.Light;

        IsVisibleChanged += LogPageStyleSettingsWindow_IsVisibleChangedAsync;

        return;
    }

    private async void LogPageStyleSettingsWindow_IsVisibleChangedAsync(object sender, DependencyPropertyChangedEventArgs e)
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

        _targetThemeComboBox.SetItems(Enum.GetValues<Theme>().Where(t => t != Theme.FollowSystem), _targetTheme, t => t.GetDisplayName());
        _targetThemeComboBox.Visibility = Visibility.Visible;
        RefreshCachedStyle();
        _resetToDefaultButton.Visibility = Visibility.Visible;

        _fontComboBox.SetItems(_styleManager.AvailableFonts, _styleManager.GetFontFamilyByName(_cachedStyle.Font), t => t.GetEnUsFamilyName());
        _fontComboBox.Visibility = Visibility.Visible;

        _textColorColorPicker.SelectedColor = _cachedStyle.NormalTextColor.ToColor();
        _textColorColorPicker.Visibility = Visibility.Visible;
        _numberColorColorPicker.SelectedColor = _cachedStyle.NumberColor.ToColor();
        _numberColorColorPicker.Visibility = Visibility.Visible;
        _filenameColorColorPicker.SelectedColor = _cachedStyle.FilenameColor.ToColor();
        _filenameColorColorPicker.Visibility = Visibility.Visible;
        _exceptionStyleComboBox.SetItems(Enum.GetValues<TextStyle>(), _cachedStyle.ExceptionTextStyle, t => t.GetDisplayName());
        _exceptionStyleComboBox.Visibility = Visibility.Visible;
        _exceptionStyleColorPicker.SelectedColor = _cachedStyle.ExceptionTextColor.ToColor();
        _exceptionStyleColorPicker.Visibility = Visibility.Visible;
        _stringColorColorPicker.SelectedColor = _cachedStyle.StringColor.ToColor();
        _stringColorColorPicker.Visibility = Visibility.Visible;
        _timeColorYMDColorPicker.SelectedColor = _cachedStyle.TimeYMDColor.ToColor();
        _timeColorYMDColorPicker.Visibility = Visibility.Visible;
        _timeColorHMSColorPicker.SelectedColor = _cachedStyle.TimeHMSColor.ToColor();
        _timeColorHMSColorPicker.Visibility = Visibility.Visible;

        _debugLogLevelStyleComboBox.SetItems(Enum.GetValues<TextStyle>(), _cachedStyle.LogLevelDebugStyle, t => t.GetDisplayName());
        _debugLogLevelStyleComboBox.Visibility = Visibility.Visible;
        _debugLogLevelStyleColorPicker.SelectedColor = _cachedStyle.LogLevelDebugColor.ToColor();
        _debugLogLevelStyleColorPicker.Visibility = Visibility.Visible;
        _infoLogLevelStyleComboBox.SetItems(Enum.GetValues<TextStyle>(), _cachedStyle.LogLevelInfoStyle, t => t.GetDisplayName());
        _infoLogLevelStyleComboBox.Visibility = Visibility.Visible;
        _infoLogLevelStyleColorPicker.SelectedColor = _cachedStyle.LogLevelInfoColor.ToColor();
        _infoLogLevelStyleColorPicker.Visibility = Visibility.Visible;
        _warningLogLevelStyleComboBox.SetItems(Enum.GetValues<TextStyle>(), _cachedStyle.LogLevelWarningStyle, t => t.GetDisplayName());
        _warningLogLevelStyleComboBox.Visibility = Visibility.Visible;
        _warningLogLevelStyleColorPicker.SelectedColor = _cachedStyle.LogLevelWarningColor.ToColor();
        _warningLogLevelStyleColorPicker.Visibility = Visibility.Visible;
        _errorLogLevelStyleComboBox.SetItems(Enum.GetValues<TextStyle>(), _cachedStyle.LogLevelErrorStyle, t => t.GetDisplayName());
        _errorLogLevelStyleComboBox.Visibility = Visibility.Visible;
        _errorLogLevelStyleColorPicker.SelectedColor = _cachedStyle.LogLevelErrorColor.ToColor();
        _errorLogLevelStyleColorPicker.Visibility = Visibility.Visible;
        _isRefreshing = false;
        return Task.CompletedTask;
    }

    private void RefreshCachedStyle()
    {
        switch (_targetTheme)
        {
            case Theme.Dark: _cachedStyle = _styleSettings.Data.RichLogViewStyleDark; break;
            case Theme.Light: _cachedStyle = _styleSettings.Data.RichLogViewStyleLight; break;
            default: throw new Exception("Unexpected enumerate value");
        }
        return;
    }

    private void ResetCachedStyle()
    {
        switch (_targetTheme)
        {
            case Theme.Dark: _cachedStyle = RichLogViewStyle.DarkThemeDefault; break;
            case Theme.Light: _cachedStyle = RichLogViewStyle.LightThemeDefault; break;
            default: throw new Exception("Unexpected enumerate value");
        }
        return;
    }

    private void SynchronizeCachedStyle()
    {
        switch (_targetTheme)
        {
            case Theme.Dark:
                _styleSettings.Data.RichLogViewStyleDark = _cachedStyle;
                _styleSettings.SynchronizeData();
                break;
            case Theme.Light:
                _styleSettings.Data.RichLogViewStyleLight = _cachedStyle;
                _styleSettings.SynchronizeData();
                break;
            default: throw new Exception("Unexpected enumerate value");
        }
        return;
    }

    private void DebugLogLevelStyleColorPicker_ColorChangedDelayed(object sender, EventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        _cachedStyle.LogLevelDebugColor = _debugLogLevelStyleColorPicker.SelectedColor.ToRGBColor();
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void DebugLogLevelStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        if (!_debugLogLevelStyleComboBox.TryGetSelectedItem(out TextStyle state))
        {
            return;
        }

        _cachedStyle.LogLevelDebugStyle = state;
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void ErrorLogLevelStyleColorPicker_ColorChangedDelayed(object sender, EventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        _cachedStyle.LogLevelErrorColor = _errorLogLevelStyleColorPicker.SelectedColor.ToRGBColor();
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void ErrorLogLevelStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        if (!_errorLogLevelStyleComboBox.TryGetSelectedItem(out TextStyle state))
        {
            return;
        }

        _cachedStyle.LogLevelErrorStyle = state;
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void ExceptionStyleColorPicker_ColorChangedDelayed(object sender, EventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        _cachedStyle.ExceptionTextColor = _exceptionStyleColorPicker.SelectedColor.ToRGBColor();
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void ExceptionStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        if (!_exceptionStyleComboBox.TryGetSelectedItem(out TextStyle state))
        {
            return;
        }

        _cachedStyle.ExceptionTextStyle = state;
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void FilenameColorColorPicker_ColorChangedDelayed(object sender, EventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        _cachedStyle.FilenameColor = _filenameColorColorPicker.SelectedColor.ToRGBColor();
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        if (!_fontComboBox.TryGetSelectedItem(out FontFamily? state))
        {
            return;
        }

        if (state is null)
        {
            return;
        }

        _cachedStyle.Font = state.GetEnUsFamilyName();
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void InfoLogLevelStyleColorPicker_ColorChangedDelayed(object sender, EventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        _cachedStyle.LogLevelInfoColor = _infoLogLevelStyleColorPicker.SelectedColor.ToRGBColor();
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void InfoLogLevelStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        if (!_infoLogLevelStyleComboBox.TryGetSelectedItem(out TextStyle state))
        {
            return;
        }

        _cachedStyle.LogLevelInfoStyle = state;
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void NumberColorColorPicker_ColorChangedDelayed(object sender, EventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        _cachedStyle.NumberColor = _numberColorColorPicker.SelectedColor.ToRGBColor();
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private async void ResetToDefaultButton_ClickAsync(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        ResetCachedStyle();
        SynchronizeCachedStyle();
        await RefreshAsync();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void StringColorColorPicker_ColorChangedDelayed(object sender, EventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        _cachedStyle.StringColor = _stringColorColorPicker.SelectedColor.ToRGBColor();
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private async void TargetThemeComboBox_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        if (!_targetThemeComboBox.TryGetSelectedItem(out Theme state))
        {
            return;
        }

        _targetTheme = state;
        await RefreshAsync();

        return;
    }

    private void TextColorColorPicker_ColorChangedDelayed(object sender, EventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        _cachedStyle.NormalTextColor = _textColorColorPicker.SelectedColor.ToRGBColor();
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void TimeColorHMSColorPicker_ColorChangedDelayed(object sender, EventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        _cachedStyle.TimeHMSColor = _timeColorHMSColorPicker.SelectedColor.ToRGBColor();
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void TimeColorYMDColorPicker_ColorChangedDelayed(object sender, EventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        _cachedStyle.TimeYMDColor = _timeColorYMDColorPicker.SelectedColor.ToRGBColor();
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void WarningLogLevelStyleColorPicker_ColorChangedDelayed(object sender, EventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        _cachedStyle.LogLevelWarningColor = _warningLogLevelStyleColorPicker.SelectedColor.ToRGBColor();
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }

    private void WarningLogLevelStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (_settings.Data.RichLogViewStyleSource != RichLogViewStyleSource.Custom)
        {
            return;
        }

        if (!_warningLogLevelStyleComboBox.TryGetSelectedItem(out TextStyle state))
        {
            return;
        }

        _cachedStyle.LogLevelWarningStyle = state;
        SynchronizeCachedStyle();

        _styleManager.RaiseStyleChanged();

        return;
    }
}
