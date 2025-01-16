using static BiliExtract.Lib.Settings.AdbSettings;

using System.Net;

namespace BiliExtract.Lib.Settings;

public class AdbSettings() : AbstractSettings<AdbSettingsData>("adbsettings.json")
{
    public class AdbSettingsData
    {
        public bool AutoStartServerIfNotStarted { get; set; } = true;
        public bool CheckServerStartedBeforeOperate { get; set; } = true;
        public bool KillServerOnExit { get; set; } = true;
        public IPAddress ServerHost { get; set; } = IPAddress.Parse("127.0.0.1");
        public int ServerPort { get; set; } = 5037;
        public IPAddress? WirelessDeviceDefaultIp { get; set; } = null;
    }
}
