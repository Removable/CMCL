using System.Runtime.Serialization;

namespace CMCL.Core.GameVersion
{
    public class GameVersionInfo
    {
        /// <summary>
        ///     版本号
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        ///     发布类型
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        ///     JSON文件地址
        /// </summary>
        [DataMember]
        public string Url { get; set; }

        /// <summary>
        ///     发布时间
        /// </summary>
        [DataMember]
        public string Time { get; set; }

        /// <summary>
        ///     开发时间
        /// </summary>
        [DataMember]
        public string ReleaseTime { get; set; }
    }
}