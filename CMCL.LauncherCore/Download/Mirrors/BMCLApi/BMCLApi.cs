using CMCL.LauncherCore.Download.Mirrors.Interface;
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
            Forge = new Forge();
        }

        public override string MirrorName { get; } = "BMCLApi";
        public override DownloadSource MirrorEnum { get; } = DownloadSource.BMCLApi;
        public override Version Version { get; }
        public override Interface.Library Library { get; }
        public override Asset Asset { get; }
        public override Interface.Forge Forge { get; }
    }
}