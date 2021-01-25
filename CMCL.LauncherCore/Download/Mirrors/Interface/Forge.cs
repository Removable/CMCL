using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CMCL.LauncherCore.GameEntities.JsonClasses;
using CMCL.LauncherCore.Resources;
using CMCL.LauncherCore.Utilities;
using Newtonsoft.Json;

namespace CMCL.LauncherCore.Download.Mirrors.Interface
{
    public abstract class Forge
    {
        private BeforeDownloadStart _beforeDownloadStart;
        private OnDownloadFinish _onDownloadFinish;
        private OnDownloadProgressChanged _onDownloadProgressChanged;
        protected virtual string MirrorUrl { get; } = "";

        /// <summary>
        /// 获取受标记的forge版本
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public virtual async ValueTask<ForgeVersion[]> GetForgeVersionList(HttpClient httpClient)
        {
            var jsonStr = await Downloader.GetStringAsync(httpClient, $"{MirrorUrl}/forge/promos")
                .ConfigureAwait(false);
            var promos = JsonConvert.DeserializeObject<ForgeVersion[]>(jsonStr);
            return promos ?? Array.Empty<ForgeVersion>();
        }

        /// <summary>
        /// 获取forge安装器
        /// </summary>
        /// <param name="selectedForge"></param>
        /// <returns>保存地址</returns>
        public async ValueTask<string> DownloadForgeInstaller(ForgeVersion selectedForge)
        {
            var build = selectedForge.Build;
            var installer = selectedForge.Build.Files.FirstOrDefault(f => f.Category == "installer") ?? new();

            var url =
                $"{MirrorUrl}/maven/net/minecraftforge/forge/{build.McVersion}-{build.Version}{(build.Branch != null ? $"-{build.Branch}" : "")}/forge-{build.McVersion}-{build.Version}{(build.Branch != null ? $"-{build.Branch}" : "")}-{installer.Category}.{installer.Format}";

            var filePath = Utils.CombineAndCheckDirectory(true, GameHelper.GetCmclCacheDir(), "forge.jar");

            //校验sha1
            if (!File.Exists(filePath) || !string.Equals(
                await Utils.GetSha1HashFromFileAsync(filePath).ConfigureAwait(false),
                selectedForge.Build.Files.FirstOrDefault(f => f.Category.Equals("installer") && f.Format.Equals("jar"))
                    ?.Hash ?? "", StringComparison.CurrentCultureIgnoreCase))
            {
                var progress = new Progress<double>();
                progress.ProgressChanged += (_, value) => { _onDownloadProgressChanged?.Invoke("", value); };
                _beforeDownloadStart?.Invoke("Forge安装程序", 1, 0);
                await Downloader.GetFileAsync(Utils.HttpClientFactory.CreateClient(), url, filePath, progress);
                _onDownloadFinish?.Invoke("Forge安装程序", 1, 1);
            }

            return filePath;
        }

        /// <summary>
        /// 安装forge
        /// </summary>
        /// <param name="forgeVersion">forge安装器信息</param>
        /// <param name="installerPath"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async ValueTask InstallForge(ForgeVersion forgeVersion, string installerPath)
        {
            var config = AppConfig.GetAppConfig();
            //创建launcher_profiles.json
            var profilePath =
                Utils.CombineAndCheckDirectory(true, config.MinecraftDir, ".minecraft", "launcher_profiles.json");
            if (!File.Exists(profilePath))
            {
                await using var w = new StreamWriter(profilePath);
                await w.WriteAsync(DefaultData.LauncherProfile);
            }

            //读取jar包内容
            await using var fs = new FileStream(installerPath, FileMode.Open);
            using var archive = new ZipArchive(fs);

            var jsonEntry = archive.GetEntry("version.json");
            if (jsonEntry == null) throw new Exception("forge安装错误，找不到版本信息");

            using var sr = new StreamReader(jsonEntry.Open());
            var jsonStr = await sr.ReadToEndAsync();
            //保存版本信息
            var forgeVersionName = $"{forgeVersion.Build.McVersion}-forge-{forgeVersion.Build.Version}";
            var savePath = Utils.CombineAndCheckDirectory(true, config.MinecraftDir, ".minecraft", "versions",
                forgeVersionName, $"{forgeVersionName}.json");
            await File.WriteAllTextAsync(savePath, jsonStr, Encoding.UTF8);
            //反序列化forge版本信息以及刷新版本信息List
            var forgeVersionInfo = JsonConvert.DeserializeObject<VersionInfo>(jsonStr);
            await GameHelper.LoadVersionInfoList();

            //下载forge库文件
            var basePath = Path.Combine(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "libraries");
            var totalCount = forgeVersionInfo.Libraries.Length;
            if (totalCount <= 0) return;

            var dic = new ConcurrentDictionary<string, int>();
            var sem = new SemaphoreSlim(AppConfig.GetAppConfig().MaxThreadCount);
            var finishedCount = 0;
            var taskArray = forgeVersionInfo.Libraries.Select(libraryInfo => Task.Run(async () =>
            {
                try
                {
                    if (!dic.TryAdd(libraryInfo.Downloads.Artifact.Url, 0)) return;

                    _beforeDownloadStart?.Invoke("下载Forge库", totalCount, finishedCount);
                    var sp = Utils.CombineAndCheckDirectory(true, basePath, libraryInfo.Downloads.Artifact.Path);
                    //转换地址
                    //TODO 这里forge-1.16.5-36.0.0.jar的下载地址为空
                    var url = TransUrl(libraryInfo.Downloads.Artifact.Url);
                    if (File.Exists(sp) && string.Equals(await Utils.GetSha1HashFromFileAsync(sp).ConfigureAwait(false),
                        libraryInfo.Downloads.Artifact.Sha1, StringComparison.OrdinalIgnoreCase))
                    {
                        finishedCount++;
                        _onDownloadFinish?.Invoke("下载Forge库", totalCount, finishedCount);
                        return;
                    }

                    await Downloader.GetFileAsync(Utils.HttpClientFactory.CreateClient(), url, sp, null)
                        .ConfigureAwait(false);

                    finishedCount++;
                    _onDownloadFinish?.Invoke("下载Forge库", totalCount, finishedCount);
                }
                finally
                {
                    sem.Release();
                }
            }));

            await Task.WhenAll(taskArray);
        }

        /// <summary>
        ///     转换下载地址
        /// </summary>
        /// <param name="originUrl"></param>
        /// <returns></returns>
        protected virtual string TransUrl(string originUrl)
        {
            var originServers = new[] {"https://libraries.minecraft.net", "https://files.minecraftforge.net"};

            return originServers.Aggregate(originUrl,
                (current, originServer) => current.Replace(originServer, MirrorUrl));
        }

        #region 事件

        public event OnDownloadFinish OnDownloadFinish
        {
            add => _onDownloadFinish += value;
            remove => _onDownloadFinish -= value;
        }

        public event OnDownloadProgressChanged OnDownloadProgressChanged
        {
            add => _onDownloadProgressChanged += value;
            remove => _onDownloadProgressChanged -= value;
        }

        public event BeforeDownloadStart BeforeDownloadStart
        {
            add => _beforeDownloadStart += value;
            remove => _beforeDownloadStart -= value;
        }

        #endregion
    }
}