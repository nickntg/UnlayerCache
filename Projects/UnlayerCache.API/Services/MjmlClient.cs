using System.Threading.Tasks;
using RestSharp;
using UnlayerCache.API.Exceptions;
using UnlayerCache.API.Models;

namespace UnlayerCache.API.Services
{
    public interface IMjmlClient
    {
        Task<string> RenderTemplate(string mjml);
    }

    public class MjmlClient : IMjmlClient
    {
        private readonly AppSettings _settings;
        public MjmlClient(AppSettings settings)
        {
            _settings = settings;
        }

        public async Task<string> RenderTemplate(string mjml)
        {
            using (var client = new RestClient(_settings.MjmlServer))
            {
                var request = new RestRequest("/render", Method.Post);
                request.AddBody(mjml);
                request.AddHeader("Content-Type", "text/plain");

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return response.Content;
                }

                throw new MjmlException($"Error rendering MJML: {response.ErrorMessage ?? response.Content}");
            }
        }
    }
}
