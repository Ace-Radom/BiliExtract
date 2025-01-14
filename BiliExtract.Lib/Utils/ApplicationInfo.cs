using BiliExtract.Lib.Extensions;
using System;
using System.Diagnostics;
using System.Reflection;

namespace BiliExtract.Lib.Utils;

public static class ApplicationInfo
{
    private static readonly Version? _version = Assembly.GetEntryAssembly()?.GetName().Version;

    public static string VersionText
    {
        get
        {
            if (_version is null)
            {
                return string.Empty;
            }
            if (_version.IsBeta())
            {
                return "BETA";
            }
            if (_version.IsPreRelease())
            {
                return $"{_version.ToString(3)} Pre Release";
            }
            return _version.ToString(3);
        }
    }

    public static string BuildText => Assembly.GetEntryAssembly()?.GetBuildDateTimeString() ?? string.Empty;

    public static string CopyrightText
    {
        get
        {
            var location = Assembly.GetExecutingAssembly()?.Location;
            if (location is null)
            {
                return string.Empty;
            }
            return FileVersionInfo.GetVersionInfo(location).LegalCopyright ?? string.Empty;
        }
    }
}
