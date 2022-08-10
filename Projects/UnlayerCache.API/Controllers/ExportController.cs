using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnlayerCache.API.Models;
using UnlayerCache.API.Services;

namespace UnlayerCache.API.Controllers
{
    [Route("[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly ILogger         _logger;
        private readonly IDynamoService  _dynamoService;
        private readonly IUnlayerService _unlayerService;

        public ExportController(IDynamoService dynamoService,
            IUnlayerService unlayerService,
            ILogger<ExportController> logger)
        {
            _dynamoService = dynamoService;
            _unlayerService = unlayerService;
            _logger = logger;
        }

        [Route("html")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UnlayerRenderRequest request)
        {
            try
            {
                var auth = Request.Headers["Authorization"];
                var key =
                    $"{auth}_{request.displayMode}_{Util.Hash.HashString(JsonConvert.SerializeObject(request.design))}";

                var cached = await _dynamoService.GetUnlayerRender(key);
                if (cached == null)
                {
                    _logger.LogDebug("Going to unlayer to get the clean render");

                    /* We first request Unlayer to render the template without
                     any merge tags of our own. In this way, we'll get a clean
                     response that we can later reuse to do our own replacement. */
                    var cleanRender = await _unlayerService.RenderTemplate(auth, new UnlayerRenderRequest
                    {
                        design = request.design,
                        displayMode = request.displayMode,
                        mergeTags = new Dictionary<string, string>()
                    });

                    if (cleanRender == null)
                    {
                        _logger.LogWarning("Unlayer responded with 422");
                        return UnprocessableEntity();
                    }

                    _logger.LogDebug("Saving to cache");

                    cached = new UnlayerCacheItem { Value = JsonConvert.SerializeObject(cleanRender) };

                    await _dynamoService.SaveUnlayerRender(new UnlayerCacheItem
                    {
                        Id = key,
                        Value = cached.Value
                    });
                }

                _logger.LogDebug("Replacing values in template");

                var vanilla = JsonConvert.DeserializeObject<UnlayerRenderResponse>(cached.Value);
                _unlayerService.LocalRender(vanilla, request);

                return Ok(vanilla);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected exception\r\n{ex}");
                return new StatusCodeResult(500);
            }
        }
    }
}
