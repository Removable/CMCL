namespace CMCL.Client.Download
{
    public class MCBBSMirror : IMirror
    {
        public string Scheme { get; } = "https";
        public string Host { get; } = "download.mcbbs.net";
        public SourceCategory SourceCategory { get; } = SourceCategory.MCBBS;
    }
}