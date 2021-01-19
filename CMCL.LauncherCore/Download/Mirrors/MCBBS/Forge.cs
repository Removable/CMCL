using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMCL.LauncherCore.Download.Mirrors.MCBBS
{
    public class Forge : Interface.Forge
    {
        public override string GetPromosUrl { get; } = "https://download.mcbbs.net/forge/promos";
    }
}
