using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace CMCL.Client.GameVersion.JsonClasses
{
    public class AssetsIndex
    {
        public class Assets
        {
            [JsonProperty("hash")] public string Hash;
            [JsonProperty("size")] public int Size;

            public string FullPath => $"{Hash.Substring(0, 2)}\\{Hash}";
        }
    }
}