using System.Runtime.InteropServices;

namespace BiliExtract.Lib;

public static partial class Native
{
    [LibraryImport("uxtheme.dll", EntryPoint = "#95A")]
    public static partial uint GetImmersiveColorFromColorSetEx(uint immersiveColorSet, uint immersiveColorType, [MarshalAs(UnmanagedType.Bool)] bool ignoreHighContrast, uint highContrastCacheMode);

    [LibraryImport("uxtheme.dll", EntryPoint = "#96W", StringMarshalling = StringMarshalling.Utf16)]
    public static partial uint GetImmersiveColorTypeFromName(string name);

    [LibraryImport("uxtheme.dll", EntryPoint = "#98A")]
    public static partial uint GetImmersiveUserColorSetPreference([MarshalAs(UnmanagedType.Bool)] bool forceCheckRegistry, [MarshalAs(UnmanagedType.Bool)] bool skipCheckOnFail);
}
