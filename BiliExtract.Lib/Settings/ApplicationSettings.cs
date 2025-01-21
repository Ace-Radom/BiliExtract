using static BiliExtract.Lib.Settings.ApplicationSettings;

namespace BiliExtract.Lib.Settings;

public class ApplicationSettings() : AbstractSettings<ApplicationSettingsData>("settings.json")
{
    public class ApplicationSettingsData
    {
        public RGBColor? AccentColor { get; set; }
        public AccentColorSource AccentColorSource { get; set; }
        public string? ExternalAdbPath { get; set; }
        public string? ExternalFFmpegPath { get; set; }
        public LogLevel MinLogLevel { get; set; } = LogLevel.Info;
        public Theme Theme { get; set; }
        public bool UseBuiltInAdb { get; set; } = true;
        public bool UseBuiltInFFmpeg { get; set; } = true;
        public Size WindowSize { get; set; }
    }
}
