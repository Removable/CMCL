using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CMCL.LauncherCore.GameEntities.JsonClasses;
using Newtonsoft.Json;

namespace CMCL.LauncherCore.Download.Mirrors.Official
{
    public class Forge: Interface.Forge
    {
        //forge官方未提供相关api，所以依旧采用mcbbs镜像
        protected override string MirrorUrl { get; } = "http://files.minecraftforge.net/";
        
        /// <summary>
        /// 获取受标记的forge版本
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public override async ValueTask<ForgeVersion[]> GetForgeVersionList()
        {
            //forge官方未提供此接口，故依旧使用bmclapi的
            var jsonStr = await Downloader.GetStringAsync("https://bmclapi2.bangbang93.com/forge/promos")
                .ConfigureAwait(false);
            var promos = JsonConvert.DeserializeObject<ForgeVersion[]>(jsonStr);
            return promos ?? Array.Empty<ForgeVersion>();
        }
    }
}
