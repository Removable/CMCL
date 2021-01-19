using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CMCL.LauncherCore.GameEntities.JsonClasses;
using Newtonsoft.Json;

namespace CMCL.LauncherCore.Download.Mirrors.Interface
{
    public abstract class Forge
    {
        protected const string GetPromosUrl = "https://bmclapi2.bangbang93.com/forge/promos";

        /// <summary>
        /// 获取受标记的forge版本
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public virtual async ValueTask<ForgePromo[]> GetForgeVersionList(HttpClient httpClient)
        {
            var jsonStr = await Downloader.GetStringAsync(httpClient, GetPromosUrl).ConfigureAwait(false);
            var promos = JsonConvert.DeserializeObject<ForgePromo[]>(jsonStr);
            return promos;
        }
    }
}