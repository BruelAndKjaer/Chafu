using System;
using System.Linq;
using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using UIKit;
using Cirrious.FluentLayouts.Touch;

namespace Fusuma
{
    public class CameraView : BaseCameraView
    {
        private UIButton _shutterButton;

        private AVCaptureStillImageOutput _imageOutput;
        private NSObject _willEnterForegroundObserver;
        private Action<UIImage> _onImage;

		private UIView buttonContainer;

        public CameraView(IntPtr handle) 
            : base(handle) { CreateView(); }
        public CameraView() { CreateView(); }

        private void CreateView()
        {
            Hidden = true;
            BackgroundColor = Configuration.BackgroundColor;

            PreviewContainer = new UIView
            {
                AccessibilityLabel = "PreviewContainer",
                BackgroundColor = UIColor.Black,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentMode = UIViewContentMode.ScaleToFill
            };
            Add(PreviewContainer);

            buttonContainer = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentMode = UIViewContentMode.ScaleToFill,
                AccessibilityLabel = "ButtonContainer",
                BackgroundColor = Configuration.BackgroundColor
            };
            Add(buttonContainer);

            _shutterButton = new UIButton()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                AccessibilityLabel = "ShutterButton",
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                Opaque = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation
            };
            _shutterButton.TouchUpInside += OnShutter;
            _shutterButton.SetImage(UIImage.FromBundle("ic_radio_button_checked"), UIControlState.Normal);
            
            FlipButton = new UIButton()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                AccessibilityLabel = "FlipButton",
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                Opaque = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation
            };
            FlipButton.TouchUpInside += OnFlip;
            FlipButton.SetImage(UIImage.FromBundle("ic_loop"), UIControlState.Normal);

            FlashButton = new UIButton()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                AccessibilityLabel = "FlashButton",
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                Opaque = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation
            };
            FlashButton.TouchUpInside += OnFlash;
            FlashButton.SetImage(UIImage.FromBundle("ic_flash_off"), UIControlState.Normal);

			buttonContainer.Add (_shutterButton);
			buttonContainer.Add (FlipButton);
            buttonContainer.Add(FlashButton);
        }

		public override void UpdateConstraints ()
		{
			RemoveConstraints (Constraints);

			AddConstraint (NSLayoutConstraint.Create (PreviewContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal,
				PreviewContainer, NSLayoutAttribute.Width, 1, 0));

			this.AddConstraints (
				PreviewContainer.AtTopOf (this, 50),
				PreviewContainer.AtLeftOf (this),
				PreviewContainer.AtRightOf (this),

				buttonContainer.Below (PreviewContainer),
				buttonContainer.AtBottomOf (this),
				buttonContainer.AtLeftOf (this),
				buttonContainer.AtRightOf (this),

				_shutterButton.Height ().EqualTo (68),
				 _shutterButton.Width ().EqualTo (68),

				 FlipButton.Height ().EqualTo (40),
				 FlipButton.Width ().EqualTo (40),

				 FlashButton.Height ().EqualTo (40),
				 FlashButton.Width ().EqualTo (40),

				_shutterButton.WithSameCenterX (buttonContainer),
				_shutterButton.WithSameCenterY (buttonContainer),

				FlipButton.WithSameCenterY (buttonContainer),
				FlipButton.AtLeftOf (buttonContainer, 15),

				FlashButton.WithSameCenterY (buttonContainer),
				FlashButton.AtRightOf (buttonContainer, 15)
			);

			base.UpdateConstraints ();
		}

        private void OnFlash(object sender, EventArgs e)
        {
            Flash();
        }

        private void OnFlip(object sender, EventArgs e)
        {
            Flip();
        }

        private void OnShutter(object sender, EventArgs e)
        {
            if (_imageOutput == null) return;

            DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
            {
                var videoConnection = _imageOutput.ConnectionFromMediaType(AVMediaType.Video);

                _imageOutput.CaptureStillImageAsynchronously(videoConnection, (buffer, error) =>
                {
                    Session?.StopRunning();

                    var data = AVCaptureStillImageOutput.JpegStillToNSData(buffer);

                    var image = new UIImage(data);
                    var imageWidth = image.Size.Width;
                    var imageHeight = image.Size.Height;

                    var previewWidth = PreviewContainer.Frame.Width;

                    var centerCoordinate = imageHeight*0.5;

                    var imageRef = image.CGImage.WithImageInRect(new CGRect(centerCoordinate - imageWidth*0.5, 0, imageWidth,
                        imageWidth));

                    DispatchQueue.MainQueue.DispatchAsync(() =>
                    {
                        if (Configuration.CropImage)
                        {
                            var resizedImage = new UIImage(imageRef, previewWidth/imageWidth, image.Orientation);
                            _onImage?.Invoke(resizedImage);
                        }
                        else
                        {
                            _onImage?.Invoke(image);
                        }

                        Session?.StopRunning();
                        Session = null;
                        Device = null;
                        _imageOutput = null;
                    });
                });
            });
        }

        public void Initialize(Action<UIImage> onImage)
        {
            if (Session != null)
                return;

            _onImage = onImage;

            FlashOnImage = Configuration.FlashOnImage ?? UIImage.FromBundle("ic_flash_on");
            FlashOffImage = Configuration.FlashOffImage ?? UIImage.FromBundle("ic_flash_off");
            var flipImage = Configuration.FlipImage ?? UIImage.FromBundle("ic_loop");
            var shutterImage = Configuration.ShutterImage ?? UIImage.FromBundle("ic_radio_button_checked");

            if (Configuration.TintIcons)
            {
                FlashButton.TintColor = Configuration.TintColor;
                FlipButton.TintColor = Configuration.TintColor;
                _shutterButton.TintColor = Configuration.TintColor;

                FlashOnImage = FlashOnImage?.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
                FlashOffImage = FlashOffImage?.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
				flipImage = flipImage?.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
				shutterImage = shutterImage?.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
            }

			FlashButton.SetImage (FlashOffImage, UIControlState.Normal);
			FlipButton.SetImage (flipImage, UIControlState.Normal);
			_shutterButton.SetImage (shutterImage, UIControlState.Normal);

            Hidden = false;

            Session = new AVCaptureSession();

            Device = Configuration.ShowBackCameraFirst
                ? AVCaptureDevice.Devices.FirstOrDefault(d => d.Position == AVCaptureDevicePosition.Back)
                : AVCaptureDevice.Devices.FirstOrDefault(d => d.Position == AVCaptureDevicePosition.Front);

            if (Device == null)
                throw new Exception("Could not find capture device, does your device have a camera?");

            FlashButton.Hidden = !Device.HasFlash;

            try
            {
                NSError error;
                VideoInput = new AVCaptureDeviceInput(Device, out error);
                Session.AddInput(VideoInput);

                _imageOutput = new AVCaptureStillImageOutput();
                Session.AddOutput(_imageOutput);

                var videoLayer = new AVCaptureVideoPreviewLayer(Session)
                {
                    Frame = PreviewContainer.Bounds,
                    VideoGravity = AVLayerVideoGravity.ResizeAspectFill
                };

                PreviewContainer.Layer.AddSublayer(videoLayer);

                Session.StartRunning();

                FocusView = new UIView(new CGRect(0, 0, 90, 90));
                var tapRecognizer = new UITapGestureRecognizer(Focus);
                PreviewContainer.AddGestureRecognizer(tapRecognizer);
            }
            catch { /* ignored */ }

            FlashConfiguration();
            StartCamera();

            _willEnterForegroundObserver = UIApplication.Notifications.ObserveWillEnterForeground(WillEnterForeground);
        }

        private void WillEnterForeground(object sender, NSNotificationEventArgs nsNotificationEventArgs)
        {
            StartCamera();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _willEnterForegroundObserver?.Dispose();
                _willEnterForegroundObserver = null;

                _shutterButton.TouchUpInside -= OnShutter;
                FlipButton.TouchUpInside -= OnFlip;
                FlashButton.TouchUpInside -= OnFlash;
            }
            base.Dispose(disposing);
        }
    }
}
