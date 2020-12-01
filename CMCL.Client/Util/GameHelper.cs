using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CMCL.Client.GameVersion.JsonClasses;
using Newtonsoft.Json;

namespace CMCL.Client.Util
{
    public static class GameHelper
    {
        /// <summary>
        ///     返回默认.minecraft文件夹位置
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultMinecraftDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        /// <summary>
        ///     获取已下载的版本
        /// </summary>
        /// <returns></returns>
        public static string[] GetDownloadedVersions()
        {
            var gameDirectory = Path.Combine(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "versions");
            var versions = Directory.GetDirectories(gameDirectory);
            return versions.Select(i => i.Substring(versions[0].LastIndexOf(@"\", StringComparison.Ordinal) + 1))
                .ToArray();
        }

        /// <summary>
        ///     获取游戏版本json的信息
        /// </summary>
        /// <param name="gameVersionId">游戏版本，如：1.16.1</param>
        /// <returns></returns>
        public static async Task<VersionInfo> GetVersionInfo(string gameVersionId)
        {
            var jsonPath = Path.Combine(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "versions", gameVersionId,
                $"{gameVersionId}.json");
            if (!File.Exists(jsonPath)) throw new Exception("找不到版本信息文件");
            var jsonStr = await File.ReadAllTextAsync(jsonPath);
            if (string.IsNullOrWhiteSpace(jsonStr)) throw new Exception("找不到版本信息文件");
            //替换下载地址
            var data = JsonConvert.DeserializeObject<VersionInfo>(jsonStr);
            if (data == null) throw new Exception("找不到版本信息文件");
            return data;
        }
    }
}