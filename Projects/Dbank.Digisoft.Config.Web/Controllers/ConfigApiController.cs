using Dbank.Digisoft.Config.Web.Services;
using Dbank.Digisoft.Consul.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dbank.Digisoft.Config.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigApiController : ControllerBase
    {
        private readonly ConfigClient _configClient;

        public ConfigApiController(ConfigClient configClient)
        {
            _configClient = configClient;
        }

        [HttpPost("[action]")]
        public async Task<string?> SaveJson([FromBody] JsonInputModel json)
        {
            if (json == null) return string.Empty;
            return await _configClient.SaveJson(json);
        }
    }
}
