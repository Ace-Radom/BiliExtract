using BiliExtract.Lib.Utils;
using System;
using System.Threading.Tasks;

namespace BiliExtract.Lib.Listener;

public class SystemThemeListener : IListener<EventArgs>
{
    public event EventHandler<EventArgs>? Changed;

    private IDisposable? _darkModeListener;
    private IDisposable? _colorizationColorListener;

    private RGBColor? _currentRegColor;

    private bool _started;

    public Task StartAsync()
    {
        if (_started)
            return Task.CompletedTask;

        _darkModeListener = SystemThemeHelper.GetDarkModeListener(OnDarkModeChanged);
        _colorizationColorListener = SystemThemeHelper.GetColorizationColorListener(OnColorizationColorChanged);

        _started = true;

        return Task.CompletedTask;
    }

    private void OnDarkModeChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void OnColorizationColorChanged()
    {
        try
        {
            var color = SystemThemeHelper.GetColorizationColor();

            // Ignore alpha channel transition events
            if (color.Equals(_currentRegColor))
            {
                return;
            }

            _currentRegColor = color;

            Changed?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Failed to notify on accent color change.", ex);
        }
    }

    public Task StopAsync()
    {
        _darkModeListener?.Dispose();
        _colorizationColorListener?.Dispose();

        _started = false;

        return Task.CompletedTask;
    }
}
