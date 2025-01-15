using BiliExtract.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace BiliExtract.Lib;

public class Log
{
    private readonly object _lock = new();
    private readonly string _logFolder;
    private readonly string _logPath;

    private static Log? _globalLogger;
    public static Log GlobalLogger
    {
        get
        {
            _globalLogger ??= new Log();
            return _globalLogger;
        }
    }

    private StringBuilder _logMessages = new();

    public bool IsLoggingToFile { get; set; }
    public string LogMessages => _logMessages.ToString();
    public string LogPath => _logPath;

    public event EventHandler? LogRefreshed;

    private Log()
    {
        _logFolder = Path.Combine(Folders.AppData, "log");
        Directory.CreateDirectory(_logFolder);
        _logPath = Path.Combine(_logFolder, $"BiliExtract_{DateTime.UtcNow:yyyy_MM_dd_HH_mm_ss}.log");
        return;
    }

    public void DumpErrorToFile(string header, Exception ex)
    {
        var errorDumpFilePath = Path.Combine(_logFolder, $"BiliExtract_ERROR_{DateTime.UtcNow:yyyy_MM_dd_HH_mm_ss}.log");
        File.AppendAllLines(errorDumpFilePath, [header, LogMessages]);
        return;
    }

    public void WriteLog(FormattableString message, LogLevel level,
        Exception? ex = null,
        [CallerFilePath] string? file = null,
        [CallerLineNumber] int lineNumber = -1,
        [CallerMemberName] string? caller = null)
    {
        lock (_lock)
        {
            var lines = new List<string>()
            {
                $"[{DateTime.UtcNow:dd/MM/yyyy HH:mm:ss.fff}] [{Environment.CurrentManagedThreadId}] {level}: {message} [{Path.GetFileName(file)}#{lineNumber}:{caller}]"
            };
            if (ex is not null)
            {
                lines.Add(DumpExceptionToString(ex));
            }

            foreach (var line in lines)
            {
#if DEBUG
                Debug.WriteLine(line);
#endif
                _logMessages.AppendLine(line);
            }
            if (IsLoggingToFile)
            {
                File.AppendAllLines(_logPath, lines);
            }

            LogRefreshed?.Invoke(this, EventArgs.Empty);
        }
        return;
    }

    private static string DumpExceptionToString(Exception ex) => new StringBuilder()
        .AppendLine("=== Exception ===")
        .AppendLine(ex.ToString())
        .AppendLine("=== Exception demystified ===")
        .AppendLine(ex.ToStringDemystified())
        .AppendLine("=== End of Exception ===")
        .AppendLine()
        .ToString();
}
