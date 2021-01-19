using System.Collections.Generic;
using System.Linq;
using CMCL.LauncherCore.Download.Mirrors.BMCLApi;
using CMCL.LauncherCore.Download.Mirrors.Interface;
using CMCL.LauncherCore.Download.Mirrors.MCBBS;
using CMCL.LauncherCore.Download.Mirrors.Official;
using CMCL.LauncherCore.GameEntities;
using CMCL.LauncherCore.Utilities;

namespace CMCL.LauncherCore.Download
{
    public static class MirrorManager
    {
        private static readonly List<Mirror> Mirrors = new()
        {
            new MCBBS(),
            new BMCLApi(),
            new OfficialSource(),
        };

        /// <summary>
        ///     获取所有镜像
        /// </summary>
        /// <returns></returns>
        public static Mirror[] GetAllMirrors()
        {
            return Mirrors.ToArray();
        }

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