using BiliExtract.Lib.Events;
using BiliExtract.Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BiliExtract.Lib.Listener;

public class TempFolderListener : IListener<TempFolderChangedEventArgs>
{
    public event EventHandler<TempFolderChangedEventArgs>? Changed;

    private readonly List<string> _watchList = [];

    private bool _started = false;
    private FileSystemWatcher? _watcher;

    public Task StartAsync()
    {
        if (_started)
        {
            return Task.CompletedTask;
        }

        _watcher = new(Folders.Temp)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };
        _watcher.Changed += Watcher_Changed;
        _watcher.Created += Watcher_Created;
        _watcher.Deleted += Watcher_Deleted;
        _watcher.Renamed += Watcher_Renamed;

        _started = true;
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _watcher?.Dispose();
        _watchList.Clear();
        _started = false;
        return Task.CompletedTask;
    }

    public void Watch(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(Folders.Temp, path);
        }

        if (!_watchList.Contains(path))
        {
            _watchList.Add(path);
        }

        return;
    }

    public void Unwatch(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(Folders.Temp, path);
        }

        _watchList.Remove(path);

        return;
    }

    private void Watcher_Changed(object sender, FileSystemEventArgs e)
    {
        if (_watchList.Contains(e.FullPath))
        {
            Changed?.Invoke(this, new(e.FullPath, FileChangedEventType.Changed));
        }
        return;
    }

    private void Watcher_Created(object sender, FileSystemEventArgs e)
    {
        if (_watchList.Contains(e.FullPath))
        {
            Changed?.Invoke(this, new(e.FullPath, FileChangedEventType.Created));
        }
        return;
    }

    private void Watcher_Deleted(object sender, FileSystemEventArgs e)
    {
        if (_watchList.Contains(e.FullPath))
        {
            Changed?.Invoke(this, new(e.FullPath, FileChangedEventType.Deleted));
        }
        return;
    }

    private void Watcher_Renamed(object sender, RenamedEventArgs e)
    {
        if (_watchList.Contains(e.OldFullPath))
        {
            Changed?.Invoke(this, new(e.OldFullPath, FileChangedEventType.Renamed));
        }
        return;
    }
}
