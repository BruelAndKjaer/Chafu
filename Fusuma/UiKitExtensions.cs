﻿using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace Fusuma
{
    public static class UiKitExtensions
    {
        public const string BorderLayerName = "FSBottomBorder";

        public static void AddBottomBorder(this UIView self, UIColor color, float width)
        {
            var border = new CALayer
            {
                BorderColor = color.CGColor,
                Frame = new CGRect(0, self.Frame.Height, self.Frame.Width, width),
                BorderWidth = width,
                Name = BorderLayerName
            };
            self.Layer.AddSublayer(border);
        }
    }
}