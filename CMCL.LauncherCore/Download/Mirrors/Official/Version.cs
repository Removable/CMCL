using System.Linq;

namespace CMCL.LauncherCore.Download.Mirrors.Official
{
    public class Version : Interface.Version
    {
        public override string ManifestUrl { get; } = "https://download.mcbbs.net/mc/game/version_manifest.json";

        protected override string TransUrl(string originUrl)
        {
            return originUrl;
        }
    }
}