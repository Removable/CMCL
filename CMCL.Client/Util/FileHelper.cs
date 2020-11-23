using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace CMCL.Client.Util
{
    internal static class FileHelper
    {
        /// <summary>
        /// 复制文件夹
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void DirCopy(string from, string to)
        {
            DirectoryInfo dir = new DirectoryInfo(from);
            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }
            foreach (DirectoryInfo sondir in dir.GetDirectories())
            {
                DirCopy(sondir.FullName, to + "\\" + sondir.Name);
            }
            foreach (FileInfo file in dir.GetFiles())
            {
                File.Copy(file.FullName, to + "\\" + file.Name, true);
            }
        }

        /// <summary>
        /// 创建文件夹，若不存在则新建文件夹
        /// </summary>
        /// <param name="dir"></param>
        public static void CreateDirectoryIfNotExist(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public static void WriteFile(string path, string content)
        {
            CreateDirectoryIfNotExist(path);
            File.WriteAllText(path, content);
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
