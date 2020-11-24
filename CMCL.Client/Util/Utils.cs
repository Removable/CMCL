using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace CMCL.Client.Util
{
    public static class Utils
    {
        /// <summary>
        /// 格式化日期
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string GetTimeString(this DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss.fff")
        {
            return dateTime.ToString(format);
        }

        /// <summary>
        /// 读取注册表，寻找安装的java路径
        /// </summary>
        /// <returns>javaw.exe路径</returns>
        public static string GetJavaDir()
        {
            try
            {
                var reg = Registry.LocalMachine;
                var openSubKey = reg.OpenSubKey("SOFTWARE");
                var registryKey = openSubKey?.OpenSubKey("JavaSoft");
                var jre = registryKey?.OpenSubKey("Java Runtime Environment");
                if (jre == null) return null;
                var javaList = new List<string>();
                foreach (var ver in jre.GetSubKeyNames())
                {
                    try
                    {
                        var command = jre.OpenSubKey(ver);
                        if (command == null) continue;
                        var str = command.GetValue("JavaHome")?.ToString();
                        if (!string.IsNullOrWhiteSpace(str))
                            javaList.Add(str + @"\bin\javaw.exe");
                    }
                    catch { return null; }
                }
                //优先java8
                foreach (var java in javaList)
                {
                    if (java.ToLower().Contains("jre8") || java.ToLower().Contains("jdk1.8") || java.ToLower().Contains("jre1.8"))
                    {
                        return java;
                    }
                }
                return javaList[0];
            }
            catch { return null; }
        }
    }
}