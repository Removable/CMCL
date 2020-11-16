using System;
using System.ComponentModel;

namespace CMCL.Client.Download
{
    public abstract class IMirror
    {
        /// <summary>
        /// 协议
        /// </summary>
        public string Scheme { get; }

        /// <summary>
        /// 源地址头
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// 拼接头
        /// </summary>
        public string FullAddress => $"{Scheme}:{Host}";

        /// <summary>
        /// 源分类
        /// </summary>
        public SourceCategory SourceCategory { get; }

        /// <summary>
        /// 是否为该镜像源
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool IsCurrentMirror(string url)
        {
            var uri = new Uri(url);
            return uri.Host == this.Host;
        }

        /// <summary>
        /// 从Minecraft官方下载地址转到镜像地址
        /// </summary>
        /// <param name="originUrl">原地址</param>
        /// <returns></returns>
        public string TranslateToCurrentMirrorUrl(string originUrl)
        {
            var finalUrl = string.Empty;

            //若非BMCLApi所属镜像源
            if (!originUrl.StartsWith("https://bmclapi.bangbang93.com") &&
                !originUrl.StartsWith("https://download.mcbbs.net"))
            {
                //版本和版本JSON以及AssetsIndex
                if (originUrl.StartsWith("https://launchermeta.mojang.com/"))
                {
                    finalUrl = originUrl.Replace("https://launchermeta.mojang.com/", this.FullAddress);
                }
                else if (originUrl.StartsWith("https://launcher.mojang.com/"))
                {
                    finalUrl = originUrl.Replace("https://launcher.mojang.com/", this.FullAddress);
                }
                //Assets
                else if (originUrl.StartsWith("http://resources.download.minecraft.net"))
                {
                    finalUrl = originUrl.Replace("http://resources.download.minecraft.net",
                        $"{this.FullAddress}/assets");
                }
                //Libraries
                else if (originUrl.StartsWith("https://libraries.minecraft.net"))
                {
                    finalUrl = originUrl.Replace("https://libraries.minecraft.net", $"{this.FullAddress}/maven");
                }
            }
            else
            {
                var uri = new Uri(originUrl);
                finalUrl = $"{this.Scheme}:{this.Host}/{uri.AbsolutePath}{uri.Query}";
            }

            return finalUrl;
        }
    }

    /// <summary>
    /// 下载源枚举
    /// </summary>
    public enum SourceCategory
    {
        /// <summary>
        /// Mojang或其他包的官方源
        /// </summary>
        [Description("Mojang或其他包的官方源")] Official = 0,

        /// <summary>
        /// BMCLApi源
        /// </summary>
        [Description("BMCLApi源")] BMCLApi = 1,

        /// <summary>
        /// MCBBS源
        /// </summary>
        [Description("MCBBS源")] MCBBS = 2,
    }
}