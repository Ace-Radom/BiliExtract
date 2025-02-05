using System;
using System.IO;
using System.Security.Cryptography;

namespace BiliExtract.Lib.Utils;

public static class Hash
{
    public static string SHA256File(string path)
    {
        if (!File.Exists(path))
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Hash non-existent file. [hash=sha256,path=\"{path}\"]");
            return string.Empty;
        }
        try
        {
            using SHA256 sha256 = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            byte[] hashBytes = sha256.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
        catch (Exception ex)
        {
            Log.GlobalLogger.WriteLog(LogLevel.Warning, $"Hash file failed. [hash=sha256,path=\"{path}\"]", ex);
            return string.Empty;
        }
    }
}
