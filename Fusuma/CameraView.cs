using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AVFoundation;
using CoreFoundation;
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
        private UIView _focusView;

        private AVCaptureSession _session;
        private AVCaptureDevice _device;
        private AVCaptureStillImageOutput _imageOutput;
        private NSObject _willEnterForegroundObserver;
        private AVCaptureDeviceInput _videoInput;
        private Action<UIImage> _onImage;

        private static bool CameraAvailable
            => AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video) == AVAuthorizationStatus.Authorized;

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
                    buttonContainer, NSLayoutAttribute.Trailing, 1, -15),
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
            if (!CameraAvailable) return;

            try
            {
                if (!_device.HasFlash) return;

                NSError error;
                if (_device.LockForConfiguration(out error))
                {
                    var mode = _device.FlashMode;
                    if (mode == AVCaptureFlashMode.Off)
                    {
                        _device.FlashMode = AVCaptureFlashMode.On;
                        _flashButton.SetImage(_flashOnImage, UIControlState.Normal);
                    }
                    else
                    {
                        _device.FlashMode = AVCaptureFlashMode.Off;
                        _flashButton.SetImage(_flashOffImage, UIControlState.Normal);
                    }
                    _device.UnlockForConfiguration();
                }
            }
            catch
            {
                _flashButton.SetImage(_flashOffImage, UIControlState.Normal);
            }
        }

        private void OnFlip(object sender, EventArgs e)
        {
            if (!CameraAvailable) return;

            _session?.StopRunning();

            try
            {
                _session?.BeginConfiguration();

                if (_session != null)
                {
                    foreach (var input in _session.Inputs)
                    {
                        _session.RemoveInput(input);
                    }

                    var position = _videoInput.Device.Position == AVCaptureDevicePosition.Front
                        ? AVCaptureDevicePosition.Back
                        : AVCaptureDevicePosition.Front;

                    foreach (var device in AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video))
                    {
                        if (device.Position == position)
                        {
                            NSError error;
                            _videoInput = new AVCaptureDeviceInput(device, out error);
                            _session.AddInput(_videoInput);
                        }
                    }
                }

                _session?.CommitConfiguration();
            }
            catch { }

            _session?.StartRunning();
        }

        private void OnShutter(object sender, EventArgs e)
        {
            if (_imageOutput == null) return;

            DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
            {
                var videoConnection = _imageOutput.ConnectionFromMediaType(AVMediaType.Video);

                _imageOutput.CaptureStillImageAsynchronously(videoConnection, (buffer, error) =>
                {
                    _session?.StopRunning();

                    var data = AVCaptureStillImageOutput.JpegStillToNSData(buffer);

                    var image = new UIImage(data);
                    var imageWidth = image.Size.Width;
                    var imageHeight = image.Size.Height;

                    var previewWidth = _previewContainer.Frame.Width;

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

                        _session?.StopRunning();
                        _session = null;
                        _device = null;
                        _imageOutput = null;
                    });
                });
            });
        }

        public void Initialize(Action<UIImage> onImage)
        {
            if (_session != null)
                return;

            _onImage = onImage;

            _flashOnImage = Configuration.FlashOnImage ?? UIImage.FromBundle("ic_flash_on");
            _flashOffImage = Configuration.FlashOffImage ?? UIImage.FromBundle("ic_flash_off");
            var flipImage = Configuration.FlipImage ?? UIImage.FromBundle("ic_loop");
            var shutterImage = Configuration.ShutterImage ?? UIImage.FromBundle("ic_radio_button_checked");

            if (Configuration.TintIcons)
            {
                _flashButton.TintColor = Configuration.TintColor;
                _flipButton.TintColor = Configuration.TintColor;
                _shutterButton.TintColor = Configuration.TintColor;

				_flashOnImage = _flashOnImage?.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
				_flashOffImage = _flashOffImage?.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
				flipImage = flipImage?.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
				shutterImage = shutterImage?.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
            }

			_flashButton.SetImage (_flashOffImage, UIControlState.Normal);
			_flipButton.SetImage (flipImage, UIControlState.Normal);
			_shutterButton.SetImage (shutterImage, UIControlState.Normal);

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
                _videoInput = new AVCaptureDeviceInput(_device, out error);
                _session.AddInput(_videoInput);

                _imageOutput = new AVCaptureStillImageOutput();
                _session.AddOutput(_imageOutput);

                var videoLayer = new AVCaptureVideoPreviewLayer(_session)
                {
                    Frame = _previewContainer.Bounds,
                    VideoGravity = AVLayerVideoGravity.ResizeAspectFill
                };

                _previewContainer.Layer.AddSublayer(videoLayer);

                _session.StartRunning();

                _focusView = new UIView(new CGRect(0, 0, 90, 90));
                var tapRecognizer = new UITapGestureRecognizer(Focus);
                _previewContainer.AddGestureRecognizer(tapRecognizer);
            }
            catch { /* ignored */ }

            FlashConfiguration();
            StartCamera();

            _willEnterForegroundObserver = UIApplication.Notifications.ObserveWillEnterForeground(WillEnterForeground);
        }

        private void Focus(UITapGestureRecognizer recognizer)
        {
            var point = recognizer.LocationInView(this);
            var viewSize = Bounds.Size;
            var newPoint = new CGPoint(point.Y/viewSize.Height, 1.0 - point.X/viewSize.Width);

            var device = AVCaptureDevice.DefaultDeviceWithMediaType(AVMediaType.Video);

            NSError error;
            if (device.LockForConfiguration(out error))
            {
                if (device.IsFocusModeSupported(AVCaptureFocusMode.AutoFocus))
                {
                    device.FocusMode = AVCaptureFocusMode.AutoFocus;
                    device.FocusPointOfInterest = newPoint;
                }

                if (device.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
                {
                    device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
                    device.ExposurePointOfInterest = newPoint;
                }

                device.UnlockForConfiguration();
            }

            _focusView.Alpha = 0;
            _focusView.Center = point;
            _focusView.BackgroundColor = UIColor.Clear;
            _focusView.Layer.BorderColor = Configuration.BaseTintColor.CGColor;
            _focusView.Layer.BorderWidth = 1;
            _focusView.Transform = CGAffineTransform.MakeScale(1.0f, 1.0f);
            Add(_focusView);

            AnimateNotify(0.8, 0.0, 0.8f, 3.0f, UIViewAnimationOptions.CurveEaseIn, () =>
            {
                _focusView.Alpha = 1;
                _focusView.Transform = CGAffineTransform.MakeScale(0.7f, 0.7f);
            }, finished =>
            {
                _focusView.Transform = CGAffineTransform.MakeScale(1.0f, 1.0f);
                _focusView.RemoveFromSuperview();
            });
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
            try
            {
                if (_device != null && _device.HasFlash)
                {
                    NSError error;
                    if (_device.LockForConfiguration(out error))
                    {
                        _device.FlashMode = AVCaptureFlashMode.Off;
                        _flashButton.SetImage(_flashOffImage, UIControlState.Normal);

                        _device.UnlockForConfiguration();
                    }
                }
            }
            catch { /* ignore */ }
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
