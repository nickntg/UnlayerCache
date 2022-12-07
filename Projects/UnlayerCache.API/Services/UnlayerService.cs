using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using UnlayerCache.API.Models;

namespace UnlayerCache.API.Services
{
    public interface IUnlayerService
    {
        Task<string> GetTemplate(string authorization, string id);
        Task<string> RenderTemplate(string authorization, UnlayerRenderRequest request);
        void LocalRender(JObject vanilla, Dictionary<string, string> mergeTags);
    }

    public class UnlayerService : IUnlayerService
    {
        private const string UnlayerApiUrl = "https://api.unlayer.com";

        public void LocalRender(JObject vanilla, Dictionary<string, string> mergeTags)
        {
	        if (mergeTags == null || mergeTags.Keys.Count == 0)
	        {
		        return;
	        }

	        var html = vanilla?.SelectToken("data.html")?.ToString();

	        var r = new Regex("\\{{(.*?)\\}}");
	        var matches = r.Matches(html);
	        foreach (var m in matches)
	        {
		        var variable = m.ToString();
		        if (!string.IsNullOrEmpty(variable))
				{
					variable = variable.Replace("{{", string.Empty).Replace("}}", string.Empty);
					if (!mergeTags.ContainsKey(variable))
					{
						mergeTags.Add(variable, string.Empty);
					}
				}
	        }

	        html = mergeTags.Aggregate(html, (current, kv) => current.Replace($"{{{{{kv.Key}}}}}", $"{kv.Value}"));

	        ((JValue)vanilla?.SelectToken("data.html")).Value = html;
        }

        public async Task<string> RenderTemplate(string authorization, UnlayerRenderRequest request)
        {
            using (var client = new RestClient(UnlayerApiUrl))
            {

                var r = new RestRequest("/v2/export/html", Method.Post);
                r.AddHeader("Accept", "application/json");
                r.AddHeader("Authorization", authorization);
                r.AddBody(JsonConvert.SerializeObject(request));

                var response = await client.ExecuteAsync(r);
                if (response.IsSuccessful)
                {
                    return response.Content;
                }

                if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                {
                    return null;
                }

                throw new InvalidOperationException($"Unlayer response failed {response.StatusCode}");
            }
        }

        public async Task<string> GetTemplate(string authorization, string id)
        {
            using (var client = new RestClient(UnlayerApiUrl))
            {

                var request = new RestRequest($"/v2/templates/{id}", Method.Get);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", authorization);

                var response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    return response.Content;
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
