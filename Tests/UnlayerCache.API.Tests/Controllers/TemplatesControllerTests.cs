using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
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
            var dynamoMock = new Mock<IDynamoService>(MockBehavior.Strict);
            dynamoMock.Setup(x => x.GetUnlayerTemplate(It.IsAny<string>()))
                .ReturnsAsync(new UnlayerCacheItem { Value = GetJsonTemplateResponse() }).Verifiable();

            var controller = GetController(dynamoMock.Object, null);

            var result = controller.Get("abc").Result;

            Assert.NotNull(result);

            var r = result as OkObjectResult;
            Assert.NotNull(r);

            var data = r.Value as UnlayerTemplateResponse;
            Assert.NotNull(data);

            dynamoMock.Verify(x => x.GetUnlayerTemplate(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void NotFoundInCache()
        {
            var dynamoMock = new Mock<IDynamoService>(MockBehavior.Strict);
            dynamoMock.Setup(x => x.GetUnlayerTemplate(It.IsAny<string>()))
                .ReturnsAsync((UnlayerCacheItem)null).Verifiable();
            dynamoMock.Setup(x => x.SaveUnlayerTemplate(It.IsAny<UnlayerCacheItem>())).Returns(Task.CompletedTask)
                .Verifiable();

            var unlayerMock = new Mock<IUnlayerService>(MockBehavior.Strict);
            unlayerMock.Setup(x => x.GetTemplate(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new UnlayerTemplateResponse()).Verifiable();

            var controller = GetController(dynamoMock.Object, unlayerMock.Object);

            var result = controller.Get("abc").Result;

            Assert.NotNull(result);

            var r = result as OkObjectResult;
            Assert.NotNull(r);

            var data = r.Value as UnlayerTemplateResponse;
            Assert.NotNull(data);

            dynamoMock.Verify(x => x.GetUnlayerTemplate(It.IsAny<string>()), Times.Once);
            dynamoMock.Verify(x => x.SaveUnlayerTemplate(It.IsAny<UnlayerCacheItem>()), Times.Once);
            unlayerMock.Verify(x => x.GetTemplate(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void NotFoundInUnlayer()
        {
            var dynamoMock = new Mock<IDynamoService>(MockBehavior.Strict);
            dynamoMock.Setup(x => x.GetUnlayerTemplate(It.IsAny<string>()))
                .ReturnsAsync((UnlayerCacheItem)null).Verifiable();

            var unlayerMock = new Mock<IUnlayerService>(MockBehavior.Strict);
            unlayerMock.Setup(x => x.GetTemplate(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((UnlayerTemplateResponse)null).Verifiable();

            var controller = GetController(dynamoMock.Object, unlayerMock.Object);

            var result = controller.Get("abc").Result;

            Assert.NotNull(result);

            var r = result as NotFoundResult;
            Assert.NotNull(r);

            dynamoMock.Verify(x => x.GetUnlayerTemplate(It.IsAny<string>()), Times.Once);
            unlayerMock.Verify(x => x.GetTemplate(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Throws()
        {
            var dynamoMock = new Mock<IDynamoService>(MockBehavior.Strict);
            dynamoMock.Setup(x => x.GetUnlayerTemplate(It.IsAny<string>()))
                .Throws(new InvalidOperationException()).Verifiable();

            var controller = GetController(dynamoMock.Object, null);

            var result = controller.Get("abc").Result;

            Assert.NotNull(result);

            var r = result as StatusCodeResult;
            Assert.NotNull(r);
            Assert.Equal(500, r.StatusCode);

            dynamoMock.Verify(x => x.GetUnlayerTemplate(It.IsAny<string>()), Times.Once);
        }

        private string GetJsonTemplateResponse()
        {
            return JsonConvert.SerializeObject(new UnlayerTemplateResponse());
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
