using System.Linq;

namespace CMCL.Client.Download.Mirrors.BMCLApi
{
    public class Version : Interface.Version
    {
        public override string ManifestUrl { get; } = "https://bmclapi2.bangbang93.com/mc/game/version_manifest.json";

        protected override string TransUrl(string originUrl)
        {
            const string server = "https://bmclapi2.bangbang93.com/";
            var originServers = new[] {"https://launchermeta.mojang.com/", "https://launcher.mojang.com/"};

            return originServers.Aggregate(originUrl, (current, originServer) => current.Replace(originServer, server));
        }
    }
}