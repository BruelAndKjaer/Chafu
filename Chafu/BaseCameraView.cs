using System;
using AVFoundation;
using CoreGraphics;
using Foundation;
using UIKit;
using Cirrious.FluentLayouts.Touch;
using CoreFoundation;
using System.Linq;
using CoreAnimation;
using System.Collections.Generic;

namespace Chafu
{
    /// <summary>
    /// Abstract base class used for camera and video view
    /// </summary>
    public abstract class BaseCameraView : UIView, IAVCaptureMetadataOutputObjectsDelegate
    {
        private CALayer _overlayLayer;
        private Dictionary<nint, CALayer> _faceLayers = new Dictionary<nint, CALayer>();

        /// <summary>
        /// Get or set the <see cref="AVCaptureSession"/>
        /// </summary>
        protected AVCaptureSession Session { get; set; }

        /// <summary>
        /// Get or set the <see cref="UIView"/> for indicating focus
        /// </summary>
        protected UIView FocusView { get; private set; }

        /// <summary>
        /// Get or set the <see cref="UIImage"/> for toggling the flash on
        /// </summary>
        protected UIImage FlashOnImage { get; private set; }

        /// <summary>
        /// Get or set the <see cref="UIImage"/> for toggling the flash off
        /// </summary>
        protected UIImage FlashOffImage { get; private set; }

        /// <summary>
        /// Get or set the <see cref="UIView"/> for the preview
        /// </summary>
        protected UIView PreviewContainer { get; }

        /// <summary>
        /// Get or set the <see cref="UIButton"/> used to flip the camera
        /// </summary>
        protected UIButton FlipButton { get; }

        /// <summary>
        /// Get or set the <see cref="UIButton"/> used to toggle flash
        /// </summary>
        protected UIButton FlashButton { get; }

        /// <summary>
        /// Get or set the <see cref="UIButton"/> used for the shutter button
        /// </summary>
        protected UIButton ShutterButton { get; }

        /// <summary>
        /// Get or set the container used for flip, flash and shutter buttons
        /// </summary>
        protected UIView ButtonContainer { get; }

        /// <summary>
        /// Get or set the <see cref="AVCaptureDevice"/>
        /// </summary>
        protected AVCaptureDevice Device { get; set; }

        /// <summary>
        /// Get or set the <see cref="AVCaptureDeviceInput"/>, video input use for preview
        /// </summary>
        protected AVCaptureDeviceInput VideoInput { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AVCaptureVideoPreviewLayer"/>, video preview layer.
        /// </summary>
        /// <value>The video preview layer.</value>
        protected AVCaptureVideoPreviewLayer VideoPreviewLayer { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AVCaptureMetadataOutput"/> used for face detection.
        /// </summary>
        /// <value>The face detection output.</value>
        protected AVCaptureMetadataOutput FaceDetectionOutput { get; set; }

        /// <summary>
        /// <see cref="EventHandler"/> which fires when access to the camera was rejected
        /// </summary>
        public event EventHandler CameraUnauthorized;

        /// <summary>
        /// Get whether the usage of the camera is authorized
        /// </summary>
        protected static bool CameraAvailable
            => AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video) ==
                              AVAuthorizationStatus.Authorized;

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="handle"></param>
        protected BaseCameraView(IntPtr handle)
            : base(handle) { }

        /// <summary>
        /// Initialize a new Camera View
        /// </summary>
        protected BaseCameraView()
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

            ButtonContainer = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentMode = UIViewContentMode.ScaleToFill,
                AccessibilityLabel = "ButtonContainer",
                ExclusiveTouch = true
            };

            Add(ButtonContainer);

            FlipButton = new UIButton
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                AccessibilityLabel = "FlipButton",
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                Opaque = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation
            };
            FlipButton.SetImage(UIImage.FromBundle("ic_loop"), UIControlState.Normal);

            FlashButton = new UIButton
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                AccessibilityLabel = "FlashButton",
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                Opaque = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation
            };
            FlashButton.SetImage(UIImage.FromBundle("ic_flash_off"), UIControlState.Normal);

            ShutterButton = new UIButton
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                AccessibilityLabel = "ShutterButton",
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                Opaque = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation
            };
            ShutterButton.SetImage(UIImage.FromBundle("ic_radio_button_checked"),
                                   UIControlState.Normal);

            ButtonContainer.Add(FlipButton);
            ButtonContainer.Add(FlashButton);
            ButtonContainer.Add(ShutterButton);

            if (Configuration.ShowSquare)
            {
                ButtonContainer.BackgroundColor = Configuration.BackgroundColor;

                this.AddConstraints(
                    ButtonContainer.Below(PreviewContainer),
                    PreviewContainer.Width().EqualTo().HeightOf(PreviewContainer)
                );
            }
            else {
                this.AddConstraints(
                    ButtonContainer.Height().EqualTo(100),
                    PreviewContainer.AtBottomOf(this)
                );
            }

            this.AddConstraints(
                PreviewContainer.AtTopOf(this),
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

        /// <summary>
        /// Set up images for buttons and set up gesture recognizers
        /// </summary>
        protected void Initialize()
        {
            if (Session != null)
                return;

            FlashOnImage = Configuration.FlashOnImage ?? UIImage.FromBundle("ic_flash_on");
            FlashOffImage = Configuration.FlashOffImage ?? UIImage.FromBundle("ic_flash_off");
            var flipImage = Configuration.FlipImage ?? UIImage.FromBundle("ic_loop");

            if (Configuration.TintIcons)
            {
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

        /// <summary>
        /// Start the camera
        /// </summary>
        public virtual void StartCamera()
        {
            var status = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);
            if (status == AVAuthorizationStatus.Authorized)
                Session?.StartRunning();
            else if (status == AVAuthorizationStatus.Denied ||
                     status == AVAuthorizationStatus.Restricted)
            {
                CameraUnauthorized?.Invoke(this, EventArgs.Empty);
                Session?.StopRunning();
            }
        }

        /// <summary>
        /// Stop the camera
        /// </summary>
        public virtual void StopCamera()
        {
            Session?.StopRunning();
        }

        private void Focus(UITapGestureRecognizer recognizer)
        {
            var point = recognizer.LocationInView(this);
            var viewSize = Bounds.Size;
            var newPoint =
                new CGPoint(point.Y / viewSize.Height, 1.0 - point.X / viewSize.Width);

            var device = AVCaptureDevice.DefaultDeviceWithMediaType(AVMediaType.Video);

            NSError error;
            if (device.LockForConfiguration(out error))
            {
                if (device.IsFocusModeSupported(AVCaptureFocusMode.AutoFocus))
                {
                    device.FocusMode = AVCaptureFocusMode.AutoFocus;
                    device.FocusPointOfInterest = newPoint;
                }

                if (device.IsExposureModeSupported(
                    AVCaptureExposureMode.ContinuousAutoExposure))
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

        /// <summary>
        /// Flip the camera
        /// </summary>
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

                    var position =
                        VideoInput.Device.Position == AVCaptureDevicePosition.Front
                        ? AVCaptureDevicePosition.Back
                        : AVCaptureDevicePosition.Front;

                    foreach (var device in
                             AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video))
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

        /// <summary>
        /// Toggle the flash or torch
        /// </summary>
        /// <param name="torch"><see cref="bool"/> indicating whether to use torch for video</param>
        protected void Flash(bool torch)
        {
            if (!CameraAvailable) return;

            try
            {
                NSError error;
                if (Device.LockForConfiguration(out error))
                {
                    if (torch && Device.HasTorch)
                    {
                        var mode = Device.TorchMode;
                        if (mode == AVCaptureTorchMode.Off)
                        {
                            Device.TorchMode = AVCaptureTorchMode.On;
                            FlashButton.SetImage(FlashOnImage, UIControlState.Normal);
                        }
                        else {
                            Device.TorchMode = AVCaptureTorchMode.Off;
                            FlashButton.SetImage(FlashOffImage, UIControlState.Normal);
                        }
                    }
                    else if (!torch && Device.HasFlash)
                    {
                        var mode = Device.FlashMode;
                        if (mode == AVCaptureFlashMode.Off)
                        {
                            Device.FlashMode = AVCaptureFlashMode.On;
                            FlashButton.SetImage(FlashOnImage, UIControlState.Normal);
                        }
                        else {
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

        /// <summary>
        /// Create flash configuration
        /// </summary>
        /// <param name="torch"><see cref="bool"/></param>
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

        /// <summary>
        /// Display No Camera when camera is missing or running in simulator
        /// </summary>
        protected void NoCameraAvailable()
        {
            var noCameraText = new UILabel
            {
                Text = Configuration.NoCameraText,
                Font = UIFont.PreferredTitle1,
                LineBreakMode = UILineBreakMode.WordWrap,
                TextColor = Configuration.BaseTintColor,
                BackgroundColor = Configuration.TintColor,
                TranslatesAutoresizingMaskIntoConstraints = false,
                TextAlignment = UITextAlignment.Center
            };

            Add(noCameraText);
            PreviewContainer.Hidden = true;

            FlipButton.Enabled = false;
            FlashButton.Enabled = false;
            ShutterButton.Enabled = false;

            this.AddConstraints(
                noCameraText.WithSameTop(PreviewContainer),
                noCameraText.WithSameBottom(PreviewContainer),
                noCameraText.WithSameLeft(PreviewContainer),
                noCameraText.WithSameRight(PreviewContainer));
        }

        /// <summary>
        /// Sets up the video preview layer.
        /// </summary>
        protected void SetupVideoPreviewLayer()
        {
            VideoPreviewLayer = new AVCaptureVideoPreviewLayer(Session)
            {
                Frame = PreviewContainer.Bounds,
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill
            };

            PreviewContainer.Layer.AddSublayer(VideoPreviewLayer);

            if (Configuration.DetectFaces)
            {
                _overlayLayer = new CALayer();
                _overlayLayer.Frame = Bounds;
                _overlayLayer.SublayerTransform =
                    CATransform3D.Identity.MakePerspective(1000);
                VideoPreviewLayer.AddSublayer(_overlayLayer);
            }
        }

        /// <summary>
        /// Setups the face detection.
        /// </summary>
        protected void SetupFaceDetection()
        {
            FaceDetectionOutput = new AVCaptureMetadataOutput();
            if (Session.CanAddOutput(FaceDetectionOutput))
            {
                Session.AddOutput(FaceDetectionOutput);

                if (FaceDetectionOutput.AvailableMetadataObjectTypes.HasFlag(
                    AVMetadataObjectType.Face))
                {
                    FaceDetectionOutput.MetadataObjectTypes = AVMetadataObjectType.Face;
                    FaceDetectionOutput.SetDelegate(this, DispatchQueue.MainQueue);
                }
                else {
                    Session.RemoveOutput(FaceDetectionOutput);
                    FaceDetectionOutput.Dispose();
                    FaceDetectionOutput = null;
                }
            }
        }

        /// <summary>
        /// Implementation of <see cref="IAVCaptureMetadataOutputObjectsDelegate"/>.
        /// 
        /// Used by <see cref="AVCaptureMetadataOutput"/> as callback when <see cref="AVMetadataObject"/>s
        /// are found in Session.
        /// </summary>
        /// <param name="captureOutput">Capture output.</param>
        /// <param name="metadataObjects">Metadata objects.</param>
        /// <param name="connection">Connection.</param>
        [Export("captureOutput:didOutputMetadataObjects:fromConnection:")]
        public void DidOutputMetadataObjects(AVCaptureMetadataOutput captureOutput,
            AVMetadataObject[] metadataObjects, AVCaptureConnection connection)
        {
            var lostFaces = _faceLayers.Keys.ToList();

            foreach (var metadata in metadataObjects.OfType<AVMetadataFaceObject>())
            {
                var transformed = VideoPreviewLayer.GetTransformedMetadataObject(metadata);
                var face = transformed as AVMetadataFaceObject;
                var bounds = transformed.Bounds;

                if (lostFaces.Contains(face.FaceID))
                    lostFaces.Remove(face.FaceID);

                CALayer faceLayer;
                if (!_faceLayers.TryGetValue(face.FaceID, out faceLayer))
                {
                    faceLayer = CreateFaceLayer();
                    _overlayLayer.AddSublayer(faceLayer);
                    _faceLayers.Add(face.FaceID, faceLayer);
                }

                faceLayer.Transform = CATransform3D.Identity;
                faceLayer.Frame = bounds;

                if (face.HasRollAngle)
                {
                    var transform = RollTransform(face.RollAngle);
                    faceLayer.Transform = faceLayer.Transform.Concat(transform);
                }

                if (face.HasYawAngle)
                {
                    var transform = YawTransform(face.YawAngle);
                    faceLayer.Transform = faceLayer.Transform.Concat(transform);
                }
            }

            RemoveLostFaces(lostFaces);
        }

        private CALayer CreateFaceLayer()
        {
            var faceLayer = new CALayer
            {
                BorderColor = Configuration.DetectedFaceBorderColor.CGColor,
                BorderWidth = Configuration.DetectedFaceBorderWidth,
                CornerRadius = Configuration.DetectedFaceCornerRadius
            };

            return faceLayer;
        }

        private void RemoveLostFaces(IEnumerable<nint> lostFaces)
        {
            foreach (var faceId in lostFaces)
            {
                if (_faceLayers.ContainsKey(faceId))
                {
                    var layer = _faceLayers[faceId];
                    _faceLayers.Remove(faceId);

                    layer.RemoveFromSuperLayer();
                    layer.Dispose();
                    layer = null;
                }
            }
        }

        private CATransform3D RollTransform(nfloat rollAngle)
        {
            var radians = rollAngle.ToRadians();
            return CATransform3D.MakeRotation(radians, 0.0f, 0.0f, 1.0f);
        }

        private CATransform3D YawTransform(nfloat yawAngle)
        {
            var radians = yawAngle.ToRadians();

            var yawTransform = CATransform3D.MakeRotation(radians, 0.0f, -1.0f, 0.0f);
            var orientationTransform = OrientationTransform();
            return orientationTransform.Concat(yawTransform);
        }

        private CATransform3D OrientationTransform()
        {
            nfloat angle = 0.0f;
            switch (UIDevice.CurrentDevice.Orientation)
            {
                case UIDeviceOrientation.PortraitUpsideDown:
                    angle = (nfloat)Math.PI;
                    break;
                case UIDeviceOrientation.LandscapeRight:
                    angle = (nfloat)(-Math.PI / 2.0f);
                    break;
                case UIDeviceOrientation.LandscapeLeft:
                    angle = (nfloat)Math.PI / 2.0f;
                    break;
                default:
                    angle = 0.0f;
                    break;
            }

            return CATransform3D.MakeRotation(angle, 0.0f, 0.0f, 1.0f);
        }
    }
}
