﻿using BiliExtract.Lib.Events;
using BiliExtract.Lib.Extensions;
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

    private Timer? _backgroundTimer;
    private DateTime _lastCleanupDateTime = DateTime.MinValue;
    private DateTime _nextCleanupDateTime = DateTime.MinValue;
    private DateTime _lastStorageUsageRefreshDateTime = DateTime.MinValue;
    private DateTime _nextStorageUsageRefreshDateTime = DateTime.MinValue;
    private long _storageUsage;
    private long _storageNormalUsage;
    private long _storageLockedUsage;
    private long _storageReleasedUsage;

    public DateTime LastCleanupDateTime => _lastCleanupDateTime;
    public DateTime NextCleanupDateTime => _nextCleanupDateTime;
    public DateTime LastStorageUsageRefreshDateTime => _lastStorageUsageRefreshDateTime;
    public DateTime NextStorageUsageRefreshDateTime => _nextStorageUsageRefreshDateTime;
    public long StorageUsageByte => _storageUsage;
    public long StorageNormalUsageByte => _storageNormalUsage;
    public long StorageLockedUsageByte => _storageLockedUsage;
    public long StorageReleasedUsageByte => _storageReleasedUsage;
    public long StorageInUseUsageByte => _storageNormalUsage + _storageLockedUsage;
    public int NormalTempFileHandleCount => GetTempFileHandleCount(TempFileState.Normal);
    public int LockedTempFileHandleCount => GetTempFileHandleCount(TempFileState.Locked);
    public int ReleasedTempFileHandleCount => GetTempFileHandleCount(TempFileState.Released);

    public event EventHandler<EventArgs>? DataChanged;

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
            if (oldHandleTemp is TempFileHandle oldHandle)
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
        lock (_lock)
        {
            if (!_tempHandleTable.TryGetValue(handle, out var data))
            {
                Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Release non-existent handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
                return false;
            }
            var state = data.State;
            if (state is TempFileState.Released)
            {
                Log.GlobalLogger.WriteLog(LogLevel.Info, $"Handle has already been released. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
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
        }
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
        lock (_lock)
        {
            if (!_tempHandleTable.TryGetValue(handle, out var data))
            {
                Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Lock non-existent handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
                return false;
            }
            if (!File.Exists(handle.Path))
            {
                Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Lock non-existent file. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
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
        }
        return true;
    }

    public bool Unlock(TempFileHandle handle)
    {
        lock (_lock)
        {
            if (!_tempHandleTable.TryGetValue(handle, out var data))
            {
                Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Unlock non-existent handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
                return false;
            }
            if (!File.Exists(handle.Path))
            {
                Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Unlock non-existent file. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
                return false;
            }
            var state = data.State;
            if (state is not TempFileState.Locked)
            {
                Log.GlobalLogger.WriteLog(LogLevel.Info, $"Unlock non-locked handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
                return true;
            }
            _tempHandleTable[handle] = new()
            {
                Hash = data.Hash,
                State = TempFileState.Normal
            };
            _tempFolderListener.Unwatch(handle.Path);
            Log.GlobalLogger.WriteLog(LogLevel.Info, $"Temp handle unlocked. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");

            SynchronizeLockedTempHandlesNoLock();
        }
        return true;
    }

    public TempFileHandle? GetTempFileHandleFromPath(string path, out TempFileState? state, out bool rescued)
    {
        rescued = false;
        lock (_lock)
        {
            var handle = GetTempFileHandleFromPathNoLock(path, out state);
            if (handle is TempFileHandle handleNotNull && state is not null)
            {
                if (state is TempFileState.Released)
                {
                    Log.GlobalLogger.WriteLog(LogLevel.Info, $"Try getting a released handle, rescue. [path=\"{handleNotNull.Path}\",register_time=\"{handleNotNull.RegisterTime}\"]");
                    var handleData = _tempHandleTable[handleNotNull];
                    _tempHandleTable[handleNotNull] = new()
                    {
                        Hash = handleData.Hash,
                        State = TempFileState.Normal
                    };
                    state = TempFileState.Normal;
                    rescued = true;
                }
            }
            return handle;
        }
    }

    public Task RefreshStorageUsageAsync()
    {
        lock (_lock)
        {
            _storageUsage = FileSize.GetDirectorySize(Folders.Temp);
            _storageNormalUsage = 0;
            _storageLockedUsage = 0;
            _storageReleasedUsage = 0;
            foreach (var handleData in _tempHandleTable)
            {
                if (File.Exists(handleData.Key.Path))
                {
                    if (handleData.Value.State is TempFileState.Normal)
                    {
                        _storageNormalUsage += FileSize.GetFileSize(handleData.Key.Path);
                    }
                    else if (handleData.Value.State is TempFileState.Locked)
                    {
                        _storageLockedUsage += FileSize.GetFileSize(handleData.Key.Path);
                    }
                    else
                    {
                        _storageReleasedUsage += FileSize.GetFileSize(handleData.Key.Path);
                    }
                }
            }
            Log.GlobalLogger.WriteLog(LogLevel.Info, $"Temp storage usage refreshed. [size={_storageUsage},normal={_storageNormalUsage},locked={_storageLockedUsage},released={_storageReleasedUsage}]");
        }
        DataChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public async Task StartBackgroundTimerAsync()
    {
        if (_backgroundTimer?.Enabled ?? false)
        {
            return;
        }

        await CleanupAsync().ConfigureAwait(false);
        await RefreshStorageUsageAsync().ConfigureAwait(false);

        _backgroundTimer = new()
        {
            AutoReset = true,
            Interval = 60000
        };
        _lastCleanupDateTime = DateTime.UtcNow;
        _nextCleanupDateTime = _lastCleanupDateTime.AddIntervalMinute(_settings.Data.AutoTempCleanupIntervalMin);
        _lastStorageUsageRefreshDateTime = DateTime.UtcNow;
        _nextStorageUsageRefreshDateTime = _lastStorageUsageRefreshDateTime.AddIntervalMinute(_settings.Data.TempFolderStorageUsageRefreshIntervalMin);
        _backgroundTimer.Start();
        _backgroundTimer.Elapsed += (_, _) => CleanupIfNeededAsync();
        _backgroundTimer.Elapsed += (_, _) => RefreshStorageUsageIfNeededAsync();
        _settings.Data.AutoTempCleanupIntervalChanged += (_, _) => RefreshNextCleanupDateTime();
        _settings.Data.TempFolderStorageUsageRefreshIntervalChanged += (_, _) => RefreshNextStorageUsageRefreshDateTime();
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Background timer started.");

        return;
    }

    public Task StopBackgroundTimerAsync()
    {
        _backgroundTimer?.Stop();
        _backgroundTimer?.Dispose();
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Background timer stoped.");
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

            int fileNotInTableCount = 0;
            foreach (var path in Directory.GetFiles(Folders.Temp, "*", SearchOption.AllDirectories))
            {
                if (GetTempFileHandleFromPathNoLock(path, out _) is null)
                {
                    File.Delete(path);
                    fileNotInTableCount++;
                }
            } // remove files not in table

            Log.GlobalLogger.WriteLog(LogLevel.Info, $"Temp cleanup completed. [count={releasedHandles.Count + fileNotInTableCount}]");
        }
        return Task.CompletedTask;
    }

    private async void CleanupIfNeededAsync()
    {
        lock (_lock)
        {
            if (DateTime.UtcNow < _nextCleanupDateTime)
            {
                Log.GlobalLogger.WriteLog(LogLevel.Debug, $"No need to cleanup. [next={_nextCleanupDateTime}]");
                return;
            }
            _lastCleanupDateTime = DateTime.UtcNow;
            _nextCleanupDateTime = _lastCleanupDateTime.AddIntervalMinute(_settings.Data.AutoTempCleanupIntervalMin);
            DataChanged?.Invoke(this, EventArgs.Empty);
            Log.GlobalLogger.WriteLog(LogLevel.Debug, $"Need to cleanup. [next={_nextCleanupDateTime}]");
        }
        await CleanupAsync().ConfigureAwait(false);
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

    private int GetTempFileHandleCount(TempFileState state)
    {
        lock (_lock)
        {
            return _tempHandleTable.Where(t => t.Value.State == state).Count();
        }
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

    private void RefreshNextCleanupDateTime()
    {
        lock (_lock)
        {
            _nextCleanupDateTime = _lastCleanupDateTime.AddIntervalMinute(_settings.Data.AutoTempCleanupIntervalMin);
            DataChanged?.Invoke(this, EventArgs.Empty);
            Log.GlobalLogger.WriteLog(LogLevel.Debug, $"Next cleanup date time refreshed. [interval={_settings.Data.AutoTempCleanupIntervalMin},next={_nextCleanupDateTime}]");
        }
        return;
    }

    private void RefreshNextStorageUsageRefreshDateTime()
    {
        lock (_lock)
        {
            _nextStorageUsageRefreshDateTime = _lastStorageUsageRefreshDateTime.AddIntervalMinute(_settings.Data.TempFolderStorageUsageRefreshIntervalMin);
            DataChanged?.Invoke(this, EventArgs.Empty);
            Log.GlobalLogger.WriteLog(LogLevel.Debug, $"Next storage usage refresh date time refreshed. [interval={_settings.Data.TempFolderStorageUsageRefreshIntervalMin},next={_nextStorageUsageRefreshDateTime}]");
        }
        return;
    }

    private async void RefreshStorageUsageIfNeededAsync()
    {
        lock (_lock)
        {
            if (DateTime.UtcNow < _nextStorageUsageRefreshDateTime)
            {
                Log.GlobalLogger.WriteLog(LogLevel.Debug, $"No need to refresh storage usage. [next={_nextStorageUsageRefreshDateTime}]");
                return;
            }
            _lastStorageUsageRefreshDateTime = DateTime.UtcNow;
            _nextStorageUsageRefreshDateTime = _lastStorageUsageRefreshDateTime.AddIntervalMinute(_settings.Data.TempFolderStorageUsageRefreshIntervalMin);
            DataChanged?.Invoke(this, EventArgs.Empty);
            Log.GlobalLogger.WriteLog(LogLevel.Debug, $"Need to refresh storage usage. [next={_nextStorageUsageRefreshDateTime}]");
        }
        await RefreshStorageUsageAsync().ConfigureAwait(false);
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
