using BiliExtract.Lib.Events;
using BiliExtract.Lib.Listener;
using BiliExtract.Lib.Settings;
using BiliExtract.Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace BiliExtract.Lib.Managers;

public class TempManager
{
    private readonly object _lock = new();
    private readonly LockedTempHandlesSettings _lockedStore = IoCContainer.Resolve<LockedTempHandlesSettings>();
    private readonly ApplicationSettings _settings = IoCContainer.Resolve<ApplicationSettings>();
    private readonly TempFolderListener _tempFolderListener = IoCContainer.Resolve<TempFolderListener>();
    private readonly Dictionary<TempFileHandle, TempFileHandleData> _tempHandleTable = [];

    private Timer? _autoCleanupTimer;
    private DateTime _lastCleanupDateTime = DateTime.MinValue;
    private DateTime _nextCleanupDateTime = DateTime.MinValue;

    public TempManager()
    {
        _tempFolderListener.Changed += TempFolderListener_Changed;
        RestoreLockedTempHandles();

        return;
    }

    public TempFileHandle Register(string filename)
    {
        string path = Path.Combine(Folders.Temp, filename);
        lock (_lock)
        {
            var oldHandleTemp = GetTempFileHandleFromPathNoLock(path, out var state);
            if (state is not null && oldHandleTemp is TempFileHandle oldHandle)
            {
                if (state is TempFileState.Locked)
                {
                    Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Register exist locked temp handle. [path=\"{oldHandle.Path}\",register_time=\"{oldHandle.RegisterTime}\"]");
                    return TempFileHandle.Empty;
                }
                DeleteNoLock(oldHandle);
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            var handle = new TempFileHandle(path, DateTime.UtcNow);
            _tempHandleTable.Add(handle, new()
            {
                Hash = string.Empty,
                State = TempFileState.Normal
            });
            Log.GlobalLogger.WriteLog(LogLevel.Info, $"New temp handle registered. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
            return handle;
        }
    }

    public bool Release(TempFileHandle handle, bool releaseLocked = false)
    {
        if (!_tempHandleTable.TryGetValue(handle, out var data))
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Release non-existent handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
            return false;
        }
        var state = data.State;
        if (state is TempFileState.Released)
        {
            Log.GlobalLogger.WriteLog(LogLevel.Info, $"Release released handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
            return true;
        }
        if (state is TempFileState.Locked)
        {
            if (!releaseLocked)
            {
                Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Release locked handle but `releaseLocked` flag set to false, blocked. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
                return false;
            }
        }
        _tempHandleTable[handle] = new TempFileHandleData()
        {
            Hash = data.Hash,
            State = TempFileState.Released
        };
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Temp handle released. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");

        SynchronizeLockedTempHandlesNoLock();

        return true;
    }

    public void Delete(TempFileHandle handle)
    {
        lock (_lock)
        {
            DeleteNoLock(handle);
        }
        return;
    }

    public bool Lock(TempFileHandle handle)
    {
        if (!_tempHandleTable.TryGetValue(handle, out var data))
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Lock non-existent handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
            return false;
        }
        var state = data.State;
        if (state is TempFileState.Released)
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Lock released handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
            return false;
        }
        if (state is TempFileState.Locked)
        {
            Log.GlobalLogger.WriteLog(LogLevel.Info, $"Relock locked handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
            return true;
        }
        _tempHandleTable[handle] = new TempFileHandleData()
        {
            Hash = Hash.SHA256File(handle.Path),
            State = TempFileState.Locked
        };
        _tempFolderListener.Watch(handle.Path);
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Temp handle locked. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");

        SynchronizeLockedTempHandlesNoLock();

        return true;
    }

    public bool Unlock(TempFileHandle handle)
    {
        if (!_tempHandleTable.TryGetValue(handle, out var data))
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Unlock non-existent handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
            return false;
        }
        var state = data.State;
        if (state is not TempFileState.Locked)
        {
            Log.GlobalLogger.WriteLog(LogLevel.Info, $"Unlock non-locked handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
            return true;
        }
        _tempHandleTable[handle] = new TempFileHandleData()
        {
            Hash = data.Hash,
            State = TempFileState.Normal
        };
        _tempFolderListener.Unwatch(handle.Path);
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Temp handle unlocked. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");

        SynchronizeLockedTempHandlesNoLock();

        return true;
    }

    public TempFileHandle? GetTempFileHandleFromPath(string path, out TempFileState? state)
    {
        lock (_lock)
        {
            return GetTempFileHandleFromPathNoLock(path, out state);
        }
    }

    public Task StartAutoCleanupTimerAsync()
    {
        if (_autoCleanupTimer?.Enabled ?? false)
        {
            return Task.CompletedTask;
        }

        _autoCleanupTimer = new()
        {
            AutoReset = true,
            Enabled = true,
            Interval = 60000
        };
        _lastCleanupDateTime = DateTime.UtcNow;
        _nextCleanupDateTime = _lastCleanupDateTime.AddMinutes(_settings.Data.AutoTempCleanupIntervalMin);
        _autoCleanupTimer.Elapsed += (_, _) => CleanupIfNeededAsync();
        _settings.Data.AutoTempCleanupIntervalChanged += (_, _) => RefreshAutoCleanupInterval();
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Auto temp cleanup timer started.");
        return Task.CompletedTask;
    }

    public Task StopAutoCleanupTimerAsync()
    {
        if (!_autoCleanupTimer?.Enabled ?? true)
        {
            return Task.CompletedTask;
        }

        _autoCleanupTimer?.Stop();
        _autoCleanupTimer?.Dispose();
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Auto temp cleanup timer stoped.");
        return Task.CompletedTask;
    }

    private Task CleanupAsync()
    {
        lock (_lock)
        {
            Log.GlobalLogger.WriteLog(LogLevel.Info, $"Starting temp cleanup.");

            foreach (var handleData in _tempHandleTable)
            {
                if (handleData.Value.State is TempFileState.Released)
                {
                    _tempFolderListener.Unwatch(handleData.Key.Path);
                    if (File.Exists(handleData.Key.Path))
                    {
                        File.Delete(handleData.Key.Path);
                    }
                }
            }
            var releasedHandles = _tempHandleTable.Where(t => t.Value.State is TempFileState.Released)
                                                  .Select(t => t.Key)
                                                  .ToList();
            foreach (var handle in releasedHandles)
            {
                _tempHandleTable.Remove(handle);
            }

            foreach (var path in Directory.GetFiles(Folders.Temp, "*", SearchOption.AllDirectories))
            {
                if (GetTempFileHandleFromPathNoLock(path, out _) is null)
                {
                    File.Delete(path);
                }
                // remove files not in table
            }

            Log.GlobalLogger.WriteLog(LogLevel.Info, $"Temp cleanup completed. [count={releasedHandles.Count}]");
        }
        return Task.CompletedTask;
    }

    private async void CleanupIfNeededAsync()
    {
        if (DateTime.UtcNow < _nextCleanupDateTime)
        {
            return;
        }
        await CleanupAsync().ConfigureAwait(false);

        lock (_lock)
        {
            _lastCleanupDateTime = DateTime.UtcNow;
            _nextCleanupDateTime = _lastCleanupDateTime.AddMinutes(_settings.Data.AutoTempCleanupIntervalMin);
        }
        
        return;
    }

    private void DeleteNoLock(TempFileHandle handle)
    {
        if (!_tempHandleTable.TryGetValue(handle, out var lastState))
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Delete non-existent handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
            return;
        }
        _tempFolderListener.Unwatch(handle.Path);
        if (File.Exists(handle.Path))
        {
            File.Delete(handle.Path);
        }
        _tempHandleTable.Remove(handle);
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Temp handle deleted manually. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\",last_state={lastState}]");
        return;
    }

    private TempFileHandle? GetTempFileHandleFromPathNoLock(string path, out TempFileState? state)
    {
        var handles = _tempHandleTable.Where(t => t.Key.Path == path);
        if (handles.Any())
        {
            state = handles.First().Value.State;
            return handles.First().Key;
        }
        state = null;
        return null;
    }

    private void RefreshAutoCleanupInterval()
    {
        lock (_lock)
        {
            _nextCleanupDateTime = _lastCleanupDateTime.AddMinutes(_settings.Data.AutoTempCleanupIntervalMin);
        }
        return;
    }

    private void RestoreLockedTempHandles()
    {
        lock (_lock)
        {
            if (_tempHandleTable.Count != 0)
            {
                Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Cannot restore locked temp handles when temp handle table is not empty.");
                return;
            }

            foreach (var handle in _lockedStore.Data.LockedTempHandles)
            {
                if (!File.Exists(handle.Handle.Path))
                {
                    continue;
                }

                var hash = Hash.SHA256File(handle.Handle.Path);
                if (string.IsNullOrEmpty(hash) || hash != handle.Hash)
                {
                    continue;
                }

                _tempHandleTable.Add(handle.Handle, new()
                {
                    Hash = hash,
                    State = TempFileState.Locked
                });
                _tempFolderListener.Watch(handle.Handle.Path);
            }
            SynchronizeLockedTempHandlesNoLock();
            Log.GlobalLogger.WriteLog(LogLevel.Info, $"Locked temp handles restored. [count={_tempHandleTable.Count}]");
        }
        return;
    }

    private void SynchronizeLockedTempHandlesNoLock()
    {
        var handles = new List<TempFileHandleStore>();
        foreach (var handleData in _tempHandleTable)
        {
            if (handleData.Value.State is TempFileState.Locked)
            {
                handles.Add(new(handleData.Key, handleData.Value.Hash ?? string.Empty));
            }
        }
        _lockedStore.Data.LockedTempHandles = handles.ToArray();
        _lockedStore.SynchronizeData();
        return;
    }

    private void TempFolderListener_Changed(object? sender, TempFolderChangedEventArgs e)
    {
        lock (_lock)
        {
            var handle = GetTempFileHandleFromPathNoLock(e.Path, out var state) ?? TempFileHandle.Empty;
            if (handle != TempFileHandle.Empty && state is TempFileState.Locked)
            {
                if (e.Type is FileChangedEventType.Changed)
                {
                    _tempHandleTable[handle] = new()
                    {
                        Hash = Hash.SHA256File(handle.Path),
                        State = TempFileState.Locked,
                    };
                } // locked temp file changed, refresh hash
                else
                {
                    Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Unexpected changes to locked temp handle, remove. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\",change_type={e.Type}]");
                    DeleteNoLock(handle);
                }
            }
            SynchronizeLockedTempHandlesNoLock();
        }
        return;
    }
}
