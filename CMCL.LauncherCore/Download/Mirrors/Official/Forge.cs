using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMCL.LauncherCore.Download.Mirrors.Official
{
    public class Forge: Interface.Forge
    {
        //forge官方未提供相关api，所以依旧采用mcbbs镜像
        protected override string MirrorUrl { get; } = "https://download.mcbbs.net";
    }
}
