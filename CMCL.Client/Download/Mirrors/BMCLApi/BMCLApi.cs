﻿using CMCL.Client.Download.Mirrors.Interface;

namespace CMCL.Client.Download.Mirrors.BMCLApi
{
    public class BMCLApi : Mirror
    {
        public BMCLApi()
        {
            Version = new Version();
        }

        public override string MirrorName { get; } = "BMCLApi源";
        public override DownloadSource MirrorEnum { get; } = DownloadSource.BMCLApi;
        public override Version Version { get; }
        public override Library Library { get; }
    }
}