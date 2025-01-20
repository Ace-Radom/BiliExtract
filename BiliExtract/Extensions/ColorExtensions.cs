using BiliExtract.Lib;
using System.Windows.Media;

namespace BiliExtract.Extensions;

public static class ColorExtensions
{
    public static RGBColor ToRGBColor(this Color color) => new(color.R, color.G, color.B);
}
