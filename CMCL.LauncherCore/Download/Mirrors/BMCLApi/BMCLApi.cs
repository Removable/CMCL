using CMCL.Core.Download.Mirrors.Interface;
using CMCL.LauncherCore.GameEntities;
using Library = CMCL.LauncherCore.Download.Mirrors.BMCLApi.Library;

namespace CMCL.LauncherCore.Download.Mirrors.BMCLApi
{
    public class BMCLApi : Mirror
    {
        public BMCLApi()
        {
            Version = new Version();
            Asset = new Asset();
            Library = new Library();
        }

        public override string MirrorName { get; } = "BMCLApi";
        public override DownloadSource MirrorEnum { get; } = DownloadSource.BMCLApi;
        public override Version Version { get; }
        public override Core.Download.Mirrors.Interface.Library Library { get; }
        public override Asset Asset { get; }
    }
}