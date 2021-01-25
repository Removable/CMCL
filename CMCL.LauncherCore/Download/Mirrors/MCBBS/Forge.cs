using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMCL.LauncherCore.Download.Mirrors.MCBBS
{
    public class Forge : Interface.Forge
    {
        protected override string MirrorUrl { get; } = "https://download.mcbbs.net";
    }
}
