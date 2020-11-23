using System;

namespace CMCL.Client.Util
{
    public static class Utils
    {
        public static string GetTimeString(this DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss.fff")
        {
            return dateTime.ToString(format);
        }
    }
}