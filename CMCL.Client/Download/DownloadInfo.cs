using System;
using System.Collections.Generic;
using System.Text;

namespace CMCL.Client.Download
{
    /// <summary>
    /// 下载信息
    /// </summary>
    public class DownloadInfo
    {
        /// <summary>
        /// 总文件数
        /// </summary>
        public int TotalFilesCount { get; set; }
        /// <summary>
        /// 当前文件索引
        /// </summary>
        public int CurrentFileIndex { get; set; }
        /// <summary>
        /// 当前文件名
        /// </summary>
        public string CurrentFileName { get; set; }
        /// <summary>
        /// 当前下载分类
        /// </summary>
        public string CurrentCategory { get; set; }
        /// <summary>
        /// 下载完成后是否标记为下载已结束
        /// </summary>
        public bool ReportFinish { get; set; }
        
        /// <summary>
        /// md5哈希值
        /// </summary>
        public string Hash { get; set; }
    }
}
