using static BiliExtract.Lib.Settings.TextStyleSettings;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliExtract.Lib.Settings;

public class TextStyleSettings() : AbstractSettings<TextStyleSettingsData>("textstylesettings.json")
{
    public class TextStyleSettingsData
    {
        public RichLogViewStyle RichLogViewStyleDark { get; set; } = RichLogViewStyle.DarkThemeDefault;
        public RichLogViewStyle RichLogViewStyleLight { get; set; } = RichLogViewStyle.LightThemeDefault;
    }
}
