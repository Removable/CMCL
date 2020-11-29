using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;

namespace CMCL.Client.Util
{
    public static class AppConfig
    {
        private static CmclConfig Configure;
        private static string _configFilePath = Path.Combine(Environment.CurrentDirectory, "Cmcl.json");

        /// <summary>
        /// 初始化配置
        /// </summary>
        public static async ValueTask InitConfig()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    Configure = new CmclConfig();
                    //序列化
                    var serialize = JsonSerializer.Serialize(Configure,
                        new JsonSerializerOptions
                        {
                            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                            WriteIndented = true,
                        });

                    await File.WriteAllTextAsync(_configFilePath, serialize, Encoding.UTF8).ConfigureAwait(false);
                }
                else
                {
                    var json = await File.ReadAllTextAsync(_configFilePath, Encoding.UTF8).ConfigureAwait(false);
                    Configure = JsonSerializer.Deserialize<CmclConfig>(json);
                }
            }
            catch (Exception e)
            {
                await LogHelper.WriteLogAsync(e).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        /// <returns></returns>
        public static CmclConfig GetAppConfig()
        {
            return Configure;
        }

        /// <summary>
        /// 按字段写入配置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static async void SetAppConfig(string key, string value)
        {
            try
            {
                var cmclConfig = GetAppConfig();
                var type = typeof(CmclConfig);
                var property = type.GetProperty(key);
                if (property == null)
                    throw new Exception("找不到配置");
                var v = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(cmclConfig, v, null);

                SaveAppConfig(cmclConfig);
            }
            catch (Exception e)
            {
                await LogHelper.WriteLogAsync(e).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        /// <param name="config"></param>
        public static async ValueTask SaveAppConfig(CmclConfig config)
        {
            //序列化
            var json = JsonSerializer.Serialize(config,
                new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    WriteIndented = true,
                });
            await File.WriteAllTextAsync(_configFilePath, json, Encoding.UTF8).ConfigureAwait(false);
            Configure = config;
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
        public string CustomJavaPath { get; set; } = Utils.GetJavaDir();

        /// <summary>
        /// 使用默认游戏安装路径
        /// </summary>
        public bool UseDefaultGameDir { get; set; } = true;

        /// <summary>
        /// .minecraft文件夹位置
        /// </summary>
        public string MinecraftDir { get; set; } = GameHelper.GetDefaultMinecraftDir();
    }
}