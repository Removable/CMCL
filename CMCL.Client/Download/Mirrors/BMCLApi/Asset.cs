namespace CMCL.Client.Download.Mirrors.BMCLApi
{
    public class Asset : Interface.Asset
    {
        protected override string Server { get; } = "https://bmclapi2.bangbang93.com/assets";
    }
}