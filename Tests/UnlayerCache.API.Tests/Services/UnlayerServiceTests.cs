using System.Collections.Generic;
using UnlayerCache.API.Models;
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
            var request = new UnlayerRenderRequest
                { mergeTags = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } } };

            var plain = new UnlayerRenderResponse { data = new Data2 { html = TestContent } };

            var service = new UnlayerService();

            service.LocalRender(plain, request);

            Assert.Equal("test b {{b}} d", plain.data.html);
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
            var request = new UnlayerRenderRequest { mergeTags = data };

            var plain = new UnlayerRenderResponse { data = new Data2 { html = TestContent } };

            var service = new UnlayerService();

            service.LocalRender(plain, request);

            Assert.Equal(TestContent, plain.data.html);
        }
    }
}
