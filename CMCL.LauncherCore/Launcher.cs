using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CMCL.LauncherCore.Download.Mirrors;
using CMCL.LauncherCore.Utilities;

namespace CMCL.LauncherCore
{
    public sealed class Launcher
    {
        private readonly CmclConfig _config;

        public Launcher()
        {
            _config = AppConfig.GetAppConfig();
        }

        public string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString();

        public async ValueTask<bool> Start()
        {
            var versionInfo = GameHelper.GetVersionInfo(_config.CurrentVersion);
            try
            {
                _beforeGameLaunch(this, "正在启动", versionInfo);

                //检查配置
                if (!ConfigCheck(out var msg))
                {
                    _onLaunchError(this, new Exception(msg));
                    return false;
                }

                //清理natives文件缓存
                _onCleanNativesDir(this, "正在清理缓存", versionInfo);
                if (!await GameHelper.CleanNativesDir())
                {
                    _onLaunchError(this, new Exception("清理缓存失败"));
                    return false;
                }

                //校验所需文件
                var mirror = MirrorManager.GetCurrentMirror();
                var baseDir = Path.Combine(_config.MinecraftDir, ".minecraft");
                _onCheckLibrariesAndAssets(this, "正在校验文件", versionInfo);
                if (!File.Exists(Path.Combine(baseDir, "versions", _config.CurrentVersion,
                        $"{_config.CurrentVersion}.json")) || //json文件
                    !File.Exists(Path.Combine(baseDir, "versions", _config.CurrentVersion,
                        $"{_config.CurrentVersion}.jar")) || //jar文件
                    (await mirror.Library.GetLibrariesDownloadList(_config.CurrentVersion, true).ConfigureAwait(false))
                    .Any() || //库文件
                    (await mirror.Asset.GetAssetsDownloadList(_config.CurrentVersion, true).ConfigureAwait(false))
                    .Any()) //资源文件
                {
                    _onLaunchError(this, new Exception("游戏文件缺失，请尝试重新下载"));
                    return false;
                }

                //解压natives文件
                _onUnzipNatives(this, "正在解压资源", versionInfo);
                await mirror.Library.UnzipNatives(_config.CurrentVersion);

                //登录
                _onMojangLogin(this, "正在登录");
                var loginResult = await MojangLogin.Login(_config.Account, _config.Password);
                if (!loginResult.IsSuccess)
                {
                    _onLaunchError(this, new Exception($"登录失败：{loginResult.Message}"));
                    return false;
                }

                //拼接启动参数
                var argument = await mirror.Version.GetStartArgument(versionInfo, loginResult);
                //子进程信息
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo(_config.CustomJavaPath, argument)
                    {
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WorkingDirectory = Path.Combine(_config.MinecraftDir, ".minecraft")
                    },
                    EnableRaisingEvents = true
                };
                process.Exited += (sender, args) => GameExitEvent(sender, process.ExitCode);
                process.OutputDataReceived += (sender, args) => GameOutputDataReceivedEvent(sender, args.Data);
                process.ErrorDataReceived += (sender, args) => GameErrorDataReceivedEvent(sender, args.Data);
                //启动
                process.Start();
                _onGameStart(this, versionInfo);

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                return true;
            }
            catch (Exception e)
            {
                await LogHelper.LogExceptionAsync(e);
                _onLaunchError(this, e);
                return false;
            }
        }

        /// <summary>
        ///     检查启动所需配置
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool ConfigCheck(out string msg)
        {
            if (GameHelper.GetVersionInfo(_config.CurrentVersion) == null)
            {
                msg = "选择的版本不存在，请重新下载";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_config.CurrentVersion))
            {
                msg = "未选择启动版本";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_config.Account) || string.IsNullOrWhiteSpace(_config.Password))
            {
                msg = "请填写用户名或密码";
                return false;
            }

            //Java安装
            if (string.IsNullOrWhiteSpace(_config.CustomJavaPath) || !File.Exists(_config.CustomJavaPath))
            {
                msg = "Java未安装或未设置Java路径";
                return false;
            }

            msg = string.Empty;
            return true;
        }

        #region 各类事件

        private void GameExitEvent(object sender, int exitCode)
        {
            _onGameExit(sender, GameHelper.GetVersionInfo(_config.CurrentVersion), exitCode);
        }

        private void GameOutputDataReceivedEvent(object sender, string data)
        {
            _onGameOutputReceived(sender, data);
        }

        private void GameErrorDataReceivedEvent(object sender, string data)
        {
            _onGameErrorReceived(sender, data);
        }

        //以下为各类事件属性
        private BeforeGameLaunch _beforeGameLaunch;
        private OnCleanNativesDir _onCleanNativesDir;
        private OnCheckLibrariesAndAssets _onCheckLibrariesAndAssets;
        private OnUnzipNatives _onUnzipNatives;
        private OnMojangLogin _onMojangLogin;
        private OnGameStart _onGameStart;
        private OnGameExit _onGameExit;
        private OnGameOutputReceived _onGameOutputReceived;
        private OnGameErrorReceived _onGameErrorReceived;
        private OnLaunchError _onLaunchError;
        private OnLogEventHandler _onLogEventHandler;

        /// <summary>
        ///     当调用Mojang登录接口时触发事件
        /// </summary>
        public event OnMojangLogin OnMojangLogin
        {
            add => _onMojangLogin += value;
            remove => _onMojangLogin -= value;
        }

        /// <summary>
        ///     当解压Natives资源时触发事件
        /// </summary>
        public event OnUnzipNatives OnUnzipNatives
        {
            add => _onUnzipNatives += value;
            remove => _onUnzipNatives -= value;
        }

        /// <summary>
        ///     当校验库和资源时触发事件
        /// </summary>
        public event OnCheckLibrariesAndAssets OnCheckLibrariesAndAssets
        {
            add => _onCheckLibrariesAndAssets += value;
            remove => _onCheckLibrariesAndAssets -= value;
        }

        /// <summary>
        ///     当清理Natives缓存时触发事件
        /// </summary>
        public event OnCleanNativesDir OnCleanNativesDir
        {
            add => _onCleanNativesDir += value;
            remove => _onCleanNativesDir -= value;
        }

        /// <summary>
        ///     收到游戏进程输出事件
        /// </summary>
        public event OnGameOutputReceived OnGameOutputReceived
        {
            add => _onGameOutputReceived += value;
            remove => _onGameOutputReceived -= value;
        }

        /// <summary>
        ///     收到游戏进程错误事件
        /// </summary>
        public event OnGameErrorReceived OnGameErrorReceived
        {
            add => _onGameErrorReceived += value;
            remove => _onGameErrorReceived -= value;
        }

        /// <summary>
        ///     游戏进程退出事件
        /// </summary>
        public event OnGameExit OnGameExit
        {
            add => _onGameExit += value;
            remove => _onGameExit -= value;
        }

        /// <summary>
        ///     游戏进程启动事件
        /// </summary>
        public event OnGameStart OnGameStart
        {
            add => _onGameStart += value;
            remove => _onGameStart -= value;
        }

        /// <summary>
        ///     启动游戏前触发事件
        /// </summary>
        public event BeforeGameLaunch BeforeGameLaunch
        {
            add => _beforeGameLaunch += value;
            remove => _beforeGameLaunch -= value;
        }

        /// <summary>
        ///     启动错误事件
        /// </summary>
        public event OnLaunchError OnLaunchError
        {
            add => _onLaunchError += value;
            remove => _onLaunchError -= value;
        }

        /// <summary>
        ///     日志记录事件
        /// </summary>
        public event OnLogEventHandler OnLogEventHandler
        {
            add => _onLogEventHandler += value;
            remove => _onLogEventHandler -= value;
        }

        #endregion
    }
}