using System;
using CoreAnimation;
using CoreGraphics;
using CoreMedia;
using UIKit;

namespace Chafu
{
    /// <summary>
    /// Extension method for <see cref="UIKit"/> classes
    /// </summary>
    public static class UiKitExtensions
    {
        /// <summary>
        /// Name for border added to tab items
        /// </summary>
        public const string BorderLayerName = "FSBottomBorder";

        /// <summary>
        /// Add bottom border to UIView
        /// </summary>
        /// <param name="self"><see cref="UIView"/> to add border to</param>
        /// <param name="color"><see cref="UIColor"/> of the border</param>
        /// <param name="width">Border width</param>
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

        /// <summary>
        /// Alignment for scaling images
        /// </summary>
        public enum UIImageAlignment
        {
            /// <summary>
            /// Center align
            /// </summary>
            Center,
            /// <summary>
            /// Left align
            /// </summary>
            Left,
            /// <summary>
            /// Top align
            /// </summary>
            Top,
            /// <summary>
            /// Right align
            /// </summary>
            Right,
            /// <summary>
            /// Bottom align
            /// </summary>
            Bottom,
            /// <summary>
            /// Top-left align
            /// </summary>
            TopLeft,
            /// <summary>
            /// Bottom-right align
            /// </summary>
            BottomRight,
            /// <summary>
            /// Bottom-left align
            /// </summary>
            BottomLeft,
            /// <summary>
            /// Top-right align
            /// </summary>
            TopRight
        }

        /// <summary>
        /// Scale mode for scaling images
        /// </summary>
        public enum UIImageScaleMode
        {
            /// <summary>
            /// Fill
            /// </summary>
            Fill,
            /// <summary>
            /// Fill with aspect ratio preserved
            /// </summary>
            AspectFill,
            /// <summary>
            /// Fit image with aspect ratio preserved
            /// </summary>
            AspectFit
        }

        /// <summary>
        /// Scale image
        /// </summary>
        /// <param name="image"><see cref="UIImage"/> to scale</param>
        /// <param name="size"><see cref="CGSize"/> size to scale to</param>
        /// <param name="scaleMode"><see cref="UIImageScaleMode"/> scale mode</param>
        /// <param name="alignment"><see cref="UIImageAlignment"/> alignment</param>
        /// <param name="trim">Trim blank space</param>
        /// <returns></returns>
        public static UIImage ScaleImage(this UIImage image, CGSize size, UIImageScaleMode scaleMode = UIImageScaleMode.AspectFit,
            UIImageAlignment alignment = UIImageAlignment.Center, bool trim = false)
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
                    case UIImageAlignment.Center:
                        originX = (float)((canvasWidth - newWidth) / 2);
                        originY = (float)((canvasHeight - newHeight) / 2);
                        break;
                    case UIImageAlignment.Top:
                        originX = (float)((canvasWidth - newWidth) / 2);
                        break;
                    case UIImageAlignment.Left:
                        originY = (float)((canvasHeight - newHeight) / 2);
                        break;
                    case UIImageAlignment.Bottom:
                        originX = (float)((canvasWidth - newWidth) / 2);
                        originY = (float)(canvasHeight - newHeight);
                        break;
                    case UIImageAlignment.Right:
                        originX = (float)(canvasWidth - newWidth);
                        originY = (float)((canvasHeight - newHeight) / 2);
                        break;
                    case UIImageAlignment.TopRight:
                        originX = (float)(canvasWidth - newWidth);
                        break;
                    case UIImageAlignment.BottomLeft:
                        originY = (float)(canvasHeight - newHeight);
                        break;
                    case UIImageAlignment.BottomRight:
                        originX = (float)(canvasWidth - newWidth);
                        originY = (float)(canvasHeight - newHeight);
                        break;
                    case UIImageAlignment.TopLeft:
                    default:
                        break;
                }
            }

            image.Draw(new CGRect(originX, originY, newWidth, newHeight));
            var scaledImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return scaledImage;
        }

        /// <summary>
        /// Convert <see cref="CMTime"/> to double
        /// </summary>
        /// <param name="duration"><see cref="CMTime"/> to convert</param>
        /// <returns><see cref="double"/> with seconds</returns>
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