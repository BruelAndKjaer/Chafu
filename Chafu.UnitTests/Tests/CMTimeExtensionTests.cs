using System;
using CoreMedia;
using Xunit;

namespace Chafu.UnitTests.Tests
{
    public class CMTimeExtensionTests
    {
        [Fact]
        public void ToSecondsTests()
        {
            var time = new CMTime(1, 10);
            var seconds = time.ToDouble();

            Assert.True(TestHelpers.AboutEqual((float)seconds, 0.1f, 3));
        }
    }
}
