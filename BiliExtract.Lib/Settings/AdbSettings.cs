using static BiliExtract.Lib.Settings.AdbSettings;

namespace BiliExtract.Lib.Settings;

public class AdbSettings() : AbstractSettings<AdbSettingsData>("adbsettings.json")
{
    public class AdbSettingsData
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 5037;
    }
}
