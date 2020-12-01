using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CMCL.Client.Util;
using Newtonsoft.Json;

namespace CMCL.Client.LoginPlugin
{
    public class MojangLogin
    {
        private const string _routeAuthenticate = "https://authserver.mojang.com/authenticate";

        public static string ClientToken;

        /// <summary>
        ///     官方正版登录
        /// </summary>
        /// <param name="username"></param>
        /// <param name="psw"></param>
        /// <returns></returns>
        public static async ValueTask<LoginResult> Login(string username, string psw)
        {
            ClientToken = Guid.NewGuid().ToString();
            var loginResult = new LoginResult(AppConfig.GetAppConfig().Account);
            try
            {
                var loginRequest = new LoginRequest(username, psw);
                //请求接口
                var client = GlobalStaticResource.HttpClientFactory.CreateClient("loginClient");
                var content = new StringContent(JsonConvert.SerializeObject(loginRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json") {CharSet = "utf-8"};
                var response = await client.PostAsync(_routeAuthenticate, content).ConfigureAwait(false);

                //读取返回字符
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    if (responseContent.Contains("Invalid credentials")) throw new Exception("用户名或密码错误！");
                    throw new Exception(responseContent);
                }

                var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
                if (loginResponse == null) throw new Exception("登录接口返回空值");
                if (!loginResponse.ClientToken.Equals(ClientToken))
                {
                    loginResult.IsSuccess = false;
                    loginResult.Message = "客户端标识和服务器返回不符，这是个不常见的错误，就算是正版启动器这里也没做任何处理，只是报了这么个错。";
                    return loginResult;
                }

                loginResult.IsSuccess = true;
                loginResult.Username = loginResponse.SelectedProfile.Name;
                loginResult.ClientIdentifier = loginResponse.ClientToken;
                loginResult.AuthUuid = loginResponse.SelectedProfile.Id;
                loginResult.AuthAccessToken = loginResponse.AccessToken;

                return loginResult;
            }
            catch (Exception exception)
            {
                loginResult.IsSuccess = false;
                loginResult.Message = exception.Message;
                await LogHelper.WriteLogAsync(exception).ConfigureAwait(false);
                throw;
            }
        }
    }
}