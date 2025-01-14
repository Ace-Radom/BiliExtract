using static BiliExtract.Lib.Settings.ApplicationSettings;

namespace BiliExtract.Lib.Settings;

public class ApplicationSettings() : AbstractSettings<ApplicationSettingsData>("settings.json")
{
    public class ApplicationSettingsData
    {
        public Theme Theme { get; set; }
        public Size WindowSize { get; set; }
    }
}
