using System.Linq;
using System.Windows.Markup;
using System.Windows.Media;

namespace BiliExtract.Extensions;

public static class LanguageSpecificStringDictionaryExtensions
{
    public static bool HasName(this LanguageSpecificStringDictionary dic, XmlLanguage language, string name) => dic.Any(t => t.Key == language && t.Value.Equals(name));
}
