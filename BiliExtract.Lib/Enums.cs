using BiliExtract.Lib.Resources;
using System.ComponentModel.DataAnnotations;

namespace BiliExtract.Lib;

public enum AccentColorSource
{
    [Display(ResourceType = typeof(Resource), Name = "AccentColorSource_System")]
    System,
    [Display(ResourceType = typeof(Resource), Name = "AccentColorSource_Custom")]
    Custom
}

public enum AdbDeviceState
{
    Unknown,
    Connected,
    Offline,
    Unauthorized
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

public enum Theme
{
    [Display(ResourceType = typeof(Resource), Name = "Theme_Light")]
    Light,
    [Display(ResourceType = typeof(Resource), Name = "Theme_Dark")]
    Dark,
    [Display(ResourceType = typeof(Resource), Name = "Theme_FollowSystem")]
    FollowSystem
}
