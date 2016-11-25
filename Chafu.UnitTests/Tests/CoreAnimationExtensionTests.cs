using System;
using UIKit;
using Xunit;

namespace Chafu.UnitTests.Tests
{
    public class CoreAnimationExtensionTests
    {
        [Theory]
        [InlineData(UIDeviceOrientation.FaceDown, 0.0f)]
        [InlineData(UIDeviceOrientation.FaceUp, 0.0f)]
        [InlineData(UIDeviceOrientation.LandscapeLeft, -Math.PI / 2.0f)]
        [InlineData(UIDeviceOrientation.LandscapeRight, Math.PI / 2.0f)]
        [InlineData(UIDeviceOrientation.Portrait, 0.0f)]
        [InlineData(UIDeviceOrientation.PortraitUpsideDown, Math.PI)]
        [InlineData(UIDeviceOrientation.Unknown, 0.0f)]
        public void OrientationTransformTest(UIDeviceOrientation orientation, float angle)
        {
            var transform = orientation.OrientationTransform();


        }
    }
}
