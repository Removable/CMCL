using CMCL.Core.Download.Mirrors.Interface;

namespace CMCL.Core.Download.Mirrors.BMCLApi
{
    public class BMCLApi : Mirror
    {
        public BMCLApi()
        {
            Version = new Version();
            Asset = new Asset();
        }

        public override string MirrorName { get; } = "BMCLApi源";
        public override DownloadSource MirrorEnum { get; } = DownloadSource.BMCLApi;
        public override Version Version { get; }
        public override Library Library { get; }
        public override Asset Asset { get; }
    }
}