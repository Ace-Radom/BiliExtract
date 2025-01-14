using System;
using System.IO;

namespace BiliExtract.Lib.Utils;

public static class Folders
{
    public static string Program => AppDomain.CurrentDomain.SetupInformation.ApplicationBase ?? string.Empty;

    public static string AppData
    {
        get
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var folderPath = Path.Combine(appData, "BiliExtract");
            Directory.CreateDirectory(folderPath);
            return folderPath;
        }
    }

    public static string Temp
    {
        get
        {
            var temp = Path.GetTempPath();
            var folderPath = Path.Combine(temp, "BiliExtract");
            Directory.CreateDirectory(folderPath);
            return folderPath;
        }
    }
}
