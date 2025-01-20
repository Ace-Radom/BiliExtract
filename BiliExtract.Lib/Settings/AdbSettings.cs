using static BiliExtract.Lib.Settings.AdbSettings;

namespace BiliExtract.Lib.Settings;

public class AdbSettings() : AbstractSettings<AdbSettingsData>("adbsettings.json")
{
    public class AdbSettingsData
    {
        public bool AutoStartServerIfNotStarted { get; set; } = true;
        public bool CheckServerStartedBeforeOperate { get; set; } = true;
        public bool KillServerOnExit { get; set; } = true;
        public string ServerIp { get; set; } = "127.0.0.1";
        public int ServerPort { get; set; } = 5037;
        public bool StartServerOnStartup { get; set; } = true;
        public string? WirelessDeviceDefaultIp { get; set; } = null;
    }
}
