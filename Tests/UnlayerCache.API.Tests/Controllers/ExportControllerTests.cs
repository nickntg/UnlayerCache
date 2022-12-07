using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
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
            var dynamoMock = new Mock<IDynamoService>(MockBehavior.Strict);
            dynamoMock.Setup(x => x.GetUnlayerRender(It.IsAny<string>())).ReturnsAsync(new UnlayerCacheItem
                { Value = GetJsonUnlayerCleanRenderResponse() }).Verifiable();

            var controller = GetController(dynamoMock.Object, new UnlayerService());

            var o = new ExpandoObject();
            o.TryAdd("displayMode", "email");
            o.TryAdd("mergeTags", new Dictionary<string, string> { { "a", "replaced" } });
            o.TryAdd("design", "{\"some\": \"design\"}");
            var result = controller.Post(JsonConvert.SerializeObject(o)).Result;

            Assert.NotNull(result);

            var r = result as OkObjectResult;
            Assert.NotNull(r);

            Assert.Contains(ReplacedTestContent, JsonConvert.SerializeObject(r.Value));

            dynamoMock.Verify(x => x.GetUnlayerRender(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void NotFoundInCache()
        {
            var dynamoMock = new Mock<IDynamoService>(MockBehavior.Strict);
            dynamoMock.Setup(x => x.GetUnlayerRender(It.IsAny<string>())).ReturnsAsync((UnlayerCacheItem)null).Verifiable();
            dynamoMock.Setup(x => x.SaveUnlayerRender(It.IsAny<UnlayerCacheItem>())).Returns(Task.CompletedTask).Verifiable();

            var unlayerMock = new Mock<IUnlayerService>(MockBehavior.Strict);
            unlayerMock.Setup(x => x.RenderTemplate(It.IsAny<string>(), It.IsAny<UnlayerRenderRequest>()))
                .ReturnsAsync(GetJsonUnlayerCleanRenderResponse).Verifiable();
            unlayerMock.Setup(x => x.LocalRender(It.IsAny<JObject>(), It.IsAny<Dictionary<string,string>>()))
                .Callback(new Action<JObject, Dictionary<string,string>>((response, request) =>
                    new UnlayerService().LocalRender(response, request))).Verifiable();

            var controller = GetController(dynamoMock.Object, unlayerMock.Object);

            var o = new ExpandoObject();
            o.TryAdd("displayMode", "email");
            o.TryAdd("mergeTags", new Dictionary<string, string> { { "a", "replaced" } });
            o.TryAdd("design", "{\"some\": \"design\"}");
            var result = controller.Post(JsonConvert.SerializeObject(o)).Result;

            Assert.NotNull(result);

            var r = result as OkObjectResult;
            Assert.NotNull(r);

            Assert.Contains(ReplacedTestContent, JsonConvert.SerializeObject(r.Value));

			dynamoMock.Verify(x => x.GetUnlayerRender(It.IsAny<string>()), Times.Once);
            dynamoMock.Verify(x => x.SaveUnlayerRender(It.IsAny<UnlayerCacheItem>()), Times.Once);
            unlayerMock.Verify(x => x.RenderTemplate(It.IsAny<string>(), It.IsAny<UnlayerRenderRequest>()), Times.Once);
            unlayerMock.Verify(x => x.LocalRender(It.IsAny<JObject>(), It.IsAny<Dictionary<string,string>>()), Times.Once);
        }

        [Fact]
        public void NotFoundInCacheAndUnlayerThrows422()
        {
            var dynamoMock = new Mock<IDynamoService>(MockBehavior.Strict);
            dynamoMock.Setup(x => x.GetUnlayerRender(It.IsAny<string>())).ReturnsAsync((UnlayerCacheItem)null).Verifiable();

            var unlayerMock = new Mock<IUnlayerService>(MockBehavior.Strict);
            unlayerMock.Setup(x => x.RenderTemplate(It.IsAny<string>(), It.IsAny<UnlayerRenderRequest>()))
                .ReturnsAsync((string)null).Verifiable();

            var controller = GetController(dynamoMock.Object, unlayerMock.Object);
            
            var o = new ExpandoObject();
            o.TryAdd("displayMode", "email");
            o.TryAdd("mergeTags", new Dictionary<string, string> { { "a", "replaced" } });
            o.TryAdd("design", "{\"some\": \"design\"}");
            var result = controller.Post(JsonConvert.SerializeObject(o)).Result;

			Assert.NotNull(result);

            var r = result as UnprocessableEntityResult;
            Assert.NotNull(r);

            dynamoMock.Verify(x => x.GetUnlayerRender(It.IsAny<string>()), Times.Once);
            unlayerMock.Verify(x => x.RenderTemplate(It.IsAny<string>(), It.IsAny<UnlayerRenderRequest>()), Times.Once);
        }

        [Fact]
        public void ThrowsError()
        {
            var dynamoMock = new Mock<IDynamoService>(MockBehavior.Strict);
            dynamoMock.Setup(x => x.GetUnlayerRender(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());

            var controller = GetController(dynamoMock.Object, null);

            var o = new ExpandoObject();
            o.TryAdd("displayMode", "email");
            o.TryAdd("mergeTags", new Dictionary<string, string> { { "a", "replaced" } });
            o.TryAdd("design", "{\"some\": \"design\"}");
            var result = controller.Post(JsonConvert.SerializeObject(o)).Result;

			Assert.NotNull(result);

            var r = result as StatusCodeResult;
            Assert.NotNull(r);
            Assert.Equal(500, r.StatusCode);

            dynamoMock.Verify(x => x.GetUnlayerRender(It.IsAny<string>()), Times.Once);
        }

        private ExportController GetController(IDynamoService dynamo, IUnlayerService unlayer)
        {
            return new ExportController(dynamo, unlayer, new NullLogger<ExportController>())
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
