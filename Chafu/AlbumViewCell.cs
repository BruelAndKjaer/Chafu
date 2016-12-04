using System;
using UIKit;
using Cirrious.FluentLayouts.Touch;

namespace Chafu
{
    /// <summary>
    /// View cell displayed in the <see cref="AlbumView"/>
    /// </summary>
    public sealed class AlbumViewCell : UICollectionViewCell
    {
        /// <summary>
        /// Key for cell reuse
        /// </summary>
        public const string Key = "AlbumViewCell";

        private UIImageView _imageView;
        private UIImageView _videoImage;
        private UILabel _timeStamp;

        private bool _isVideo;
        private double _duration;

        /// <inheritdoc/>
        public override bool Selected
        {
            get
            {
                return base.Selected;
            }
            set
            {
                base.Selected = value;
                Layer.BorderColor = value ?
                    Configuration.TintColor.CGColor : UIColor.Clear.CGColor;
                Layer.BorderWidth = value ? 2 : 0;
            }
        }

        /// <summary>
        /// Image displayed in the view cell
        /// </summary>
        public UIImage Image
        {
            get { return _imageView.Image; }
            set { _imageView.Image = value; }
        }

        /// <summary>
        /// Duration in seconds used for showing a time stamp on video cells
        /// </summary>
        public double Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                UpdateTimestamp(_duration);
            }
        }

        private void UpdateTimestamp(double duration)
        {
            var time = TimeSpan.FromSeconds(duration);
            var timestamp = $"{time.Minutes:00}:{time.Seconds:00}";
            _timeStamp.Text = timestamp;
        }

        /// <summary>
        /// Get or set whether the cell is a video
        /// </summary>
        public bool IsVideo
        {
            get { return _isVideo; }
            set
            {
                _isVideo = value;
                _timeStamp.Hidden = !_isVideo;
                _videoImage.Hidden = !_isVideo;
            }
        }

        /// <summary>
        /// Create a <see cref="AlbumViewCell"/>
        /// </summary>
        public AlbumViewCell()
        {
            CreateView();
        }

        /// <summary>
        /// Create a <see cref="AlbumViewCell"/>, used by iOS with native handle
        /// </summary>
        public AlbumViewCell(IntPtr handle) : base(handle)
        {
            CreateView();
        }

        private void CreateView()
        {
            ClipsToBounds = true;

            _imageView = new UIImageView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentMode = UIViewContentMode.ScaleAspectFill,
                AccessibilityLabel = "Image"
            };

            _videoImage = new UIImageView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Hidden = true,
                TintColor = UIColor.White,
                Image = Configuration.VideoImage ?? UIImage.FromBundle("ic_videocam"),
                AccessibilityLabel = "VideoThumbnail"
            };

            _timeStamp = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Hidden = true,
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Right,
                AccessibilityLabel = "VideoTimeStamp"
            };

            Add(_imageView);
            Add(_videoImage);
            Add(_timeStamp);

            this.AddConstraints(
                _imageView.AtBottomOf(this),
                _imageView.AtTopOf(this),
                _imageView.AtLeftOf(this),
                _imageView.AtRightOf(this),

                _videoImage.AtLeftOf(this, 2f),
                _videoImage.AtBottomOf(this, 2f),
                _videoImage.ToLeftOf(_timeStamp),
                _videoImage.Width().EqualTo().HeightOf(_videoImage),
                _videoImage.Height().EqualTo().HeightOf(_timeStamp),

                _timeStamp.AtRightOf(this, 2f),
                _timeStamp.AtBottomOf(this, 2f));
        }
    }
}
