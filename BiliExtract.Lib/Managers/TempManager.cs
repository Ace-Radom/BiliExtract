using BiliExtract.Lib.Settings;
using BiliExtract.Lib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BiliExtract.Lib.Managers;

public class TempManager
{
    private readonly object _lock = new();
    private readonly LockedTempHandlesSettings _lockedStore = IoCContainer.Resolve<LockedTempHandlesSettings>();
    private readonly Dictionary<TempFileHandle, TempFileState> _tempHandleTable = [];

    public TempManager()
    {
        if (!Directory.Exists(Folders.Temp))
        {
            Directory.CreateDirectory(Folders.Temp);
        }

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
                    Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Register exist locked temp file. [path=\"{oldHandle.Path}\",register_time=\"{oldHandle.RegisterTime}\"]");
                    return TempFileHandle.Empty;
                }
                DeleteNoLock(oldHandle);
            }
            var handle = new TempFileHandle(path, DateTime.UtcNow);
            _tempHandleTable.Add(handle, TempFileState.Normal);
            Log.GlobalLogger.WriteLog(LogLevel.Info, $"New temp file registered. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
            return handle;
        }
    }

    public bool Release(TempFileHandle handle, bool releaseLocked = false)
    {
        if (!_tempHandleTable.TryGetValue(handle, out var state))
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Release non-existent handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
            return false;
        }
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
        _tempHandleTable[handle] = TempFileState.Released;
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Temp file released. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");

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
        if (!_tempHandleTable.TryGetValue(handle, out var state))
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Lock non-existent handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
            return false;
        }
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
        _tempHandleTable[handle] = TempFileState.Locked;
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Temp file locked. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");

        SynchronizeLockedTempHandlesNoLock();

        return true;
    }

    public bool Unlock(TempFileHandle handle)
    {
        if (!_tempHandleTable.TryGetValue(handle, out var state))
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Unlock non-existent handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
            return false;
        }
        if (state is not TempFileState.Locked)
        {
            Log.GlobalLogger.WriteLog(LogLevel.Info, $"Unlock non-locked handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
            return true;
        }
        _tempHandleTable[handle] = TempFileState.Normal;
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Temp file unlocked. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");

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

    private void DeleteNoLock(TempFileHandle handle)
    {
        if (!_tempHandleTable.TryGetValue(handle, out var lastState))
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Delete non-existent handle. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\"]");
            return;
        }
        if (File.Exists(handle.Path))
        {
            File.Delete(handle.Path);
        }
        _tempHandleTable.Remove(handle);
        Log.GlobalLogger.WriteLog(LogLevel.Info, $"Temp file deleted manually. [path=\"{handle.Path}\",register_time=\"{handle.RegisterTime}\",last_state={lastState}]");
        return;
    }

    private TempFileHandle? GetTempFileHandleFromPathNoLock(string path, out TempFileState? state)
    {
        var handles = _tempHandleTable.Where(t => t.Key.Path == path);
        if (handles.Any())
        {
            state = handles.First().Value;
            return handles.First().Key;
        }
        state = null;
        return null;
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
                if (!File.Exists(handle.Path))
                {
                    continue;
                }
                _tempHandleTable.Add(handle, TempFileState.Locked);
            }
            SynchronizeLockedTempHandlesNoLock();
            Log.GlobalLogger.WriteLog(LogLevel.Info, $"Locked temp handles restored. [count={_tempHandleTable.Count}]");
        }
        return;
    }

    private void SynchronizeLockedTempHandlesNoLock()
    {
        var handles = new List<TempFileHandle>();
        foreach (var handleState in _tempHandleTable)
        {
            if (handleState.Value is TempFileState.Locked)
            {
                handles.Add(handleState.Key);
            }
        }
        _lockedStore.Data.LockedTempHandles = handles.ToArray();
        _lockedStore.SynchronizeData();
        return;
    }
}
