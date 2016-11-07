using System;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Chafu
{
    /// <summary>
    /// AlbumViewController is used if you want to show a stand-alone media picker without camera
    /// </summary>
    [Register("AlbumViewController")]
    public class AlbumViewController : BaseChafuViewController
    {
        /// <summary>
        /// <see cref="EventHandler{t}"/> with <see cref="UIImage"/> which fires when an Image is selected
        /// </summary>
        public event EventHandler<UIImage> ImageSelected;

        /// <summary>
        /// <see cref="EventHandler{t}"/> with the <see cref="NSUrl"/> of the video, which fires when a Video is selcted
        /// </summary>
        public event EventHandler<NSUrl> VideoSelected;

        /// <summary>
        /// <see cref="EventHandler"/> which fires when this <see cref="AlbumViewController"/> is dismissed
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// <see cref="EventHandler"/> which fires when Extra button is pressed
        /// </summary>
        public event EventHandler Extra;

        /// <summary>
        /// <see cref="EventHandler{T}"/> with <see cref="MediaItem"/> which fires when either a Image or Video is deleted
        /// </summary>
        public event EventHandler<MediaItem> Deleted;

        private AlbumView _album;
        private MenuView _menu;
        private bool _showExtraButton;
        private bool _showDoneButton;
		private bool _showDeleteButton;

        /// <summary>
        /// Gets or sets the media types shown from Photo Library.
        /// </summary>
        /// <value><see cref="MediaType"/> media types. 
        /// Defaults to both <see cref="MediaType.Image"/> and <see cref="MediaType.Video"/>.</value>
        public MediaType MediaTypes { get; set; } = MediaType.Image | MediaType.Video;

        /// <summary>
        /// Get or set whether to show the Extra button in the Menu
        /// </summary>
        public bool ShowExtraButton
        {
            get { return _showExtraButton; }
            set
            {
                _showExtraButton = value;
                if (_menu != null)
                    _menu.ExtraButtonHidden = !_showExtraButton;
            }
        }

        /// <summary>
        /// Get or set whether to show the Done button in the Menu
        /// </summary>
        public bool ShowDoneButton
        {
            get { return _showDoneButton; }
            set
            {
                _showDoneButton = value;
                if (_menu != null)
                    _menu.DoneButtonHidden = !_showDoneButton;
            }
        }

        /// <summary>
        /// Get or set whether to show the Delete button in the Menu
        /// </summary>
		public bool ShowDeleteButton
		{
			get { return _showDeleteButton; }
			set
			{
				_showDeleteButton = value;
				if (_menu != null)
					_menu.DeleteButtonHidden = !_showDeleteButton;
			}
		}

        /// <inheritdoc />
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            CreateViews();
            CreateConstraints();

            _menu.ExtraButtonHidden = !ShowExtraButton;
            _menu.DoneButtonHidden = !ShowDoneButton;
			_menu.DeleteButtonHidden = !ShowDeleteButton;
        }

        private void CreateViews()
        {
            View.BackgroundColor = Configuration.BackgroundColor;

            if (CellSize == CGSize.Empty)
                CellSize = CalculateCellSize();

            _album = new AlbumView(CellSize) {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Configuration.BackgroundColor
            };

            AlbumDataSource = LazyDataSource(_album, CellSize, MediaTypes);
            AlbumDelegate = LazyDelegate(_album, AlbumDataSource);
            _album.Initialize(AlbumDataSource, AlbumDelegate);

            _menu = new MenuView {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Configuration.BackgroundColor
            };

            Add(_menu);
            Add(_album);
        }

        private void CreateConstraints()
        {
            View.AddConstraints(
                _menu.AtTopOf(View),
                _menu.AtLeftOf(View),
                _menu.AtRightOf(View),
                _menu.Height().EqualTo(50),

                _album.Below(_menu),
                _album.AtBottomOf(View),
                _album.AtLeftOf(View),
                _album.AtRightOf(View)
                );

            View.BringSubviewToFront(_menu);
        }

        /// <inheritdoc />
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            _menu.Done += OnDone;
            _menu.Closed += OnClosed;
            _menu.Extra += OnExtra;
			_menu.Deleted += OnDelete;


            _album.ImageCropView.SetNeedsLayout();
            _album.MovieView.SetNeedsLayout();
            _album.CollectionView.SetNeedsLayout();
        }

        /// <inheritdoc />
        public override void ViewDidDisappear(bool animated)
        {
            _menu.Done -= OnDone;
            _menu.Closed -= OnClosed;
            _menu.Extra -= OnExtra;
			_menu.Deleted -= OnDelete;

            base.ViewDidDisappear(animated);
        }

        /// <summary>
        /// Dismiss this <see cref="AlbumViewController"/>
        /// </summary>
        /// <param name="animated">Optional: <see cref="bool"/> describing whether to dismiss animated or not. 
        /// Default value is <c>true</c></param>
        public void Dismiss(bool animated = true)
        {
            DismissViewController(animated, () => Closed?.Invoke(this, EventArgs.Empty));
        }

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            Dismiss();
        }

        private void OnExtra(object sender, EventArgs eventArgs)
        {
            Extra?.Invoke(this, EventArgs.Empty);
        }

        private UIAlertController _deleteAlertController;

        private UIAlertController EnsureAlertController()
        {
            if (_deleteAlertController == null)
            {
                _deleteAlertController = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
                _deleteAlertController.AddAction(UIAlertAction.Create(Configuration.DeleteTitle,
                    UIAlertActionStyle.Destructive, Delete));
                _deleteAlertController.AddAction(UIAlertAction.Create(Configuration.CancelTitle, UIAlertActionStyle.Cancel,
                    action => { }));
            }

            return _deleteAlertController;
        }

        private static void SetAlertControllerTitleAndMessage(MediaItem item, UIAlertController controller)
        {
            if (item == null) return;
            if (controller == null) return;

            if (item.MediaType == MediaType.Image)
            {
                if (!string.IsNullOrEmpty(Configuration.DeletePhotoTitle))
                    controller.Title = Configuration.DeletePhotoTitle;
                if (!string.IsNullOrEmpty(Configuration.DeletePhotoMessage))
                    controller.Message = Configuration.DeletePhotoMessage;
            }
            else if (item.MediaType == MediaType.Video)
            {
                if (!string.IsNullOrEmpty(Configuration.DeleteVideoTitle))
                    controller.Title = Configuration.DeleteVideoTitle;
                if (!string.IsNullOrEmpty(Configuration.DeleteVideoMessage))
                    controller.Message = Configuration.DeleteVideoMessage;
            }
        }

        private void Delete(UIAlertAction action)
        {
            Delete();
        }

        private void Delete()
        {
            var item = AlbumDataSource.DeleteCurrentMediaItem();
            Deleted?.Invoke(this, item);
        }

        private void OnDelete(object sender, EventArgs eventArgs)
        {
            if (Configuration.ShowActionSheetOnDelete)
            {
                var deleteController = EnsureAlertController();

                var item = new MediaItem
                {
                    MediaType = AlbumDataSource.CurrentMediaType,
                    Path = AlbumDataSource.CurrentMediaPath
                };

                SetAlertControllerTitleAndMessage(item, deleteController);

                PresentViewController(deleteController, true, () => { });
            }
            else
                Delete();
        }

        private void OnDone(object sender, EventArgs e)
        {
            if (AlbumDataSource.CurrentMediaType == MediaType.Image)
                OnImageSelected();
            if (AlbumDataSource.CurrentMediaType == MediaType.Video)
                OnVideoSelected();
        }

        private void OnImageSelected()
        {
            if (Configuration.CropImage)
            {
                Console.WriteLine("Cropping image before handing it over");
                AlbumDataSource.GetCroppedImage(croppedImage => {
                    ImageSelected?.Invoke(this, croppedImage);
                    Dismiss();
                });
            }
            else
            {
                Console.WriteLine("Not cropping image");
                ImageSelected?.Invoke(this, _album.ImageCropView.Image);
                Dismiss();
            }
        }

        private void OnVideoSelected()
        {
            var url = _album.MoviePlayerController.ContentUrl;
            VideoSelected?.Invoke(this, url);
            Dismiss();
        }

        /// <inheritdoc />
        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations() => UIInterfaceOrientationMask.Portrait;

        /// <inheritdoc />
        public override UIInterfaceOrientation InterfaceOrientation => UIInterfaceOrientation.Portrait;

        /// <inheritdoc />
        public override bool PrefersStatusBarHidden() => Configuration.PreferStatusbarHidden;

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _menu.Done -= OnDone;
                _menu.Closed -= OnClosed;
				_menu.Extra -= OnExtra;
				_menu.Deleted -= OnDelete;
            }

            base.Dispose(disposing);
        }
    }
}