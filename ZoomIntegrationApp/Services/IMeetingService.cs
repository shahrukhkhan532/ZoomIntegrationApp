using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using ZoomIntegrationApp.Models;

namespace ZoomIntegrationApp.Services
{
    public interface IMeetingService
    {
        Task AuthenticationSuccess(string Code);
        Task CreateMeeting();
        Task DeleteMeeting(string ID);
        Task<List<MeetingResponce>> GetAll();
        Task GetMeetingByID(string ID);
        Task<ZoomUser> GetUserDetails();
        Task RefreshToken();
    }
    public class MeetingService : IMeetingService
    {
        #region
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IFiles _filesWriter;
        private readonly IOptions<Zoom> _options;

        public MeetingService(
            IMemoryCache memoryCache, IHttpClientFactory httpClientFactory, IFiles filesWriter,
            IOptions<Zoom> options)
        {
            _memoryCache = memoryCache;
            _httpClientFactory = httpClientFactory;
            _filesWriter = filesWriter;
            _options = options;
        }
        #endregion
        public string AuthorizationHeadewr
        {
            get
            {
                var planTextBytes = Encoding.UTF8.GetBytes($"{_options.Value.ClientID}:{_options.Value.ClientSecret}");
                var encodedString = Convert.ToBase64String(planTextBytes);
                return $"Basic {encodedString}";
            }
        }
        public async Task AuthenticationSuccess(string Code)
        {
            var client = _httpClientFactory.CreateClient();

            var url = string.Format(_options.Value.AccessTokenUrl, Code, _options.Value.RedirectUrl);
            client.DefaultRequestHeaders.Add("Authorization", string.Format(AuthorizationHeadewr));
            var response = await client.PostAsync(url, null);
            string result = await response.Content.ReadAsStringAsync();

            var model = JsonConvert.DeserializeObject<LoginResponce>(result);
            var cacheExpiryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(50),
                Priority = CacheItemPriority.High,
                SlidingExpiration = TimeSpan.FromMinutes(50)
            };
            _memoryCache.Set(_options.Value.CacheKey, model, cacheExpiryOptions);
        }
        public async Task<ZoomUser> GetUserDetails()
        {
            var model = _memoryCache.Get<LoginResponce>(_options.Value.CacheKey);

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", model.access_token));
            var response = await client.GetAsync(_options.Value.UserDetailsURL);
            var result = await response.Content.ReadAsStringAsync();

            ZoomUser? zoomUser = JsonConvert.DeserializeObject<ZoomUser>(result);

            _filesWriter.WriteUserDetails(result);

            var cacheExpiryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(50),
                Priority = CacheItemPriority.High,
                SlidingExpiration = TimeSpan.FromMinutes(50)
            };
            _memoryCache.Set(_options.Value.UserCacheKey, zoomUser, cacheExpiryOptions);
            return zoomUser ?? new ZoomUser();
        }
        public async Task RefreshToken()
        {
            var model = _memoryCache.Get<LoginResponce>(_options.Value.CacheKey);

            var client = _httpClientFactory.CreateClient();

            var query = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = model.refresh_token
            };
            client.DefaultRequestHeaders.Add("Authorization", string.Format(AuthorizationHeadewr));
            var url = QueryHelpers.AddQueryString("https://zoom.us/oauth/token", query);
            var response = await client.PostAsync(url, null);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string result = await response.Content.ReadAsStringAsync();

                model = JsonConvert.DeserializeObject<LoginResponce>(result);
                var cacheExpiryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(50),
                    Priority = CacheItemPriority.High,
                    SlidingExpiration = TimeSpan.FromMinutes(50)
                };
                _memoryCache.Set(_options.Value.CacheKey, model, cacheExpiryOptions);
            }
        }
        public async Task CreateMeeting()
        {
            Meeting meeting = new Meeting()
            {
                Duration = 60,
                Agenda = "This is test meeting description",
                Time = 14,
                Topic = "Test...",
                Date = DateTime.Now.AddDays(1)
            };

            var user = _memoryCache.Get<ZoomUser>(_options.Value.UserCacheKey);
            var model = _memoryCache.Get<LoginResponce>(_options.Value.CacheKey);

            var json = JsonConvert.SerializeObject(meeting);

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", model.access_token));
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var responce = await client.PostAsync(string.Format(_options.Value.MeetingURL, user.id), data);

            string result = await responce.Content.ReadAsStringAsync();

            var model_responce = JsonConvert.DeserializeObject<MeetingResponce>(result);

            _filesWriter.MeetingDetails(result);
        }
        public async Task GetMeetingByID(string ID)
        {
            var model = _memoryCache.Get<LoginResponce>(_options.Value.CacheKey);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", model.access_token));
            var responce = await client.GetAsync($"https://api.zoom.us/v2/meetings/{ID}");
            if(responce.StatusCode == HttpStatusCode.OK)
            {
                var json = await responce.Content.ReadAsStringAsync();
                _filesWriter.MeetingDetails(json);
            }
        }
        public async Task<List<MeetingResponce>> GetAll()
        {
            var model = _memoryCache.Get<LoginResponce>(_options.Value.CacheKey);
            var user = _memoryCache.Get<ZoomUser>(_options.Value.UserCacheKey);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", model.access_token));
            var responce = await client.GetAsync($"https://api.zoom.us/v2/users/{user.id}/meetings");
            if (responce.StatusCode == HttpStatusCode.OK)
            {
                var json = await responce.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<Root>(json);
                _filesWriter.AllMeeting(json);
                return obj.meetings;
            }
            return null;
        }
        public async Task DeleteMeeting(string ID)
        {
            var model = _memoryCache.Get<LoginResponce>(_options.Value.CacheKey);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", model.access_token));
            var responce = await client.DeleteAsync($"https://api.zoom.us/v2/meetings/{ID}");
            if (responce.StatusCode == HttpStatusCode.NoContent)
            {
                var json = await responce.Content.ReadAsStringAsync();
                _filesWriter.DeleteMeeting(json);
            }
        }
    }
}
