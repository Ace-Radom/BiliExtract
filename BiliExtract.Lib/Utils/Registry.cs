using Microsoft.Win32;
using System;
using System.IO;
using System.Management;
using System.Security.Principal;

namespace BiliExtract.Lib.Utils;

public static class Registry
{
    public static bool KeyExists(string hive, string subKey)
    {
        try
        {
            using var baseKey = GetBaseKey(hive);
            using var registryKey = baseKey.OpenSubKey(subKey);
            return registryKey is not null;
        }
        catch
        {
            return false;
        }
    }

    public static bool ValueExists(string hive, string subKey, string valueName)
    {
        try
        {
            var keyName = Path.Combine(hive, subKey);
            var value = Microsoft.Win32.Registry.GetValue(keyName, valueName, null);
            return value is not null;
        }
        catch
        {
            return false;
        }
    }

    public static T GetValue<T>(string hive, string subKey, string valueName, T defaultValue, bool doNotExpand = false)
    {
        using var baseKey = GetBaseKey(hive);
        var value = baseKey.OpenSubKey(subKey)?.GetValue(valueName, defaultValue, doNotExpand ? RegistryValueOptions.DoNotExpandEnvironmentNames : RegistryValueOptions.None);

        if (value is not T t)
            return defaultValue;

        return t;
    }

    public static IDisposable ObserveValue(string hive, string path, string valueName, Action handler)
    {
        if (hive is "HKEY_CURRENT_USER" or "HKCU")
            hive = WindowsIdentity.GetCurrent().User?.Value ?? throw new InvalidOperationException("Current user value is null");

        var pathFormatted = @$"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{hive}\\{path.Replace(@"\", @"\\")}' AND ValueName = '{valueName}'";

        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Starting listener... [hive={hive}, pathFormatted={pathFormatted}, key={valueName}]");

        var watcher = new ManagementEventWatcher(pathFormatted);
        watcher.EventArrived += (_, e) =>
        {
                Log.GlobalLogger.WriteLog(LogLevel.Info, $"Event arrived [classPath={e.NewEvent.ClassPath}, hive={hive}, pathFormatted={pathFormatted}, key={valueName}]");

            handler();
        };
        watcher.Start();

        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Started listener [hive={hive}, pathFormatted={pathFormatted}, key={valueName}]");

        return watcher;
    }

    private static RegistryKey GetBaseKey(string hive) => hive switch
    {
        "HKLM" or "HKEY_LOCAL_MACHINE" => Microsoft.Win32.Registry.LocalMachine,
        "HKCU" or "HKEY_CURRENT_USER" => Microsoft.Win32.Registry.CurrentUser,
        "HKU" or "HKEY_USERS" => Microsoft.Win32.Registry.Users,
        "HKCR" or "HKEY_CLASSES_ROOT " => Microsoft.Win32.Registry.ClassesRoot,
        "HKCC" or "HKEY_CURRENT_CONFIG  " => Microsoft.Win32.Registry.CurrentConfig,
        _ => throw new ArgumentException(@"Unknown hive.", nameof(hive))
    };
}
