using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;
using Xunit;

namespace Chafu.UnitTests.Tests
{
    public class BottomBorderTests
    {
        [Fact]
        public void AddBottomBorderTest()
        {
            RunOnUiThread(() =>
            {
                var view = CreateView();
                view.AddBottomBorder(UIColor.Yellow, 100);

                Assert.True(view.Layer.Sublayers.Any(
                    layer => layer.Name == UiKitExtensions.BorderLayerName));
            });
        }

        [Fact]
        public void RemoveBottomBorderTest()
        {
            RunOnUiThread(() =>
            {
                var view = CreateView();
                view.AddBottomBorder(UIColor.Yellow, 100);

                var layer = view.Layer.Sublayers.FirstOrDefault(
                    l => l.Name == UiKitExtensions.BorderLayerName);

                layer.RemoveFromSuperLayer();

                Assert.True(view.Layer.Sublayers == null);
            });
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(100)]
        public void BottomBorderWidthTest(int width)
        {
            RunOnUiThread(() =>
            {
                var view = CreateView();
                view.AddBottomBorder(UIColor.Yellow, width);

                var layer = view.Layer.Sublayers.FirstOrDefault(
                    l => l.Name == UiKitExtensions.BorderLayerName);

                var frame = layer.Frame;

                Assert.True(layer.Frame.X == 0);
                Assert.True(layer.Frame.Y == view.Frame.Height - width);
                Assert.True(layer.Frame.Width == view.Frame.Width);
                Assert.True(layer.Frame.Height == width);
            });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void BottomBorderWidthFailTest(int width)
        {
            RunOnUiThread(() =>
            {
                var view = CreateView();
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => view.AddBottomBorder(UIColor.Yellow, width));
            });
        }

        private UIView CreateView()
            => new UIView { Frame = new CGRect(0, 0, 100, 20) };

        private void RunOnUiThread(Action action)
        {
            new NSObject().InvokeOnMainThread(action);
        }
    }
}
