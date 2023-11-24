using Dbank.Digisoft.Config.Abstractions;
using Dbank.Digisoft.Consul.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Dbank.Digisoft.Config.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KVController : ControllerBase
    {
        private readonly ILogger<KVController> _logger;
        private readonly IKeyValueHelper _kvHelper;

        public KVController(ILogger<KVController> logger, IKeyValueHelper kvHelper)
        {
            _logger = logger;
            _kvHelper = kvHelper;
        }

        [HttpGet("getmerged/{app}/{env}")]
        public async Task<string> GetMerged(string app, string env)
        {
            try
            {
                if (!ModelState.IsValid) return null!;
                var result = await _kvHelper.GetMergedJsonContent(Path.Combine(app, env));
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in endpoint {Method}", nameof(Get));
                return null!;
            }
        }

        [HttpGet("get/{app}/{env}")]
        public async Task<string> Get(string app, string env)
        {
            try
            {
                if (!ModelState.IsValid) return null!;
                var result = await _kvHelper.GetJsonContent(Path.Combine(app, env));
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in endpoint {Method}", nameof(Get));
                return null!;
            }
        }

        [HttpGet("get/json/{app}/{env}")]
        public async Task<string> GetByJson(string app, string env)
        {
            try
            {
                if (!ModelState.IsValid) return null!;
                var result = _kvHelper.GetJsonFiles(Path.Combine(app, env));
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in endpoint {Method}", nameof(Get));
                return null!;
            }
        }

        [HttpGet("get/apps")]
        public async Task<List<string>> GetApps()
        {
            try
            {
                if (!ModelState.IsValid) return null!;
                var dirs = await _kvHelper.GetAppDirectories();
                var directoryNames = new List<string>();
                dirs.ForEach(r => directoryNames.Add(Path.GetFileName(r)));
                return directoryNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in endpoint {Method}", nameof(GetApps));
                return null!;
            }
        }

        [HttpGet("get/app/by/{path}")]
        public async Task<List<string>> GetApps(string path)
        {
            try
            {
                if (!ModelState.IsValid) return null!;
                var dirs = await _kvHelper.GetAppDirectories(path);
                var directoryNames = new List<string>();
                dirs.ForEach(r => directoryNames.Add(Path.GetFileName(r)));
                return directoryNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in endpoint {Method}", nameof(GetApps));
                return null!;
            }
        }

        [HttpPost("[action]")]
        public async Task<string> ScanFolder([FromForm] string dir)
        => await Task.FromResult(_kvHelper.ScanFolder(dir));

        [HttpGet("[action]")]
        public async Task<string?> Get()
        {
            var path = Request.Query;
            var key = path.FirstOrDefault().Key;
            if (key == null)
            {
                var linkList = await _kvHelper.GetDirectoryByPath(string.Empty);
                if (linkList == null) return string.Empty;
                return JsonConvert.SerializeObject(linkList, Formatting.Indented);
            }

            if (_kvHelper.IsValidFile(key))
            {
                if (Path.GetExtension(key) != ".json") return string.Empty;
                var jsonContent = _kvHelper.GetJson(key);
                return Regex.Unescape(jsonContent!);
            }
            else
            {
                var linkList = await _kvHelper.GetDirectoryByPath(key);
                if (linkList == null) return string.Empty;
                return JsonConvert.SerializeObject(linkList, Formatting.Indented);
            }
        }

        [HttpPost("save")]
        public async Task<string?> SaveJson([FromBody] JsonInputModel model)
        {
            return await _kvHelper.UpdateJson(model) ? "Json saved": "Json failed";
        }
    }
}
