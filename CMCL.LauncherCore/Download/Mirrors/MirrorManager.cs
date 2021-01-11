using System.Collections.Generic;
using System.Linq;
using CMCL.Core.Download.Mirrors.Interface;
using CMCL.LauncherCore.GameEntities;
using CMCL.LauncherCore.Utilities;

namespace CMCL.LauncherCore.Download.Mirrors
{
    public static class MirrorManager
    {
        private static readonly List<Mirror> Mirrors = new()
        {
            new MCBBS.MCBBS(),
            new BMCLApi.BMCLApi()
        };

        /// <summary>
        ///     根据下载源名称获取镜像
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Mirror GetMirrorByName(string name)
        {
            return Mirrors.FirstOrDefault(i => i.MirrorName == name);
        }

        /// <summary>
        ///     根据下载源枚举获取镜像
        /// </summary>
        /// <param name="downloadSource"></param>
        /// <returns></returns>
        public static Mirror GetMirrorByEnum(DownloadSource downloadSource)
        {
            return Mirrors.FirstOrDefault(i => i.MirrorEnum == downloadSource);
        }

        /// <summary>
        ///     获取当前配置的下载源
        /// </summary>
        /// <returns></returns>
        public static Mirror GetCurrentMirror()
        {
            return GetMirrorByEnum(AppConfig.GetAppConfig().DownloadSource);
        }
    }
}