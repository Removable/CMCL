using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace CMCL.Client.Download
{
    /// <summary>
    /// http client资源池
    /// </summary>
    public static class HttpClientPool
    {
        private static List<HttpClientPack> Pool = new List<HttpClientPack>();
        private static readonly object _poolLock = new object();

        public static HttpClientPack GetHttpClient()
        {
            lock (_poolLock)
            {
                var clientPack = Pool.FirstOrDefault(i => !i.InUse);
                if (clientPack == null)
                {
                    clientPack = new HttpClientPack
                    {
                        InUse = false,
                        client = new HttpClient(new HttpClientHandler {AllowAutoRedirect = false})
                    };
                    Pool.Add(clientPack);
                }

                clientPack.InUse = true;
                return clientPack;
            }
        }
    }

    /// <summary>
    /// http client资源
    /// </summary>
    public class HttpClientPack : IDisposable
    {
        /// <summary>
        /// HTTP客户端
        /// </summary>
        public HttpClient client { get; init; }

        /// <summary>
        /// 是否使用中
        /// </summary>
        public bool InUse { get; set; }

        public void Dispose()
        {
            this.InUse = false;
        }
    }
}