using BiliExtract.Lib;
using BiliExtract.Lib.Settings;
using System.Windows;
using Wpf.Ui.Appearance;

namespace BiliExtract.Managers;

public class ThemeManager
{
    private readonly ApplicationSettings _settings = IoCContainer.Resolve<ApplicationSettings>();

    private bool _isWatchingSystemTheme = false;

    public void RefreshTheme()
    {
        switch (_settings.Data.Theme) {
            case Theme.Dark:
                UnwatchSystemTheme();
                ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                break;
            case Theme.Light:
                UnwatchSystemTheme();
                ApplicationThemeManager.Apply(ApplicationTheme.Light);
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

    private void WatchSystemTheme()
    {
        if (_isWatchingSystemTheme)
        {
            return;
        }

        SystemThemeWatcher.Watch(Application.Current.MainWindow);
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
