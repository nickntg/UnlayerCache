using System;
using System.Collections.Generic;
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
        private readonly IMjmlService    _mjmlService;

        public TemplatesController(IDynamoService dynamoService,
            IUnlayerService unlayerService,
            IMjmlService mjmlService,
            ILogger<TemplatesController> logger)
        {
            _dynamoService = dynamoService;
            _unlayerService = unlayerService;
            _mjmlService = mjmlService;
            _logger = logger;
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            try
            {
	            _logger.LogInformation("Get for {id}", id);

                var auth = Request.Headers["Authorization"];
                var key = $"{auth}_{id}";

                var cached = await _dynamoService.GetUnlayerTemplate(key);
                if (cached != null)
                {
                    _logger.LogInformation("{id} was found in the cache", id);
                    return Ok(JsonConvert.DeserializeObject<ExpandoObject>(cached));
                }

                _logger.LogInformation("{id} not cached, going to unlayer", id);

                string uncached;

                try
                {
                    uncached = await _unlayerService.GetTemplate(auth, id);

                    if (uncached is null)
                    {
                        _logger.LogWarning("{id} not found in Unlayer, trying MJML templates", id);
                        throw new KeyNotFoundException();
                    }
                }
                catch (Exception)
                {
                    var mjmlTemplate = await _mjmlService.GetExpandedTemplate(id);

                    if (mjmlTemplate is null)
                    {
                        _logger.LogWarning("{id} or one of its components not found as MJML", id);
                        return NotFound();
                    }

                    var mocked = new UnlayerTemplateResponseMocked
                    {
                        success = true,
                        data = new DataMocked
                        {
                            id = 0,
                            displayMode = "mjml",
                            design = mjmlTemplate,
                            name = id
                        }
                    };

                    uncached = JsonConvert.SerializeObject(mocked);
                }

                _logger.LogInformation("Saving {id} to cache", id);
                await _dynamoService.SaveUnlayerTemplate(new UnlayerCacheItem
                    { Id = key, Value = uncached });

                return Ok(JsonConvert.DeserializeObject<ExpandoObject>(uncached));
            }
            catch (Exception ex)
            {
	            _logger.LogError("Unexpected exception\r\n{ex}", ex);
                return new StatusCodeResult(500);
            }
        }
    }
}
