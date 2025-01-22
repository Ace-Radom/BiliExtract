using BiliExtract.Lib;
using BiliExtract.Lib.Events;
using BiliExtract.Managers;
using BiliExtract.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BiliExtract.Views.Pages;

public partial class LogPage
{
    private readonly RichLogViewStyleManager _styleManager = IoCContainer.Resolve<RichLogViewStyleManager>();
    private readonly ThemeManager _themeManager = IoCContainer.Resolve<ThemeManager>();

    private static readonly SolidColorBrush _darkThemeRichTextBoxBackgroundColor = new(Color.FromArgb(0xB3, 0x1E, 0x1E, 0x1E));
    private static readonly SolidColorBrush _lightThemeRichTextBoxBackgroundColor = new(Colors.White);

    public LogPage()
    {
        InitializeComponent();

        UpdateLogCountTextBlock();
        UpdateLogRichTextBoxBackgroundColor();

        _logRichTextBox.FontFamily = _styleManager.DefaultFont;
        _logRichTextBox.Document.Blocks.Clear();
        _logRichTextBox.Document.Blocks.AddRange(_styleManager.ParseLogMessages(Log.GlobalLogger.LogMessages.Split('\n')));
        UpdateLogRichTextBoxPageWidth();

        _logRichTextBox.LayoutUpdated += (_, _) => UpdateLogRichTextBoxPageWidth();
        _themeManager.ThemeApplied += (_, _) => UpdateLogRichTextBoxBackgroundColor();
        IsVisibleChanged += LogPage_IsVisibleChanged;

        return;
    }

    private void LogPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsVisible)
        {
            UpdateLogCountTextBlock();
            _logRichTextBox.Document.Blocks.Clear();
            _logRichTextBox.Document.Blocks.AddRange(_styleManager.ParseLogMessages(Log.GlobalLogger.LogMessages.Split('\n')));
            UpdateLogRichTextBoxPageWidth();
            Log.GlobalLogger.LogRefreshed += GlobalLogger_LogRefreshed;
        }
        else
        {
            Log.GlobalLogger.LogRefreshed -= GlobalLogger_LogRefreshed;
        }
        
        return;
    }

    private void GlobalLogger_LogRefreshed(object sender, LogRefreshedEventArgs e)
    {
        UpdateLogCountTextBlock();
        _logRichTextBox.Document.Blocks.AddRange(_styleManager.ParseLogMessages(e.NewLogMessages));
        UpdateLogRichTextBoxPageWidth();
        return;
    }

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
        _logRichTextBox.HorizontalScrollBarVisibility = (_logCountTextBlock.ActualWidth < ft.Width + 12) ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        return;
    }
}
