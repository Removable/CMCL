using System.Linq;

namespace CMCL.LauncherCore.Download.Mirrors.BMCLApi
{
    public class Library : Interface.Library
    {
        /// <summary>
        ///     转换下载地址
        /// </summary>
        /// <param name="originUrl"></param>
        /// <returns></returns>
        protected override string TransUrl(string originUrl)
        {
            const string server = "https://bmclapi2.bangbang93.com/maven/";
            var originServers = new[] {"https://libraries.minecraft.net/", "https://files.minecraftforge.net/maven/"};

            return originServers.Aggregate(originUrl, (current, originServer) => current.Replace(originServer, server));
        }
    }
}