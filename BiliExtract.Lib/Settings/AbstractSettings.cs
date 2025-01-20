using BiliExtract.Lib.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;

namespace BiliExtract.Lib.Settings;

public abstract class AbstractSettings<T> where T : class, new()
{
    protected readonly JsonSerializerSettings _jsonSerializerSettings;
    private readonly string _storePath;
    private readonly string _storeFileName;

    protected virtual T Default => new();

    public T Data => _data ??= LoadData() ?? Default;

    private T? _data;

    protected AbstractSettings(string fileName)
    {
        _jsonSerializerSettings = new()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            Converters = { new StringEnumConverter() }
        };
        _storeFileName = fileName;
        _storePath = Path.Combine(Folders.AppData, _storeFileName);
        return;
    }

    public void SynchronizeData()
    {
        var settingsDataText = JsonConvert.SerializeObject(_data, _jsonSerializerSettings);
        File.WriteAllText(_storePath, settingsDataText);
        return;
    }

    public virtual T? LoadData()
    {
        if (!File.Exists(_storePath))
        {
            return null;
        }

        T? data = null;
        try
        {
            var settingsDataText = File.ReadAllText(_storePath);
            data = JsonConvert.DeserializeObject<T>(settingsDataText, _jsonSerializerSettings);

            if (data is null)
            {
                TryBackup();
            }
        }
        catch (Exception ex)
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Failed to load settings data, try backup. [type={GetType().Name}]", ex);
            TryBackup();
        }

        return data;
    }

    private void TryBackup()
    {
        try
        {
            if (!File.Exists(_storePath))
            {
                return;
            }

            var backupFileName = $"{Path.GetFileNameWithoutExtension(_storeFileName)}_backup_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(_storeFileName)}";
            var backupPath = Path.Combine(Folders.AppData, backupFileName);
            File.Copy(_storePath, backupPath);
        }
        catch (Exception ex)
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Failed to backup old settings file. [type={GetType().Name}]", ex);
        }
        return;
    }
}
