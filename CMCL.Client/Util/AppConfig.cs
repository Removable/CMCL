using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace CMCL.Client.Util
{
    public class AppConfig
    {
        private static string _configFilePath = Path.Combine(Environment.CurrentDirectory, "Cmcl.json");

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        public static async void InitConfigFile()
        {
            try
            {
                if (File.Exists(_configFilePath)) return;

                //序列化
                var serialize = JsonSerializer.Serialize(new CmclConfig(), new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    WriteIndented = true,
                });

                await File.WriteAllTextAsync(_configFilePath, serialize, Encoding.UTF8);
            }
            catch (Exception e)
            {
                await LogHelper.WriteLog(e);
            }
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<string> GetAppSettings(string key)
        {
            try
            {
                var configStr = await File.ReadAllTextAsync(_configFilePath, Encoding.UTF8);
                var cmclConfig = JsonSerializer.Deserialize<CmclConfig>(configStr);
                var o = typeof(CmclConfig).GetProperty(key)?.GetValue(cmclConfig, null);
                var value = Convert.ToString(o);
                return string.IsNullOrEmpty(value) ? null : value;
            }
            catch (Exception e)
            {
                await LogHelper.WriteLog(e);
                throw;
            }
        }

        /// <summary>
        /// 写入配置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static async void SetAppSettings(string key, string value)
        {
            try
            {
                var cmclConfig = await File.ReadAllTextAsync(_configFilePath, Encoding.UTF8);
                var type = typeof(CmclConfig);
                var property = type.GetProperty(key);
                if (property == null)
                    throw new Exception("找不到配置");
                var v = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(cmclConfig, v, null);
            }
            catch (Exception e)
            {
                await LogHelper.WriteLog(e);
                throw;
            }
        }
    }

    /// <summary>
    /// 配置映射类
    /// </summary>
    public class CmclConfig
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 选中的启动游戏版本
        /// </summary>
        public string CurrentVersion { get; set; } = string.Empty;

        /// <summary>
        /// 自定义的java路径
        /// </summary>
        public string CustomJavaPath { get; set; } = string.Empty;
    }
}