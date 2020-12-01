using CMCL.Client.Download.Mirrors.Interface;

namespace CMCL.Client.Download.Mirrors.BMCLApi
{
    public class BMCLApi : Mirror
    {
        public BMCLApi()
        {
            this.Version = new Version();
        }

        public override string MirrorName { get; } = "BMCLApi源";
        public override DownloadSource MirrorEnum { get; } = DownloadSource.BMCLApi;
        public override Version Version { get; }
    }
}