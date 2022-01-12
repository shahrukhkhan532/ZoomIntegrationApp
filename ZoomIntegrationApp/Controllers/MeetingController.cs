using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using ZoomIntegrationApp.Models;

namespace ZoomIntegrationApp.Controllers
{
    public class MeetingController : Controller
    {
        private readonly IConfiguration _iConfig;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<Zoom> _options;

        public MeetingController(
            IConfiguration iConfig,
            IWebHostEnvironment environment,
            IHttpClientFactory httpClientFactory,
            IOptions<Zoom> options)
        {
            _iConfig = iConfig;
            _environment = environment;
            _httpClientFactory = httpClientFactory;
            _options = options;
        }
        public string AuthorizationHeadewr
        {
            get
            {
                var planTextBytes = Encoding.UTF8.GetBytes($"{_options.Value.ClientID}:{_options.Value.ClientSecret}");
                var encodedString = Convert.ToBase64String(planTextBytes);
                return $"Basic {encodedString}";
            }
        }
        public IActionResult SignIn()
        {
            var saf = string.Format(_options.Value.AuthorizationUrl, _options.Value.ClientID, _options.Value.RedirectUrl);
            return Redirect(saf);
        }
        public async Task<IActionResult> zoom(string code)
        {
            var client = _httpClientFactory.CreateClient();
            var url = string.Format(_options.Value.AccessTokenUrl, code, _options.Value.RedirectUrl);
            client.DefaultRequestHeaders.Add("Authorization", string.Format(AuthorizationHeadewr));
            var response = await client.PostAsync(url, null);

            string result = await response.Content.ReadAsStringAsync();

            var model = JsonConvert.DeserializeObject<LoginResponce>(result);

            string directoryPath = _environment.WebRootPath + "\\sitemaps\\";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string filepath = directoryPath + "\\appsettings.json";
            System.IO.File.WriteAllText(filepath, JsonConvert.SerializeObject(model, Formatting.Indented));


            await GetUserDetails(model.access_token);
            return RedirectToAction("Index");
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task GetUserDetails(string accessToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken));
            var response = await client.GetAsync("https://api.zoom.us/v2/users/me");
            var resulrt = await response.Content.ReadAsStringAsync();

            string directoryPath = _environment.WebRootPath + "\\sitemaps\\";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string filepath = directoryPath + "\\usersDetails.json";
            System.IO.File.WriteAllText(filepath, resulrt);
        }
    }
}
