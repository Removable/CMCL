﻿using CMCL.Core.Download.Mirrors.Interface;

namespace CMCL.Core.Download.Mirrors.MCBBS
{
    public class MCBBS : Mirror
    {
        public MCBBS()
        {
            Version = new Version();
            Library = new Library();
            Asset = new Asset();
        }

        public override string MirrorName { get; } = "MCBBS源";

        public override DownloadSource MirrorEnum { get; } = DownloadSource.MCBBS;
        public override Interface.Version Version { get; }
        public override Interface.Library Library { get; }
        public override Interface.Asset Asset { get; }
    }
}