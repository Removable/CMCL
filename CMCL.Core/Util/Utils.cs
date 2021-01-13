﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace CMCL.Core.Util
{
    public static class Utils
    {
        /// <summary>
        ///     格式化日期
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string GetTimeString(this DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss.fff")
        {
            return dateTime.ToString(format);
        }

        /// <summary>
        ///     读取注册表，寻找安装的java路径
        /// </summary>
        /// <returns>javaw.exe路径</returns>
        public static string GetJavaDir()
        {
            try
            {
                if (OperatingSystem.IsWindows())
                    return GetWindowsJava();
                if (OperatingSystem.IsMacOS())
                    return string.Empty; //TODO 待适配
                if (OperatingSystem.IsLinux())
                    return string.Empty; //TODO 待适配

                throw new Exception("不受支持的操作系统");
            }
            catch
            {
                return string.Empty;
            }
        }

        [SupportedOSPlatform("windows")]
        private static string GetWindowsJava()
        {
            try
            {
                //从注册表查找
                var reg = Registry.LocalMachine;
                var openSubKey = reg.OpenSubKey("SOFTWARE");
                var registryKey = openSubKey?.OpenSubKey("JavaSoft");
                var jre = registryKey?.OpenSubKey("Java Runtime Environment");
                if (jre == null)
                {
                    //从环境变量查找
                    var variables = Environment.GetEnvironmentVariables();
                    var pathVariable = variables["Path"];
                    if (pathVariable == null) return string.Empty;

                    var array = pathVariable.ToString()?.Split(';');
                    if (array == null || array.Length <= 0) return string.Empty;

                    foreach (var s in array)
                        if (s.Contains("javapath"))
                            return IOHelper.CombineAndCheckDirectory(s, "javaw.exe");

                    return string.Empty;
                }

                var javaList = new List<string>();
                foreach (var ver in jre.GetSubKeyNames())
                    try
                    {
                        var command = jre.OpenSubKey(ver);
                        if (command == null) continue;
                        var str = command.GetValue("JavaHome")?.ToString();
                        if (!string.IsNullOrWhiteSpace(str))
                            javaList.Add(str + @"\bin\javaw.exe");
                    }
                    catch
                    {
                        return string.Empty;
                    }

                //优先java8
                foreach (var java in javaList)
                    if (java.ToLower().Contains("jre8") || java.ToLower().Contains("jdk1.8") ||
                        java.ToLower().Contains("jre1.8"))
                        return java;

                return javaList[0];
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     获取当前操作系统
        /// </summary>
        /// <returns></returns>
        public static SupportedOS GetOS()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return SupportedOS.Windows;
                case PlatformID.Unix:
                    return SupportedOS.Linux;
                case PlatformID.MacOSX:
                    return SupportedOS.Osx;
                default:
                    return SupportedOS.Other;
            }
        }
    }

    /// <summary>
    ///     窗口消失的处理方法
    /// </summary>
    public enum WindowDisappear
    {
        /// <summary>
        ///     无处理
        /// </summary>
        None,

        /// <summary>
        ///     关闭
        /// </summary>
        Close,

        /// <summary>
        ///     隐藏
        /// </summary>
        Hide
    }

    /// <summary>
    ///     受支持的操作系统
    /// </summary>
    public enum SupportedOS
    {
        [Description("windows")] Windows,
        [Description("linux")] Linux,
        [Description("osx")] Osx,
        [Description("other")] Other
    }
}