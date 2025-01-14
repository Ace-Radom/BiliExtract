using System;
using System.Globalization;
using System.Reflection;

namespace BiliExtract.Lib.Extensions;

public static class AssemblyExtensions
{
    public static DateTime? GetBuildDateTime(this Assembly assembly)
    {
        const string buildVersionStartPrefix = "+build";

        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion is null)
        {
            return null;
        }

        var informationalVersion = attribute.InformationalVersion;
        var buildVersionStartIndex = informationalVersion.IndexOf(buildVersionStartPrefix, StringComparison.InvariantCultureIgnoreCase);
        if (buildVersionStartIndex < 0)
        {
            return null;
        }

        var buildVersionText = informationalVersion[(buildVersionStartIndex + buildVersionStartPrefix.Length)..];
        if (DateTime.TryParseExact(buildVersionText, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
        {
            return dateTime;
        }

        return null;
    }

    public static string? GetBuildDateTimeString(this Assembly assembly)
    {
        return GetBuildDateTime(assembly)?.ToString("yyyyMMddHHmmss");
    }
}
