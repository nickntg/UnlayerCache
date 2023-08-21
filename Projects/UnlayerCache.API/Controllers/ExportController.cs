using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public async Task<IActionResult> Post([FromBody] dynamic request)
        {
            try
            {
                var auth = Request.Headers["Authorization"];
                var o = (JObject)JsonConvert.DeserializeObject(request.ToString());
                var displayMode = FindProperty(o, "displayMode");
                UnlayerMergeTags tags = JsonConvert.DeserializeObject<UnlayerMergeTags>(request.ToString());
                var design = JsonConvert.DeserializeObject(FindProperty(o, "design"));
                var key =
                    $"{auth}_{displayMode}_{Util.Hash.HashString(JsonConvert.SerializeObject(design))}";

                var cached = await _dynamoService.GetUnlayerRender(key);
                if (cached == null)
                {
                    _logger.LogInformation("Going to unlayer to get the clean render for {key}", key);
                    /* We first request Unlayer to render the template without
                     any merge tags of our own. In this way, we'll get a clean
                     response that we can later reuse to do our own replacement. */
                    var cleanRender = await _unlayerService.RenderTemplate(auth, new UnlayerRenderRequest
                    {
	                    design = design,
	                    displayMode = displayMode,
	                    mergeTags = new Dictionary<string, string>()
                    });

                    if (cleanRender == null)
                    {
                        _logger.LogWarning("Unlayer responded with 422");
                        return UnprocessableEntity();
                    }

                    _logger.LogInformation("Saving to cache, key {key}", key);

                    cached = new UnlayerCacheItem { Id = key, Value = cleanRender };

                    await _dynamoService.SaveUnlayerRender(cached);
                }

                _logger.LogInformation("Replacing values in template, key {key}", key);

                var vanilla = (JObject)JsonConvert.DeserializeObject(cached.Value);
                _unlayerService.LocalRender(vanilla, tags.mergeTags);
				return Ok(JsonConvert.DeserializeObject<ExpandoObject>(vanilla.ToString()));
            }
            catch (Exception ex)
            {
	            _logger.LogError("Unexpected exception\r\n{ex}", ex);
                return new StatusCodeResult(500);
            }
        }

        private string FindProperty(JObject o, string propertyName)
        {
	        var p = o.First;
	        while (p != null)
	        {
		        var property = (JProperty)p;
		        if (String.Equals(property.Name, propertyName, StringComparison.CurrentCultureIgnoreCase))
		        {
			        return property.Value.ToString();
		        }

		        p = p.Next;
	        }

	        return null;
        }
	}
}
