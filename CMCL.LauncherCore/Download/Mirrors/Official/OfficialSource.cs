using CMCL.Core.Download.Mirrors.Interface;
using CMCL.LauncherCore.GameEntities;

namespace CMCL.LauncherCore.Download.Mirrors.Official
{
    public class OfficialSource : Mirror
    {
        public OfficialSource()
        {
            Version = new Version();
            Library = new Library();
            Asset = new Asset();
        }

        public override string MirrorName { get; } = "Official";

        public override DownloadSource MirrorEnum { get; } = DownloadSource.Official;
        public override Interface.Version Version { get; }
        public override Core.Download.Mirrors.Interface.Library Library { get; }
        public override Interface.Asset Asset { get; }
    }
}