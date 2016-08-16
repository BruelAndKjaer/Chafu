using System;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Chafu
{
	public class ChafuViewController : UIViewController
	{
		public event EventHandler Closed;
		public event EventHandler<UIImage> ImageSelected;
		public event EventHandler<NSUrl> VideoSelected;

		private AlbumView _albumView;
		private CameraView _cameraView;
		private VideoView _videoView;

        private Mode _mode = Mode.NotSelected;
		private UIButton _libraryButton;
		private UIButton _doneButton;
		private UIButton _closeButton;
		private UIButton _videoButton;
		private UIButton _cameraButton;
		private UILabel _menuTitle;
		private UIView _menuView;

        /// <summary>
        /// Gets or sets a value indicating whether video tab is shown.
        /// </summary>
        /// <value><c>true</c> if has video; otherwise, <c>false</c>.</value>
		public bool HasVideo { get; set; } = false;

        /// <summary>
        /// Gets or sets the album collectionview data source. If null, it will default to show photos from phone gallery.
        /// </summary>
        /// <value>The album data source.</value>
		public ChafuAlbumDataSource AlbumDataSource { get; set; }

        /// <summary>
        /// Gets or sets the album delegate.
        /// </summary>
        /// <value>The album delegate.</value>
		public UICollectionViewDelegate AlbumDelegate { get; set; }

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			_menuView = new UIView {
				TranslatesAutoresizingMaskIntoConstraints = false,
				Opaque = false,
				AccessibilityLabel = "MenuView"
			};

			View.AddSubviews (_menuView);

			_closeButton = new UIButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				LineBreakMode = UILineBreakMode.MiddleTruncation,
				VerticalAlignment = UIControlContentVerticalAlignment.Center,
				HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
				ContentMode = UIViewContentMode.ScaleToFill,
				Opaque = false,
				ContentEdgeInsets = new UIEdgeInsets (6, 6, 6, 6),
				AccessibilityLabel = "CloseButton"
			};

			_doneButton = new UIButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				LineBreakMode = UILineBreakMode.MiddleTruncation,
				VerticalAlignment = UIControlContentVerticalAlignment.Center,
				HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
				ContentMode = UIViewContentMode.ScaleToFill,
				Opaque = false,
				ContentEdgeInsets = new UIEdgeInsets (8, 8, 8, 8),
				AccessibilityLabel = "DoneButton"
			};

			_menuTitle = new UILabel {
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

			_menuView.AddSubviews (_closeButton, _doneButton, _menuTitle);

			_libraryButton = new UIButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				LineBreakMode = UILineBreakMode.MiddleTruncation,
				HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
				VerticalAlignment = UIControlContentVerticalAlignment.Center,
				ContentMode = UIViewContentMode.ScaleToFill,
				ContentEdgeInsets = new UIEdgeInsets (2, 2, 2, 2),
				AccessibilityLabel = "LibraryButton"
			};

			_videoButton = new UIButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				LineBreakMode = UILineBreakMode.MiddleTruncation,
				HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
				VerticalAlignment = UIControlContentVerticalAlignment.Center,
				ContentMode = UIViewContentMode.ScaleToFill,
				ContentEdgeInsets = new UIEdgeInsets (2, 2, 2, 2),
				AccessibilityLabel = "VideoButton"
			};

			_cameraButton = new UIButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				LineBreakMode = UILineBreakMode.MiddleTruncation,
				HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
				VerticalAlignment = UIControlContentVerticalAlignment.Center,
				ContentMode = UIViewContentMode.ScaleToFill,
				ContentEdgeInsets = new UIEdgeInsets (2, 2, 2, 2),
				AccessibilityLabel = "PhotoButton"
			};

			View.AddSubviews (_libraryButton, _cameraButton);

			_cameraView = new CameraView {
				BackgroundColor = Configuration.BackgroundColor,
				TranslatesAutoresizingMaskIntoConstraints = false
			};
			_albumView = new AlbumView {
				BackgroundColor = Configuration.BackgroundColor,
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			View.AddSubviews (_cameraView, _albumView);

			View.AddConstraints (
				_menuView.Height ().EqualTo (50),
				_menuView.AtTopOf (View),
				_menuView.AtLeftOf (View),
				_menuView.AtRightOf (View),

				_closeButton.AtLeftOf (_menuView, 8),
				_closeButton.AtTopOf (_menuView, 8),
				_closeButton.Width ().EqualTo (40),
				_closeButton.Height ().EqualTo (40),

				_menuTitle.WithSameCenterY (_menuView).Plus (2),
				_menuTitle.ToRightOf (_closeButton, 8),
				_menuTitle.Height ().EqualTo (21),

				_doneButton.ToRightOf (_menuTitle, 8),
				_doneButton.AtTopOf (_menuView, 8),
				_doneButton.Width ().EqualTo (40),
				_doneButton.Height ().EqualTo (40),
				_doneButton.AtRightOf (View, 8),

				_albumView.AtTopOf (View),
				_albumView.AtLeftOf (View),
				_albumView.AtRightOf (View),

				_cameraView.AtTopOf (View),
				_cameraView.WithSameLeft (_albumView),
				_cameraView.WithSameRight (_albumView),

				_libraryButton.AtLeftOf (View),
				_libraryButton.AtBottomOf (View),

				_cameraButton.ToRightOf (_libraryButton),
				_cameraButton.AtBottomOf (View),

				_libraryButton.Height ().EqualTo (45),
				_cameraButton.Height ().EqualTo (45),

				_cameraButton.WithSameWidth (_libraryButton),

				_albumView.Above (_libraryButton),
				_cameraView.Above (_libraryButton)
			);

			View.BackgroundColor = Configuration.BackgroundColor;
			_menuView.BackgroundColor = Configuration.BackgroundColor;
			_menuView.AddBottomBorder (UIColor.Black, 1);
			_menuTitle.TextColor = Configuration.BaseTintColor;

			var albumImage = Configuration.AlbumImage ?? UIImage.FromBundle ("ic_insert_photo");
			var cameraImage = Configuration.CameraImage ?? UIImage.FromBundle ("ic_photo_camera");
			var videoImage = Configuration.VideoImage ?? UIImage.FromBundle ("ic_videocam");
			var checkImage = Configuration.CheckImage ?? UIImage.FromBundle ("ic_check");
			var closeImage = Configuration.CloseImage ?? UIImage.FromBundle ("ic_close");

			if (Configuration.TintIcons) {
				albumImage = albumImage.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
				cameraImage = cameraImage.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
				videoImage = videoImage.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
				checkImage = checkImage.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
				closeImage = closeImage.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
			}

			_libraryButton.SetImage (albumImage, UIControlState.Normal);
			_libraryButton.SetImage (albumImage, UIControlState.Highlighted);
			_libraryButton.SetImage (albumImage, UIControlState.Selected);

			_cameraButton.SetImage (cameraImage, UIControlState.Normal);
			_cameraButton.SetImage (cameraImage, UIControlState.Highlighted);
			_cameraButton.SetImage (cameraImage, UIControlState.Selected);

			_videoButton.SetImage (videoImage, UIControlState.Normal);
			_videoButton.SetImage (videoImage, UIControlState.Highlighted);
			_videoButton.SetImage (videoImage, UIControlState.Selected);

			_closeButton.SetImage (closeImage, UIControlState.Normal);
			_closeButton.SetImage (closeImage, UIControlState.Highlighted);
			_closeButton.SetImage (closeImage, UIControlState.Selected);

			_doneButton.SetImage (checkImage, UIControlState.Normal);

			if (Configuration.TintIcons) {
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

			if (HasVideo) {
				_videoView = new VideoView {
					BackgroundColor = Configuration.BackgroundColor,
					TranslatesAutoresizingMaskIntoConstraints = false
				};

				View.AddSubviews (_videoView, _videoButton);

				View.AddConstraints (
					_videoView.AtTopOf (View),
					_videoView.WithSameLeft (_albumView),
					_videoView.WithSameRight (_albumView),

					_videoButton.ToRightOf (_cameraButton),
					_videoButton.AtBottomOf (View),
					_videoButton.AtRightOf (View),
					_videoButton.WithSameWidth (_cameraButton),
					_videoButton.Height ().EqualTo (45),

					_videoView.Above (_libraryButton)
				);
			} else {
				View.AddConstraints (
					_cameraButton.AtRightOf (View)
				);
			}

            if (Configuration.ModeOrder == ModeOrder.LibraryFirst)
                ChangeMode(Mode.Library, false);
            else
                ChangeMode(Mode.Camera, false);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			_albumView.LayoutIfNeeded ();
			_cameraView.LayoutIfNeeded ();

			if (AlbumDataSource == null && AlbumDelegate == null) {
				Console.WriteLine ("DataSource and Delegate are null for AlbumView, using default.");
				AlbumDataSource = new PhotoGalleryDataSource (_albumView, new CGSize (100, 100));
				AlbumDelegate = new PhotoGalleryDelegate (_albumView, (PhotoGalleryDataSource)AlbumDataSource);
			}

			_albumView.Initialize (AlbumDataSource, AlbumDelegate);
			_cameraView.Initialize (OnImage);

			if (HasVideo) {
				_videoView.LayoutIfNeeded ();
				_videoView.Initialize (OnVideo);
			}

			_libraryButton.TouchUpInside += LibraryButtonPressed;
			_closeButton.TouchUpInside += CloseButtonPressed;
			_cameraButton.TouchUpInside += CameraButtonPressed;
			_videoButton.TouchUpInside += VideoButtonPressed;
			_doneButton.TouchUpInside += DoneButtonPressed;

            if (_mode == Mode.Camera)
                _cameraView?.StartCamera();
            if (_mode == Mode.Video)
                _videoView?.StartCamera();
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			StopAll ();

			_libraryButton.TouchUpInside -= LibraryButtonPressed;
			_closeButton.TouchUpInside -= CloseButtonPressed;
			_cameraButton.TouchUpInside -= CameraButtonPressed;
			_videoButton.TouchUpInside -= VideoButtonPressed;
			_doneButton.TouchUpInside -= DoneButtonPressed;
		}

		public override bool PrefersStatusBarHidden () => Configuration.PreferStatusbarHidden;

		private void CloseButtonPressed (object sender, EventArgs e)
		{
			DismissViewController (true, () => {
				Closed?.Invoke (this, EventArgs.Empty);
			});
		}

		private void LibraryButtonPressed (object sender, EventArgs eventArgs)
		{
			ChangeMode(Mode.Library);
		}

		private void CameraButtonPressed (object sender, EventArgs e)
		{
			ChangeMode(Mode.Camera);
		}

		private void VideoButtonPressed (object sender, EventArgs e)
		{
			ChangeMode(Mode.Video);
		}

		private void DoneButtonPressed (object sender, EventArgs e)
		{
			var view = _albumView.ImageCropView;

			if (Configuration.CropImage) {
				var normalizedX = view.ContentOffset.X / view.ContentSize.Width;
				var normalizedY = view.ContentOffset.Y / view.ContentSize.Height;

				var normalizedWidth = view.Frame.Width / view.ContentSize.Width;
				var normalizedHeight = view.Frame.Height / view.ContentSize.Height;

				var cropRect = new CGRect (normalizedX, normalizedY, normalizedWidth, normalizedHeight);

				Console.WriteLine ("Cropping image before handing it over");
				AlbumDataSource.GetCroppedImage (cropRect, (croppedImage) => {
					ImageSelected?.Invoke (this, croppedImage);
					DismissViewController (true, () => Closed?.Invoke (this, EventArgs.Empty));
				});
			} else {
				Console.WriteLine ("Not cropping image");
				ImageSelected?.Invoke (this, view.Image);
				DismissViewController (true, () => Closed?.Invoke (this, EventArgs.Empty));
			}
		}

		private void OnVideo (NSUrl nsUrl)
		{
			VideoSelected?.Invoke (this, nsUrl);
			DismissViewController (true, () => Closed?.Invoke (this, EventArgs.Empty));
		}

		private void OnImage (UIImage uiImage)
		{
			ImageSelected?.Invoke (this, uiImage);
			DismissViewController (true, () => Closed?.Invoke (this, EventArgs.Empty));
		}

		private void ChangeMode (Mode mode, bool startStopCamera = true)
		{
			if (_mode == mode) return;

            if (startStopCamera) {
                switch (_mode) {
                    case Mode.Camera:
                        _cameraView?.StopCamera();
                        break;
                    case Mode.Video:
                        _videoView?.StopCamera();
                        break;
                }
            }
			
			_mode = mode;

			DisHighlightButtons ();

			switch (mode) {
    			case Mode.Library:
    				_albumView.Hidden = false;
    				_cameraView.Hidden = true;
    				if (_videoView != null)
    					_videoView.Hidden = true;
                    _doneButton.Hidden = false;
    				_menuTitle.Text = Configuration.CameraRollTitle;

    				HighlightButton (_libraryButton);
    				View.BringSubviewToFront (_albumView);
    				break;
    			case Mode.Camera:
    				_albumView.Hidden = true;
    				_cameraView.Hidden = false;
    				if (_videoView != null)
    					_videoView.Hidden = true;
                    _doneButton.Hidden = true;
    				_menuTitle.Text = Configuration.CameraTitle;

    				HighlightButton (_cameraButton);
    				View.BringSubviewToFront (_cameraView);
                    if (startStopCamera)
    				    _cameraView.StartCamera ();
    				break;
    			case Mode.Video:
    				_albumView.Hidden = true;
    				_cameraView.Hidden = true;
    				_videoView.Hidden = false;
                    _doneButton.Hidden = true;
    				_menuTitle.Text = Configuration.VideoTitle;

    				HighlightButton (_videoButton);
    				View.BringSubviewToFront (_videoView);
                    if (startStopCamera)
    				    _videoView.StartCamera ();
    				break;
			}

			View.BringSubviewToFront (_menuView);
		}

		private void DisHighlightButtons ()
		{
			_cameraButton.TintColor = Configuration.BaseTintColor;
			_libraryButton.TintColor = Configuration.BaseTintColor;

			var buttons = new [] { _cameraButton, _videoButton, _libraryButton };

			foreach (var button in buttons) {
				if (button == null) continue;
				if (button.Layer.Sublayers == null) continue;
				if (button.Layer.Sublayers.Length <= 1) continue;

				foreach (var layer in button.Layer.Sublayers) {
					if (layer.Name == UiKitExtensions.BorderLayerName)
						layer.RemoveFromSuperLayer ();
				}
			}

			if (_videoButton != null)
				_videoButton.TintColor = Configuration.BaseTintColor;
		}

		private static void HighlightButton (UIView button)
		{
			button.TintColor = Configuration.TintColor;
			button.AddBottomBorder (Configuration.TintColor, 3);
		}

		private void StopAll()
		{
			_videoView?.StopCamera();
			_cameraView?.StopCamera();
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations () => UIInterfaceOrientationMask.Portrait;
		public override UIInterfaceOrientation InterfaceOrientation => UIInterfaceOrientation.Portrait;
	}
}
