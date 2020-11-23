using System.Configuration;
using System.Linq;

namespace CMCL.Client.Util
{
    public class AppConfig
    {
        private static Configuration _appConfig =
            ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);

        /// <summary>
        /// 读取配置
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetAppSettings(string key)
        {
            return _appConfig.AppSettings.Settings[key].Value;
        }

        /// <summary>
        /// 写入配置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetAppSettings(string key, string value)
        {
            if (_appConfig.AppSettings.Settings.AllKeys.Contains(key))
                _appConfig.AppSettings.Settings[key].Value = value;
            else
                _appConfig.AppSettings.Settings.Add(key, value);
            _appConfig.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}