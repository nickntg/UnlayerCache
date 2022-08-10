using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Moq;

namespace UnlayerCache.API.Tests
{
    public class MockingHelpers
    {
        public static Mock<HttpRequest> GetMockedHttpRequest()
        {
            var request = new Mock<HttpRequest>();
            request.Setup(x => x.Headers).Returns(() =>
                new HeaderDictionary(new Dictionary<string, StringValues> { { "Authorization", "test" } }));
            return request;
        }

        public static Mock<HttpContext> GetMockedHttpContext(Mock<HttpRequest> mockedRequest)
        {
            var context = new Mock<HttpContext>();
            context.SetupGet(x => x.Request).Returns(mockedRequest.Object);
            return context;
        }

        public static ControllerContext GetControllerContext()
        {
            return new ControllerContext(new ActionContext(GetMockedHttpContext(GetMockedHttpRequest()).Object,
                new RouteData(),
                new ControllerActionDescriptor()));
        }
    }
}
