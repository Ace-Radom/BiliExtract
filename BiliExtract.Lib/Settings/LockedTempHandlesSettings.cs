using static BiliExtract.Lib.Settings.LockedTempHandlesSettings;

namespace BiliExtract.Lib.Settings;

public class LockedTempHandlesSettings() : AbstractSettings<LockedTempHandlesSettingsData>("locked_temp_handles_store.json")
{
    public class LockedTempHandlesSettingsData
    {
        public TempFileHandle[] LockedTempHandles { get; set; } = [];
    }
}
