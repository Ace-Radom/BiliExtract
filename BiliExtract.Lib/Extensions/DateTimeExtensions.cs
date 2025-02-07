using System;

namespace BiliExtract.Lib.Extensions;

public static class DateTimeExtensions
{
    public static DateTime AddIntervalMinute(this DateTime dateTime, int intervalMinute) => dateTime.AddMinutes(intervalMinute).AddSeconds(-1);
    // minus 1 sec to make sure job will be triggered properly
}
