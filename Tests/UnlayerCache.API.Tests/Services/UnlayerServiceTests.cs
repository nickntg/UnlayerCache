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
        private const string TestRepeatContent = "test START_BLOCK{{a}} {{b}} {{c}}END_BLOCK";
        private const string TestContentAndRepeatContent = "test {{a}} START_BLOCK{{b}} {{c}} {{d}}END_BLOCK";

        [Theory]
        [InlineData("a", "a1", "b", "b", "d", "d", "test a1 b1  d1b2  d2")]
        [InlineData("", "", "b", "b", "d", "d", "test {{a}} b1  d1b2  d2")]
        public void ReplacesContentAndRepeatContent(string mergeTag, string mergeValue, 
            string firstRepeatMergeTag, string firstRepeatMergeValue, string secondRepeatMergeTag, string secondRepeatMergeValue,
            string expected)
        {
            var service = new UnlayerService();

            var o = new ExpandoObject();
            var oHtml = new ExpandoObject();
            oHtml.TryAdd("html", TestContentAndRepeatContent);
            o.TryAdd("data", oHtml);

            var plain = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(o));

            var mergeTags = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(mergeTag))
            {
                mergeTags.Add(mergeTag, mergeValue);
            }

            var repeatMergeTags = new List<Dictionary<string, string>>();
            for (var i = 1; i <= 2; i++)
            {
                var dic = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(firstRepeatMergeValue))
                {
                    dic.Add(firstRepeatMergeTag, $"{firstRepeatMergeValue}{i}");
                }

                if (!string.IsNullOrEmpty(secondRepeatMergeTag))
                {
                    dic.Add(secondRepeatMergeTag, $"{secondRepeatMergeValue}{i}");
                }

                repeatMergeTags.Add(dic);
            }

            service.LocalRender(plain, repeatMergeTags);
            service.LocalRender(plain, mergeTags);

            Assert.Equal(expected, plain?.SelectToken("data.html")?.ToString());
        }

        [Fact]
        public void ReplacesRepeatContent()
        {
            var service = new UnlayerService();

            var o = new ExpandoObject();
            var oHtml = new ExpandoObject();
            oHtml.TryAdd("html", TestRepeatContent);
            o.TryAdd("data", oHtml);

            var plain = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(o));

            service.LocalRender(plain,
                new List<Dictionary<string, string>>
                {
                    new() { { "a", "b1" }, { "c", "d1" } },
                    new() { { "a", "b2" }, { "c", "d2" } }
                });

            Assert.Equal("test b1  d1b2  d2", plain?.SelectToken("data.html")?.ToString());
        }

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
