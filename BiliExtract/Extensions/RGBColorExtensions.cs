using BiliExtract.Lib;
using System.Windows.Media;

namespace BiliExtract.Extensions;

public static class RGBColorExtensions
{
    public static SolidColorBrush ToBrush(this RGBColor color) => new(color.ToColor());
    public static Color ToColor(this RGBColor color) => Color.FromRgb(color.R, color.G, color.B);
}
