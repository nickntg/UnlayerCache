using System;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using UnlayerCache.API.Models;

namespace UnlayerCache.API.Services
{
    public interface IUnlayerService
    {
        Task<UnlayerTemplateResponse> GetTemplate(string authorization, string id);
        Task<UnlayerRenderResponse> RenderTemplate(string authorization, UnlayerRenderRequest request);
        void LocalRender(UnlayerRenderResponse vanilla, UnlayerRenderRequest request);
    }

    public class UnlayerService : IUnlayerService
    {
        private const string UnlayerApiUrl = "https://api.unlayer.com";

        public void LocalRender(UnlayerRenderResponse vanilla, UnlayerRenderRequest request)
        {
            if (request?.mergeTags == null)
            {
                return;
            }

            foreach (var kv in request.mergeTags)
            {
                vanilla.data.html = vanilla.data.html.Replace($"{{{{{kv.Key}}}}}", $"{kv.Value}");
            }
        }

        public async Task<UnlayerRenderResponse> RenderTemplate(string authorization, UnlayerRenderRequest request)
        {
            using (var client = new RestClient(UnlayerApiUrl))
            {

                var r = new RestRequest("/v2/export/html", Method.Post);
                r.AddHeader("Accept", "application/json");
                r.AddHeader("Authorization", authorization);
                r.AddBody(request);

                var response = await client.ExecuteAsync<UnlayerRenderResponse>(r);
                if (response.IsSuccessful)
                {
                    return response.Data;
                }

                if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    return null;
                }

                throw new InvalidOperationException($"Unlayer response failed {response.StatusCode}");
            }
        }

        public async Task<UnlayerTemplateResponse> GetTemplate(string authorization, string id)
        {
            using (var client = new RestClient(UnlayerApiUrl))
            {

                var request = new RestRequest($"/v2/templates/{id}", Method.Get);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", authorization);

                var response = await client.ExecuteAsync<UnlayerTemplateResponse>(request);
                if (response.IsSuccessful)
                {
                    return response.Data;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw new InvalidOperationException($"Unlayer response failed {response.StatusCode}");
            }
        }
    }
}
