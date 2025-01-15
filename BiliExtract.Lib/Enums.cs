using BiliExtract.Lib.Resources;
using System.ComponentModel.DataAnnotations;

namespace BiliExtract.Lib;

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
