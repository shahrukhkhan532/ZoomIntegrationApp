using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using ZoomIntegrationApp.Models;
using ZoomIntegrationApp.Services;

namespace ZoomIntegrationApp.Controllers
{
    public class MeetingController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<Zoom> _options;
        private readonly IMemoryCache _memoryCache;
        private readonly IMeetingService _meetingService;

        public MeetingController(
            IWebHostEnvironment environment,
            IHttpClientFactory httpClientFactory,
            IOptions<Zoom> options,
            IMemoryCache memoryCache,
            IMeetingService meetingService)
        {
            _environment = environment;
            _httpClientFactory = httpClientFactory;
            _options = options;
            _memoryCache = memoryCache;
            _meetingService = meetingService;
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
            var Url = string.Format(_options.Value.AuthorizationUrl, _options.Value.ClientID, _options.Value.RedirectUrl);
            return Redirect(Url);
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
            var cacheExpiryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(50),
                Priority = CacheItemPriority.High,
                SlidingExpiration = TimeSpan.FromMinutes(50)
            };
            _memoryCache.Set("cacheKey", model, cacheExpiryOptions);
            return RedirectToAction("Index");
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetUserDetails()
        {
            var user = await _meetingService.GetUserDetails();
            return View(user);
        }
    }
}
