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
        [Description("官方源")] Offical = 1,

        /// <summary>
        ///     MCBBS源
        /// </summary>
        [Description("MCBBS源")] MCBBS = 2,

        /// <summary>
        ///     BMCLApi源
        /// </summary>
        [Description("BMCLApi源")] BMCLApi = 3
    }

    /// <summary>
    ///     窗口消失的处理方法
    /// </summary>
    public enum WindowDisappear
    {
        /// <summary>
        ///     无处理
        /// </summary>
        None,

        /// <summary>
        ///     关闭
        /// </summary>
        Close,

        /// <summary>
        ///     隐藏
        /// </summary>
        Hide
    }
}