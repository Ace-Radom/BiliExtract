using BiliExtract.Lib;
using BiliExtract.Lib.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace BiliExtract.Views.Windows;

public partial class MainWindow
{

    public MainWindow()
    {
        InitializeComponent();

        return;
    }

    /// <summary>
    /// Raises the closed event.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // Make sure that closing this window will begin the process of closing the application.
        Application.Current.Shutdown();

        return;
    }

    private void LoggingToFileBadge_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter and not Key.Space)
        {
            return;
        }

        OpenCurrentLogFile();
        return;
    }

    private void LoggingToFileBadge_MouseButtonDown(object sender, MouseButtonEventArgs e) => OpenCurrentLogFile();

    private static void OpenCurrentLogFile()
    {
        try
        {
            if (!Directory.Exists(Folders.AppData))
            {
                return;
            }

            Process.Start("explorer", Log.GlobalLogger.LogPath);
        }
        catch (Exception ex)
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Failed to open current log file. [path=\"{Log.GlobalLogger.LogPath}\"]", ex);
        }
        return;
    }
}
