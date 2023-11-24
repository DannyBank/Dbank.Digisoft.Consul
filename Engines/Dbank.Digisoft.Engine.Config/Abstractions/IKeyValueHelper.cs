using Dbank.Digisoft.Consul.Models;
using Newtonsoft.Json.Linq;

namespace Dbank.Digisoft.Config.Abstractions
{
    public interface IKeyValueHelper
    {
        Task<List<string>> GetAppDirectories(string path = null!);
        Task<List<(string? LinkName, string? LinkHref)>?> GetDirectoryByPath(string key);
        Task<List<JObject>?> GetJsonContent(string key);
        Task<JObject?> GetMergedJsonContent(string key);
        string? GetJson(string key);
        Dictionary<string, string> GetJsonFiles(string key);
        bool IsValidFile(string key);
        string ScanFolder(string directory);
        Task<bool> UpdateJson(JsonInputModel model);
    }
}
