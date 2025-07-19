using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using UnlayerCache.API.Controllers;
using UnlayerCache.API.Models;
using UnlayerCache.API.Services;
using Xunit;

namespace UnlayerCache.API.Tests.Controllers
{
    public class TemplatesControllerTests
    {
        [Fact]
        public void FoundInCache()
        {
	        var dynamo = A.Fake<IDynamoService>(x => x.Strict());
	        A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
		        .Returns(Task.FromResult("{\"data\":\"test\"}"));

            var controller = GetController(dynamo, null, null);

            var result = controller.Get("abc").Result;

            Assert.NotNull(result);

            var r = result as OkObjectResult;
            Assert.NotNull(r);

			A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
				.MustHaveHappenedOnceExactly();

        }

        [Fact]
        public void NotFoundInCacheFoundInUnlayer()
        {
	        var dynamo = A.Fake<IDynamoService>(x => x.Strict());
	        A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
		        .Returns(Task.FromResult<string>(null));
	        A.CallTo(() => dynamo.SaveUnlayerTemplate(A<UnlayerCacheItem>.Ignored))
		        .Returns(Task.CompletedTask);

            var unlayer = A.Fake<IUnlayerService>(x => x.Strict());
            A.CallTo(() => unlayer.GetTemplate(A<string>.Ignored, A<string>.Ignored))
	            .Returns(Task.FromResult(string.Empty));

            var controller = GetController(dynamo, unlayer, null);

            var result = controller.Get("abc").Result;

            Assert.NotNull(result);

            var r = result as OkObjectResult;
            Assert.NotNull(r);

            A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
	            .MustHaveHappenedOnceExactly();
            A.CallTo(() => dynamo.SaveUnlayerTemplate(A<UnlayerCacheItem>.Ignored))
	            .MustHaveHappenedOnceExactly();
            A.CallTo(() => unlayer.GetTemplate(A<string>.Ignored, A<string>.Ignored))
	            .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void NotFoundInCacheOrUnlayerOrMjml()
        {
	        var dynamo = A.Fake<IDynamoService>(x => x.Strict());
	        A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
		        .Returns(Task.FromResult<string>(null));

            var mjmlService = A.Fake<IMjmlService>(x => x.Strict());
            A.CallTo(() => mjmlService.GetExpandedTemplate(A<string>.Ignored))
                .Returns(Task.FromResult<string>(null));

			var unlayer = A.Fake<IUnlayerService>(x => x.Strict());
			A.CallTo(() => unlayer.GetTemplate(A<string>.Ignored, A<string>.Ignored))
				.Returns(Task.FromResult<string>(null));

            var controller = GetController(dynamo, unlayer, mjmlService);

            var result = controller.Get("abc").Result;

            Assert.NotNull(result);

            var r = result as NotFoundResult;
            Assert.NotNull(r);

            A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
	            .MustHaveHappenedOnceExactly();
			A.CallTo(() => unlayer.GetTemplate(A<string>.Ignored, A<string>.Ignored))
				.MustHaveHappenedOnceExactly();
            A.CallTo(() => mjmlService.GetExpandedTemplate(A<string>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void NotFoundInCacheOrUnlayerFoundMjml()
        {
            var dynamo = A.Fake<IDynamoService>(x => x.Strict());
            A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
                .Returns(Task.FromResult<string>(null));
            A.CallTo(() => dynamo.SaveUnlayerTemplate(A<UnlayerCacheItem>.Ignored))
                .Returns(Task.CompletedTask);

            var mjmlService = A.Fake<IMjmlService>(x => x.Strict());
            A.CallTo(() => mjmlService.GetExpandedTemplate(A<string>.Ignored))
                .Returns("mjml template");

            var unlayer = A.Fake<IUnlayerService>(x => x.Strict());
            A.CallTo(() => unlayer.GetTemplate(A<string>.Ignored, A<string>.Ignored))
                .Returns(Task.FromResult<string>(null));

            var controller = GetController(dynamo, unlayer, mjmlService);

            var result = controller.Get("abc").Result;

            Assert.NotNull(result);

            var r = result as OkObjectResult;
            Assert.NotNull(r);

            A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => unlayer.GetTemplate(A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => mjmlService.GetExpandedTemplate(A<string>.Ignored))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => dynamo.SaveUnlayerTemplate(A<UnlayerCacheItem>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Throws()
        {
	        var dynamo = A.Fake<IDynamoService>(x => x.Strict());
	        A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
		        .Throws<InvalidOperationException>();

            var controller = GetController(dynamo, null, null);

            var result = controller.Get("abc").Result;

            Assert.NotNull(result);

            var r = result as StatusCodeResult;
            Assert.NotNull(r);
            Assert.Equal(500, r.StatusCode);

			A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
                .MustHaveHappenedOnceExactly();
		}

        private TemplatesController GetController(IDynamoService dynamo, IUnlayerService unlayer, IMjmlService mjmlService)
        {
            return new TemplatesController(dynamo, unlayer, mjmlService, new NullLogger<TemplatesController>())
            {
                ControllerContext = MockingHelpers.GetControllerContext()
            };
        }
    }
}
