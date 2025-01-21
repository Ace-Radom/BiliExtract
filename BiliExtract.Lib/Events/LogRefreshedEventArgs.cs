using System;

namespace BiliExtract.Lib.Events;

public class LogRefreshedEventArgs(string[] newLogMessages)
{
    public string[] NewLogMessages { get; set; } = newLogMessages;
}
