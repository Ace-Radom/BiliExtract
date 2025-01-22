using BiliExtract.Lib;
using System.Windows;
using System.Windows.Documents;

namespace BiliExtract.Extensions;

public static class RunExtensions
{
    public static void ApplyTextStyle(this Run run, TextStyle style)
    {
        switch (style)
        {
            case TextStyle.Normal:
                run.FontStyle = FontStyles.Normal;
                run.FontWeight = FontWeights.Normal;
                break;
            case TextStyle.Bold:
                run.FontStyle = FontStyles.Normal;
                run.FontWeight = FontWeights.Bold;
                break;
            case TextStyle.Italic:
                run.FontStyle = FontStyles.Italic;
                run.FontWeight = FontWeights.Normal;
                break;
            case TextStyle.Underline:
                run.FontStyle = FontStyles.Normal;
                run.FontWeight = FontWeights.Normal;
                run.TextDecorations = TextDecorations.Underline;
                break;
            default:
                run.FontStyle = FontStyles.Normal;
                run.FontWeight = FontWeights.Normal;
                break;
        }
        return;
    }

    public static Run SetTextAndStyle(this Run run, string text, RGBColor foregroundColor, TextStyle style = TextStyle.Normal)
    {
        run.Text = text;
        run.Foreground = foregroundColor.ToBrush();
        run.ApplyTextStyle(style);
        return run;
    }
}
