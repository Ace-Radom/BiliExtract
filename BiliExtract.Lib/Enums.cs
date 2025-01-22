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
    [Display(ResourceType = typeof(Resource), Name = "LogLevel_Debug")]
    Debug,
    [Display(ResourceType = typeof(Resource), Name = "LogLevel_Info")]
    Info,
    [Display(ResourceType = typeof(Resource), Name = "LogLevel_Warning")]
    Warning,
    [Display(ResourceType = typeof(Resource), Name = "LogLevel_Error")]
    Error
}

public enum RichLogViewStyleSource
{
    [Display(ResourceType = typeof(Resource), Name = "RichLogViewStyleSource_Default")]
    Default,
    [Display(ResourceType = typeof(Resource), Name = "RichLogViewStyleSource_Custom")]
    Custom
}

public enum TextStyle
{
    Normal,
    Bold,
    Italic,
    Underline
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
