using BiliExtract.Lib.Events;
using BiliExtract.Lib.Settings;
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
    private readonly string _logFileName;
    private readonly string _logFolder;
    private readonly string _logPath;
    private readonly StringBuilder _logMessagesBuilder = new();
    
    private int _logMessagesCount = 0;
    private LogLevel? _minLogLevel = null;

    private static Log? _globalLogger;
    public static Log GlobalLogger
    {
        get
        {
            _globalLogger ??= new Log();
            return _globalLogger;
        }
    }

    public bool IsLoggingToFile { get; set; } = true;
    public string LogMessages => _logMessagesBuilder.ToString();
    public int LogMessagesCount => _logMessagesCount;
    public string LogFileName => _logFileName;
    public string LogPath => _logPath;

    public event LogRefreshedEventHandler? LogRefreshed;

    private Log()
    {
        _logFolder = Path.Combine(Folders.AppData, "log");
        Directory.CreateDirectory(_logFolder);
        _logFileName = $"BiliExtract_{DateTime.UtcNow:yyyy_MM_dd_HH_mm_ss}.log";
        _logPath = Path.Combine(_logFolder, _logFileName);
        return;
    }

    public void DumpErrorToFile(string header, Exception ex)
    {
        var errorDumpFilePath = Path.Combine(_logFolder, $"BiliExtract_ERROR_{DateTime.UtcNow:yyyy_MM_dd_HH_mm_ss}.log");
        File.AppendAllLines(errorDumpFilePath, [header, LogMessages]);
        return;
    }

    public void WriteLog(LogLevel level, FormattableString message,
        Exception? ex = null,
        [CallerFilePath] string? file = null,
        [CallerLineNumber] int lineNumber = -1,
        [CallerMemberName] string? caller = null)
    {
        if (_minLogLevel is null && IoCContainer.IsInitialized)
        {
            _minLogLevel = IoCContainer.Resolve<ApplicationSettings>().Data.MinLogLevel;
        }

        if (_minLogLevel is not null && level < _minLogLevel)
        {
            return;
        }

        lock (_lock)
        {
            var lines = new List<string>()
            {
                $"[{DateTime.UtcNow:yyyy/MM/dd HH:mm:ss.fff}] [{Environment.CurrentManagedThreadId}] {level}: {message} [{Path.GetFileName(file)}#{lineNumber}:{caller}]"
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
                _logMessagesBuilder.AppendLine(line);
            }
            if (IsLoggingToFile)
            {
                File.AppendAllLines(_logPath, lines);
            }
            _logMessagesCount++;
            LogRefreshed?.Invoke(this, new(lines.ToArray()));
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
