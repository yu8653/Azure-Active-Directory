using System;
using System.Web;
using System.Net.Http;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using RestSharp;
using NPOI.SS.Formula.Functions;

namespace AzureAdTest
{
    public partial class MicrosoftAzureAD : System.Web.UI.Page
    {
        string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];

        string redirectUri = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];

        string tenant = System.Configuration.ConfigurationManager.AppSettings["Tenant"];

        string key = System.Configuration.ConfigurationManager.AppSettings["key1"];

        string scope = "https://graph.microsoft.com/user.read";//user.read same as API/Permissions name 

        protected void Page_Load(object sender, EventArgs e)
        {
            SignIn();
        }


        /// <summary>
        /// Send an OpenID Connect sign-in request.
        /// Alternatively, you can just decorate the SignIn method with the [Authorize] attribute
        /// </summary>
        public void SignIn()
        {
            if (!Request.IsAuthenticated)
            {
                if (this.ClientQueryString.Length > 0)
                {
                    string qs = this.ClientQueryString;
                    System.Collections.Specialized.NameValueCollection parsed = HttpUtility.ParseQueryString(qs);
                    string code = parsed["code"];
                    RequestToken(code);
                }
                else
                {
                    HttpContext.Current.GetOwinContext().Authentication.Challenge(
                        new AuthenticationProperties { RedirectUri = "~/" },
                        OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }
            }

          
        }


        /// <summary>
        /// Send an OpenID Connect sign-out request.
        /// </summary>
        public void SignOut()
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut(
                    OpenIdConnectAuthenticationDefaults.AuthenticationType,
                    CookieAuthenticationDefaults.AuthenticationType);
        }

        private void RequestToken(string Code)
        {
           
            RestClient client = new RestClient($"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token");
            client.Timeout = -1;
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("client_id", clientId);
            request.AddParameter("scope", scope);
            request.AddParameter("redirect_uri", redirectUri);
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("client_secret", key);
            request.AddParameter("code", Code);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);


            string access_token = "";
            string id_token = "";
            try
            {
                dynamic Token = JsonConvert.DeserializeObject<dynamic>(response.Content);
                access_token = Token.access_token;
                id_token = Token.id_token;
            }
            catch
            {
                Error Error = JsonConvert.DeserializeObject<Error>(response.Content);
                string ErrorMsg = Error.error + "\r\n" + Error.error_description;
                Response.Write($"<script>alert({ErrorMsg});</script>");
            }


            UseAccessToken(access_token);

        }

        private void UseAccessToken(string AccessToken) {


            RestClient client = new RestClient("https://graph.microsoft.com/v1.0/me");
            client.Timeout = -1;
            RestRequest request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", $"Bearer {AccessToken}");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            UserInfo UserInfo = JsonConvert.DeserializeObject<UserInfo>(response.Content);

            Response.Write($"displayName: {UserInfo.displayName}</br>");
            Response.Write($"givenName: {UserInfo.givenName}</br>");
            Response.Write($"mail: {UserInfo.mail}</br>");
            Response.Write($"surname: {UserInfo.surname}</br>");
            Response.Write($"userPrincipalName: {UserInfo.userPrincipalName}</br>");

        }


        private void RefreshAccessToken(string RefreshToken) {

            var client = new RestClient($"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            //request.AddHeader("Cookie", "buid=0.AAAADQSIkWdsW0yxEjajBLZtrXbeMWemFK5Jl7xuumkUOR4zAAA.AQABAAEAAAAGV_bv21oQQ4ROqh0_1-tA0_5RUljsKLJ7Nt505r0Pdzoa40aBhtvZiQwszUXhd1-YQ-BM7KvlW9XMxCkeqySZ-l53B6-P3GoSrEoM1cVMtLU8qlEQslJTTWNEMSDhQj0gAA; fpc=AmB8btgysu5EgR4oG1uctjX-bjY7AgAAAFNE_tYOAAAA");
            request.AddParameter("client_id", clientId);
            request.AddParameter("scope", scope);
            request.AddParameter("redirect_uri", redirectUri);
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("client_secret", key);
            request.AddParameter("refresh_token", RefreshToken);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

        }
    }

    public class Token
    {
        public string token_type { get; set; }
        public string scope { get; set; }
        public string expires_in { get; set; }
        public string ext_expires_in { get; set; }
        public string access_token { get; set; }
        public string id_token { get; set; }
    }

    public class UserInfo {
  
        public string displayName { get; set; }
        public string givenName { get; set; }
        public string jobTitle { get; set; }
        public string mail { get; set; }
        public string mobilePhone { get; set; }
        public string officeLocation { get; set; }
        public string preferredLanguage { get; set; }
        public string surname { get; set; }
        public string userPrincipalName { get; set; }
        public string id { get; set; }


    }
    public class Error
    {
        public string error { get; set; }
        public string error_description { get; set; }
        public List<int> error_codes { get; set; }
        public string timestamp { get; set; }
        public string trace_id { get; set; }
        public string correlation_id { get; set; }
    }
}