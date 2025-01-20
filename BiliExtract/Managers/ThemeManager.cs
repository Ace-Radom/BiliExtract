using BiliExtract.Extensions;
using BiliExtract.Lib;
using BiliExtract.Lib.Settings;
using BiliExtract.Lib.Utils;
using System;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace BiliExtract.Managers;

public class ThemeManager
{
    private static readonly RGBColor DefaultAccentColor = new(255, 33, 33);

    private readonly ApplicationSettings _settings = IoCContainer.Resolve<ApplicationSettings>();

    private bool _isWatchingSystemTheme = false;

    public void Refresh()
    {
        RefreshTheme();
        RefreshAccentColor();
        return;
    }

    private void RefreshTheme()
    {
        switch (_settings.Data.Theme) {
            case Theme.Dark:
                UnwatchSystemTheme();
                ApplicationThemeManager.Apply(ApplicationTheme.Dark, Wpf.Ui.Controls.WindowBackdropType.Mica, false);
                break;
            case Theme.Light:
                UnwatchSystemTheme();
                ApplicationThemeManager.Apply(ApplicationTheme.Light, Wpf.Ui.Controls.WindowBackdropType.Mica, false);
                break;
            case Theme.FollowSystem:
                WatchSystemTheme();
                break;
            default:
                WatchSystemTheme();
                break;
        };
        return;
    }

    private void RefreshAccentColor()
    {
        var accentColor = GetAccentColor().ToColor();
        ApplicationAccentColorManager.ApplySystemAccent();
        //ApplicationAccentColorManager.Apply(
        //    accentColor, ApplicationTheme.Dark, false
        //);
        return;
    }

    public RGBColor GetAccentColor()
    {
        switch (_settings.Data.AccentColorSource)
        {
            case AccentColorSource.Custom:
                return _settings.Data.AccentColor ?? DefaultAccentColor;
            case AccentColorSource.System:
                try
                {
                    return SystemThemeHelper.GetAccentColor();
                }
                catch (Exception ex)
                {
                    Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Failed to get system accent color, using default instead.", ex);
                    return DefaultAccentColor;
                }
            default:
                return DefaultAccentColor;
        }
    }

    private void WatchSystemTheme()
    {
        if (_isWatchingSystemTheme)
        {
            return;
        }
        
        ApplicationThemeManager.ApplySystemTheme(false);
        //ApplicationThemeManager.Apply(
        //    SystemThemeHelper.IsDarkMode() ? ApplicationTheme.Dark : ApplicationTheme.Light,
        //    Wpf.Ui.Controls.WindowBackdropType.Mica,
        //    false
        //);
        SystemThemeWatcher.Watch(Application.Current.MainWindow, Wpf.Ui.Controls.WindowBackdropType.Mica, false);
        _isWatchingSystemTheme = true;
        return;
    }

    private void UnwatchSystemTheme()
    {
        if (!_isWatchingSystemTheme) 
        {
            return;
        }

        SystemThemeWatcher.UnWatch(Application.Current.MainWindow);
        _isWatchingSystemTheme = false;
        return;
    }
}
