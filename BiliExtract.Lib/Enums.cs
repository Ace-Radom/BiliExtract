using BiliExtract.Lib.Resources;
using System.ComponentModel.DataAnnotations;

namespace BiliExtract.Lib;

public enum Theme
{
    [Display(ResourceType = typeof(Translations), Name = "Theme_Light")]
    Light,
    [Display(ResourceType = typeof(Translations), Name = "Theme_Dark")]
    Dark,
    [Display(ResourceType = typeof(Translations), Name = "Theme_FollowSystem")]
    FollowSystem
}
