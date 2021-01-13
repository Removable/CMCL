using System.ComponentModel;

namespace CMCL.LauncherCore.GameEntities
{
    /// <summary>
    ///     受支持的操作系统
    /// </summary>
    public enum SupportedOS
    {
        [Description("windows")] Windows,
        [Description("linux")] Linux,
        [Description("osx")] Osx,
        [Description("other")] Other
    }

    /// <summary>
    ///     下载源
    /// </summary>
    public enum DownloadSource
    {
        /// <summary>
        ///     所有下载项的官方源
        /// </summary>
        [Description("Official")] Official = 1,

        /// <summary>
        ///     MCBBS源
        /// </summary>
        [Description("MCBBS")] MCBBS = 2,

        /// <summary>
        ///     BMCLApi源
        /// </summary>
        [Description("BMCLApi")] BMCLApi = 3
    }
}