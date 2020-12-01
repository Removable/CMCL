namespace CMCL.Client.Download.Mirrors.MCBBS
{
    public class Version : Interface.Version
    {
        public override string ManifestUrl { get; } = "https://download.mcbbs.net/mc/game/version_manifest.json";
    }
}