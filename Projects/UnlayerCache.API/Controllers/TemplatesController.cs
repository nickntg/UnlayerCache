using System;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnlayerCache.API.Models;
using UnlayerCache.API.Services;

namespace UnlayerCache.API.Controllers
{
    [Route("[controller]")]
    public class TemplatesController : ControllerBase
    {
        private readonly ILogger         _logger;
        private readonly IDynamoService  _dynamoService;
        private readonly IUnlayerService _unlayerService;

        public TemplatesController(IDynamoService dynamoService,
            IUnlayerService unlayerService,
            ILogger<TemplatesController> logger)
        {
            _dynamoService = dynamoService;
            _unlayerService = unlayerService;
            _logger = logger;
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            try
            {
                _logger.LogDebug($"Get for {id}");

                var auth = Request.Headers["Authorization"];
                var key = $"{auth}_{id}";

                var cached = await _dynamoService.GetUnlayerTemplate(key);
                if (cached != null)
                {
                    _logger.LogDebug($"{id} was found in the cache");
                    return Ok(JsonConvert.DeserializeObject<ExpandoObject>(cached));
                }

                _logger.LogDebug($"{id} not cached, going to unlayer");
                var uncached = await _unlayerService.GetTemplate(auth, id);

                if (uncached == null)
                {
                    _logger.LogWarning($"{id} not found");
                    return NotFound();
                }

                _logger.LogDebug($"Saving {id} to cache");
                await _dynamoService.SaveUnlayerTemplate(new UnlayerCacheItem
                    { Id = key, Value = uncached });

                return Ok(uncached);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected exception\r\n{ex}");
                return new StatusCodeResult(500);
            }
        }
    }
}
