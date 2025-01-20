using BiliExtract.Lib;
using System.Windows.Media;

namespace BiliExtract.Extensions;

public static class RGBColorExtensions
{
    public static Color ToColor(this RGBColor color) => Color.FromRgb(color.R, color.G, color.B);
}
