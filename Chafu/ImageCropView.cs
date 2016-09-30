using System;
using CoreGraphics;
using UIKit;

namespace Chafu
{
    /// <summary>
    /// View used to scroll and zoom images for cropping
    /// </summary>
    public class ImageCropView : UIScrollView
    {
        private readonly UIImageView _imageView = new UIImageView();
        private UIImage _image;

        /// <summary>
        /// Get or set the <see cref="CGSize"/> with the size of the image
        /// </summary>
        public CGSize ImageSize { get; set; }

        /// <summary>
        /// Get or set the <see cref="UIImage"/>
        /// </summary>
        public UIImage Image
        {
            get { return _image; }
            set
            {
                _image = value;

                if (_image != null)
                {
                    if (!_imageView.IsDescendantOfView(this))
                    {
                        _imageView.Alpha = 1.0f;
                        Add(_imageView);
                    }
                }
                else
                {
                    _imageView.Image = null;
                    return;
                }

                var imageSize = ImageSize == CGSize.Empty ? _image.Size : ImageSize;
                if (imageSize.Width < Frame.Width || imageSize.Height < Frame.Height)
                {
                    // The width or height of the image is smaller than the frame size

                    if (imageSize.Width > imageSize.Height)
                    {
                        var ratio = Frame.Width/imageSize.Width;
                        _imageView.Frame = new CGRect(0, 0, Frame.Width, imageSize.Height*ratio);
                    }
                    else
                    {
                        var ratio = Frame.Height/imageSize.Height;
                        _imageView.Frame = new CGRect(0, 0, imageSize.Width*ratio, Frame.Height);
                    }
                    _imageView.Center = Center;
                }
                else
                {
                    // The width or height of the image is bigger than the frame size

                    if (imageSize.Width > imageSize.Height)
                    {
                        var ratio = Frame.Height/imageSize.Height;
                        _imageView.Frame = new CGRect(0, 0, imageSize.Width*ratio, Frame.Height);
                    }
                    else
                    {
                        var ratio = Frame.Width/imageSize.Width;
                        _imageView.Frame = new CGRect(0,0, Frame.Width, imageSize.Height * ratio);
                    }

                    ContentOffset = new CGPoint(_imageView.Center.X - Center.X,
                        _imageView.Center.Y - Center.Y);
                }

                ContentSize = new CGSize(_imageView.Frame.Width + 1, _imageView.Frame.Height + 1);
                _imageView.Image = _image;
                ZoomScale = 1.0f;
            }
        }

        /// <summary>
        /// Create a new <see cref="ImageCropView"/>
        /// </summary>
        public ImageCropView()
        {
            BackgroundColor = Configuration.BackgroundColor;
            Frame = new CGRect(CGPoint.Empty, CGSize.Empty);
            ClipsToBounds = true;
            _imageView.Alpha = 0.0f;

            _imageView.Frame = new CGRect(CGPoint.Empty, CGSize.Empty);

            MaximumZoomScale = 2.0f;
            MinimumZoomScale = 0.8f;
            ShowsHorizontalScrollIndicator = false;
            ShowsVerticalScrollIndicator = false;
            BouncesZoom = true;
            Bounces = true;

            Delegate = new ScrollViewDelegate(this);
        }

        /// <summary>
        /// Get or set whether the view is scrollable
        /// </summary>
        public bool Scrollable
        {
            get { return ScrollEnabled; }
            set { ScrollEnabled = value; }
        }

        private class ScrollViewDelegate : UIScrollViewDelegate
        {
            private readonly ImageCropView _view;

            public ScrollViewDelegate(ImageCropView view)
            {
                _view = view;
            }

            public override void DidZoom(UIScrollView scrollView)
            {
                var boundsSize = scrollView.Bounds.Size;
                var contentsFrame = _view._imageView.Frame;

                if (contentsFrame.Size.Width < boundsSize.Width)
                    contentsFrame.X = (boundsSize.Width - contentsFrame.Size.Width) / 2.0f;
                else
                    contentsFrame.X = 0.0f;

                if (contentsFrame.Size.Height < boundsSize.Height)
                    contentsFrame.Y = (boundsSize.Height - contentsFrame.Size.Height) / 2.0f;
                else
                    contentsFrame.Y = 0.0f;

                _view._imageView.Frame = contentsFrame;
            }

            public override void ZoomingEnded(UIScrollView scrollView, UIView withView, nfloat atScale)
            {
                _view.ContentSize = new CGSize(_view._imageView.Frame.Width + 1, _view._imageView.Frame.Height + 1);
            }

            public override UIView ViewForZoomingInScrollView(UIScrollView scrollView)
            {
                return _view._imageView;
            }
        }
    }
}
