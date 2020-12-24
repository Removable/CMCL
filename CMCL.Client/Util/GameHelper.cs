using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CMCL.Client.Download.Mirrors;
using CMCL.Client.GameVersion.JsonClasses;
using Newtonsoft.Json;

namespace CMCL.Client.Util
{
    public static class GameHelper
    {
        /// <summary>
        /// 版本文件信息集合
        /// </summary>
        public static List<VersionInfo> VersionInfoList = new();

        /// <summary>
        /// 加载版本文件信息
        /// </summary>
        /// <returns></returns>
        public static async Task LoadVersionInfoList()
        {
            VersionInfoList.Clear();

            var directory =
                new DirectoryInfo(Path.Combine(AppConfig.GetAppConfig().MinecraftDir, @".minecraft\versions"));
            if (directory.Exists)
            {
                var childrenDir = directory.GetDirectories();
                foreach (var dir in childrenDir)
                {
                    var jsonPath = Path.Combine(dir.FullName, $"{dir.Name}.json");
                    if (!File.Exists(jsonPath)) continue;
                    var jsonStr = await File.ReadAllTextAsync(jsonPath);
                    try
                    {
                        VersionInfoList.Add(JsonConvert.DeserializeObject<VersionInfo>(jsonStr));
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }
        

        /// <summary>
        ///     获取游戏版本json的信息
        /// </summary>
        /// <param name="gameVersionId">游戏版本，如：1.16.1</param>
        /// <returns></returns>
        public static VersionInfo GetVersionInfo(string gameVersionId)
        {
            #region 暂时不用

            // var jsonPath = IOHelper.CombineAndCheckDirectory(AppConfig.GetAppConfig().MinecraftDir, ".minecraft",
            //     "versions", gameVersionId, $"{gameVersionId}.json");
            // if (!File.Exists(jsonPath))
            // {
            //     await MirrorManager.GetCurrentMirror().Version.DownloadJsonAsync(gameVersionId);
            // }
            //
            // //校验sha1，如果sha1不正确就重新下载一次
            // var sha1 = await IOHelper.GetSha1HashFromFileAsync(jsonPath);
            // var correctSha1 =
            //     Regex.Match(
            //         MirrorManager.GetCurrentMirror().Version.VersionManifest.Versions
            //             .FirstOrDefault(i => i.Id == gameVersionId)?.Url ?? string.Empty, @"(?<=/)\w{40}(?=/)");
            // if (!string.Equals(sha1, correctSha1.Value, StringComparison.OrdinalIgnoreCase))
            // {
            //     await MirrorManager.GetCurrentMirror().Version.DownloadJsonAsync(gameVersionId);
            //     sha1 = await IOHelper.GetSha1HashFromFileAsync(jsonPath);
            //     correctSha1 =
            //         Regex.Match(
            //             MirrorManager.GetCurrentMirror().Version.VersionManifest.Versions
            //                 .FirstOrDefault(i => i.Id == gameVersionId)?.Url ?? string.Empty, @"(?<=/)\w{40}(?=/)");
            //     if (!string.Equals(sha1, correctSha1.Value, StringComparison.OrdinalIgnoreCase))
            //     {
            //         throw new Exception("找不到版本信息文件");
            //     }
            // }
            //
            // var jsonStr = await File.ReadAllTextAsync(jsonPath);
            // if (string.IsNullOrWhiteSpace(jsonStr)) throw new Exception("找不到版本信息文件");
            // //替换下载地址
            // var data = JsonConvert.DeserializeObject<VersionInfo>(jsonStr);
            // if (data == null) throw new Exception("找不到版本信息文件");
            // return data;

            #endregion

            return VersionInfoList.FirstOrDefault(i => i.Id == gameVersionId);
        }

        /// <summary>
        ///     返回默认.minecraft文件夹位置
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultMinecraftDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        /// <summary>
        /// 返回natives文件夹地址
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public static string GetNativesDir(string versionId)
        {
            return IOHelper.CombineAndCheckDirectory(GetCmclCacheDir(), $"natives-{versionId}-{Guid.NewGuid():N}");
        }

        /// <summary>
        /// 清理natives文件夹
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> CleanNativesDir()
        {
            try
            {
                var baseDir = new DirectoryInfo(GetCmclCacheDir());
                if (baseDir.Exists)
                {
                    var nativesDir = baseDir.GetDirectories($"natives-*");
                    foreach (var di in nativesDir)
                    {
                        di.Delete(true);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                await LogHelper.WriteLogAsync(e);
                return false;
            }
        }

        /// <summary>
        /// 返回本启动器缓存文件夹位置
        /// </summary>
        /// <param name="checkExist">不存在时是否创建</param>
        /// <returns></returns>
        public static string GetCmclCacheDir(bool checkExist = true)
        {
            if (checkExist)
                return IOHelper.CombineAndCheckDirectory(Environment.CurrentDirectory, "Temp");
            return Path.Combine(Environment.CurrentDirectory, "Temp");
        }
    }
}