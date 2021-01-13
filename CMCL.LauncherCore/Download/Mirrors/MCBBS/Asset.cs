namespace CMCL.LauncherCore.Download.Mirrors.MCBBS
{
    public class Asset : Interface.Asset
    {
        protected override string Server { get; } = "https://download.mcbbs.net/assets";
    }
}