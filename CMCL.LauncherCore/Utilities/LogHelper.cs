using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ComponentUtil.Common.Data;

namespace CMCL.LauncherCore.Utilities
{
    /// <summary>
    ///     日志级别
    /// </summary>
    public enum LogLevel
    {
        [Description("信息")] Info,
        [Description("警告")] Warn,
        [Description("错误")] Error
    }

    public class LogHelper
    {
        private static readonly string LogDirectory =
            Utils.CombineAndCheckDirectory(false, Environment.CurrentDirectory, "logs");

        private static string GetLogContent(Exception exception, LogLevel logLevel)
        {
            var logContent =
                $"\r\n------------------\r\n【时间】{DateTime.Now.GetTimeString()}\r\n【{logLevel.GetDescription()}】{exception.Message}\r\n【位置】{exception.StackTrace}";
            return logContent;
        }

        public static async Task LogExceptionAsync(Exception exception, LogLevel logLevel = LogLevel.Error)
        {
            var logContent = GetLogContent(exception, logLevel);
            await File.AppendAllTextAsync(Path.Combine(LogDirectory, DateTime.Now.ToString("yyyyMMdd") + ".txt"),
                logContent, Encoding.UTF8).ConfigureAwait(false);
        }

        public static void LogException(Exception exception, LogLevel logLevel = LogLevel.Error)
        {
            var logContent = GetLogContent(exception, logLevel);
            File.AppendAllText(Path.Combine(LogDirectory, DateTime.Now.ToString("yyyyMMdd") + ".txt"), logContent,
                Encoding.UTF8);
        }

        public static async Task WriteLogAsync(LogLevel logLevel, string shortMsg, string fullMsg)
        {
            var logContent =
                $"\r\n------------------\r\n【时间】{DateTime.Now.GetTimeString()}\r\n【级别】{logLevel.GetDescription()}\r\n【描述】{shortMsg}\r\n【详细】{fullMsg}";
            await File.AppendAllTextAsync(Path.Combine(LogDirectory, DateTime.Now.ToString("yyyyMMdd") + ".txt"),
                logContent, Encoding.UTF8).ConfigureAwait(false);
        }
    }
}