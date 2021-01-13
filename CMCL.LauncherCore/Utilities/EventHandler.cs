using System;
using CMCL.LauncherCore.GameEntities.JsonClasses;
using CMCL.LauncherCore.Launch;

namespace CMCL.LauncherCore.Utilities
{
    #region 游戏启动相关事件

    public delegate void OnLogEventHandler(object sender, string log);

    public delegate void OnGameOutputReceived(object sender, string data);

    public delegate void OnGameErrorReceived(object sender, string data);

    public delegate void OnGameExit(object sender, VersionInfo versionInfo, int exitCode);

    public delegate void OnGameStart(object sender, VersionInfo versionInfo);

    public delegate void BeforeGameLaunch(object sender, string status, VersionInfo versionInfo);

    public delegate void OnCleanNativesDir(object sender, string status, VersionInfo versionInfo);

    public delegate void OnCheckLibrariesAndAssets(object sender, string status, VersionInfo versionInfo);

    public delegate void OnUnzipNatives(object sender, string status, VersionInfo versionInfo);

    public delegate void OnMojangLogin(object sender, string data);

    public delegate void OnLaunchError(Launcher launcher, Exception exception);

    #endregion

    #region 下载相关事件

    public delegate void BeforeDownloadStart(string currentTaskName, int totalCount, int finishedCount);

    public delegate void OnDownloadProgressChanged(string msg, double progress);

    public delegate void OnDownloadFinish(string currentTaskName, int totalCount, int finishedCount);

    #endregion
}