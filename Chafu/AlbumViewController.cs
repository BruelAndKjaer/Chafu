using System;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Chafu
{
    [Register("AlbumViewController")]
    public class AlbumViewController : BaseChafuViewController
    {
        public event EventHandler<UIImage> ImageSelected;
        public event EventHandler<NSUrl> VideoSelected;
        public event EventHandler Closed;
        public event EventHandler Extra;
        public event EventHandler<MediaItem> Deleted;

        private AlbumView _album;
        private MenuView _menu;
        private bool _showExtraButton;
        private bool _showDoneButton;
		private bool _showDeleteButton;

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

            AlbumDataSource = LazyDataSource(_album, CellSize);
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
        
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            _menu.Done += OnDone;
            _menu.Closed += OnClosed;
            _menu.Extra += OnExtra;
			_menu.Deleted += OnDelete;
        }

        public override void ViewDidDisappear(bool animated)
        {
            _menu.Done -= OnDone;
            _menu.Closed -= OnClosed;
            _menu.Extra -= OnExtra;
			_menu.Deleted -= OnDelete;

            base.ViewDidDisappear(animated);
        }

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
                _deleteAlertController = UIAlertController.Create("", "", UIAlertControllerStyle.ActionSheet);
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

            if (item.MediaType == ChafuMediaType.Image)
            {
                if (!string.IsNullOrEmpty(Configuration.DeletePhotoTitle))
                    controller.Title = Configuration.DeletePhotoTitle;
                if (!string.IsNullOrEmpty(Configuration.DeletePhotoMessage))
                    controller.Message = Configuration.DeletePhotoMessage;
            }
            else if (item.MediaType == ChafuMediaType.Video)
            {
                if (!string.IsNullOrEmpty(Configuration.DeleteVideoTitle))
                    controller.Title = Configuration.DeleteVideoTitle;
                if (!string.IsNullOrEmpty(Configuration.DeleteVideoMessage))
                    controller.Message = Configuration.DeleteVideoMessage;
            }
        }

        private void Delete(UIAlertAction action)
        {
            var item = AlbumDataSource.DeleteCurrentMediaItem();
            Deleted?.Invoke(this, item);
        }

        private void OnDelete(object sender, EventArgs eventArgs)
		{
		    var deleteController = EnsureAlertController();

            var item = new MediaItem
            {
                MediaType = AlbumDataSource.CurrentMediaType,
                Path = AlbumDataSource.CurrentMediaPath
            };

            SetAlertControllerTitleAndMessage(item, deleteController);

            PresentViewController(deleteController, true, () => {});
		}

        private void OnDone(object sender, EventArgs e)
        {
            if (AlbumDataSource.CurrentMediaType == ChafuMediaType.Image)
                OnImageSelected();
            if (AlbumDataSource.CurrentMediaType == ChafuMediaType.Video)
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

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations() => UIInterfaceOrientationMask.Portrait;
        public override UIInterfaceOrientation InterfaceOrientation => UIInterfaceOrientation.Portrait;

        public override bool PrefersStatusBarHidden() => Configuration.PreferStatusbarHidden;

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