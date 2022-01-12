using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ZoomIntegrationApp.Models;

namespace ZoomIntegrationApp.Services
{
    public interface IMeetingService
    {
        Task<ZoomUser> GetUserDetails();
    }
    public class MeetingService : IMeetingService
    {
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
        public async Task<ZoomUser> GetUserDetails()
        {
            var model = _memoryCache.Get<LoginResponce>(_options.Value.CacheKey);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", model.access_token));
            var response = await client.GetAsync("https://api.zoom.us/v2/users/me");
            var result = await response.Content.ReadAsStringAsync();
            ZoomUser zoomUser = JsonConvert.DeserializeObject<ZoomUser>(result);
            _filesWriter.WriteUserDetails(result);
            return zoomUser;
        }
    }
}
