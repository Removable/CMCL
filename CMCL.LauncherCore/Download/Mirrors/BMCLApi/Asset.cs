namespace CMCL.Core.Download.Mirrors.BMCLApi
{
    public class Asset : LauncherCore.Download.Mirrors.Interface.Asset
    {
        protected override string Server { get; } = "https://bmclapi2.bangbang93.com/assets";
    }
}