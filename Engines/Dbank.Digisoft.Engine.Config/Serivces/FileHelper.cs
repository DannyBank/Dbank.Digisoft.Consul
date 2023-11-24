using Dbank.Digisoft.Config.Abstractions;
using Dbank.Digisoft.Config.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Dbank.Digisoft.Config.Serivces
{
    public class FileHelper: IFileHelper
    {
        private readonly AppSettings _settings;
        private readonly ILogger<FileHelper> _logger;

        public FileHelper(IOptionsSnapshot<AppSettings> settings,
            ILogger<FileHelper> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public (bool, DirectoryInfo?) CreateDirectory(string path, string dirName)
        {
            var directory = Path.Combine(path, dirName);
            if (!Directory.Exists(directory))
                return (false, null);
            if (directory != string.Empty)
            {
                var info = Directory.CreateDirectory(directory);
                return (true, info);
            }
            return (false, null);
        }

        public bool RenameDirectory(string path, string dirName, string newName)
        {
            var oldDirectory = Path.Combine(path, dirName);
            var newDirectory = Path.Combine(path, newName);
            if (Directory.Exists(oldDirectory))
            {
                if (newDirectory != string.Empty)
                {
                    Directory.Move(oldDirectory, newDirectory);
                    return Directory.Exists(newDirectory);
                }
                return false;
            }
            return false;
        }

        public bool DeleteDirectory(string path, string dirName)
        {
            var directory = Path.Combine(path, dirName);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory);
                return !Directory.Exists(directory);
            }
            return false;
        }

        public bool RenameFile(string path, string fileName, string newFilename)
        {
            var oldDirectory = Path.Combine(path, fileName);
            var newDirectory = Path.Combine(path, newFilename);
            if (File.Exists(oldDirectory))
            {
                if (newDirectory != string.Empty)
                {
                    File.Move(oldDirectory, newDirectory);
                    return File.Exists(newDirectory);
                }
                return false;
            }
            return false;
        }

        public bool DeleteFile(string path, string fileName)
        {
            var directory = Path.Combine(path, fileName);
            if (File.Exists(directory))
            {
                File.Delete(directory);
                return !File.Exists(directory);
            }
            return false;
        }

        public bool UpdateFile(string path, string contents)
        {
            try
            {
                if (!File.Exists(path)) return false;
                File.WriteAllText(path, contents.Trim());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred processing {Method}", nameof(UpdateFile));
                return false;
            }
        }

        public async Task<List<JObject>?> GetContents(Dictionary<string, string>? filePaths)
        {
            if (filePaths == null || filePaths.Count == 0) return null;

            var jsonList = new List<JObject>();
            foreach (var path in filePaths)
            {
                var keyValues = await GetKeyValuesFromPath(path.Value);
                if (keyValues == null || string.IsNullOrWhiteSpace(keyValues))
                    continue;
                var kvJson = JObject.Parse(keyValues);
                jsonList.Add(kvJson);
            }
            return jsonList;
        }

        public async Task<string?> GetKeyValuesFromPath(string path)
        {
            try
            {
                using StreamReader r = new (path);
                return await r.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred processing {Method}", nameof(GetKeyValuesFromPath));
                return string.Empty;
            }
        }

        public (bool Success, string? Type, Dictionary<string, string>? DirectoriesAndFiles) GetDirectoriesAndFiles(
            string environment)
        {
            if (File.Exists(environment))
                return GetJsonFileContents(environment);
            else if (Directory.Exists(environment))
                return GetDirectoryContents(environment);
            else
                return (false, null, null);
        }

        private (bool, string?, Dictionary<string, string>?) GetDirectoryContents(string path)
        {
            var extension = Path.GetExtension(path);
            extension = (string.IsNullOrEmpty(extension))? "json": extension;
            if (!_settings.AllowedExtensions.Contains(extension))
                return (false, null, null);
            var filePaths = Directory.GetFiles(path, $"*.{extension}", SearchOption.AllDirectories);
            var dict = new Dictionary<string, string>();
            filePaths.ToList().ForEach(r => { dict.Add(Path.GetFileName(r), r); });
            return (filePaths.Any(), "dir", dict);
        }

        private static (bool, string?, Dictionary<string, string>?) GetJsonFileContents(string path)
        {
            var contents = LoadJson(path);
            if (contents!.Count <= 0) 
                return (false, null!, null);
            return (true, "file", contents);
        }

        private static Dictionary<string, string>? LoadJson(string path)
        {
            using StreamReader r = new (path);
            string json = r.ReadToEnd();
            return string.IsNullOrWhiteSpace(json) ? 
                new Dictionary<string, string>() :
                JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public string? LoadJsonAsString(string path)
        {
            using StreamReader r = new (path);
            return r.ReadToEnd();
        }

        public string ScanFolder(DirectoryInfo directory, string indentation = "\t", int maxLevel = -1, int deep = 0)
        {
            StringBuilder builder = new ();
            builder.AppendLine(string.Concat(Enumerable.Repeat(indentation, deep)) + directory.Name);

            if (maxLevel == -1 || deep < maxLevel)            
                foreach (var subdirectory in directory.GetDirectories())
                    builder.Append(ScanFolder(subdirectory, indentation, maxLevel, deep + 1));            

            foreach (var file in directory.GetFiles())
                builder.AppendLine(string.Concat(Enumerable.Repeat(indentation, deep + 1)) + file.Name);

            return builder.ToString();
        }
    }
}
