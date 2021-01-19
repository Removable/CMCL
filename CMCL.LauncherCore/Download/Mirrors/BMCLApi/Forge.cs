using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMCL.LauncherCore.Download.Mirrors.BMCLApi
{
    public class Forge : Interface.Forge
    {
        public override string GetPromosUrl { get; } = "https://bmclapi2.bangbang93.com/forge/promos";
    }
}
