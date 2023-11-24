using Dbank.Digisoft.Consul.Models;
using Newtonsoft.Json;
using System.Text;

namespace Dbank.Digisoft.Config.Web.Services
{
    public class ConfigClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ConfigClient> _logger;

        public ConfigClient(HttpClient httpClient, ILogger<ConfigClient> logger,
            IConfiguration confg)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(confg.GetConnectionString("Engine.Config"));
            _logger = logger;
        }

        public async Task<string?> GetList(string dir = null)
        {
            try
            {
                var response = await _httpClient.GetAsync(new Uri($"api/kv/get/?{dir}", UriKind.Relative));
                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred processing {Method}", nameof(GetAllApps));
                return null;
            }
        }

        public async Task<List<string>?> GetAllApps()
        {
            try
            {
                var response = await _httpClient.GetAsync(new Uri("api/kv/get/apps", UriKind.Relative));
                if (!response.IsSuccessStatusCode) return null;
                var httpOut = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<string>?>(httpOut);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred processing {Method}", nameof(GetAllApps));
                return null;
            }
        }

        public async Task<List<string>?> GetByApp(string path)
        {
            try
            {
                var response = await _httpClient.GetAsync(new Uri($"api/kv/get/app/by/{path}", UriKind.Relative));
                if (!response.IsSuccessStatusCode) return null;
                var httpOut = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<string>?>(httpOut);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred processing {Method}", nameof(GetByApp));
                return null;
            }
        }

        public string ReplaceFirstOccurrence(string source, string find, string replace)
        {
            int Place = source.IndexOf(find);
            string result = source.Remove(Place, find.Length).Insert(Place, replace);
            return result;
        }

        public async Task<string?> SaveJson(JsonInputModel json)
        {
            try
            {
                var postData = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(new Uri($"api/kv/save", UriKind.Relative), postData);
                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred processing {Method}", nameof(SaveJson));
                return null;
            }
        }
    }
}
