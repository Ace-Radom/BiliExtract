using System;

namespace BiliExtract.Lib.Extensions;

public static class VersionExtensions
{
    public static bool IsBeta(this Version version) => version switch
    {
        { Major: 0, Minor: 0, Build: 1, Revision: 0 } => true,
        _ => false
    };

    public static bool IsPreRelease(this Version version) => version switch
    {
        { Major: 0 } => true,
        _ => false
    };
}
