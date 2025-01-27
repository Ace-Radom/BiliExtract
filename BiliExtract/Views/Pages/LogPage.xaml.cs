using BiliExtract.Lib;
using BiliExtract.Lib.Events;
using BiliExtract.Managers;
using BiliExtract.Resources;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BiliExtract.Views.Pages;

public partial class LogPage
{
    private readonly object _lock = new();
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

    private async Task RefreshAsync()
    {
        await Dispatcher.InvokeAsync(() =>
        {
            UpdateLogCountTextBlock();
            _logRichTextBox.Document.Blocks.Clear();
            var font = _styleManager.Font;
            if (font is not null)
            {
                _logRichTextBox.FontFamily = font;
            }
            _logRichTextBox.Document.Blocks.AddRange(_styleManager.ParseLogMessages(Log.GlobalLogger.LogMessages.Split('\n')));
            UpdateLogRichTextBoxPageWidth();
        });

        return;
    }

    private async void GlobalLogger_LogRefreshedAsync(object sender, LogRefreshedEventArgs e)
    {
        await Task.Run(() =>
        {
            Dispatcher.Invoke(() =>
            {
                UpdateLogCountTextBlock();
                _logRichTextBox.Document.Blocks.AddRange(_styleManager.ParseLogMessages(e.NewLogMessages));
                UpdateLogRichTextBoxPageWidth();
            });
        });
        return;
    }

    private async void GlobalLogger_StyleChangedAsync(object? sender, System.EventArgs e) => await Dispatcher.InvokeAsync(async () => await RefreshAsync());

    private void UpdateLogCountTextBlock()
    {
        _logCountTextBlock.Text = string.Format(Resource.LogPage_LogCountTextBlock_Text, Log.GlobalLogger.LogMessagesCount);
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
}
