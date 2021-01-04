using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using CMCL.Client.Game;
using CMCL.Client.GameVersion.JsonClasses;
using CMCL.Client.LoginPlugin;
using CMCL.Client.Util;
using CMCL.Client.Window;
using ComponentUtil.Common.Data;
using Newtonsoft.Json;

namespace CMCL.Client.Download.Mirrors.Interface
{
    public abstract class Version
    {
        public GameVersionManifest VersionManifest;
        public virtual string ManifestUrl { get; } = "";

        /// <summary>
        ///     获取版本列表
        /// </summary>
        /// <returns></returns>
        public virtual async ValueTask<GameVersionManifest> LoadGameVersionList(HttpClient httpClient)
        {
            var jsonStr = await Downloader.GetStringAsync(httpClient, ManifestUrl).ConfigureAwait(false);
            var gameVersionManifest = JsonConvert.DeserializeObject<GameVersionManifest>(jsonStr);
            VersionManifest = gameVersionManifest;
            return gameVersionManifest;
        }

        /// <summary>
        ///     下载版本json和jar，并显示下载进度
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public virtual async ValueTask DownloadJsonAndJarAsync(string versionId)
        {
            var downloadFrm = DownloadFrm.GetInstance(Application.Current.MainWindow);
            downloadFrm.Dispatcher.BeginInvoke(new Action(() =>
            {
                downloadFrm.DataContext = Downloader.DownloadInfoHandler;
                downloadFrm.ShowDialog();
            }));

            try
            {
                await DownloadJsonAsync(versionId);

                await DownloadJarAsync(versionId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                downloadFrm.Dispatcher.BeginInvoke(new Action(() => { downloadFrm.Close(); }));
            }
        }

        /// <summary>
        ///     下载版本json
        /// </summary>
        /// <param name="versionId">游戏版本号</param>
        /// <returns></returns>
        public virtual async ValueTask DownloadJsonAsync(string versionId)
        {
            var version = VersionManifest.Versions.FirstOrDefault(i => i.Id == versionId);
            if (version == null) throw new Exception("找不到指定版本");

            //转换地址
            var url = TransUrl(version.Url);

            await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(), url,
                IOHelper.CombineAndCheckDirectory(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "versions",
                    versionId, $"{versionId}.json"), $"下载{versionId}.json");

            //重新加载版本信息列表
            await GameHelper.LoadVersionInfoList();
        }

        /// <summary>
        ///     下载版本jar
        /// </summary>
        /// <param name="versionId">游戏版本号</param>
        /// <returns></returns>
        public virtual async ValueTask DownloadJarAsync(string versionId)
        {
            var versionInfo = GameHelper.GetVersionInfo(versionId);

            var filePath = IOHelper.CombineAndCheckDirectory(AppConfig.GetAppConfig().MinecraftDir, ".minecraft",
                "versions", versionId,
                $"{versionId}.jar");

            //转换地址
            var url = TransUrl(versionInfo.Downloads.Client.Url);

            //校验sha1
            if (!File.Exists(filePath) ||
                !string.Equals(await IOHelper.GetSha1HashFromFileAsync(filePath).ConfigureAwait(false),
                    versionInfo.Downloads.Client.Sha1, StringComparison.CurrentCultureIgnoreCase))
                await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(), url, filePath, "");
        }

        /// <summary>
        ///     转换下载地址
        /// </summary>
        /// <param name="originUrl"></param>
        /// <returns></returns>
        protected virtual string TransUrl(string originUrl)
        {
            const string server = "";
            var originServers = new[] {"https://launchermeta.mojang.com/", "https://launcher.mojang.com/"};

            return originServers.Aggregate(originUrl, (current, originServer) => current.Replace(originServer, server));
        }

        public virtual async ValueTask<string> GetStartArgument(VersionInfo versionInfo, LoginResult loginInfo)
        {
            var config = AppConfig.GetAppConfig();
            // var argResult = new StringBuilder($"\"{config.CustomJavaPath}\"");
            var argResult = new StringBuilder();

            foreach (var argStr in versionInfo.Arguments.Jvm)
                if (argStr!.ToString()!.Contains("\"rules\":"))
                {
                    var argument = JsonConvert.DeserializeObject<ArgumentsEntity>(argStr.ToString() ?? string.Empty);
                    if (argument?.Rules == null || !argument.Rules.Any()) continue;
                    var valueStr = argument.Value?.ToString();
                    if (string.IsNullOrWhiteSpace(valueStr)) continue;
                    foreach (var rule in argument.Rules)
                    {
                        if (rule.Action.Equals("allow"))
                            if ((string.IsNullOrWhiteSpace(rule.OS.Name) ||
                                 rule.OS.Name.Equals(Utils.GetOS().GetDescription(),
                                     StringComparison.OrdinalIgnoreCase))
                                && (string.IsNullOrWhiteSpace(rule.OS.Arch) || rule.OS.Arch.Equals(
                                    RuntimeInformation.OSArchitecture.ToString(), StringComparison.OrdinalIgnoreCase))
                                && (string.IsNullOrWhiteSpace(rule.OS.Version) ||
                                    Regex.IsMatch(Environment.OSVersion.Version.ToString(), rule.OS.Version)))
                            {
                                if (valueStr.StartsWith("["))
                                {
                                    var args = Regex.Matches(valueStr, "\\\".+\\\"");
                                    foreach (Match m in args)
                                    {
                                        var s = m.Value.Contains(" ") ? m.Value : m.Value.Trim('\"');
                                        argResult.Append($" {s}");
                                    }
                                }
                                else
                                {
                                    argResult.Append($" {argument.Value}");
                                }
                            }

                        if (rule.Action.Equals("disallow"))
                            if ((string.IsNullOrWhiteSpace(rule.OS.Name) ||
                                 !rule.OS.Name.Equals(Utils.GetOS().GetDescription(),
                                     StringComparison.OrdinalIgnoreCase))
                                && (string.IsNullOrWhiteSpace(rule.OS.Arch) ||
                                    !rule.OS.Arch.Equals(RuntimeInformation.OSArchitecture.ToString(),
                                        StringComparison.OrdinalIgnoreCase))
                                && (string.IsNullOrWhiteSpace(rule.OS.Version) ||
                                    !Regex.IsMatch(Environment.OSVersion.Version.ToString(), rule.OS.Version)))
                            {
                                if (valueStr.StartsWith("["))
                                {
                                    var args = Regex.Matches(valueStr, "\\\"\\S+\\\"");
                                    foreach (Match m in args)
                                    {
                                        var s = m.Value.Contains(" ") ? m.Value : m.Value.Trim('\"');
                                        argResult.Append($" {s}");
                                    }
                                }
                                else
                                {
                                    argResult.Append($" {argument.Value}");
                                }
                            }
                    }
                }
                else
                {
                    argResult.Append($" {await ReplaceArg(argStr.ToString())}");
                }

            foreach (var argStr in versionInfo.Arguments.Game)
                if (argStr!.ToString()!.Contains("\"rules\":"))
                    continue;
                else
                    argResult.Append($" \"{await ReplaceArg(argStr.ToString())}\"");

            argResult.Append(
                $" -Xmx{config.JavaMemory.ToString()}M -XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=32M");

            return argResult.ToString();

            async Task<string> ReplaceArg(string argStr)
            {
                switch (argStr)
                {
                    case "${auth_player_name}":
                        return loginInfo.Username;
                    case "${version_name}":
                        return versionInfo.Id;
                    case "${game_directory}":
                        return Path.Combine(config.MinecraftDir, ".minecraft");
                    case "${assets_root}":
                        return Path.Combine(config.MinecraftDir, ".minecraft", "assets");
                    case "${assets_index_name}":
                        return versionInfo.Assets;
                    case "${auth_uuid}":
                        return loginInfo.AuthUuid;
                    case "${auth_access_token}":
                        return loginInfo.AuthAccessToken;
                    case "${user_type}":
                        return "Legacy";
                    case "${version_type}":
                        return versionInfo.Type;
                    case "-Djava.library.path=${natives_directory}":
                        return
                            $"\"{argStr.Replace("${natives_directory}", GameHelper.GetNativesDir(versionInfo.Id))}\"";
                    case "-Dminecraft.launcher.brand=${launcher_name}":
                        return argStr.Replace("${launcher_name}", "CMCL");
                    case "-Dminecraft.launcher.version=${launcher_version}":
                        return argStr.Replace("${launcher_version}",
                            Assembly.GetExecutingAssembly().GetName().Version?.ToString());
                    case "${classpath}":
                    {
                        var sb = new StringBuilder();
                        var libraries = await MirrorManager.GetCurrentMirror().Library
                            .GetLibrariesDownloadList(versionInfo.Id, false, false).ConfigureAwait(false);
                        var delimiter = Utils.GetOS() switch
                        {
                            SupportedOS.Windows => ";",
                            _ => ":"
                        };
                        foreach (var libraryInfo in libraries)
                        {
                            sb.Append(libraryInfo.savePath);
                            sb.Append(delimiter);
                        }

                        sb.Append(Path.Combine(config.MinecraftDir, ".minecraft", "versions", versionInfo.Id,
                            $"{versionInfo.Id}.jar"));
                        return $"\"{sb}\" {versionInfo.MainClass}";
                    }
                    default:
                        return argStr;
                }
            }
        }
    }
}