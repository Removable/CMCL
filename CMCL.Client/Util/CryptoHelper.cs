using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CMCL.Client.Util
{
    public static class CryptoHelper
    {
        public static string GetSha1HashFromFile(string filename)
        {
            if (!File.Exists(filename)) return null;
            var file = new FileStream(filename, FileMode.Open);
            var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            var retVal = sha1.ComputeHash(file);
            file.Close();

            return Byte2String(retVal);
        }

        private static string Byte2String(IEnumerable<byte> buffer)
        {
            var sb = new StringBuilder();
            foreach (var t in buffer)
            {
                sb.Append(t.ToString("x2"));
            }
            return sb.ToString();
        }
        
        public static string Md5(string str)
        {
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var retVal = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            return Byte2String(retVal);
        }
    }
}