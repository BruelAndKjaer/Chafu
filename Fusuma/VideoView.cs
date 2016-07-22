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
        private UIButton _toggleRecordingButton;
        private bool _isRecording;
        private AVCaptureMovieFileOutput _videoOutput;
        private UIImage _videoStartImage;
        private UIImage _videoStopImage;
        private Action<NSUrl> _onVideoFinished;

        public VideoView(IntPtr handle) 
            : base(handle) { CreateView(); }
        public VideoView() { CreateView(); }

        private void CreateView()
        {
            Hidden = true;
            ContentMode = UIViewContentMode.ScaleToFill;
            Frame = new CGRect(0, 0, 400, 600);
            AutoresizingMask = UIViewAutoresizing.All;
            BackgroundColor = Configuration.BackgroundColor;

            PreviewContainer = new UIView
            {
                AccessibilityLabel = "PreviewContainer",
                Frame = new CGRect(0, 50, 400, 400),
                BackgroundColor = UIColor.Black,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentMode = UIViewContentMode.ScaleToFill
            };
            Add(PreviewContainer);
            AddConstraint(NSLayoutConstraint.Create(PreviewContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal,
                PreviewContainer, NSLayoutAttribute.Width, 1, 0));

            var buttonContainer = new UIView
            {
                Frame = new CGRect(0, 450, 400, 150),
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentMode = UIViewContentMode.ScaleToFill,
                AccessibilityLabel = "ButtonContainer",
                BackgroundColor = Configuration.BackgroundColor
            };
            Add(buttonContainer);

            _toggleRecordingButton = new UIButton(new CGRect(166, 41, 68, 68))
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                AccessibilityLabel = "ShutterButton",
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                Opaque = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation
            };
            _toggleRecordingButton.TouchUpInside += OnToggleRecording;
            _toggleRecordingButton.SetImage(UIImage.FromBundle("ic_radio_button_checked"), UIControlState.Normal);
            buttonContainer.Add(_toggleRecordingButton);

            var heightConstraint = NSLayoutConstraint.Create(_toggleRecordingButton, NSLayoutAttribute.Height,
                NSLayoutRelation.Equal);
            heightConstraint.Constant = 68;
            var widthConstraint = NSLayoutConstraint.Create(_toggleRecordingButton, NSLayoutAttribute.Width,
                NSLayoutRelation.Equal);
            widthConstraint.Constant = 68;
            _toggleRecordingButton.AddConstraints(new[] { heightConstraint, widthConstraint });

            FlipButton = new UIButton(new CGRect(15, 55, 40, 40))
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
            buttonContainer.Add(FlipButton);

            heightConstraint = NSLayoutConstraint.Create(FlipButton, NSLayoutAttribute.Height,
                NSLayoutRelation.Equal);
            heightConstraint.Constant = 40;
            widthConstraint = NSLayoutConstraint.Create(FlipButton, NSLayoutAttribute.Width,
                NSLayoutRelation.Equal);
            widthConstraint.Constant = 40;
            FlipButton.AddConstraints(new[] { heightConstraint, widthConstraint });

            FlashButton = new UIButton(new CGRect(15, 55, 40, 40))
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
            buttonContainer.Add(FlashButton);

            heightConstraint = NSLayoutConstraint.Create(FlashButton, NSLayoutAttribute.Height,
                NSLayoutRelation.Equal);
            heightConstraint.Constant = 40;
            widthConstraint = NSLayoutConstraint.Create(FlashButton, NSLayoutAttribute.Width,
                NSLayoutRelation.Equal);
            widthConstraint.Constant = 40;
            FlashButton.AddConstraints(new[] { heightConstraint, widthConstraint });

            buttonContainer.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(_toggleRecordingButton, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal,
                    buttonContainer, NSLayoutAttribute.CenterX, 1, 0),
                NSLayoutConstraint.Create(FlashButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal,
                    buttonContainer, NSLayoutAttribute.Trailing, 1, -15),
                NSLayoutConstraint.Create(_toggleRecordingButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal,
                    buttonContainer, NSLayoutAttribute.CenterY, 1, 0),
                NSLayoutConstraint.Create(FlashButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal,
                    buttonContainer, NSLayoutAttribute.CenterY, 1, 0),
                NSLayoutConstraint.Create(FlipButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal,
                    buttonContainer, NSLayoutAttribute.CenterY, 1, 0),
                NSLayoutConstraint.Create(FlipButton, NSLayoutAttribute.Leading, NSLayoutRelation.Equal,
                    buttonContainer, NSLayoutAttribute.Leading, 1, 15)
            });

            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(buttonContainer, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Bottom, 1, 0),
                NSLayoutConstraint.Create(buttonContainer, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Trailing, 1, 0),
                NSLayoutConstraint.Create(buttonContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal,
                    PreviewContainer, NSLayoutAttribute.Bottom, 1, 0),
                NSLayoutConstraint.Create(buttonContainer, NSLayoutAttribute.Leading, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Leading, 1, 0),
                NSLayoutConstraint.Create(PreviewContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Top, 1, 50),
                NSLayoutConstraint.Create(PreviewContainer, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Trailing, 1, 0),
                NSLayoutConstraint.Create(PreviewContainer, NSLayoutAttribute.Leading, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Leading, 1, 0)
            });
        }

        public void Initialize(Action<NSUrl> onVideoFinished)
        {
            if (Session != null) return;

            _onVideoFinished = onVideoFinished;

            Hidden = false;

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

            FocusView = new UIView(new CGRect(0, 0, 90, 90));
            var tapRecognizer = new UITapGestureRecognizer(Focus);
            PreviewContainer.AddGestureRecognizer(tapRecognizer);


            FlashOnImage = Configuration.FlashOnImage ?? UIImage.FromBundle("ic_flash_on");
            FlashOffImage = Configuration.FlashOffImage ?? UIImage.FromBundle("ic_flash_off");
            var flipImage = Configuration.FlipImage ?? UIImage.FromBundle("ic_loop");
            _videoStartImage = Configuration.VideoStartImage ?? UIImage.FromBundle("video_button");
            _videoStopImage = Configuration.VideoStopImage ?? UIImage.FromBundle("video_button_rec");

            if (Configuration.TintIcons)
            {
                FlashButton.TintColor = Configuration.TintColor;
                FlipButton.TintColor = Configuration.TintColor;
				_toggleRecordingButton.TintColor = Configuration.TintColor;

                FlashOnImage = FlashOnImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                FlashOffImage = FlashOffImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                flipImage = flipImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                _videoStartImage = _videoStartImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                _videoStopImage = _videoStopImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }

            FlashButton.SetImage(FlashOffImage, UIControlState.Normal);
            FlipButton.SetImage(flipImage, UIControlState.Normal);
            _toggleRecordingButton.SetImage(_videoStartImage, UIControlState.Normal);

            FlashConfiguration();
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
            Flash();
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
            _toggleRecordingButton.SetImage(shotImage, UIControlState.Normal);

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
    }
}
