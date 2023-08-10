using System;
using System.Net;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace spot
{
    public class TokenManager
    {
        static HttpListener listener = new HttpListener();
        public static string Token = " ";
        static string RefreshToken = " ";
        public static DateTime ExpireTime; 
        public static void GetToken()
        {
            var (verifier, challenge) = PKCEUtil.GenerateCodes(120);
            var loginRequest = new LoginRequest(
                new Uri("http://localhost:5000/callback"),
                Program.ClientID,
                LoginRequest.ResponseType.Code
            )
            {
                CodeChallengeMethod = "S256",
                CodeChallenge = challenge,
                Scope = new[] {
                    Scopes.UserModifyPlaybackState,
                    Scopes.UserReadCurrentlyPlaying,
                    Scopes.UserReadPlaybackState,
                    Scopes.PlaylistModifyPrivate,
                    Scopes.PlaylistModifyPublic,
                }
            };
            var Uri = loginRequest.ToUri();
            BrowserUtil.Open(Uri);
            listener.Prefixes.Add("http://localhost:5000/");
            listener.Start();

            bool RunServer = true;
            string? code;
            while (RunServer == true)
            {
                HttpListenerContext ctx = listener.GetContext();
                HttpListenerRequest req = ctx.Request;
                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/callback"))
                {
                    code = req.Url.AbsoluteUri;
                    code = code.Substring(code.IndexOf("=")+1);
                    RunServer = false;
                    GetCallback(code, verifier).Wait();
                }
            }
            listener.Close();

        }

        ///Gets the response from the web server
        private static async Task GetCallback(string code,string verifier)//Gets the token using the code and ClientID
        {
            var initialResponse = await new OAuthClient().RequestToken(
            new PKCETokenRequest(Program.ClientID, code, new Uri("http://localhost:5000/callback"), verifier)
            );
            Token = initialResponse.AccessToken;
            RefreshToken = initialResponse.RefreshToken;

            ExpireTime = DateTime.Now.AddSeconds(3000);
            File.WriteAllText(Program.TF2Directory+"tf2consoleoutput.txt", "");
            
            Program.Start();
        }

        ///Refreshes the token
        public static async Task RefreshTheToken()
        {
            var newResponse = await new OAuthClient().RequestToken(
            new PKCETokenRefreshRequest(Program.ClientID, RefreshToken)
            );
            Token = newResponse.AccessToken;
            RefreshToken = newResponse.RefreshToken;
            ExpireTime = DateTime.Now.AddSeconds(3000);
            SpotifyManager.Init();

            File.WriteAllText(Program.TF2Directory+"tf2consoleoutput.txt", "");
        }
    }
}