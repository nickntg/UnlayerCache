using UnlayerCache.API.Util;
using Xunit;

namespace UnlayerCache.API.Tests.Util
{
    public class UtilTests
    {
        [Fact]
        public void VerifyHash()
        {
            Assert.Equal("DJwzU9ezebgB9mCtsXHJylEmx614N2IEAYxQQ3eIhVU=", Hash.HashString("1234509876"));
        }
    }
}
