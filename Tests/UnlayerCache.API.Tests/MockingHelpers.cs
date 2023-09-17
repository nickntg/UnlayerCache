using System.Collections.Generic;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace UnlayerCache.API.Tests
{
    public class MockingHelpers
    {
        public static HttpRequest GetMockedHttpRequest()
        {
	        var request = A.Fake<HttpRequest>();
            A.CallTo(() => request.Headers)
	            .Returns(new HeaderDictionary(new Dictionary<string, StringValues> { { "Authorization", "test" } }));
            return request;
        }

        public static HttpContext GetMockedHttpContext(HttpRequest request)
        {
	        var context = A.Fake<HttpContext>();
	        A.CallTo(() => context.Request)
		        .Returns(request);
            return context;
        }

        public static ControllerContext GetControllerContext()
        {
            return new ControllerContext(new ActionContext(GetMockedHttpContext(GetMockedHttpRequest()),
                new RouteData(),
                new ControllerActionDescriptor()));
        }
    }
}
