using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AVFoundation;
using CoreGraphics;
using Foundation;
using HealthKit;
using UIKit;

namespace Fusuma
{
    public class CameraView : UIView
    {
        private UIView _previewContainer;
        private UIButton _flipButton;
        private UIButton _flashButton;
        private UIButton _shutterButton;
        private UIImage _flashOnImage;
        private UIImage _flashOffImage;

        private AVCaptureSession _session;
        private AVCaptureDevice _device;
        private AVCaptureStillImageOutput _imageOutput;
        private NSObject _willEnterForegroundObserver;

        public CameraView(IntPtr handle) 
            : base(handle) { CreateView(); }
        public CameraView() { CreateView(); }

        private void CreateView()
        {
            Hidden = true;
            ContentMode = UIViewContentMode.ScaleToFill;
            Frame = new CGRect(0,0, 400, 600);
            AutoresizingMask = UIViewAutoresizing.All;
            BackgroundColor = Configuration.BackgroundColor;

            _previewContainer = new UIView
            {
                AccessibilityLabel = "PreviewContainer",
                Frame = new CGRect(0, 50, 400, 400),
                BackgroundColor = UIColor.Black,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentMode = UIViewContentMode.ScaleToFill
            };
            Add(_previewContainer);
            AddConstraint(NSLayoutConstraint.Create(_previewContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal,
                _previewContainer, NSLayoutAttribute.Width, 1, 0));

            var buttonContainer = new UIView
            {
                Frame = new CGRect(0, 450, 400, 150),
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentMode = UIViewContentMode.ScaleToFill,
                AccessibilityLabel = "ButtonContainer",
                BackgroundColor = Configuration.BackgroundColor
            };
            Add(buttonContainer);

            _shutterButton = new UIButton(new CGRect(166, 41, 68, 68))
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
            buttonContainer.Add(_shutterButton);

            var heightConstraint = NSLayoutConstraint.Create(_shutterButton, NSLayoutAttribute.Height,
                NSLayoutRelation.Equal);
            heightConstraint.Constant = 68;
            var widthConstraint = NSLayoutConstraint.Create(_shutterButton, NSLayoutAttribute.Width,
                NSLayoutRelation.Equal);
            widthConstraint.Constant = 68;
            _shutterButton.AddConstraints(new[] { heightConstraint, widthConstraint});

            _flipButton = new UIButton(new CGRect(15, 55, 40, 40))
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                AccessibilityLabel = "FlipButton",
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                Opaque = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation
            };
            _flipButton.TouchUpInside += OnFlip;
            _flipButton.SetImage(UIImage.FromBundle("ic_loop"), UIControlState.Normal);
            buttonContainer.Add(_flipButton);

            heightConstraint = NSLayoutConstraint.Create(_flipButton, NSLayoutAttribute.Height,
                NSLayoutRelation.Equal);
            heightConstraint.Constant = 40;
            widthConstraint = NSLayoutConstraint.Create(_flipButton, NSLayoutAttribute.Width,
                NSLayoutRelation.Equal);
            widthConstraint.Constant = 40;
            _flipButton.AddConstraints(new[] { heightConstraint, widthConstraint });

            _flashButton = new UIButton(new CGRect(15, 55, 40, 40))
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                AccessibilityLabel = "FlashButton",
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                Opaque = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation
            };
            _flashButton.TouchUpInside += OnFlash;
            _flashButton.SetImage(UIImage.FromBundle("ic_flash_off"), UIControlState.Normal);
            buttonContainer.Add(_flashButton);

            heightConstraint = NSLayoutConstraint.Create(_flashButton, NSLayoutAttribute.Height,
                NSLayoutRelation.Equal);
            heightConstraint.Constant = 40;
            widthConstraint = NSLayoutConstraint.Create(_flashButton, NSLayoutAttribute.Width,
                NSLayoutRelation.Equal);
            widthConstraint.Constant = 40;
            _flashButton.AddConstraints(new[] { heightConstraint, widthConstraint });

            buttonContainer.AddConstraints(new []
            {
                NSLayoutConstraint.Create(_shutterButton, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, 
                    buttonContainer, NSLayoutAttribute.CenterX, 1, 0),    
                NSLayoutConstraint.Create(_flashButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal,
                    buttonContainer, NSLayoutAttribute.Trailing, 1, 15),
                NSLayoutConstraint.Create(_shutterButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal,
                    buttonContainer, NSLayoutAttribute.CenterY, 1, 0),
                NSLayoutConstraint.Create(_flashButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal,
                    buttonContainer, NSLayoutAttribute.CenterY, 1, 0),
                NSLayoutConstraint.Create(_flipButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal,
                    buttonContainer, NSLayoutAttribute.CenterY, 1, 0),
                NSLayoutConstraint.Create(_flipButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal,
                    buttonContainer, NSLayoutAttribute.Leading, 1, 15)
            });

            AddConstraints(new []
            {
                NSLayoutConstraint.Create(buttonContainer, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, 
                    this, NSLayoutAttribute.Bottom, 1, 0),
                NSLayoutConstraint.Create(buttonContainer, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Trailing, 1, 0), 
                NSLayoutConstraint.Create(buttonContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal,
                    _previewContainer, NSLayoutAttribute.Bottom, 1, 0),
                NSLayoutConstraint.Create(buttonContainer, NSLayoutAttribute.Leading, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Leading, 1, 0),
                NSLayoutConstraint.Create(_previewContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Top, 1, 50),
                NSLayoutConstraint.Create(_previewContainer, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Trailing, 1, 0),
                NSLayoutConstraint.Create(_previewContainer, NSLayoutAttribute.Leading, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Leading, 1, 0)
            });
        }

        private void OnFlash(object sender, EventArgs e)
        {
                
        }

        private void OnFlip(object sender, EventArgs e)
        {

        }

        private void OnShutter(object sender, EventArgs e)
        {
                
        }

        public void Initialize()
        {
            if (_session != null)
                return;

            _flashOnImage = Configuration.FlashOnImage ?? UIImage.FromBundle("ic_flash_on");
            _flashOffImage = Configuration.FlashOffImage ?? UIImage.FromBundle("ic_flash_off");
            var flipImage = Configuration.FlipImage ?? UIImage.FromBundle("ic_loop");
            var shutterImage = Configuration.ShutterImage ?? UIImage.FromBundle("ic_radio_button_checked");

            if (Configuration.TintIcons)
            {
                _flashButton.TintColor = Configuration.TintColor;
                _flipButton.TintColor = Configuration.TintColor;
                _shutterButton.TintColor = Configuration.TintColor;

                _flashButton.SetImage(_flashOffImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                    UIControlState.Normal);
                _flipButton.SetImage(flipImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                    UIControlState.Normal);
                _shutterButton.SetImage(shutterImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate),
                    UIControlState.Normal);
            }
            else
            {
                _flashButton.SetImage(_flashOffImage, UIControlState.Normal);
                _flipButton.SetImage(flipImage, UIControlState.Normal);
                _shutterButton.SetImage(shutterImage, UIControlState.Normal);
            }

            Hidden = false;

            _session = new AVCaptureSession();

            _device = Configuration.ShowBackCameraFirst
                ? AVCaptureDevice.Devices.FirstOrDefault(d => d.Position == AVCaptureDevicePosition.Back)
                : AVCaptureDevice.Devices.FirstOrDefault(d => d.Position == AVCaptureDevicePosition.Front);

            if (_device == null)
                throw new Exception("Could not find capture device, does your device have a camera?");

            _flashButton.Hidden = !_device.HasFlash;

            try
            {
                NSError error;
                var videoInput = new AVCaptureDeviceInput(_device, out error);
                _session.AddInput(videoInput);

                _imageOutput = new AVCaptureStillImageOutput();
                _session.AddOutput(_imageOutput);

                var videoLayer = new AVCaptureVideoPreviewLayer(_session)
                {
                    Frame = _previewContainer.Bounds,
                    VideoGravity = AVLayerVideoGravity.ResizeAspectFill
                };

                _previewContainer.Layer.AddSublayer(videoLayer);

                _session.StartRunning();
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

        public void StartCamera()
        {
            var status = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);
            if (status == AVAuthorizationStatus.Authorized)
                _session?.StartRunning();
            else if (status == AVAuthorizationStatus.Denied || status == AVAuthorizationStatus.Restricted)
                _session?.StopRunning();
        }

        public void StopCamera()
        {
            _session?.StopRunning();
        }

        private void FlashConfiguration()
        {

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _willEnterForegroundObserver?.Dispose();
                _willEnterForegroundObserver = null;

                _shutterButton.TouchUpInside -= OnShutter;
                _flipButton.TouchUpInside -= OnFlip;
                _flashButton.TouchUpInside -= OnFlash;
            }
            base.Dispose(disposing);
        }
    }
}
