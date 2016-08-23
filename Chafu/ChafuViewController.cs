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
        public event EventHandler CameraRollUnauthorized;
        public event EventHandler CameraUnauthorized;

		private CameraView _cameraView;
		private VideoView _videoView;

        private Mode _mode = Mode.NotSelected;
		private UIButton _libraryButton;
		private UIButton _videoButton;
		private UIButton _cameraButton;
		private ChafuMenuView _menuView;

        /// <summary>
        /// Gets the AlbumView
        /// </summary>
        public AlbumView AlbumView { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether video tab is shown.
        /// </summary>
        /// <value><c>true</c> if has video; otherwise, <c>false</c>.</value>
		public bool HasVideo { get; set; } = false;

        /// <summary>
        /// Gets the album collectionview data source. Use <see cref="LazyDataSource"/> to create your own Data Source.
        /// </summary>
        /// <value>The album data source.</value>
		public ChafuAlbumDataSource AlbumDataSource { get; private set; }

        /// <summary>
        /// Gets the album delegate. Use <see cref="LazyDelegate"/> to create your own Delegate.
        /// </summary>
        /// <value>The album delegate.</value>
		public ChafuAlbumDelegate AlbumDelegate { get; private set; }

	    public Func<AlbumView, CGSize, ChafuAlbumDataSource> LazyDataSource { get; set; } =
	        (view, size) => new PhotoGalleryDataSource(view, size);

	    public Func<AlbumView, ChafuAlbumDataSource, ChafuAlbumDelegate> LazyDelegate { get; set; } =
	        (view, source) => new PhotoGalleryDelegate(view, (PhotoGalleryDataSource)source);

        public CGSize CellSize { get; set; } = new CGSize(100, 100);

        public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

            _menuView = new ChafuMenuView
            {
                BackgroundColor = Configuration.BackgroundColor,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Opaque = false,
                AccessibilityLabel = "MenuView"
            };
            _menuView.AddBottomBorder(UIColor.Black, 1);
            View.AddSubviews (_menuView);

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
			AlbumView = new AlbumView {
				BackgroundColor = Configuration.BackgroundColor,
				TranslatesAutoresizingMaskIntoConstraints = false,
                CellSize = CellSize
            };

			View.AddSubviews (_cameraView, AlbumView);

			View.AddConstraints (
				_menuView.Height ().EqualTo (50),
				_menuView.AtTopOf (View),
				_menuView.AtLeftOf (View),
				_menuView.AtRightOf (View),

				AlbumView.AtTopOf (View),
				AlbumView.AtLeftOf (View),
				AlbumView.AtRightOf (View),

				_cameraView.AtTopOf (View),
				_cameraView.WithSameLeft (AlbumView),
				_cameraView.WithSameRight (AlbumView),

				_libraryButton.AtLeftOf (View),
				_libraryButton.AtBottomOf (View),

				_cameraButton.ToRightOf (_libraryButton),
				_cameraButton.AtBottomOf (View),

				_libraryButton.Height ().EqualTo (45),
				_cameraButton.Height ().EqualTo (45),

				_cameraButton.WithSameWidth (_libraryButton),

				AlbumView.Above (_libraryButton),
				_cameraView.Above (_libraryButton)
			);

			View.BackgroundColor = Configuration.BackgroundColor;

			var albumImage = Configuration.AlbumImage ?? UIImage.FromBundle ("ic_insert_photo");
			var cameraImage = Configuration.CameraImage ?? UIImage.FromBundle ("ic_photo_camera");
			var videoImage = Configuration.VideoImage ?? UIImage.FromBundle ("ic_videocam");

			if (Configuration.TintIcons) {
				albumImage = albumImage?.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
				cameraImage = cameraImage?.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
				videoImage = videoImage?.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
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

			if (Configuration.TintIcons) {
				_libraryButton.TintColor = Configuration.TintColor;
				_libraryButton.AdjustsImageWhenHighlighted = false;
				_cameraButton.TintColor = Configuration.TintColor;
				_cameraButton.AdjustsImageWhenHighlighted = false;
				_videoButton.TintColor = Configuration.TintColor;
				_videoButton.AdjustsImageWhenHighlighted = false;
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
					_videoView.WithSameLeft (AlbumView),
					_videoView.WithSameRight (AlbumView),

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

			AlbumView.LayoutIfNeeded ();
			_cameraView.LayoutIfNeeded ();

		    var albumDataSource = LazyDataSource(AlbumView, AlbumView.CellSize);
		    var albumDelegate = LazyDelegate(AlbumView, albumDataSource);
		    AlbumDataSource = albumDataSource;
		    AlbumDelegate = albumDelegate;

			AlbumView.Initialize (AlbumDataSource, AlbumDelegate);
			_cameraView.Initialize (OnImage);

			if (HasVideo) {
				_videoView.LayoutIfNeeded ();
				_videoView.Initialize (OnVideo);
                _videoView.CameraUnauthorized += OnCameraUnauthorized;
			}

			_libraryButton.TouchUpInside += LibraryButtonPressed;
		    _menuView.Closed += CloseButtonPressed;
		    _menuView.Done += DoneButtonPressed;
            _cameraButton.TouchUpInside += CameraButtonPressed;
			_videoButton.TouchUpInside += VideoButtonPressed;
			
            AlbumDataSource.CameraRollUnauthorized += CameraRollUnauthoized;
            _cameraView.CameraUnauthorized += OnCameraUnauthorized;

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
            _menuView.Closed -= CloseButtonPressed;
            _menuView.Done -= DoneButtonPressed;
            _cameraButton.TouchUpInside -= CameraButtonPressed;
			_videoButton.TouchUpInside -= VideoButtonPressed;
            AlbumDataSource.CameraRollUnauthorized -= CameraRollUnauthoized;
            _cameraView.CameraUnauthorized -= OnCameraUnauthorized;
            if (_videoView != null)
                _videoView.CameraUnauthorized -= OnCameraUnauthorized;
		}

		public override bool PrefersStatusBarHidden () => Configuration.PreferStatusbarHidden;

        private void OnCameraUnauthorized(object sender, EventArgs e){
            CameraUnauthorized?.Invoke(this, e);
        }

        private void CameraRollUnauthoized(object sender, EventArgs e)
        {
            CameraRollUnauthorized?.Invoke(this, e);
        }

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
			var view = AlbumView.ImageCropView;

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
    				AlbumView.Hidden = false;
    				_cameraView.Hidden = true;
    				if (_videoView != null)
    					_videoView.Hidden = true;
                    _menuView.DoneButtonHidden = false;
    				_menuView.Title = Configuration.CameraRollTitle;

    				HighlightButton (_libraryButton);
    				View.BringSubviewToFront (AlbumView);
    				break;
    			case Mode.Camera:
    				AlbumView.Hidden = true;
    				_cameraView.Hidden = false;
    				if (_videoView != null)
    					_videoView.Hidden = true;
                    _menuView.DoneButtonHidden = true;
                    _menuView.Title = Configuration.CameraTitle;

    				HighlightButton (_cameraButton);
    				View.BringSubviewToFront (_cameraView);
                    if (startStopCamera)
    				    _cameraView.StartCamera ();
    				break;
    			case Mode.Video:
    				AlbumView.Hidden = true;
    				_cameraView.Hidden = true;
    				_videoView.Hidden = false;
                    _menuView.DoneButtonHidden = true;
                    _menuView.Title = Configuration.VideoTitle;

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
			    if (button?.Layer.Sublayers == null) continue;

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
