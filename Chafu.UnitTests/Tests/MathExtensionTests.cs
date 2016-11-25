using System;
using Xunit;

namespace Chafu.UnitTests.Tests
{
    public class MathExtensionTests
    {
        [Theory]
        [InlineData(0.0f, 0.0f, 5)]
        [InlineData(-0.0f, 0.0f, 5)]
        [InlineData(30.0f, Math.PI / 6f, 5)]
        [InlineData(45.0f, Math.PI / 4f, 5)]
        [InlineData(60.0f, Math.PI / 3f, 5)]
        [InlineData(90.0f, Math.PI / 2f, 5)]
        [InlineData(120.0f, 2 * Math.PI / 3f, 5)]
        [InlineData(135.0f, 3 * Math.PI / 4f, 5)]
        [InlineData(150.0f, 5 * Math.PI / 6f, 5)]
        [InlineData(180.0f, Math.PI, 5)]
        [InlineData(270.0f, 3 * Math.PI / 2f, 5)]
        [InlineData(360.0f, 2 * Math.PI, 5)]
        public void ToRadiansTest(float degrees, float expected, int prescision)
        {
            var radians = ((nfloat)degrees).ToRadians();

            Assert.True(TestHelpers.AboutEqual(radians, expected, prescision));
        }
    }
}
