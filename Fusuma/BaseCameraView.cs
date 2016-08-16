using System;
using AVFoundation;
using CoreGraphics;
using Foundation;
using UIKit;
using Cirrious.FluentLayouts.Touch;

namespace Fusuma
{
    public class BaseCameraView : UIView
    {
        protected AVCaptureSession Session;
        protected UIView FocusView;
        protected UIImage FlashOnImage;
        protected UIImage FlashOffImage;
        protected UIView PreviewContainer;

        protected UIButton FlipButton;
        protected UIButton FlashButton;
        protected UIButton ShutterButton;
        protected UIView ButtonContainer;

        protected AVCaptureDevice Device;
        protected AVCaptureDeviceInput VideoInput;

        protected static bool CameraAvailable
            => AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video) == AVAuthorizationStatus.Authorized;

        public BaseCameraView(IntPtr handle) 
            : base(handle) { }

        public BaseCameraView() { 
            Hidden = true;
            BackgroundColor = Configuration.BackgroundColor;

            PreviewContainer = new UIView {
                AccessibilityLabel = "PreviewContainer",
                BackgroundColor = UIColor.Black,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentMode = UIViewContentMode.ScaleToFill
            };
            Add(PreviewContainer);

            ButtonContainer = new UIView {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentMode = UIViewContentMode.ScaleToFill,
                AccessibilityLabel = "ButtonContainer",
                ExclusiveTouch = true
            };

            Add(ButtonContainer);

            FlipButton = new UIButton {
                TranslatesAutoresizingMaskIntoConstraints = false,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                AccessibilityLabel = "FlipButton",
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                Opaque = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation
            };
            FlipButton.SetImage(UIImage.FromBundle("ic_loop"), UIControlState.Normal);
        
            FlashButton = new UIButton {
                TranslatesAutoresizingMaskIntoConstraints = false,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                AccessibilityLabel = "FlashButton",
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                Opaque = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation
            };
            FlashButton.SetImage(UIImage.FromBundle("ic_flash_off"), UIControlState.Normal);

            ShutterButton = new UIButton {
                TranslatesAutoresizingMaskIntoConstraints = false,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                AccessibilityLabel = "ShutterButton",
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                Opaque = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation
            };
            ShutterButton.SetImage(UIImage.FromBundle("ic_radio_button_checked"), UIControlState.Normal);
        
            ButtonContainer.Add(FlipButton);
            ButtonContainer.Add(FlashButton);
            ButtonContainer.Add(ShutterButton);

            if (Configuration.ShowSquare) {
                ButtonContainer.BackgroundColor = Configuration.BackgroundColor;

                this.AddConstraints(
                    ButtonContainer.Below(PreviewContainer), 
                    PreviewContainer.Width().EqualTo().HeightOf(PreviewContainer)
                );
            } else {
                this.AddConstraints(
                    ButtonContainer.Height().EqualTo(100), 
                    PreviewContainer.AtBottomOf(this)
                );
            }

            this.AddConstraints(
                PreviewContainer.AtTopOf(this, 50),
                PreviewContainer.AtLeftOf(this),
                PreviewContainer.AtRightOf(this),

                ButtonContainer.AtBottomOf(this),
                ButtonContainer.AtLeftOf(this),
                ButtonContainer.AtRightOf(this),

                ShutterButton.Height().EqualTo(68),
                ShutterButton.Width().EqualTo(68),

                FlipButton.Height().EqualTo(40),
                FlipButton.Width().EqualTo(40),

                FlashButton.Height().EqualTo(40),
                FlashButton.Width().EqualTo(40),

                ShutterButton.WithSameCenterX(ButtonContainer),
                ShutterButton.WithSameCenterY(ButtonContainer),

                FlipButton.WithSameCenterY(ButtonContainer),
                FlipButton.AtLeftOf(ButtonContainer, 15),

                FlashButton.WithSameCenterY(ButtonContainer),
                FlashButton.AtRightOf(ButtonContainer, 15)
            );

            BringSubviewToFront(ButtonContainer);
        }

        protected void Initialize()
        {
            if (Session != null)
                return;

            FlashOnImage = Configuration.FlashOnImage ?? UIImage.FromBundle("ic_flash_on");
            FlashOffImage = Configuration.FlashOffImage ?? UIImage.FromBundle("ic_flash_off");
            var flipImage = Configuration.FlipImage ?? UIImage.FromBundle("ic_loop");

            if (Configuration.TintIcons) {
                FlashButton.TintColor = Configuration.TintColor;
                FlipButton.TintColor = Configuration.TintColor;

                FlashOnImage = FlashOnImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                FlashOffImage = FlashOffImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                flipImage = flipImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }

            FlashButton.SetImage(FlashOffImage, UIControlState.Normal);
            FlipButton.SetImage(flipImage, UIControlState.Normal);

            FocusView = new UIView(new CGRect(0, 0, 90, 90));
            var tapRecognizer = new UITapGestureRecognizer(Focus);
            PreviewContainer.AddGestureRecognizer(tapRecognizer);

            Hidden = false;
        }

        public void StartCamera()
        {
            var status = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);
            if (status == AVAuthorizationStatus.Authorized)
                Session?.StartRunning();
            else if (status == AVAuthorizationStatus.Denied || status == AVAuthorizationStatus.Restricted)
                Session?.StopRunning();
        }

        public virtual void StopCamera()
        {
            Session?.StopRunning();
        }

        protected void Focus(UITapGestureRecognizer recognizer)
        {
            var point = recognizer.LocationInView(this);
            var viewSize = Bounds.Size;
            var newPoint = new CGPoint(point.Y / viewSize.Height, 1.0 - point.X / viewSize.Width);

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

            FocusView.Alpha = 0;
            FocusView.Center = point;
            FocusView.BackgroundColor = UIColor.Clear;
            FocusView.Layer.BorderColor = Configuration.BaseTintColor.CGColor;
            FocusView.Layer.BorderWidth = 1;
            FocusView.Transform = CGAffineTransform.MakeScale(1.0f, 1.0f);
            Add(FocusView);

            AnimateNotify(0.8, 0.0, 0.8f, 3.0f, UIViewAnimationOptions.CurveEaseIn, () =>
            {
                FocusView.Alpha = 1;
                FocusView.Transform = CGAffineTransform.MakeScale(0.7f, 0.7f);
            }, finished =>
            {
                FocusView.Transform = CGAffineTransform.MakeScale(1.0f, 1.0f);
                FocusView.RemoveFromSuperview();
            });
        }

        protected void Flip()
        {
            if (!CameraAvailable) return;

            Session?.StopRunning();

            try
            {
                Session?.BeginConfiguration();

                if (Session != null)
                {
                    foreach (var input in Session.Inputs)
                    {
                        Session.RemoveInput(input);
                    }

                    var position = VideoInput.Device.Position == AVCaptureDevicePosition.Front
                        ? AVCaptureDevicePosition.Back
                        : AVCaptureDevicePosition.Front;

                    foreach (var device in AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video))
                    {
                        if (device.Position == position)
                        {
                            NSError error;
                            VideoInput = new AVCaptureDeviceInput(device, out error);
                            Session.AddInput(VideoInput);
                        }
                    }
                }

                Session?.CommitConfiguration();
            }
            catch { }

            Session?.StartRunning();
        }

        protected void Flash(bool torch)
        {
            if (!CameraAvailable) return;

            try
            {
                NSError error;
                if (Device.LockForConfiguration(out error)) {
                    if (torch && Device.HasTorch) {
                        var mode = Device.TorchMode;
                        if (mode == AVCaptureTorchMode.Off){
                            Device.TorchMode = AVCaptureTorchMode.On;
                            FlashButton.SetImage(FlashOnImage, UIControlState.Normal);
                        } else {
                            Device.TorchMode = AVCaptureTorchMode.Off;
                            FlashButton.SetImage(FlashOffImage, UIControlState.Normal);
                        }
                    }
                    else if (!torch && Device.HasFlash){
                        var mode = Device.FlashMode;
                        if (mode == AVCaptureFlashMode.Off) {
                            Device.FlashMode = AVCaptureFlashMode.On;
                            FlashButton.SetImage(FlashOnImage, UIControlState.Normal);
                        } else {
                            Device.FlashMode = AVCaptureFlashMode.Off;
                            FlashButton.SetImage(FlashOffImage, UIControlState.Normal);
                        }
                    }

                    Device.UnlockForConfiguration();
                }
            }
            catch
            {
                FlashButton.SetImage(FlashOffImage, UIControlState.Normal);
            }
        }

        protected void FlashConfiguration(bool torch)
        {
            try
            {
                if (Device == null) return;

                NSError error;
                if (Device.LockForConfiguration(out error)) 
                {
                    if (!torch && Device.HasFlash) 
                    {
                        Device.FlashMode = AVCaptureFlashMode.Off;
                        FlashButton.SetImage(FlashOffImage, UIControlState.Normal);
                    } 
                    else if (torch && Device.HasTorch) 
                    {
                        Device.TorchMode = AVCaptureTorchMode.Off;
                        FlashButton.SetImage(FlashOffImage, UIControlState.Normal);
                    }
                    Device.UnlockForConfiguration();
                }
            }
            catch { /* ignore */ }
        }
    }
}
