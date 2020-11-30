using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMCL.Client.Download
{
    public enum DownloadSource
    {
        /// <summary>
        /// 所有下载项的官方源
        /// </summary>
        [Description("官方源")] Offical = 1,

        /// <summary>
        /// MCBBS源
        /// </summary>
        [Description("MCBBS源")] MCBBS = 2,

        /// <summary>
        /// BMCLApi源
        /// </summary>
        [Description("BMCLApi源")] BMCLApi = 3,
    }
}
