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

            var controller = GetController(dynamo, null);

            var result = controller.Get("abc").Result;

            Assert.NotNull(result);

            var r = result as OkObjectResult;
            Assert.NotNull(r);

			A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
				.MustHaveHappenedOnceExactly();

        }

        [Fact]
        public void NotFoundInCache()
        {
	        var dynamo = A.Fake<IDynamoService>(x => x.Strict());
	        A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
		        .Returns(Task.FromResult((string)null));
	        A.CallTo(() => dynamo.SaveUnlayerTemplate(A<UnlayerCacheItem>.Ignored))
		        .Returns(Task.CompletedTask);

            var unlayer = A.Fake<IUnlayerService>(x => x.Strict());
            A.CallTo(() => unlayer.GetTemplate(A<string>.Ignored, A<string>.Ignored))
	            .Returns(Task.FromResult(string.Empty));

            var controller = GetController(dynamo, unlayer);

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
        public void NotFoundInUnlayer()
        {
	        var dynamo = A.Fake<IDynamoService>(x => x.Strict());
	        A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
		        .Returns(Task.FromResult((string)null));

			var unlayer = A.Fake<IUnlayerService>(x => x.Strict());
			A.CallTo(() => unlayer.GetTemplate(A<string>.Ignored, A<string>.Ignored))
				.Returns(Task.FromResult((string)null));

            var controller = GetController(dynamo, unlayer);

            var result = controller.Get("abc").Result;

            Assert.NotNull(result);

            var r = result as NotFoundResult;
            Assert.NotNull(r);

            A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
	            .MustHaveHappenedOnceExactly();
			A.CallTo(() => unlayer.GetTemplate(A<string>.Ignored, A<string>.Ignored))
				.MustHaveHappenedOnceExactly();
		}

        [Fact]
        public void Throws()
        {
	        var dynamo = A.Fake<IDynamoService>(x => x.Strict());
	        A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
		        .Throws<InvalidOperationException>();

            var controller = GetController(dynamo, null);

            var result = controller.Get("abc").Result;

            Assert.NotNull(result);

            var r = result as StatusCodeResult;
            Assert.NotNull(r);
            Assert.Equal(500, r.StatusCode);

			A.CallTo(() => dynamo.GetUnlayerTemplate(A<string>.Ignored))
                .MustHaveHappenedOnceExactly();
		}

        private TemplatesController GetController(IDynamoService dynamo, IUnlayerService unlayer)
        {
            return new TemplatesController(dynamo, unlayer, new NullLogger<TemplatesController>())
            {
                ControllerContext = MockingHelpers.GetControllerContext()
            };
        }
    }
}
