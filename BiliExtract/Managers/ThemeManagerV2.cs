using BiliExtract.Extensions;
using BiliExtract.Lib;
using BiliExtract.Lib.Listener;
using BiliExtract.Lib.Settings;
using BiliExtract.Lib.Utils;
using System;
using System.Windows;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;

namespace BiliExtract.Managers;

public class ThemeManagerV2
{
    private static readonly RGBColor DefaultAccentColor = new(255, 33, 33);

    private readonly ApplicationSettings _settings;
    private readonly SystemThemeListener _listener;

    public event EventHandler? ThemeApplied;

    public ThemeManagerV2(SystemThemeListener systemThemeListener, ApplicationSettings settings)
    {
        _listener = systemThemeListener;
        _settings = settings;

        _listener.Changed += (_, _) => Application.Current.Dispatcher.Invoke(Apply);
    }

    public void Apply()
    {
        SetTheme();
        SetColor();

        ThemeApplied?.Invoke(this, EventArgs.Empty);
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
                        Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Couldn't check system accent color; using default.", ex);

                    return DefaultAccentColor;
                }
            default:
                return DefaultAccentColor;
        }
    }

    private bool IsDarkMode()
    {
        var theme = _settings.Data.Theme;

        switch (theme)
        {
            case Theme.Dark:
                return true;
            case Theme.Light:
                return false;
            case Theme.FollowSystem:
                try
                {
                    return SystemThemeHelper.IsDarkMode();
                }
                catch (Exception ex)
                {
                    Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Couldn't check system theme; assuming Dark Mode.", ex);
                    return true;
                }
            default:
                return true;
        }
    }

    private void SetTheme()
    {
        var theme = IsDarkMode() ? ApplicationTheme.Dark : ApplicationTheme.Light;
        ApplicationThemeManager.Apply(theme, WindowBackdropType.Mica, false);
    }

    private void SetColor()
    {
        var accentColor = GetAccentColor().ToColor();
        ApplicationAccentColorManager.Apply(systemAccent: accentColor,
            primaryAccent: accentColor,
            secondaryAccent: accentColor,
            tertiaryAccent: accentColor);
    }
}
