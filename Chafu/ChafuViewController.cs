using System;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Chafu
{
    /// <summary>
    /// <see cref="ChafuViewController"/>. Present this when you want to show images from the camera roll 
    /// and when you want to take new pictures and video
    /// </summary>
    [Register("ChafuViewController")]
    public class ChafuViewController : BaseChafuViewController
    {
        /// <summary>
        /// <see cref="EventHandler"/> which triggers when this ViewController is dismissed
        /// </summary>
		public event EventHandler Closed;

        /// <summary>
        /// <see cref="EventHandler{T}"/> with <see cref="UIImage"/> which triggers when an image is selected
        /// </summary>
		public event EventHandler<UIImage> ImageSelected;

        /// <summary>
        /// <see cref="EventHandler{T}"/> with <see cref="NSUrl"/> containing the path to the video, 
        /// which triggers when video is selected
        /// </summary>
		public event EventHandler<NSUrl> VideoSelected;

        /// <summary>
        /// <see cref="EventHandler"/> which triggers when permission was rejected to the Photo Library
        /// </summary>
        public event EventHandler CameraRollUnauthorized;

        /// <summary>
        /// <see cref="EventHandler"/> which triggers when permission was rejected to the Camera
        /// </summary>
        public event EventHandler CameraUnauthorized;

		private CameraView _cameraView;
		private VideoView _videoView;

        private Mode _mode = Mode.NotSelected;
		private UIButton _libraryButton;
		private UIButton _videoButton;
		private UIButton _cameraButton;
		private MenuView _menuView;

        /// <summary>
        /// Gets the AlbumView
        /// </summary>
        public AlbumView AlbumView { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether video tab is shown.
        /// </summary>
        /// <value><c>true</c> if has video; otherwise, <c>false</c>.</value>
		public bool HasVideo { get; set; } = false;

        /// <inheritdoc />
        public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

            _menuView = new MenuView
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

            if (CellSize == CGSize.Empty)
		        CellSize = CalculateCellSize();

			AlbumView = new AlbumView(CellSize) {
				BackgroundColor = Configuration.BackgroundColor,
				TranslatesAutoresizingMaskIntoConstraints = false
            };

			View.AddSubviews (_cameraView, AlbumView);

			View.AddConstraints (
				_menuView.Height ().EqualTo (50),
				_menuView.AtTopOf (View),
				_menuView.AtLeftOf (View),
				_menuView.AtRightOf (View),

				AlbumView.AtLeftOf (View),
				AlbumView.AtRightOf (View),
                AlbumView.Below(_menuView),

				_cameraView.WithSameLeft (AlbumView),
				_cameraView.WithSameRight (AlbumView),
                _cameraView.Below(_menuView),

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
					_videoView.WithSameLeft (AlbumView),
					_videoView.WithSameRight (AlbumView),
                    _videoView.Below(_menuView),

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
		}

        /// <inheritdoc />
        public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			AlbumView.LayoutIfNeeded ();
			_cameraView.LayoutIfNeeded ();

		    var albumDataSource = LazyDataSource(AlbumView, AlbumView.CellSize, MediaType.Photo | MediaType.Video);
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
			
            AlbumDataSource.CameraRollUnauthorized += OnCameraRollUnauthorized;
            _cameraView.CameraUnauthorized += OnCameraUnauthorized;

		    ChangeMode(Configuration.ModeOrder == ModeOrder.LibraryFirst ? 
                Mode.Library : Mode.Camera);
		}

        /// <inheritdoc />
        public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			StopAll ();

			_libraryButton.TouchUpInside -= LibraryButtonPressed;
            _menuView.Closed -= CloseButtonPressed;
            _menuView.Done -= DoneButtonPressed;
            _cameraButton.TouchUpInside -= CameraButtonPressed;
			_videoButton.TouchUpInside -= VideoButtonPressed;
            AlbumDataSource.CameraRollUnauthorized -= OnCameraRollUnauthorized;
            _cameraView.CameraUnauthorized -= OnCameraUnauthorized;
            if (_videoView != null)
                _videoView.CameraUnauthorized -= OnCameraUnauthorized;
		}

        /// <inheritdoc />
        public override bool PrefersStatusBarHidden () => Configuration.PreferStatusbarHidden;

        private void OnCameraUnauthorized(object sender, EventArgs e){
            CameraUnauthorized?.Invoke(this, e);
        }

        private void OnCameraRollUnauthorized(object sender, EventArgs e)
        {
            CameraRollUnauthorized?.Invoke(this, e);
        }

		private void CloseButtonPressed (object sender, EventArgs e)
		{
			Dismiss();
		}

        /// <summary>
        /// Dismiss the View Controller
        /// </summary>
        /// <param name="animated"><see cref="bool"/> indicating whether to dismiss animated</param>
        public void Dismiss(bool animated = true)
        {
            DismissViewController(animated, () => {
                Closed?.Invoke(this, EventArgs.Empty);
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
			if (AlbumDataSource.CurrentMediaType == ChafuMediaType.Image)
			{
				if (Configuration.CropImage) {
					Console.WriteLine("Cropping image before handing it over");
					AlbumDataSource.GetCroppedImage(croppedImage =>
					{
						ImageSelected?.Invoke(this, croppedImage);
					});
				}
				else {
                    Console.WriteLine("Not cropping image");
					ImageSelected?.Invoke(this, AlbumView.ImageCropView.Image);
				}
			}

			if (AlbumDataSource.CurrentMediaType == ChafuMediaType.Video)
			{
				var url = AlbumView.MoviePlayerController.ContentUrl;
				VideoSelected?.Invoke(this, url);
			}

			DismissViewController(true, () => Closed?.Invoke(this, EventArgs.Empty));
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

        /// <inheritdoc />
        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations () => UIInterfaceOrientationMask.Portrait;

        /// <inheritdoc />
        public override UIInterfaceOrientation InterfaceOrientation => UIInterfaceOrientation.Portrait;
	}
}
