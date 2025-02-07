using System.IO;

namespace BiliExtract.Lib.Utils;

public static class FileSize
{
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
