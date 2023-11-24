using Dbank.Digisoft.Config.Abstractions;
using Dbank.Digisoft.Consul.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dbank.Digisoft.Config.Serivces
{
    public class KeyValueHelper: IKeyValueHelper
    {
        private readonly ILogger<KeyValueHelper> _logger;
        private readonly IFileHelper _fileHelper;
        private readonly string _kvUrl;

        public KeyValueHelper(ILogger<KeyValueHelper> logger, IFileHelper fileHelper)
        {
            _logger = logger;
            _fileHelper = fileHelper;
            _kvUrl = Path.Combine(Environment.CurrentDirectory, "KeyValues");
        }

        public async Task<List<string>> GetAppDirectories(string path = null!)
        => await Task.FromResult(Directory.GetDirectories(path != null? Path.Combine(_kvUrl, path): _kvUrl).ToList());

        public async Task<List<JObject>?> GetJsonContent(string key)
        {
            try
            {
                var environment = Path.Combine(_kvUrl, key);
                var paths = _fileHelper.GetDirectoriesAndFiles(environment);
                var jObjects = await _fileHelper.GetContents(paths!.DirectoriesAndFiles!);
                return jObjects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred processing {Method}", nameof(GetJsonContent));
                return null;
            }
        }

        public async Task<JObject?> GetMergedJsonContent(string key)
        {
            try
            {
                var environment = Path.Combine(_kvUrl, key);
                var paths = _fileHelper.GetDirectoriesAndFiles(environment);
                var jObjects = await _fileHelper.GetContents(paths!.DirectoriesAndFiles!);
                return MergeJsonObjects(jObjects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred processing {Method}", nameof(GetJsonContent));
                return null;
            }
        }

        public string? GetJson(string key)
        {
            try
            {
                key = key.Replace("/", "\\");
                var environment = Path.Combine(_kvUrl, key);
                var json = _fileHelper.LoadJsonAsString(environment);
                return JsonConvert.SerializeObject(json, Formatting.Indented);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred processing {Method}", nameof(GetJson));
                return null;
            }
        }

        public Dictionary<string, string> GetJsonFiles(string key)
        {
            try
            {
                var environment = Path.Combine(_kvUrl, key);
                var paths = _fileHelper.GetDirectoriesAndFiles(environment);
                return paths.DirectoriesAndFiles!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred processing {Method}", nameof(GetJsonFiles));
                return new();
            }
        }

        private JObject MergeJsonObjects(List<JObject> objects)
        {
            try
            {
                if (objects == null || objects.Count == 0) return new();
                JObject json = new();
                foreach (JObject JSONObject in objects)
                {
                    foreach (var property in JSONObject)
                    {
                        string name = property.Key;
                        JToken value = property.Value;
                        json.Add(name, value);
                    }
                }
                return json;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred processing {Method}", nameof(MergeJsonObjects));
                return null!;
            }
        }

        public string ScanFolder(string directory)
        => _fileHelper.ScanFolder(
                new DirectoryInfo(Path.Combine(_kvUrl, directory ?? string.Empty)));

        public async Task<List<(string? LinkName, string? LinkHref)>?> GetDirectoryByPath(string key)
        {
            key = string.IsNullOrEmpty(key) ? _kvUrl : Path.Combine(_kvUrl, key);
            var subDirectories = Directory.GetDirectories(key).ToList();
            var subFiles = Directory.GetFiles(key, "*.json").ToList();
            var directoryContents = new List<(string? LinkName, string? LinkHref)>();
            subDirectories.ForEach(r => { directoryContents.Add((Path.GetFileName(r), r.Replace(_kvUrl, "").Replace("\\","/")));});
            subFiles.ForEach(r => { directoryContents.Add((Path.GetFileName(r), r.Replace(_kvUrl, "").Replace("\\", "/")));});
            return await Task.FromResult(directoryContents);
        }

        public bool IsValidFile(string key) => File.Exists(Path.Combine(_kvUrl, key));

        public async Task<bool> UpdateJson(JsonInputModel model)
        {
            var path = Path.Combine(_kvUrl, model.Path.Replace("/", "\\"));
            return await Task.FromResult(_fileHelper.UpdateFile(path, model.Value));
        }
    }
}
