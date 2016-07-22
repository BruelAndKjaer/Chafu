using System;
using System.Collections.Generic;
using System.Text;
using AVFoundation;
using CoreGraphics;
using Foundation;
using UIKit;

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

        protected AVCaptureDevice Device;
        protected AVCaptureDeviceInput VideoInput;

        protected static bool CameraAvailable
            => AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video) == AVAuthorizationStatus.Authorized;

        public BaseCameraView(IntPtr handle) 
            : base(handle) { }

        public BaseCameraView() { }

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

        protected void Flash()
        {
            if (!CameraAvailable) return;

            try
            {
                if (!Device.HasFlash) return;

                NSError error;
                if (Device.LockForConfiguration(out error))
                {
                    var mode = Device.FlashMode;
                    if (mode == AVCaptureFlashMode.Off)
                    {
                        Device.FlashMode = AVCaptureFlashMode.On;
                        FlashButton.SetImage(FlashOnImage, UIControlState.Normal);
                    }
                    else
                    {
                        Device.FlashMode = AVCaptureFlashMode.Off;
                        FlashButton.SetImage(FlashOffImage, UIControlState.Normal);
                    }
                    Device.UnlockForConfiguration();
                }
            }
            catch
            {
                FlashButton.SetImage(FlashOffImage, UIControlState.Normal);
            }
        }

        protected void FlashConfiguration()
        {
            try
            {
                if (Device != null && Device.HasFlash)
                {
                    NSError error;
                    if (Device.LockForConfiguration(out error))
                    {
                        Device.FlashMode = AVCaptureFlashMode.Off;
                        FlashButton.SetImage(FlashOffImage, UIControlState.Normal);

                        Device.UnlockForConfiguration();
                    }
                }
            }
            catch { /* ignore */ }
        }
    }
}
