using System;

namespace CMCL.Core.Util
{
    public class FileSha1Error : ApplicationException
    {
        public FileSha1Error(string message) : base(message)
        {
        }

        public override string Message => base.Message;
    }
}