using BiliExtract.Lib.Settings;
using BiliExtract.Lib.Utils;
using System;
using System.Diagnostics;
using System.IO;

namespace BiliExtract.Lib.Adb;

public static class AdbCommandHandler
{
    private static readonly ApplicationSettings _settings = IoCContainer.Resolve<ApplicationSettings>();

    public static string AdbPath
    {
        get
        {
            if (_settings.Data.UseBuiltInAdb)
            {
                return Path.Combine(Folders.Program, "bin", "platform-tools", "adb.exe");
            }
            else
            {
                var externalAdbPath = _settings.Data.ExternalAdbPath;
                if (externalAdbPath is null)
                {
                    return string.Empty;
                }
                if (!Path.Exists(externalAdbPath) || !Path.IsPathRooted(externalAdbPath) || !(Path.GetFileName(externalAdbPath) == "adb.exe"))
                {
                    return string.Empty;
                }
                return externalAdbPath;
            }
        }
    }

    public static int Execute(string command, out string stdout, out string stderr)
    {
        var adbPath = AdbPath;
        if (string.IsNullOrEmpty(adbPath))
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"ADB executable not found. [cmd=\"{command}\"]");
            throw new InvalidOperationException("ADB executable not found");
        }

        var psi = new ProcessStartInfo(adbPath, command)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(psi);
        if (process is null)
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Start ADB process failed. [adb=\"{adbPath}\",cmd=\"{command}\"]");
            throw new SystemException("Start ADB process failed");
        }

        stdout = process.StandardOutput.ReadToEnd();
        stderr = process.StandardError.ReadToEnd();
        return process.ExitCode;
    }
}
