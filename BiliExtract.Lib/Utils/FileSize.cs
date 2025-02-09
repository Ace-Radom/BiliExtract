using System;
using System.IO;

namespace BiliExtract.Lib.Utils;

public static class FileSize
{
    public static string ConvertDataSizeByteToString(long size, DataSizePrefix prefix)
    {
        if (size < 0)
        {
            return string.Empty;
        }

        var carry = prefix is DataSizePrefix.Binary ? 1024.0 : 1000.0;
        var kiloPrefix = prefix is DataSizePrefix.Binary ? "KiB" : "kB";
        var megaPrefix = prefix is DataSizePrefix.Binary ? "MiB" : "MB";
        var gigaPrefix = prefix is DataSizePrefix.Binary ? "GiB" : "GB";
        var teraPrefix = prefix is DataSizePrefix.Binary ? "TiB" : "TB";

        if (size < carry)
        {
            return $"{size} byte";
        }
        else if (size < Math.Pow(carry, 2))
        {
            return $"{size / carry:N2}{kiloPrefix}";
        }
        else if (size < Math.Pow(carry, 3))
        {
            return $"{size / Math.Pow(carry, 2):N2}{megaPrefix}";
        }
        else if (size < Math.Pow(carry, 4))
        {
            return $"{size / Math.Pow(carry, 3):N2}{gigaPrefix}";
        }
        else
        {
            return $"{size / Math.Pow(carry, 4):N2}{teraPrefix}";
        }
    }
        
    public static long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path))
        {
            return -1;
        }
        long size = 0;
        var di = new DirectoryInfo(path);
        foreach (var fi in di.GetFiles())
        {
            size += fi.Length;
        }
        var subDis = di.GetDirectories();
        if (subDis.Length > 0)
        {
            foreach (var subDi in subDis)
            {
                size += GetDirectorySize(subDi.FullName);
            }
        }
        return size;
    }

    public static long GetFileSize(string path)
    {
        if (!File.Exists(path))
        {
            return -1;
        }
        var fi = new FileInfo(path);
        return fi.Length;
    }
}
