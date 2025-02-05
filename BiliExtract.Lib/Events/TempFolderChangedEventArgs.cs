using System;

namespace BiliExtract.Lib.Events;

public class TempFolderChangedEventArgs(string path, FileChangedEventType type) : EventArgs
{
    public string Path { get; } = path;
    public FileChangedEventType Type { get; } = type;
}
