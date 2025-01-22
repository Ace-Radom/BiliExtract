using BiliExtract.Extensions;
using BiliExtract.Lib;
using BiliExtract.Lib.Listener;
using BiliExtract.Lib.Settings;
using BiliExtract.Lib.Utils;
using System;
using System.Windows;

namespace BiliExtract.Managers;

public class ThemeManager
{
    private static readonly RGBColor DefaultAccentColor = new(255, 33, 33);

    private readonly ApplicationSettings _settings;
    private readonly SystemThemeListener _listener;

    public event EventHandler? ThemeApplied;

    public ThemeManager(SystemThemeListener systemThemeListener, ApplicationSettings settings)
    {
        _listener = systemThemeListener;
        _settings = settings;

        _listener.Changed += (_, _) => Application.Current.Dispatcher.Invoke(Apply);

        return;
    }

    public void Apply()
    {
        SetTheme();
        SetColor();

        ThemeApplied?.Invoke(this, EventArgs.Empty);

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
                    return SystemTheme.GetAccentColor();
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

    public bool IsDarkMode()
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
                    return SystemTheme.IsDarkMode();
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
        var theme = IsDarkMode() ? Wpf.Ui.Appearance.ThemeType.Dark : Wpf.Ui.Appearance.ThemeType.Light;
        Wpf.Ui.Appearance.Theme.Apply(theme, Wpf.Ui.Appearance.BackgroundType.Mica, false);
        return;
    }

    private void SetColor()
    {
        var accentColor = GetAccentColor().ToColor();
        Wpf.Ui.Appearance.Accent.Apply(
            systemAccent: accentColor,
            primaryAccent: accentColor,
            secondaryAccent: accentColor,
            tertiaryAccent: accentColor
        );
        return;
    }
}
