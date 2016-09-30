using System;
using System.IO;
using System.Linq;
using AssetsLibrary;
using AVFoundation;
using Foundation;
using UIKit;

namespace Chafu
{
    /// <summary>
    /// Video view used to preview camera and record video
    /// </summary>
    public class VideoView : BaseCameraView, IAVCaptureFileOutputRecordingDelegate
    {
        private bool _isRecording;
        private AVCaptureMovieFileOutput _videoOutput;
        private AVCaptureDeviceInput _audioInput;
        private UIImage _videoStartImage;
        private UIImage _videoStopImage;
        private Action<NSUrl> _onVideoFinished;

        private NSObject _willEnterForegroundObserver;

        /// <summary>
        /// <see cref="EventHandler{T}"/> with <see cref="NSError"/> which triggers when saving video to Photos Library fails
        /// </summary>
        public event EventHandler<NSError> SaveToPhotosAlbumError;

        /// <summary>
        /// Create a VideoView
        /// </summary>
        /// <param name="handle"></param>
        public VideoView(IntPtr handle) 
            : base(handle) { CreateView(); }
        /// <summary>
        /// Create a VideoView
        /// </summary>
        public VideoView() { CreateView(); }

        private void CreateView()
        {
            ShutterButton.TouchUpInside += OnToggleRecording;
            FlipButton.TouchUpInside += OnFlip;
            FlashButton.TouchUpInside += OnFlash;
        }

        private bool _initialized;

        /// <summary>
        /// Initialize video view
        /// </summary>
        /// <param name="onVideoFinished">Callback <see cref="Action{T}"/> with <see cref="NSUrl"/> 
        /// with the url of the video when recording is done</param>
        /// <param name="startCamera">Optional: <see cref="bool"/> describing whether to start the camera immediately.
        /// Defaults to <c>true</c></param>
        public void Initialize(Action<NSUrl> onVideoFinished, bool startCamera = true)
        {
            if (_initialized) return;

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

            _willEnterForegroundObserver = UIApplication.Notifications.ObserveWillEnterForeground(WillEnterForeground);

            _initialized = true;
        }

        private void WillEnterForeground(object sender, NSNotificationEventArgs e)
        {
            StartCamera();
        }

        /// <summary>
        /// Start camera preview
        /// </summary>
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

                try {
                    NSError error;
                    VideoInput = new AVCaptureDeviceInput(Device, out error);

                    Session.AddInput(VideoInput);

                    _videoOutput = new AVCaptureMovieFileOutput {MinFreeDiskSpaceLimit = 1024*1024};

                    if (Session.CanAddOutput(_videoOutput))
                        Session.AddOutput(_videoOutput);

                    if (Configuration.RecordAudio)
                    {
                        var audioDevice = AVCaptureDevice.DefaultDeviceWithMediaType(AVMediaType.Audio);

                        _audioInput = new AVCaptureDeviceInput(audioDevice, out error);
                        if (Session.CanAddInput(_audioInput))
                            Session.AddInput(_audioInput);
                    }
                    

                    var videoLayer = new AVCaptureVideoPreviewLayer(Session) {
                        Frame = PreviewContainer.Bounds,
                        VideoGravity = AVLayerVideoGravity.ResizeAspectFill
                    };

                    PreviewContainer.Layer.AddSublayer(videoLayer);

                    Session.StartRunning();
                } catch { /* ignore */ }

                FlashConfiguration(true);
            }

            base.StartCamera();
        }

        /// <summary>
        /// Stop camera preview
        /// </summary>
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
                var connection = _videoOutput.ConnectionFromMediaType(AVMediaType.Video);
                if (connection.SupportsVideoOrientation)
                    connection.VideoOrientation = GetOrientation();
                _videoOutput.StartRecordingToOutputFile(outputUrl, this);
            }
            else
            {
                _videoOutput.StopRecording();
                FlipButton.Enabled = true;
                FlashButton.Enabled = true;
            }
        }

        /// <inheritdoc cref="IAVCaptureFileOutputRecordingDelegate"/>
        public void FinishedRecording(AVCaptureFileOutput captureOutput, NSUrl outputFileUrl, NSObject[] connections, 
            NSError error)
        {
            if (error != null)
            {
                SaveToPhotosAlbumError?.Invoke(this, error);
            }
            else
            {
                if (Configuration.SaveToPhotosAlbum)
                {
                    var al = new ALAssetsLibrary();
                    al.WriteVideoToSavedPhotosAlbum(outputFileUrl, (url, nsError) =>
                    {
                        if (nsError != null)
                            SaveToPhotosAlbumError?.Invoke(this, nsError);
                    });
                }

                _onVideoFinished?.Invoke(outputFileUrl);
            }
        }

        private static AVCaptureVideoOrientation GetOrientation()
        {
            var orientation = UIDevice.CurrentDevice.Orientation;
            switch (orientation)
            {
                case UIDeviceOrientation.LandscapeLeft:
                    return AVCaptureVideoOrientation.LandscapeRight;
                case UIDeviceOrientation.LandscapeRight:
                    return AVCaptureVideoOrientation.LandscapeLeft;
                case UIDeviceOrientation.PortraitUpsideDown:
                    return AVCaptureVideoOrientation.PortraitUpsideDown;
                default:
                    return AVCaptureVideoOrientation.Portrait;
            }
        }

        /// <inheritdoc />
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
