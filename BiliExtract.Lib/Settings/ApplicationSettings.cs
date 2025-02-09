using static BiliExtract.Lib.Settings.ApplicationSettings;

using System;

namespace BiliExtract.Lib.Settings;

public class ApplicationSettings() : AbstractSettings<ApplicationSettingsData>("settings.json")
{
    public class ApplicationSettingsData
    {
        private int _autoTempCleanupIntervalMin = 1;
        private int _tempFolderStorageUsageRefreshIntervalMin = 1;

        public event EventHandler<EventArgs>? AutoTempCleanupIntervalChanged;
        public event EventHandler<EventArgs>? TempFolderStorageUsageRefreshIntervalChanged;

        public RGBColor? AccentColor { get; set; }
        public AccentColorSource AccentColorSource { get; set; }
        public int AutoTempCleanupIntervalMin
        {
            get => _autoTempCleanupIntervalMin;
            set
            {
                if (_autoTempCleanupIntervalMin != value)
                {
                    _autoTempCleanupIntervalMin = value;
                    AutoTempCleanupIntervalChanged?.Invoke(this, EventArgs.Empty);
                }
                return;
            }
        }
        public DataSizePrefix DataSizePrefix { get; set; } = DataSizePrefix.Metric;
        public string? ExternalAdbPath { get; set; }
        public string? ExternalFFmpegPath { get; set; }
        public LogLevel MinLogLevel { get; set; } = LogLevel.Info;
        public RichLogViewStyleSource RichLogViewStyleSource { get; set; }
        public int TempFolderStorageUsageRefreshIntervalMin
        {
            get => _tempFolderStorageUsageRefreshIntervalMin;
            set
            {
                if (_tempFolderStorageUsageRefreshIntervalMin != value)
                {
                    _tempFolderStorageUsageRefreshIntervalMin = value;
                    TempFolderStorageUsageRefreshIntervalChanged?.Invoke(this, EventArgs.Empty);
                }
                return;
            }
        }
        public Theme Theme { get; set; }
        public bool UseBuiltInAdb { get; set; } = true;
        public bool UseBuiltInFFmpeg { get; set; } = true;
        public Size WindowSize { get; set; }
    }
}
