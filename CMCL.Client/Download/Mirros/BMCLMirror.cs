namespace CMCL.Client.Download
{
    /// <summary>
    /// MBCLApi源
    /// </summary>
    public class BMCLMirror : IMirror
    {
        public string Scheme { get; } = "https";
        public string Host { get; } = "bmclapi2.bangbang93.com";
        public SourceCategory SourceCategory { get; } = SourceCategory.BMCLApi;
    }
}