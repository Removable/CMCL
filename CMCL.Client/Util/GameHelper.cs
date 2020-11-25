using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMCL.Client.Util
{
    public static class GameHelper
    {
        /// <summary>
        /// 返回默认.minecraft文件夹位置
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultMinecraftDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        /// <summary>
        /// 获取已下载的版本
        /// </summary>
        /// <returns></returns>
        public static string[] GetDownloadedVersions()
        {
            var gameDirectory = Path.Combine(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "versions");
            var versions = Directory.GetDirectories(gameDirectory);
            return versions.Select(i=> i.Substring(versions[0].LastIndexOf(@"\", StringComparison.Ordinal) + 1)).ToArray();
        }
    }
}
