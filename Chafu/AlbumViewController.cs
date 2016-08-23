using System;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using UIKit;

namespace Chafu
{
    public class AlbumViewController : UIViewController
    {
        public event EventHandler<UIImage> ImageSelected;
        public event EventHandler Closed;

        private AlbumView _album;
        private ChafuMenuView _menu;

        /// <summary>
        /// Gets the album collectionview data source. Use <see cref="LazyDataSource"/> to create your own Data Source.
        /// </summary>
        /// <value>The album data source.</value>
        public ChafuAlbumDataSource AlbumDataSource { get; private set; }

        /// <summary>
        /// Gets the album delegate. Use <see cref="LazyDelegate"/> to create your own Delegate.
        /// </summary>
        /// <value>The album delegate.</value>
        public ChafuAlbumDelegate AlbumDelegate { get; private set; }

        public Func<AlbumView, CGSize, ChafuAlbumDataSource> LazyDataSource { get; set; } =
            (view, size) => new PhotoGalleryDataSource(view, size);

        public Func<AlbumView, ChafuAlbumDataSource, ChafuAlbumDelegate> LazyDelegate { get; set; } =
            (view, @delegate) => new PhotoGalleryDelegate(view, (PhotoGalleryDataSource)@delegate);

        public CGSize CellSize { get; set; } = new CGSize(100, 100);

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

            _menu = new ChafuMenuView
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

                _album.AtTopOf(View),
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
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            _menu.Done -= OnDone;
            _menu.Closed -= OnClosed;
        }

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            DismissViewController(true, () => Closed?.Invoke(this, EventArgs.Empty));
        }

        private void OnDone(object sender, EventArgs e)
        {
            var view = _album.ImageCropView;

            if (Configuration.CropImage)
            {
                var normalizedX = view.ContentOffset.X / view.ContentSize.Width;
                var normalizedY = view.ContentOffset.Y / view.ContentSize.Height;

                var normalizedWidth = view.Frame.Width / view.ContentSize.Width;
                var normalizedHeight = view.Frame.Height / view.ContentSize.Height;

                var cropRect = new CGRect(normalizedX, normalizedY, normalizedWidth, normalizedHeight);

                Console.WriteLine("Cropping image before handing it over");
                AlbumDataSource.GetCroppedImage(cropRect, (croppedImage) => {
                                                                                ImageSelected?.Invoke(this, croppedImage);
                                                                                DismissViewController(true, () => Closed?.Invoke(this, EventArgs.Empty));
                });
            }
            else
            {
                Console.WriteLine("Not cropping image");
                ImageSelected?.Invoke(this, view.Image);
                DismissViewController(true, () => Closed?.Invoke(this, EventArgs.Empty));
            }
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