using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnlayerCache.API.Controllers;
using UnlayerCache.API.Models;
using UnlayerCache.API.Services;
using Xunit;

namespace UnlayerCache.API.Tests.Controllers
{
    public class ExportControllerTests
    {
        private const string TestContent         = "test {{a}} {{b}} {{c}}";
        private const string ReplacedTestContent = "test replaced";

        [Fact]
        public void FoundInCache()
        {
	        var dynamo = A.Fake<IDynamoService>(x => x.Strict());
	        A.CallTo(() => dynamo.GetUnlayerRender(A<string>.Ignored))
		        .Returns(Task.FromResult(new UnlayerCacheItem
			        { Value = GetJsonUnlayerCleanRenderResponse() }));

            var controller = GetController(dynamo, new UnlayerService(), null);

            var o = new ExpandoObject();
            o.TryAdd("displayMode", "email");
            o.TryAdd("mergeTags", new Dictionary<string, string> { { "a", "replaced" } });
            o.TryAdd("design", "{\"some\": \"design\"}");
            var result = controller.Post(JsonConvert.SerializeObject(o)).Result;

            Assert.NotNull(result);

            var r = result as OkObjectResult;
            Assert.NotNull(r);

            Assert.Contains(ReplacedTestContent, JsonConvert.SerializeObject(r.Value));

            A.CallTo(() => dynamo.GetUnlayerRender(A<string>.Ignored))
	            .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void NotFoundInCacheUnlayerTemplate()
        {
	        var dynamo = A.Fake<IDynamoService>(x => x.Strict());
	        A.CallTo(() => dynamo.GetUnlayerRender(A<string>.Ignored))
		        .Returns(Task.FromResult((UnlayerCacheItem)null));
	        A.CallTo(() => dynamo.SaveUnlayerRender(A<UnlayerCacheItem>.Ignored))
		        .Returns(Task.CompletedTask);

			var unlayer = A.Fake<IUnlayerService>(x => x.Strict());
			A.CallTo(() => unlayer.RenderTemplate(A<string>.Ignored, A<UnlayerRenderRequest>.Ignored))
				.Returns(Task.FromResult(GetJsonUnlayerCleanRenderResponse()));
			A.CallTo(() => unlayer.LocalRender(A<JObject>.Ignored, A<Dictionary<string, string>>.Ignored))
				.Invokes((JObject response, Dictionary<string, string> request) =>
				{
					new UnlayerService().LocalRender(response, request);
				});
            A.CallTo(() => unlayer.LocalRender(A<JObject>.Ignored, A<List<Dictionary<string, string>>>.Ignored))
                .Invokes((JObject response, List<Dictionary<string, string>> request) =>
                {
                    new UnlayerService().LocalRender(response, request);
                });

            var controller = GetController(dynamo, unlayer, null);

            var o = new ExpandoObject();
            o.TryAdd("displayMode", "email");
            o.TryAdd("mergeTags", new Dictionary<string, string> { { "a", "replaced" } });
            o.TryAdd("design", "{\"some\": \"design\"}");
            var result = controller.Post(JsonConvert.SerializeObject(o)).Result;

            Assert.NotNull(result);

            var r = result as OkObjectResult;
            Assert.NotNull(r);

            Assert.Contains(ReplacedTestContent, JsonConvert.SerializeObject(r.Value));

            A.CallTo(() => dynamo.GetUnlayerRender(A<string>.Ignored))
	            .MustHaveHappenedOnceExactly();
            A.CallTo(() => dynamo.SaveUnlayerRender(A<UnlayerCacheItem>.Ignored))
				.MustHaveHappenedOnceExactly();
			A.CallTo(() => unlayer.RenderTemplate(A<string>.Ignored, A<UnlayerRenderRequest>.Ignored))
				.MustHaveHappenedOnceExactly();
			A.CallTo(() => unlayer.LocalRender(A<JObject>.Ignored, A<Dictionary<string, string>>.Ignored))
				.MustHaveHappenedOnceExactly();
            A.CallTo(() => unlayer.LocalRender(A<JObject>.Ignored, A<List<Dictionary<string, string>>>.Ignored))
                .MustHaveHappenedOnceExactly();

        }

        [Fact]
        public void NotFoundInCacheMjmlTemplate()
        {
            var dynamo = A.Fake<IDynamoService>(x => x.Strict());
            A.CallTo(() => dynamo.GetUnlayerRender(A<string>.Ignored))
                .Returns(Task.FromResult((UnlayerCacheItem)null));
            A.CallTo(() => dynamo.SaveUnlayerRender(A<UnlayerCacheItem>.Ignored))
                .Returns(Task.CompletedTask);

            var unlayer = A.Fake<IUnlayerService>(x => x.Strict());
            A.CallTo(() => unlayer.LocalRender(A<JObject>.Ignored, A<Dictionary<string, string>>.Ignored))
                .Invokes((JObject response, Dictionary<string, string> request) =>
                {
                    new UnlayerService().LocalRender(response, request);
                });
            A.CallTo(() => unlayer.LocalRender(A<JObject>.Ignored, A<List<Dictionary<string, string>>>.Ignored))
                .Invokes((JObject response, List<Dictionary<string, string>> request) =>
                {
                    new UnlayerService().LocalRender(response, request);
                });

            var mjmlService = A.Fake<IMjmlService>(x => x.Strict());
            A.CallTo(() => mjmlService.RenderExpandedTemplate(A<string>.Ignored))
                .Returns(TestContent);

            var controller = GetController(dynamo, unlayer, mjmlService);

            var o = new ExpandoObject();
            o.TryAdd("displayMode", "mjml");
            o.TryAdd("mergeTags", new Dictionary<string, string> { { "a", "replaced" } });
            o.TryAdd("design", "{\"some\": \"design\"}");
            var result = controller.Post(JsonConvert.SerializeObject(o)).Result;

            Assert.NotNull(result);

            var r = result as OkObjectResult;
            Assert.NotNull(r);

            Assert.Contains(ReplacedTestContent, JsonConvert.SerializeObject(r.Value));

            A.CallTo(() => dynamo.GetUnlayerRender(A<string>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => dynamo.SaveUnlayerRender(A<UnlayerCacheItem>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => mjmlService.RenderExpandedTemplate(A<string>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => unlayer.LocalRender(A<JObject>.Ignored, A<Dictionary<string, string>>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => unlayer.LocalRender(A<JObject>.Ignored, A<List<Dictionary<string, string>>>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void NotFoundInCacheAndUnlayerThrows422()
        {
	        var dynamo = A.Fake<IDynamoService>(x => x.Strict());
	        A.CallTo(() => dynamo.GetUnlayerRender(A<string>.Ignored))
		        .Returns(Task.FromResult((UnlayerCacheItem)null));

			var unlayer = A.Fake<IUnlayerService>(x => x.Strict());
			A.CallTo(() => unlayer.RenderTemplate(A<string>.Ignored, A<UnlayerRenderRequest>.Ignored))
				.Returns(Task.FromResult((string)null));

            var controller = GetController(dynamo, unlayer, null);
            
            var o = new ExpandoObject();
            o.TryAdd("displayMode", "email");
            o.TryAdd("mergeTags", new Dictionary<string, string> { { "a", "replaced" } });
            o.TryAdd("design", "{\"some\": \"design\"}");
            var result = controller.Post(JsonConvert.SerializeObject(o)).Result;

			Assert.NotNull(result);

            var r = result as UnprocessableEntityResult;
            Assert.NotNull(r);

            A.CallTo(() => dynamo.GetUnlayerRender(A<string>.Ignored))
				.MustHaveHappenedOnceExactly();
			A.CallTo(() => unlayer.RenderTemplate(A<string>.Ignored, A<UnlayerRenderRequest>.Ignored))
	            .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void ThrowsError()
        {
	        var dynamo = A.Fake<IDynamoService>(x => x.Strict());
	        A.CallTo(() => dynamo.GetUnlayerRender(A<string>.Ignored))
		        .Throws<InvalidOperationException>();

            var controller = GetController(dynamo, null, null);

            var o = new ExpandoObject();
            o.TryAdd("displayMode", "email");
            o.TryAdd("mergeTags", new Dictionary<string, string> { { "a", "replaced" } });
            o.TryAdd("design", "{\"some\": \"design\"}");
            var result = controller.Post(JsonConvert.SerializeObject(o)).Result;

			Assert.NotNull(result);

            var r = result as StatusCodeResult;
            Assert.NotNull(r);
            Assert.Equal(500, r.StatusCode);

			A.CallTo(() => dynamo.GetUnlayerRender(A<string>.Ignored))
				.MustHaveHappenedOnceExactly();
		}

        private ExportController GetController(IDynamoService dynamo, IUnlayerService unlayer, IMjmlService mjmlService)
        {
            return new ExportController(dynamo, unlayer, mjmlService, new NullLogger<ExportController>())
            {
                ControllerContext = MockingHelpers.GetControllerContext()
            };
        }

        private string GetJsonUnlayerCleanRenderResponse()
        {
            return $"{{\"data\":{{\"html\":\"{TestContent}\"}}}}";
        }
    }
}
