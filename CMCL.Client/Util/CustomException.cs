using System;

namespace CMCL.Client.Util
{
    public class FileSha1Error : ApplicationException
    {
        public FileSha1Error(string message) : base(message)
        {
        }

        public override string Message => base.Message;
    }
}