using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CMCL.Client.Util
{
    public class LogHelper
    {
        public static async Task WriteLog(Exception exception)
        {
            var logContent =
                $"\r\n------------------\r\n【时间】{DateTime.Now.GetTimeString()}\r\n【错误】{exception.Message}\r\n【详细】{exception.StackTrace}";
            await File.AppendAllTextAsync(
                Path.Combine(Environment.CurrentDirectory, "logs", DateTime.Now.ToString("yyyyMMdd") + ".txt"),
                logContent, Encoding.UTF8);
        }
    }
}