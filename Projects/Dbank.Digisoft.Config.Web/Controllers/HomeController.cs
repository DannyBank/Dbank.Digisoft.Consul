using Dbank.Digisoft.Config.Web.Models;
using Dbank.Digisoft.Config.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Dbank.Digisoft.Config.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ConfigClient _client;

        public HomeController(ConfigClient configClient)
        {
            _client = configClient;
        }

        public async Task<IActionResult> Index(string dir = null!)
        {
            if (!string.IsNullOrEmpty(dir) && dir.StartsWith('/')) dir = dir[1..];
            var isFile = !string.IsNullOrEmpty(Path.GetExtension(dir)) && Path.GetExtension(dir).Contains("json");
            var outputList = await _client.GetList(dir) ?? string.Empty;
            var list = new List<(string, string)>();
            var viewList = new List<(string, string)>();
            if (isFile)
                return View(new IndexViewModel
                {
                    Directory = dir ?? "KeyValues",
                    IsFile = isFile,
                    Output = outputList.Trim('"'),
                    ViewList = list
                });
            else
            {
                list = JsonConvert.DeserializeObject<List<(string, string)>?>(outputList) ?? new();
                list!.ForEach(r => { viewList.Add((r.Item1.Replace(".json", ""), _client.ReplaceFirstOccurrence(r.Item2, "/", ""))); });
                return View(new IndexViewModel
                {
                    Directory = dir ?? "KeyValues",
                    IsFile = isFile,
                    Output = string.Empty,
                    ViewList = viewList
                });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}