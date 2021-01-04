namespace CMCL.Core.Download.Mirrors.Interface
{
    public abstract class Mirror
    {
        /// <summary>
        ///     镜像名
        /// </summary>
        public abstract string MirrorName { get; }

        /// <summary>
        ///     镜像枚举
        /// </summary>
        public abstract DownloadSource MirrorEnum { get; }

        /// <summary>
        ///     版本
        /// </summary>
        public abstract Version Version { get; }

        /// <summary>
        ///     库
        /// </summary>
        public abstract Library Library { get; }

        /// <summary>
        ///     资源
        /// </summary>
        public abstract Asset Asset { get; }
    }
}