using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CMCL.Client.GameVersion.JsonClasses;
using Microsoft.Win32;

namespace CMCL.Client.Util
{
    internal static class FileHelper
    {
        public static string GetSha1HashFromFile(string filename)
        {
            if (!File.Exists(filename)) return null;
            var file = new FileStream(filename, FileMode.Open);
            var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            var retVal = sha1.ComputeHash(file);
            file.Close();

            return Byte2String(retVal);

            static string Byte2String(IEnumerable<byte> buffer)
            {
                var sb = new StringBuilder();
                foreach (var t in buffer)
                {
                    sb.Append(t.ToString("x2"));
                }
                return sb.ToString();
            }
        }

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
        /// 获取文件sha1
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static async ValueTask<string> GetSha1(string filePath)
        {
            var sc = new StringBuilder();
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            try
            {
                await using var file = new FileStream(filePath, FileMode.Open);
                var sha1 = new SHA1CryptoServiceProvider();
                var value = await sha1.ComputeHashAsync(file);
                file.Close();

                foreach (var v in value)
                {
                    sc.Append(v.ToString("x2"));
                }

                return sc.ToString();
            }
            catch (Exception ex)
            {
                await LogHelper.WriteLogAsync(ex);
                return string.Empty;
            }
        }
    }
}