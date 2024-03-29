﻿using System.Runtime.Serialization;

namespace CMCL.LauncherCore.GameEntities
{
    public class VersionManifest
    {
        /// <summary>
        ///     游戏最新版本信息
        /// </summary>
        [DataMember]
        public GameLatestVersion Latest { get; set; }

        /// <summary>
        ///     游戏版本列表
        /// </summary>
        [DataMember]
        public VersionListInfo[] Versions { get; set; }
    }

    /// <summary>
    ///     最新版本信息
    /// </summary>
    public class GameLatestVersion
    {
        /// <summary>
        ///     稳定版本
        /// </summary>
        [DataMember]
        public string Release { get; set; }

        /// <summary>
        ///     快照版本
        /// </summary>
        [DataMember]
        public string Snapshot { get; set; }
    }
}