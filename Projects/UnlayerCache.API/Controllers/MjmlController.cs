using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UnlayerCache.API.Models;
using UnlayerCache.API.Services;

namespace UnlayerCache.API.Controllers
{
    [Route("[controller]")]
    public class MjmlController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMjmlService _mjmlService;

        public MjmlController(IMjmlService mjmlService,
            ILogger<TemplatesController> logger)
        {
            _mjmlService = mjmlService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("Listing all MJML templates");

            return Ok(await _mjmlService.ListTemplates());
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            _logger.LogInformation("Getting MJML template with ID {id}", id);

            var template = await _mjmlService.GetTemplate(id);

            if (template is null)
            {
                return NotFound();
            }

            return Ok(template);
        }

        [HttpGet]
        [Route("{id}/render")]
        public async Task<IActionResult> Render([FromRoute] string id)
        {
            _logger.LogInformation("Rendering MJML template with ID {id}", id);

            var rendered = await _mjmlService.RenderTemplate(id);

            if (rendered is null)
            {
                return NotFound();
            }

            return Ok(rendered);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            _logger.LogInformation("Deleting MJML template with ID {id}", id);

            await _mjmlService.DeleteTemplate(id);

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MjmlTemplate template)
        {
            _logger.LogInformation("Creating new MJML template");

            var createdTemplate = await _mjmlService.CreateTemplate(template);

            return CreatedAtAction(nameof(Get), new { id = createdTemplate.Id }, createdTemplate);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] MjmlTemplate template)
        {
            _logger.LogInformation("Updating MJML template with ID {id}", id);

            if (template.Id != HttpUtility.UrlDecode(id))
            {
                return BadRequest("Template ID mismatch");
            }

            return Ok(await _mjmlService.UpdateTemplate(template));
        }
    }
}
