using System.Linq;

namespace CMCL.LauncherCore.Download.Mirrors.MCBBS
{
    public class Version : Interface.Version
    {
        public override string ManifestUrl { get; } = "https://download.mcbbs.net/mc/game/version_manifest.json";

        protected override string TransUrl(string originUrl)
        {
            const string server = "https://download.mcbbs.net/";
            var originServers = new[] {"https://launchermeta.mojang.com/", "https://launcher.mojang.com/"};

            return originServers.Aggregate(originUrl, (current, originServer) => current.Replace(originServer, server));
        }
    }
}