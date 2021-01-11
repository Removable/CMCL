using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMCL.LauncherCore.GameEntities.JsonClasses;
using Newtonsoft.Json;

namespace CMCL.LauncherCore.Utilities
{
    public class GameHelper
    {
        #region 获取下载CancellationTokenSource

        private static CancellationTokenSource _downloadCancellationTokenSource;

        /// <summary>
        ///     获取下载CancellationTokenSource
        /// </summary>
        /// <returns></returns>
        public static CancellationTokenSource GetDownloadCancellationToken()
        {
            if (_downloadCancellationTokenSource == null || _downloadCancellationTokenSource.IsCancellationRequested)
                _downloadCancellationTokenSource = new CancellationTokenSource();

            return _downloadCancellationTokenSource;
        }

        #endregion

        #region 游戏有关

        /// <summary>
        ///     版本文件信息集合
        /// </summary>
        public static List<VersionInfo> VersionInfoList = new();

        public static string NativesDirName = $"natives-$versionId-{Guid.NewGuid():N}";

        /// <summary>
        ///     加载版本文件信息
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
        ///     返回natives文件夹地址
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public static string GetNativesDir(string versionId)
        {
            return Utils.CombineAndCheckDirectory(false, GetCmclCacheDir(),
                NativesDirName.Replace("$versionId", versionId));
        }

        /// <summary>
        ///     清理natives文件夹
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> CleanNativesDir()
        {
            try
            {
                var baseDir = new DirectoryInfo(GetCmclCacheDir());
                if (baseDir.Exists)
                {
                    var nativesDir = baseDir.GetDirectories("natives-*");
                    foreach (var di in nativesDir) di.Delete(true);
                }

                return true;
            }
            catch (Exception e)
            {
                await LogHelper.LogExceptionAsync(e);
                return false;
            }
        }

        /// <summary>
        ///     返回本启动器缓存文件夹位置
        /// </summary>
        /// <returns></returns>
        public static string GetCmclCacheDir()
        {
            return Utils.CombineAndCheckDirectory(false, Environment.CurrentDirectory, "Temp");
        }

        public static async ValueTask ApplicationInit()
        {
            //初始化配置
            await AppConfig.InitConfig().ConfigureAwait(false);

            //获取所有Version的json
            await LoadVersionInfoList();
        }

        #endregion
    }
}