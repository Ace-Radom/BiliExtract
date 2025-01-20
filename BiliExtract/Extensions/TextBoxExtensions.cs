using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BiliExtract.Extensions;

public static class TextBoxExtensions
{
    public static void SetErrorBorderStyle(this TextBox textBox) => textBox.BorderBrush = Brushes.Red;

    public static void SetNormalBorderStyle(this TextBox textBox)
    {
        var parentFrameworkElement = textBox.Parent as FrameworkElement;
        if (parentFrameworkElement is not null)
        {
            var defaultBorderBrush = parentFrameworkElement.FindResource("ControlElevationBorderBrush") as Brush;
            if (defaultBorderBrush is not null)
            {
                textBox.BorderBrush = defaultBorderBrush;
            }
            else
            {
                textBox.BorderBrush = Brushes.Gray;
            }
        }
        else
        {
            textBox.BorderBrush = Brushes.Gray;
        }
        return;
    }
}
