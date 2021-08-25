using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CMCL.LauncherCore.GameEntities;
using Microsoft.Win32;

namespace CMCL.LauncherCore.Utilities
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
                            return Path.Combine(s, "javaw.exe");

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

        #region 一些IO方面的方法

        /// <summary>
        ///     获取文件sha1
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static async ValueTask<string> GetSha1HashFromFileAsync(string filePath)
        {
            var sc = new StringBuilder();
            if (!File.Exists(filePath)) return string.Empty;
            try
            {
                await using var file = new FileStream(filePath, FileMode.Open);
                var sha1 = new SHA1CryptoServiceProvider();
                var value = await sha1.ComputeHashAsync(file);
                file.Close();

                foreach (var v in value) sc.Append(v.ToString("x2"));

                return sc.ToString();
            }
            catch (Exception ex)
            {
                await LogHelper.LogExceptionAsync(ex);
                return string.Empty;
            }
        }

        /// <summary>
        ///     拼接地址，地址中的文件夹若不存在则创建
        /// </summary>
        /// <param name="isFile">是否为文件</param>
        /// <param name="path"></param>
        public static string CombineAndCheckDirectory(bool isFile, params string[] path)
        {
            var newPath = Path.Combine(path).Replace(@"/", @"\");
            if (isFile)
                CreateDirectoryIfNotExist(newPath.Substring(0, newPath.LastIndexOf(@"\", StringComparison.Ordinal)));
            else
                CreateDirectoryIfNotExist(newPath);

            return newPath;
        }

        /// <summary>
        ///     创建文件夹，若不存在则新建文件夹
        /// </summary>
        /// <param name="dir"></param>
        private static void CreateDirectoryIfNotExist(string dir)
        {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        #endregion
    }
}