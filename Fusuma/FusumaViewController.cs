using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Fusuma
{
    public class FusumaViewController : UIViewController
    {
        public event EventHandler Closed;
        public event EventHandler<UIImage> ImageSelected;
        public event EventHandler<NSUrl> VideoSelected;


        private AlbumView _albumView;
        private CameraView _cameraView;
        private VideoView _videoView;

        private Mode _mode = Mode.Camera;
        private UIView _cameraViewContainer;
        private UIView _libraryViewContainer;
        private UIView _videoViewContainer;
        private UIButton _libraryButton;
        private UIButton _doneButton;
        private UIButton _closeButton;
        private UIButton _videoButton;
        private UIButton _cameraButton;
        private UILabel _menuTitle;
        private UIView _menuView;

        public bool HasVideo { get; set; }
        public FusumaAlbumDataSource AlbumDataSource { get; set; }
        public UICollectionViewDelegate AlbumDelegate { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.ContentMode = UIViewContentMode.ScaleToFill;
            View.AutoresizingMask = UIViewAutoresizing.All;
            View.Frame = new CGRect(0, 0, 600, 600);

            _videoViewContainer = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Frame = new CGRect(0, 0, 600, 555),
                BackgroundColor = UIColor.Black,
                AccessibilityLabel = "VideoViewContainer"
            };

            _cameraViewContainer = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Frame = new CGRect(0, 0, 600, 555),
                BackgroundColor = UIColor.Black,
                AccessibilityLabel = "CameraViewContainer"
            };

            _libraryViewContainer = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Frame = new CGRect(0, 0, 600, 555),
                BackgroundColor = UIColor.Black,
                AccessibilityLabel = "LibraryViewContainer"
            };

            _menuView = new UIView
            {
                Frame = new CGRect(0, 0, 600, 50),
                TranslatesAutoresizingMaskIntoConstraints = false,
                AccessibilityLabel = "MenuView"
            };

            View.AddSubviews(_videoViewContainer, _cameraViewContainer, _libraryViewContainer, _menuView);

            _closeButton = new UIButton(new CGRect(8, 8, 40, 40))
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                ContentMode = UIViewContentMode.ScaleToFill,
                Opaque = false,
                ContentEdgeInsets = new UIEdgeInsets(6, 6, 6, 6),
                AccessibilityLabel = "CloseButton"
            };

            _doneButton = new UIButton(new CGRect(522, 6, 40, 40))
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                ContentMode = UIViewContentMode.ScaleToFill,
                Opaque = false,
                ContentEdgeInsets = new UIEdgeInsets(8, 8, 8, 8),
                AccessibilityLabel = "DoneButton"
            };

            _menuTitle = new UILabel(new CGRect(56, 17, 488, 21))
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentMode = UIViewContentMode.Left,
                TextAlignment = UITextAlignment.Center,
                UserInteractionEnabled = false,
                Opaque = false,
                BaselineAdjustment = UIBaselineAdjustment.AlignBaselines,
                AdjustsFontSizeToFitWidth = false,
                LineBreakMode = UILineBreakMode.TailTruncation,
                Text = Configuration.CameraRollTitle,
                TextColor = UIColor.White,
                AccessibilityLabel = "MenuTitle"
            };

            _menuView.AddSubviews(_closeButton, _doneButton, _menuTitle);

            _menuView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(_closeButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _menuView,
                    NSLayoutAttribute.Leading, 1, 8),
                NSLayoutConstraint.Create(_menuView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 50),
                NSLayoutConstraint.Create(_menuView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _doneButton,
                    NSLayoutAttribute.Trailing, 1, 8),
                NSLayoutConstraint.Create(_menuTitle, NSLayoutAttribute.Leading, NSLayoutRelation.Equal,
                    _closeButton, NSLayoutAttribute.Trailing, 1, 8),
                NSLayoutConstraint.Create(_doneButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _menuTitle,
                    NSLayoutAttribute.Trailing, 1, 8),
                NSLayoutConstraint.Create(_doneButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _menuView,
                    NSLayoutAttribute.Top, 1, 6),
                NSLayoutConstraint.Create(_menuTitle, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, _menuView,
                    NSLayoutAttribute.CenterY, 1, 2),
                NSLayoutConstraint.Create(_closeButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _menuView,
                    NSLayoutAttribute.Top, 1, 8),
                NSLayoutConstraint.Create(_closeButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 40),
                NSLayoutConstraint.Create(_closeButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1, 40),
                NSLayoutConstraint.Create(_doneButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 40),
                NSLayoutConstraint.Create(_doneButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1, 40),
                NSLayoutConstraint.Create(_menuTitle, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 21)
            });

            _libraryButton = new UIButton
            {
                Frame = new CGRect(0, 555, 200, 45),
                TranslatesAutoresizingMaskIntoConstraints = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                ContentMode = UIViewContentMode.ScaleToFill,
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                AccessibilityLabel = "LibraryButton"
            };

            _videoButton = new UIButton
            {
                Frame = new CGRect(400, 555, 200, 45),
                TranslatesAutoresizingMaskIntoConstraints = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                ContentMode = UIViewContentMode.ScaleToFill,
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                AccessibilityLabel = "VideoButton"
            };

            _cameraButton = new UIButton
            {
                Frame = new CGRect(200, 555, 200, 45),
                TranslatesAutoresizingMaskIntoConstraints = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                ContentMode = UIViewContentMode.ScaleToFill,
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                AccessibilityLabel = "PhotoButton"
            };

            View.AddSubviews(_libraryButton, _videoButton, _cameraButton);

            View.AddConstraints(new []
            {
                NSLayoutConstraint.Create(_menuView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, View, NSLayoutAttribute.Trailing, 1, 0),
                NSLayoutConstraint.Create(_libraryViewContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1, 0), 
                NSLayoutConstraint.Create(_cameraButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _libraryButton, NSLayoutAttribute.Trailing, 1, 0), 
                NSLayoutConstraint.Create(_menuView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View, NSLayoutAttribute.Leading, 1, 0), 
                NSLayoutConstraint.Create(_cameraButton, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1, 0),
                NSLayoutConstraint.Create(_cameraButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _libraryButton, NSLayoutAttribute.Top, 1, 0), 
                NSLayoutConstraint.Create(_cameraViewContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1,0),
                NSLayoutConstraint.Create(_cameraButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _videoButton, NSLayoutAttribute.Top, 1, 0), 
                NSLayoutConstraint.Create(_menuView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1, 0), 
                NSLayoutConstraint.Create(_cameraButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, _libraryButton, NSLayoutAttribute.Width, 1,0), 
                NSLayoutConstraint.Create(_videoViewContainer, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _libraryViewContainer, NSLayoutAttribute.Leading, 1, 0), 
                NSLayoutConstraint.Create(_videoButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, View, NSLayoutAttribute.Trailing, 1, 0), 
                NSLayoutConstraint.Create(_libraryViewContainer, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, View, NSLayoutAttribute.Trailing, 1,0), 
                NSLayoutConstraint.Create(_cameraButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, _videoButton, NSLayoutAttribute.Width, 1, 0), 
                NSLayoutConstraint.Create(_cameraViewContainer, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _libraryViewContainer, NSLayoutAttribute.Trailing, 1, 0), 
                NSLayoutConstraint.Create(_videoViewContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1, 0), 
                NSLayoutConstraint.Create(_libraryViewContainer, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View, NSLayoutAttribute.Leading, 1, 0), 
                NSLayoutConstraint.Create(_videoButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _cameraButton, NSLayoutAttribute.Trailing, 1, 0), 
                NSLayoutConstraint.Create(_libraryButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View, NSLayoutAttribute.Leading, 1, 0), 
                NSLayoutConstraint.Create(_videoViewContainer, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _libraryViewContainer, NSLayoutAttribute.Trailing, 1, 0),
                NSLayoutConstraint.Create(_cameraViewContainer, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _libraryViewContainer, NSLayoutAttribute.Leading, 1, 0),
                NSLayoutConstraint.Create(_cameraButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _libraryViewContainer, NSLayoutAttribute.Bottom, 1, 0),   
                NSLayoutConstraint.Create(_libraryButton, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1, 0), 
                NSLayoutConstraint.Create(_cameraViewContainer, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _libraryViewContainer, NSLayoutAttribute.Bottom, 1, 0), 
                NSLayoutConstraint.Create(_videoViewContainer, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _libraryViewContainer, NSLayoutAttribute.Bottom, 1, 0)
            });

            View.BackgroundColor = Configuration.BackgroundColor;
            _menuView.BackgroundColor = Configuration.BackgroundColor;
            _menuView.AddBottomBorder(UIColor.Black, 1);
            _menuTitle.TextColor = Configuration.BaseTintColor;

            var albumImage = Configuration.AlbumImage ?? UIImage.FromBundle("ic_insert_photo");
            var cameraImage = Configuration.CameraImage ?? UIImage.FromBundle("ic_photo_camera");
            var videoImage = Configuration.VideoImage ?? UIImage.FromBundle("ic_videocam");
            var checkImage = Configuration.CheckImage ?? UIImage.FromBundle("ic_check");
            var closeImage = Configuration.CloseImage ?? UIImage.FromBundle("ic_close");

            if (Configuration.TintIcons)
            {
                albumImage = albumImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                cameraImage = cameraImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                videoImage = videoImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                checkImage = checkImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                closeImage = closeImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }

            _libraryButton.SetImage(albumImage, UIControlState.Normal);
            _libraryButton.SetImage(albumImage, UIControlState.Highlighted);
            _libraryButton.SetImage(albumImage, UIControlState.Selected);

            _cameraButton.SetImage(cameraImage, UIControlState.Normal);
            _cameraButton.SetImage(cameraImage, UIControlState.Highlighted);
            _cameraButton.SetImage(cameraImage, UIControlState.Selected);

            _videoButton.SetImage(videoImage, UIControlState.Normal);
            _videoButton.SetImage(videoImage, UIControlState.Highlighted);
            _videoButton.SetImage(videoImage, UIControlState.Selected);

            _closeButton.SetImage(closeImage, UIControlState.Normal);
            _closeButton.SetImage(closeImage, UIControlState.Highlighted);
            _closeButton.SetImage(closeImage, UIControlState.Selected);

            _doneButton.SetImage(checkImage, UIControlState.Normal);

            if (Configuration.TintIcons)
            {
                _libraryButton.TintColor = Configuration.TintColor;
                _libraryButton.AdjustsImageWhenHighlighted = false;
                _cameraButton.TintColor = Configuration.TintColor;
                _cameraButton.AdjustsImageWhenHighlighted = false;
                _videoButton.TintColor = Configuration.TintColor;
                _videoButton.AdjustsImageWhenHighlighted = false;
                _doneButton.TintColor = Configuration.TintColor;
            }

            _cameraButton.ClipsToBounds = true;
            _libraryButton.ClipsToBounds = true;
            _videoButton.ClipsToBounds = true;

            ChangeMode(Mode.Library);

            _cameraViewContainer.AddSubview(_cameraView ?? (_cameraView = new CameraView()));
            _libraryViewContainer.AddSubview(_albumView ?? (_albumView = new AlbumView()));
            _videoViewContainer.AddSubview(_videoView ?? (_videoView = new VideoView()));

            if (HasVideo)
            {
                _videoView.RemoveFromSuperview();

                View.AddConstraint(NSLayoutConstraint.Create(View, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal,
                    _cameraButton, NSLayoutAttribute.Trailing, 1, 0));

                View.LayoutIfNeeded();
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            _albumView.Frame = new CGRect(CGPoint.Empty, _libraryViewContainer.Frame.Size);
            _albumView.LayoutIfNeeded();

            _cameraView.Frame = new CGRect(CGPoint.Empty, _cameraViewContainer.Frame.Size);
            _cameraView.LayoutIfNeeded();

            if (AlbumDataSource == null && AlbumDelegate == null)
            {
                Console.WriteLine("DataSource and Delegate are null for AlbumView, using default.");
                AlbumDataSource = new PhotoGalleryDataSource(_albumView, new CGSize(100, 100));
                AlbumDelegate = new PhotoGalleryDelegate(_albumView, (PhotoGalleryDataSource)AlbumDataSource);
            }

            _albumView.Initialize(AlbumDataSource, AlbumDelegate);
            _cameraView.Initialize(OnImage);

            if (HasVideo)
            {
                _videoView.Frame = new CGRect(CGPoint.Empty, _videoViewContainer.Frame.Size);
                _videoView.LayoutIfNeeded();
                _videoView.Initialize(OnVideo);
            }

            _libraryButton.TouchUpInside += LibraryButtonPressed;
            _closeButton.TouchUpInside += CloseButtonPressed;
            _cameraButton.TouchUpInside += CameraButtonPressed;
            _videoButton.TouchUpInside += VideoButtonPressed;
            _doneButton.TouchUpInside += DoneButtonPressed;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            StopAll();
        }

        public override bool PrefersStatusBarHidden() => Configuration.PreferStatusbarHidden;

        private void CloseButtonPressed(object sender, EventArgs e)
        {
            DismissViewController(true, () =>
            {
                Closed?.Invoke(this, EventArgs.Empty);
            });
        }

        private void LibraryButtonPressed(object sender, EventArgs eventArgs)
        {
            ChangeMode(Mode.Library);
        }

        private void CameraButtonPressed(object sender, EventArgs e)
        {
            ChangeMode(Mode.Camera);
        }

        private void VideoButtonPressed(object sender, EventArgs e)
        {
            ChangeMode(Mode.Video);
        }

        private async void DoneButtonPressed(object sender, EventArgs e)
        {
            var view = _albumView.ImageCropView;

            if (Configuration.CropImage)
            {
                var normalizedX = view.ContentOffset.X/view.ContentSize.Width;
                var normalizedY = view.ContentOffset.Y/view.ContentSize.Height;

                var normalizedWidth = view.Frame.Width/view.ContentSize.Width;
                var normalizedHeight = view.Frame.Height/view.ContentSize.Height;

                var cropRect = new CGRect(normalizedX, normalizedY, normalizedWidth, normalizedHeight);

                Console.WriteLine("Cropping image before handing it over");
                var image = await AlbumDataSource.GetCroppedImage(cropRect);

                InvokeOnMainThread(() =>
                {
                    ImageSelected?.Invoke(this, image);
                    DismissViewController(true, () => Closed?.Invoke(this, EventArgs.Empty));
                });
            }
            else
            {
                Console.WriteLine("Not cropping image");
                ImageSelected?.Invoke(this, view.Image);
                DismissViewController(true, () => Closed?.Invoke(this, EventArgs.Empty));
            }
        }

        private void OnVideo(NSUrl nsUrl)
        {
            VideoSelected?.Invoke(this, nsUrl);
            DismissViewController(true, () => Closed?.Invoke(this, EventArgs.Empty));
        }

        private void OnImage(UIImage uiImage)
        {
            ImageSelected?.Invoke(this, uiImage);
            DismissViewController(true, () => Closed?.Invoke(this, EventArgs.Empty));
        }

        private void ChangeMode(Mode mode)
        {
            if (_mode == mode) return;

            switch (mode)
            {
                case Mode.Camera:
                    _cameraView.StopCamera();
                    break;
                case Mode.Video:
                    _videoView.StopCamera();
                    break;
            }

            _mode = mode;

            DisHighlightButtons();

            switch (mode)
            {
                case Mode.Library:
                    _menuTitle.Text = Configuration.CameraRollTitle;
                    _doneButton.Hidden = false;

                    HighlightButton(_libraryButton);
                    View.BringSubviewToFront(_libraryViewContainer);
                    break;
                case Mode.Camera:
                    _menuTitle.Text = Configuration.CameraTitle;
                    _doneButton.Hidden = true;

                    HighlightButton(_cameraButton);
                    View.BringSubviewToFront(_cameraViewContainer);
                    _cameraView.StartCamera();
                    break;
                case Mode.Video:
                    _menuTitle.Text = Configuration.VideoTitle;
                    _doneButton.Hidden = true;

                    HighlightButton(_videoButton);
                    View.BringSubviewToFront(_videoViewContainer);
                    _videoView.StartCamera();
                    break;
            }

            View.BringSubviewToFront(_menuView);
        }

        private void DisHighlightButtons()
        {
            _cameraButton.TintColor = Configuration.BaseTintColor;
            _libraryButton.TintColor = Configuration.BaseTintColor;

            var buttons = new[] {_cameraButton, _videoButton, _libraryButton};

            foreach (var button in buttons)
            {
                if (button.Layer.Sublayers.Length <= 1) continue;

                foreach (var layer in button.Layer.Sublayers)
                {
                    if (layer.Name == UiKitExtensions.BorderLayerName)
                        layer.RemoveFromSuperLayer();
                }
            }

            _videoButton.TintColor = Configuration.BaseTintColor;
        }

        private static void HighlightButton(UIView button)
        {
            button.TintColor = Configuration.TintColor;
            button.AddBottomBorder(Configuration.TintColor, 3);
        }

        private void StopAll()
        {
            if (HasVideo)
                _videoView?.StopCamera();
            _cameraView?.StopCamera();
        }
    }
}
