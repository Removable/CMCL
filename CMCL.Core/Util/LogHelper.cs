﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CMCL.Core.Util
{
    public class LogHelper
    {
        private static readonly string LogDirectory =
            IOHelper.CombineAndCheckDirectory(Environment.CurrentDirectory, "logs");

        private static string GetLogContent(Exception exception)
        {
            var logContent =
                $"\r\n------------------\r\n【时间】{DateTime.Now.GetTimeString()}\r\n【错误】{exception.Message}\r\n【位置】{exception.StackTrace}";
            return logContent;
        }

        public static async Task WriteLogAsync(Exception exception)
        {
            var logContent = GetLogContent(exception);
            if (!Directory.Exists(LogDirectory)) Directory.CreateDirectory(LogDirectory);
            await File.AppendAllTextAsync(
                IOHelper.CombineAndCheckDirectory(LogDirectory, DateTime.Now.ToString("yyyyMMdd") + ".txt"),
                logContent, Encoding.UTF8).ConfigureAwait(false);
        }

        public static void WriteLog(Exception exception)
        {
            var logContent = GetLogContent(exception);
            if (!Directory.Exists(LogDirectory)) Directory.CreateDirectory(LogDirectory);
            File.AppendAllText(
                IOHelper.CombineAndCheckDirectory(LogDirectory, DateTime.Now.ToString("yyyyMMdd") + ".txt"), logContent,
                Encoding.UTF8);
        }
    }
}