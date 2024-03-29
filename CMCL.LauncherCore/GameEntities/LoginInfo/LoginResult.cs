﻿using System;
using ComponentUtil.Common.Crypto;

namespace CMCL.LauncherCore.GameEntities.LoginInfo
{
    public class LoginResult
    {
        public LoginResult(string username, string uid = null, string clientIdentifier = null)
        {
            Username = username;
            Uuid = AccessToken = uid ?? Guid.Parse(EncryptionHelper.Md5("OfflinePlayer:" + username)).ToString();
            ClientIdentifier = clientIdentifier ?? Guid.NewGuid().ToString();
        }

        public string Username { get; set; }
        public string Uuid { get; set; }
        public string AccessToken { get; set; }
        public string SID { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string ClientIdentifier { get; set; }
        public string AuthUuid { get; set; }
        public string AuthAccessToken { get; set; }
    }
}