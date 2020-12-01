using CMCL.Client.Download.Mirrors.Interface;

namespace CMCL.Client.Download.Mirrors.MCBBS
{
    public class MCBBS : Mirror
    {
        public MCBBS()
        {
            Version = new Version();
        }

        public override string MirrorName { get; } = "MCBBS源";

        public override DownloadSource MirrorEnum { get; } = DownloadSource.MCBBS;
        public override Interface.Version Version { get; }
    }
}