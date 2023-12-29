using Microsoft.AspNetCore.Mvc;
using OAuthImplementation.Views.SignIn;
using System.Net.Http;
using Newtonsoft.Json;
namespace OAuthImplementation.Controllers
{
    public class SignInController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly  string? _clientId;
        private readonly string? _clientSecret;
        private readonly string? _redirectUri;

        public SignInController(IConfiguration configuration)
        {
            _configuration = configuration;
            _clientId = _configuration["GoogleCredentials:ClientId"];
            _clientSecret = _configuration["GoogleCredentials:ClientSecret"];
            _redirectUri = _configuration["GoogleCredentials:RedirectUri"];
        }

        public  async Task<IActionResult> Index()
        {
            string token = HttpContext.Session.GetString("User");
            if (token == "" || token == null)
            {
                return View();
            }
            else
            {
                return View("Mails",await GetLatestEmailsAsync(token));
            }
        }

        //public async Task<List<Email>> GetUserProfileAsync(string accessToken)
        //{
        //    HttpClient client = new HttpClient
        //    {
        //        BaseAddress = new Uri($"https://www.googleapis.com"),
        //    };
        //    string url = $"https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token={accessToken}";
        //    var response = await client.GetAsync(url);
        //    var result= await GetLatestEmailsAsync(accessToken);
        //    return result;

        //}

        public async Task<List<Email>> GetLatestEmailsAsync(string accessToken)
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri("https://www.googleapis.com/gmail/v1/"),
                DefaultRequestHeaders = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken) }
            };
            string userId = "me"; // 'me' refers to the authenticated user
            string url = $"users/{userId}/messages?maxResults=5"; // Fetching 5 emails

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var emailListResponse = JsonConvert.DeserializeObject<EmailListResponse>(content);

                List<Email> emails = new List<Email>();

                foreach (var message in emailListResponse.Messages)
                {
                    var email = await GetEmailDetailsAsync(client, userId, message.Id);
                    emails.Add(email);
                }

                return emails;
            }
            return null;
        }

        public async Task<Email> GetEmailDetailsAsync(HttpClient client, string userId, string emailId)
        {
            string url = $"users/{userId}/messages/{emailId}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var email = JsonConvert.DeserializeObject<Email>(content);
                return email;
            }
            else
            {
                // Handle error in fetching email details
                return null;
            }
        }

        public void LoginUsingGoogle()
        {
            Response.Redirect($"https://accounts.google.com/o/oauth2/v2/auth?client_id={_clientId}&response_type=code&scope=https://mail.google.com/&redirect_uri={_redirectUri}&state=abcdef");
        }

        public async Task<ActionResult> SaveGoogleUser(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return View("Error");
            }
            var client = new HttpClient
            {
                BaseAddress = new Uri($"https://www.googleapis.com")
            };
            var requestUrl = $"oauth2/v4/token?code={code}&client_id={_clientId}&client_secret={_clientSecret}&redirect_uri={_redirectUri}&grant_type=authorization_code";
            var dict = new Dictionary<string, string>
            {
                { "Content-Type", "application/x-www-form-urlencoded" }
            };
            var req = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new FormUrlEncodedContent(dict)
            };
            var response = await client.SendAsync(req);
            var token = JsonConvert.DeserializeObject<Token>(await response.Content.ReadAsStringAsync());
            HttpContext.Session.SetString("User", token.AccessToken);
            return View("Mails", await GetLatestEmailsAsync(token.AccessToken));

        }

        public void SignOut()
        {
            HttpContext.Session.SetString("User","") ;
        }

    }
}
