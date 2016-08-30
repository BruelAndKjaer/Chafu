using System;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Chafu
{
    public class AlbumViewController : UIViewController
    {
        public event EventHandler<UIImage> ImageSelected;
        public event EventHandler<NSUrl> VideoSelected;
        public event EventHandler Closed;
        public event EventHandler Extra;

        private AlbumView _album;
        private MenuView _menu;
        private bool _showExtraButton;

        /// <summary>
        /// Gets the album collectionview data source. Use <see cref="LazyDataSource"/> to create your own Data Source.
        /// </summary>
        /// <value>The album data source.</value>
        public BaseAlbumDataSource AlbumDataSource { get; private set; }

        /// <summary>
        /// Gets the album delegate. Use <see cref="LazyDelegate"/> to create your own Delegate.
        /// </summary>
        /// <value>The album delegate.</value>
        public BaseAlbumDelegate AlbumDelegate { get; private set; }

        public Func<AlbumView, CGSize, BaseAlbumDataSource> LazyDataSource { get; set; } =
            (view, size) => new PhotoGalleryDataSource(view, size);

        public Func<AlbumView, BaseAlbumDataSource, BaseAlbumDelegate> LazyDelegate { get; set; } =
            (view, @delegate) => new PhotoGalleryDelegate(view, (PhotoGalleryDataSource)@delegate);

        public CGSize CellSize { get; set; } = new CGSize(100, 100);

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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = Configuration.BackgroundColor;

            _album = new AlbumView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Configuration.BackgroundColor,
                CellSize = CellSize
            };

            AlbumDataSource = LazyDataSource(_album, CellSize);
            AlbumDelegate = LazyDelegate(_album, AlbumDataSource);
            _album.Initialize(AlbumDataSource, AlbumDelegate);

            _menu = new MenuView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Configuration.BackgroundColor
            };

            Add(_menu);
            Add(_album);

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

            _menu.ExtraButtonHidden = !ShowExtraButton;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            _menu.Done += OnDone;
            _menu.Closed += OnClosed;
            _menu.Extra += OnExtra;

            AlbumDataSource.ShowFirstImage();
        }

        public override void ViewDidDisappear(bool animated)
        {
            _menu.Done -= OnDone;
            _menu.Closed -= OnClosed;
            _menu.Extra -= OnExtra;

            base.ViewDidDisappear(animated);
        }

        public void Dismiss()
        {
            DismissViewController(true, () => Closed?.Invoke(this, EventArgs.Empty));
        }

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            Dismiss();
        }

        private void OnExtra(object sender, EventArgs eventArgs)
        {
            Extra?.Invoke(this, EventArgs.Empty);
        }

        private void OnDone(object sender, EventArgs e)
        {
            if (AlbumDataSource.CurrentMediaType == ChafuMediaType.Image)
            {
                if (Configuration.CropImage) {
                    Console.WriteLine("Cropping image before handing it over");
                    AlbumDataSource.GetCroppedImage(croppedImage => {
                        ImageSelected?.Invoke(this, croppedImage);
                    });
                }
                else {
                    Console.WriteLine("Not cropping image");
                    ImageSelected?.Invoke(this, _album.ImageCropView.Image);
                }
            }

            if (AlbumDataSource.CurrentMediaType == ChafuMediaType.Video)
            {
                var url = _album.MoviePlayerController.ContentUrl;
                VideoSelected?.Invoke(this, url);
            }

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
            }

            base.Dispose(disposing);
        }
    }
}