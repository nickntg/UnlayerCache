using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnlayerCache.API.Services;
using Xunit;

namespace UnlayerCache.API.Tests.Services
{
    public class UnlayerServiceTests
    {
        private const string TestContent = "test {{a}} {{b}} {{c}}";

        [Fact]
        public void ReplacesContent()
        {
            var service = new UnlayerService();

            var o = new ExpandoObject();
            var oHtml = new ExpandoObject();
            oHtml.TryAdd("html", TestContent);
            o.TryAdd("data", oHtml);

            var plain = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(o));

            service.LocalRender(plain,
	            new Dictionary<string, string> { { "a", "b" }, { "c", "d" } });

            Assert.Equal("test b  d", plain?.SelectToken("data.html")?.ToString());
        }

        [Fact]
        public void DoesNotAlterContentWithNullData()
        {
            DoesNotAlter(null);
        }

        [Fact]
        public void DoesNotAlterContentWithEmptyData()
        {
            DoesNotAlter(new Dictionary<string, string>());
        }

        private void DoesNotAlter(Dictionary<string, string> data)
        {
	        var service = new UnlayerService();

	        var o = new ExpandoObject();
	        var oHtml = new ExpandoObject();
	        oHtml.TryAdd("html", TestContent);
	        o.TryAdd("data", oHtml);

			//service.LocalRender(plain, request);

			var plain = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(o));

			service.LocalRender(plain, data);

			Assert.Equal(TestContent, plain?.SelectToken("data.html")?.ToString());
        }
    }
}
