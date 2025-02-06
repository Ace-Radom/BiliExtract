using static BiliExtract.Lib.Settings.TextStyleSettings;

namespace BiliExtract.Lib.Settings;

public class TextStyleSettings() : AbstractSettings<TextStyleSettingsData>("text_style_settings.json")
{
    public class TextStyleSettingsData
    {
        public RichLogViewStyle RichLogViewStyleDark { get; set; } = RichLogViewStyle.DarkThemeDefault;
        public RichLogViewStyle RichLogViewStyleLight { get; set; } = RichLogViewStyle.LightThemeDefault;
    }
}
