using BiliExtract.Lib;
using BiliExtract.Lib.Events;
using BiliExtract.Lib.Extensions;
using BiliExtract.Lib.Settings;
using BiliExtract.Managers;
using BiliExtract.Resources;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BiliExtract.Views.Pages;

public partial class LogPage
{
    private readonly ApplicationSettings _settings = IoCContainer.Resolve<ApplicationSettings>();
    private readonly RichLogViewStyleManager _styleManager = IoCContainer.Resolve<RichLogViewStyleManager>();
    private readonly ThemeManager _themeManager = IoCContainer.Resolve<ThemeManager>();

    private static readonly SolidColorBrush _darkThemeRichTextBoxBackgroundColor = new(Color.FromArgb(0xB3, 0x1E, 0x1E, 0x1E));
    private static readonly SolidColorBrush _lightThemeRichTextBoxBackgroundColor = new(Colors.White);

    public LogPage()
    {
        InitializeComponent();

        UpdateLogRichTextBoxBackgroundColor();

        _logRichTextBox.LayoutUpdated += (_, _) => UpdateLogRichTextBoxPageWidth();
        _themeManager.ThemeApplied += (_, _) => UpdateLogRichTextBoxBackgroundColor();
        IsVisibleChanged += LogPage_IsVisibleChangedAsync;

        return;
    }

    private async void LogPage_IsVisibleChangedAsync(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsVisible)
        {
            await RefreshAsync();
            _styleManager.StyleChanged += GlobalLogger_StyleChangedAsync;
            Log.GlobalLogger.LogRefreshed += GlobalLogger_LogRefreshedAsync;
        }
        else
        {
            _styleManager.StyleChanged -= GlobalLogger_StyleChangedAsync;
            Log.GlobalLogger.LogRefreshed -= GlobalLogger_LogRefreshedAsync;
        }

        return;
    }

    private Task RefreshAsync()
    {
        UpdateLogCountTextBlock();
        UpdateLogMinLevelTextBlock();
        _logRichTextBox.Document.Blocks.Clear();
        var font = _styleManager.Font;
        if (font is not null)
        {
            _logRichTextBox.FontFamily = font;
        }
        _logRichTextBox.Document.Blocks.AddRange(_styleManager.ParseLogMessages(Log.GlobalLogger.LogMessages.Split('\n')));
        UpdateLogRichTextBoxPageWidth();

        return Task.CompletedTask;
    }

    private async void GlobalLogger_LogRefreshedAsync(object sender, LogRefreshedEventArgs e)
    {
        await Dispatcher.InvokeAsync(() =>
        {
            UpdateLogCountTextBlock();
            _logRichTextBox.Document.Blocks.AddRange(_styleManager.ParseLogMessages(e.NewLogMessages));
            UpdateLogRichTextBoxPageWidth();
        });
        return;
    }

    private async void GlobalLogger_StyleChangedAsync(object? sender, EventArgs e) => await Dispatcher.InvokeAsync(async () => await RefreshAsync());

    private void UpdateLogCountTextBlock()
    {
        _logCountTextBlock.Text = string.Format(Resource.LogPage_LogCountTextBlock_Text, Log.GlobalLogger.LogMessagesCount);
        return;
    }

    private void UpdateLogMinLevelTextBlock()
    {
        _logMinLevelTextBlock.Text = string.Format(Resource.LogPage_LogMinLevelTextBlock_Text, _settings.Data.MinLogLevel.GetDisplayName());
        return;
    }

    private void UpdateLogRichTextBoxBackgroundColor()
    {
        _logRichTextBox.Background = _themeManager.IsDarkMode() ? _darkThemeRichTextBoxBackgroundColor : _lightThemeRichTextBoxBackgroundColor;
        return;
    }

    private void UpdateLogRichTextBoxPageWidth()
    {
        string text = new TextRange(_logRichTextBox.Document.ContentStart, _logRichTextBox.Document.ContentEnd).Text;
        FormattedText ft = new(text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(_logRichTextBox.FontFamily, _logRichTextBox.FontStyle, _logRichTextBox.FontWeight, _logRichTextBox.FontStretch),
            _logRichTextBox.FontSize,
            Brushes.Black
        );
        _logRichTextBox.Document.PageWidth = ft.Width + 12;
        _logRichTextBox.HorizontalScrollBarVisibility = (_logRichTextBox.ActualWidth < ft.Width + 12) ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        return;
    }

    private void SaveFluentIconButton_Click(object sender, RoutedEventArgs e)
    {
        var logMsgs = Log.GlobalLogger.LogMessages;
        SaveFileDialog sfd = new()
        {
            Title = Resource.SaveFileDialog_SaveLog_Title,
            Filter = $"{Resource.SaveFileDialog_Filter_LogFile}|*.log|{Resource.SaveFileDialog_Filter_TextFile}|*.txt|{Resource.SaveFileDialog_Filter_AllFile}|*.*",
            DefaultExt = "*.log",
            AddExtension = true,
            FileName = $"BiliExtract_{DateTime.UtcNow:yyyy_MM_dd_HH_mm_ss}"
        };
        if (sfd.ShowDialog() == true)
        {
            string filePath = sfd.FileName;
            try
            {
                File.WriteAllText(filePath, logMsgs);
            }
            catch (Exception ex)
            {
                Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Save log file failed. [path=\"{filePath}\"]", ex);
            }
        }
        return;
    }
}
