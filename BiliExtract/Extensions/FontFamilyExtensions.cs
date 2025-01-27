using System.Windows.Markup;
using System.Windows.Media;

namespace BiliExtract.Extensions;

public static class FontFamilyExtensions
{
    public static string GetEnUsFamilyName(this FontFamily font) => font.FamilyNames[XmlLanguage.GetLanguage("en-us")];
}
