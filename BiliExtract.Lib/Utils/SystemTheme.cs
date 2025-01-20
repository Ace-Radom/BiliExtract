using System;
using System.ComponentModel;

namespace BiliExtract.Lib.Utils;

public static class SystemTheme
{
    private const string REGISTRY_HIVE = "HKEY_CURRENT_USER";

    private const string PERSONALIZE_REGISTRY_PATH = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string APPS_USE_LIGHT_THEME_REGISTRY_KEY = "AppsUseLightTheme";

    private const string DWM_REGISTRY_PATH = @"Software\Microsoft\Windows\DWM";
    private const string DWM_COLORIZATION_COLOR_REGISTRY_KEY = "ColorizationColor";

    public static bool IsDarkMode()
    {
        var registryValue = Registry.GetValue(REGISTRY_HIVE, PERSONALIZE_REGISTRY_PATH, APPS_USE_LIGHT_THEME_REGISTRY_KEY, -1);
        if (registryValue == -1)
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Read registry key failed. [key=\"{REGISTRY_HIVE}\\{PERSONALIZE_REGISTRY_PATH}\\{APPS_USE_LIGHT_THEME_REGISTRY_KEY}\"]");
            throw new InvalidOperationException($"Couldn't read {APPS_USE_LIGHT_THEME_REGISTRY_KEY} setting");
        }
        return registryValue == 0;
    }

    public static RGBColor GetColorizationColor()
    {
        var registryValue = Registry.GetValue(REGISTRY_HIVE, DWM_REGISTRY_PATH, DWM_COLORIZATION_COLOR_REGISTRY_KEY, -1);
        if (registryValue == -1)
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Read registry key failed. [key=\"{REGISTRY_HIVE}\\{DWM_REGISTRY_PATH}\\{DWM_COLORIZATION_COLOR_REGISTRY_KEY}\"]");
            throw new InvalidOperationException($"Couldn't read the {DWM_COLORIZATION_COLOR_REGISTRY_KEY} setting");
        }

        var bytes = BitConverter.GetBytes(registryValue);
        return new(bytes[2], bytes[1], bytes[0]);
    }

    public static RGBColor GetAccentColor()
    {
        var colorName = IsDarkMode() ? "SystemAccentLight2" : "SystemAccentDark1";
        var colorType = Native.GetImmersiveColorTypeFromName("Immersive" + colorName);
        if (colorType == 0xFFFFFFFF)
        {
            throw new Win32Exception($"Failed to get accent color. [name=\"{colorName}\"]");
        }

        var activeColorSet = Native.GetImmersiveUserColorSetPreference(false, false);
        var nativeColor = Native.GetImmersiveColorFromColorSetEx(activeColorSet, colorType, false, 0);
        var r = (byte)((0x000000FF & nativeColor) >> 0);
        var g = (byte)((0x0000FF00 & nativeColor) >> 8);
        var b = (byte)((0x00FF0000 & nativeColor) >> 16);

        return new(r, g, b);
    }

    internal static IDisposable GetDarkModeListener(Action callback)
    {
        return Registry.ObserveValue(REGISTRY_HIVE, PERSONALIZE_REGISTRY_PATH, APPS_USE_LIGHT_THEME_REGISTRY_KEY, callback);
    }

    internal static IDisposable GetColorizationColorListener(Action callback)
    {
        return Registry.ObserveValue(REGISTRY_HIVE, DWM_REGISTRY_PATH, DWM_COLORIZATION_COLOR_REGISTRY_KEY, callback);
    }
}
