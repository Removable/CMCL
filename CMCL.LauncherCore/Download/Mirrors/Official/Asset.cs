namespace CMCL.LauncherCore.Download.Mirrors.Official
{
    public class Asset : Interface.Asset
    {
        protected override string Server { get; } = "http://resources.download.minecraft.net";

        protected override string TransUrl(string originUrl)
        {
            if (originUrl.StartsWith("http"))
            {
                return originUrl;
            }

            return $"{Server}/{originUrl}";
        }
    }
}