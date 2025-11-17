using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using UnlayerCache.API.Models;

namespace UnlayerCache.API.Services
{
    public interface IMjmlService
    {
        Task<MjmlTemplate> CreateTemplate(MjmlTemplate template);
        Task<MjmlTemplate> UpdateTemplate(MjmlTemplate template);
        Task<MjmlTemplate> GetTemplate(string id);
        Task<string> GetExpandedTemplate(string id);
        Task DeleteTemplate(string id);
        Task<IList<MjmlTemplate>> ListTemplates();
        Task<string> RenderTemplate(string id);
        Task<string> RenderExpandedTemplate(string templateBody);
    }
    public class MjmlService : IMjmlService
    {
        private readonly IMjmlClient _mjmlClient;
        private readonly IDynamoService _dynamoService;

        public MjmlService(IMjmlClient mjmlClient, IDynamoService dynamoService)
        {
            _mjmlClient = mjmlClient;
            _dynamoService = dynamoService;
        }

        public async Task<MjmlTemplate> CreateTemplate(MjmlTemplate template)
        {
            return await _dynamoService.SaveMjmlTemplate(template);
        }

        public async Task<MjmlTemplate> UpdateTemplate(MjmlTemplate template)
        {
            return await _dynamoService.UpdateMjmlTemplate(template);
        }

        public async Task<MjmlTemplate> GetTemplate(string id)
        {
            return await _dynamoService.GetMjmlTemplate(HttpUtility.UrlDecode(id));
        }

        public async Task DeleteTemplate(string id)
        {
            await _dynamoService.DeleteMjmlTemplate(HttpUtility.UrlDecode(id));
        }

        public async Task<IList<MjmlTemplate>> ListTemplates()
        {
            return await _dynamoService.ListMjmlTemplates();
        }

        public async Task<string> GetExpandedTemplate(string id)
        {
            try
            {
                return await ExpandMjmlIncludes(id);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public async Task<string> RenderTemplate(string id)
        {
            try
            {
                var completeMjml = await ExpandMjmlIncludes(id);

                return await _mjmlClient.RenderTemplate(completeMjml);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public async Task<string> RenderExpandedTemplate(string templateBody)
        {
            try
            {
                return await _mjmlClient.RenderTemplate(templateBody);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        private async Task<string> ExpandMjmlIncludes(string id)
        {
            var template = await GetTemplate(id);
            if (template is null)
            {
                throw new KeyNotFoundException($"MJML template with id {id} was not found");
            }

            var includeRegex = new Regex(@"<mj-include\s+path=""([^""]+)""\s*/?>", RegexOptions.IgnoreCase);
            
            return includeRegex.Replace(template.Body, matches =>
            {
                var included = GetTemplate(matches.Groups[1].Value).Result;
                if (included is null)
                {
                    throw new KeyNotFoundException($"MJML template with id {matches.Groups[1].Value} was not found");
                }

                return ExpandMjmlIncludes(matches.Groups[1].Value).Result;
            });
        }
    }
}
