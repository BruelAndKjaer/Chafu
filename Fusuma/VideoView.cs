using System;
using System.IO;
using System.Linq;
using AVFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using UIKit;

namespace Fusuma
{
    public class VideoView : BaseCameraView, IAVCaptureFileOutputRecordingDelegate
    {
        private bool _isRecording;
        private AVCaptureMovieFileOutput _videoOutput;
        private UIImage _videoStartImage;
        private UIImage _videoStopImage;
        private Action<NSUrl> _onVideoFinished;

        NSObject _willEnterForegroundObserver;


        public VideoView(IntPtr handle) 
            : base(handle) { CreateView(); }
        public VideoView() { CreateView(); }

        private void CreateView()
        {
            ShutterButton.TouchUpInside += OnToggleRecording;
            FlipButton.TouchUpInside += OnFlip;
            FlashButton.TouchUpInside += OnFlash;
        }

        public void Initialize(Action<NSUrl> onVideoFinished, bool startCamera = true)
        {
            if (Session != null) return;

            _onVideoFinished = onVideoFinished;

            _videoStartImage = Configuration.VideoStartImage ?? UIImage.FromBundle("video_button");
            _videoStopImage = Configuration.VideoStopImage ?? UIImage.FromBundle("video_button_rec");

            if (Configuration.TintIcons) {
                ShutterButton.TintColor = Configuration.TintColor;

                _videoStartImage = _videoStartImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                _videoStopImage = _videoStopImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }

            ShutterButton.SetImage(_videoStartImage, UIControlState.Normal);

            Initialize();

            Session = new AVCaptureSession();

            Device = Configuration.ShowBackCameraFirst
                ? AVCaptureDevice.Devices.FirstOrDefault(d => d.Position == AVCaptureDevicePosition.Back)
                : AVCaptureDevice.Devices.FirstOrDefault(d => d.Position == AVCaptureDevicePosition.Front);

            if (Device == null)
                throw new Exception("Could not find capture device, does your device have a camera?");

            try
            {
                NSError error;
                VideoInput = new AVCaptureDeviceInput(Device, out error);

                Session.AddInput(VideoInput);

                _videoOutput = new AVCaptureMovieFileOutput();

                var totalSeconds = 60L;
                var timeScale = 30; //FPS

                var maxDuration = new CMTime(totalSeconds, timeScale);

                _videoOutput.MaxRecordedDuration = maxDuration;
                _videoOutput.MinFreeDiskSpaceLimit = 1024*1024;

                if (Session.CanAddOutput(_videoOutput))
                    Session.AddOutput(_videoOutput);

                var videoLayer = new AVCaptureVideoPreviewLayer(Session)
                {
                    Frame = PreviewContainer.Bounds,
                    VideoGravity = AVLayerVideoGravity.ResizeAspectFill
                };

                PreviewContainer.Layer.AddSublayer(videoLayer);

                Session.StartRunning();
            }
            catch { /* ignore */ }

            FlashConfiguration(true);
            if (startCamera)
                StartCamera();

            _willEnterForegroundObserver = UIApplication.Notifications.ObserveWillEnterForeground(WillEnterForeground);
        }

        void WillEnterForeground(object sender, NSNotificationEventArgs e)
        {
            StartCamera();
        }

        public override void StopCamera()
        {
            if (_isRecording)
                ToggleRecording();
            base.StopCamera();
        }

        private void OnFlash(object sender, EventArgs e)
        {
            Flash(true);
        }

        private void OnFlip(object sender, EventArgs e)
        {
            Flip();
        }

        private void OnToggleRecording(object sender, EventArgs e)
        {
            ToggleRecording();
        }

        private void ToggleRecording()
        {
            if (_videoOutput == null) return;

            _isRecording = !_isRecording;

            var shotImage = _isRecording ? _videoStopImage : _videoStartImage;
            ShutterButton.SetImage(shotImage, UIControlState.Normal);

            if (_isRecording)
            {
                var outputPath = Path.Combine(Path.GetTempPath(), "output.mov");
                var outputUrl = NSUrl.FromString("file:///" + outputPath);

                var fileManager = NSFileManager.DefaultManager;
                if (fileManager.FileExists(outputPath))
                {
                    NSError error;
                    fileManager.Remove(outputPath, out error);
                    if (error != null)
                    {
                        Console.WriteLine($"Error removing item at path {outputPath}");
                        _isRecording = false;
                        return;
                    }
                }

                FlipButton.Enabled = false;
                FlashButton.Enabled = false;
                _videoOutput.StartRecordingToOutputFile(outputUrl, this);
            }
            else
            {
                _videoOutput.StopRecording();
                FlipButton.Enabled = true;
                FlashButton.Enabled = true;
            }
        }

        public void FinishedRecording(AVCaptureFileOutput captureOutput, NSUrl outputFileUrl, NSObject[] connections, 
            NSError error)
        {
            _onVideoFinished?.Invoke(outputFileUrl);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _willEnterForegroundObserver?.Dispose();
                _willEnterForegroundObserver = null;

                ShutterButton.TouchUpInside -= OnToggleRecording;
                FlipButton.TouchUpInside -= OnFlip;
                FlashButton.TouchUpInside -= OnFlash;
            }

            base.Dispose(disposing);
        }
    }
}
