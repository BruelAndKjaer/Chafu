using System;
using System.Diagnostics;
using System.Linq;
using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Chafu
{
    public class CameraView : BaseCameraView
    {
        private AVCaptureStillImageOutput _imageOutput;
        private NSObject _willEnterForegroundObserver;
        private Action<UIImage> _onImage;

        public event EventHandler<NSError> SaveToPhotosAlbumError;

        public CameraView(IntPtr handle) 
            : base(handle) { CreateView(); }
        public CameraView() { CreateView(); }

        private void CreateView()
        {
            ShutterButton.TouchUpInside += OnShutter;
            FlipButton.TouchUpInside += OnFlip;
            FlashButton.TouchUpInside += OnFlash;
        }

        private void OnFlash(object sender, EventArgs e)
        {
            Flash(false);
        }

        private void OnFlip(object sender, EventArgs e)
        {
            Flip();
        }

        private void OnShutter(object sender, EventArgs e)
        {
            if (_imageOutput == null) return;
            var orientation = GetOrientation();
            Debug.WriteLine($"Capturing image with device orientation: {orientation.Item1} and image orientation: {orientation.Item2}");

            DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
            {
                var videoConnection = _imageOutput.ConnectionFromMediaType(AVMediaType.Video);
                if (videoConnection.SupportsVideoOrientation)
                    videoConnection.VideoOrientation = orientation.Item1;

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
                            var resizedImage = new UIImage(imageRef, previewWidth/imageWidth, orientation.Item2);
                            SaveToPhotosAlbum(resizedImage);
                            _onImage?.Invoke(resizedImage);
                        }
                        else
                        {
                            SaveToPhotosAlbum(image);
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

        private void SaveToPhotosAlbum(UIImage image)
        {
            if (!Configuration.SaveToPhotosAlbum) return;

            image.SaveToPhotosAlbum((uiImage, nsError) =>
            {
                if (nsError != null)
                    SaveToPhotosAlbumError?.Invoke(this, nsError);
            });
        }

        private bool _initialized;

        public void Initialize(Action<UIImage> onImage)
        {
            if (_initialized)
                return;

            _onImage = onImage;

            var shutterImage = Configuration.ShutterImage ?? UIImage.FromBundle("ic_radio_button_checked");

            if (Configuration.TintIcons)
            {
                ShutterButton.TintColor = Configuration.TintColor;
				shutterImage = shutterImage?.ImageWithRenderingMode (UIImageRenderingMode.AlwaysTemplate);
            }

			ShutterButton.SetImage (shutterImage, UIControlState.Normal);

            Initialize();

            _willEnterForegroundObserver = UIApplication.Notifications.ObserveWillEnterForeground(WillEnterForeground);

            _initialized = true;
        }

        public override void StartCamera()
        {
            if (Session == null) {
                Session = new AVCaptureSession();

                Device = Configuration.ShowBackCameraFirst
                    ? AVCaptureDevice.Devices.FirstOrDefault(d => d.Position == AVCaptureDevicePosition.Back)
                    : AVCaptureDevice.Devices.FirstOrDefault(d => d.Position == AVCaptureDevicePosition.Front);

                if (Device == null)
                {
                    NoCameraAvailable();
                    Console.WriteLine("Could not find capture device, does your device have a camera?");
                    return;
                }

                FlashButton.Hidden = !Device.HasFlash;

                try {
                    NSError error;
                    VideoInput = new AVCaptureDeviceInput(Device, out error);
                    Session.AddInput(VideoInput);

                    _imageOutput = new AVCaptureStillImageOutput();

                    Session.AddOutput(_imageOutput);

                    var videoLayer = new AVCaptureVideoPreviewLayer(Session) {
                        Frame = PreviewContainer.Bounds,
                        VideoGravity = AVLayerVideoGravity.ResizeAspectFill
                    };

                    PreviewContainer.Layer.AddSublayer(videoLayer);

                    Session.StartRunning();
                } catch { /* ignored */ }

                FlashConfiguration(false);
            }

            base.StartCamera();
        }

        private void WillEnterForeground(object sender, NSNotificationEventArgs nsNotificationEventArgs)
        {
            StartCamera();
        }

        public Tuple<AVCaptureVideoOrientation, UIImageOrientation> GetOrientation()
        {
            var orientation = UIDevice.CurrentDevice.Orientation;
            switch (orientation)
            {
                case UIDeviceOrientation.LandscapeLeft:
                    return new Tuple<AVCaptureVideoOrientation, UIImageOrientation>(
                        AVCaptureVideoOrientation.LandscapeLeft, UIImageOrientation.Up);
                case UIDeviceOrientation.LandscapeRight:
                    return new Tuple<AVCaptureVideoOrientation, UIImageOrientation>(
                        AVCaptureVideoOrientation.LandscapeRight, UIImageOrientation.Down);

                case UIDeviceOrientation.PortraitUpsideDown:
                    return new Tuple<AVCaptureVideoOrientation, UIImageOrientation>(
                        AVCaptureVideoOrientation.PortraitUpsideDown, UIImageOrientation.Left);
                default:
                    return new Tuple<AVCaptureVideoOrientation, UIImageOrientation>(
                        AVCaptureVideoOrientation.Portrait, UIImageOrientation.Right);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _willEnterForegroundObserver?.Dispose();
                _willEnterForegroundObserver = null;

                ShutterButton.TouchUpInside -= OnShutter;
                FlipButton.TouchUpInside -= OnFlip;
                FlashButton.TouchUpInside -= OnFlash;
            }
            base.Dispose(disposing);
        }
    }
}
