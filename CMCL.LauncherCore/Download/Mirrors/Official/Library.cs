using System.Linq;

namespace CMCL.LauncherCore.Download.Mirrors.Official
{
    public class Library : Core.Download.Mirrors.Interface.Library
    {
        /// <summary>
        ///     转换下载地址
        /// </summary>
        /// <param name="originUrl"></param>
        /// <returns></returns>
        protected override string TransUrl(string originUrl)
        {
            return originUrl;
        }
    }
}