using System;
using CoreAnimation;
using CoreGraphics;
using CoreMedia;
using UIKit;

namespace Chafu
{
    public static class UiKitExtensions
    {
        public const string BorderLayerName = "FSBottomBorder";

        public static void AddBottomBorder(this UIView self, UIColor color, float width)
        {
            var border = new CALayer
            {
                BorderColor = color.CGColor,
                Frame = new CGRect(0, self.Frame.Height - width, self.Frame.Width, width),
                BorderWidth = width,
                Name = BorderLayerName
            };
            self.Layer.AddSublayer(border);
        }

        public enum UIImageAligment
        {
            Center,
            Left,
            Top,
            Right,
            Bottom,
            TopLeft,
            BottomRight,
            BottomLeft,
            TopRight
        }

        public enum UIImageScaleMode
        {
            Fill,
            AspectFill,
            AspectFit
        }

        public static UIImage ScaleImage(this UIImage image, CGSize size, UIImageScaleMode scaleMode = UIImageScaleMode.AspectFit,
            UIImageAligment alignment = UIImageAligment.Center, bool trim = false)
        {
            var width = size == CGSize.Empty ? 1 : size.Width;
            var height = size == CGSize.Empty ? 1 : size.Height;

            var widthScale = width / image.Size.Width;
            var heightScale = height / image.Size.Height;

            switch (scaleMode)
            {
                case UIImageScaleMode.AspectFit:
                    {
                        var scale = (nfloat)Math.Min(widthScale, heightScale);
                        widthScale = scale;
                        heightScale = scale;
                        break;
                    }
                case UIImageScaleMode.AspectFill:
                    {
                        var scale = (nfloat)Math.Max(widthScale, heightScale);
                        widthScale = scale;
                        heightScale = scale;
                        break;
                    }
            }

            var newWidth = widthScale * image.Size.Width;
            var newHeight = heightScale * image.Size.Height;

            var canvasWidth = trim ? newWidth : size.Width;
            var canvasHeight = trim ? newHeight : size.Height;

            UIGraphics.BeginImageContextWithOptions(new CGSize(canvasWidth, canvasHeight), false, 0);

            var originX = 0f;
            var originY = 0f;

            if (scaleMode == UIImageScaleMode.AspectFit)
            {
                switch (alignment)
                {
                    case UIImageAligment.Center:
                        originX = (float)((canvasWidth - newWidth) / 2);
                        originY = (float)((canvasHeight - newHeight) / 2);
                        break;
                    case UIImageAligment.Top:
                        originX = (float)((canvasWidth - newWidth) / 2);
                        break;
                    case UIImageAligment.Left:
                        originY = (float)((canvasHeight - newHeight) / 2);
                        break;
                    case UIImageAligment.Bottom:
                        originX = (float)((canvasWidth - newWidth) / 2);
                        originY = (float)(canvasHeight - newHeight);
                        break;
                    case UIImageAligment.Right:
                        originX = (float)(canvasWidth - newWidth);
                        originY = (float)((canvasHeight - newHeight) / 2);
                        break;
                    case UIImageAligment.TopRight:
                        originX = (float)(canvasWidth - newWidth);
                        break;
                    case UIImageAligment.BottomLeft:
                        originY = (float)(canvasHeight - newHeight);
                        break;
                    case UIImageAligment.BottomRight:
                        originX = (float)(canvasWidth - newWidth);
                        originY = (float)(canvasHeight - newHeight);
                        break;
                    case UIImageAligment.TopLeft:
                    default:
                        break;
                }
            }

            image.Draw(new CGRect(originX, originY, newWidth, newHeight));
            var scaledImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return scaledImage;
        }


        public static double ToDouble(this CMTime duration)
        {
            if (duration.IsIndefinite)
                return double.NaN;

            if (duration.IsNegativeInfinity)
                return double.NegativeInfinity;

            if (duration.IsPositiveInfinity)
                return double.PositiveInfinity;

            return duration.Seconds;
        }
    }
}