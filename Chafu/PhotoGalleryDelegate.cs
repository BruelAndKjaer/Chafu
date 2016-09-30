using Foundation;
using UIKit;

namespace Chafu
{
    /// <summary>
    /// Delegate for Assets from Photo Library
    /// </summary>
    public class PhotoGalleryDelegate : BaseAlbumDelegate
    {
        private readonly AlbumView _albumView;
        private readonly PhotoGalleryDataSource _dataSource;

        /// <summary>
        /// Create a new delegate
        /// </summary>
        /// <param name="albumView"><see cref="AlbumView"/> attached to</param>
        /// <param name="dataSource"><see cref="PhotoGalleryDataSource"/> attached to</param>
        public PhotoGalleryDelegate(AlbumView albumView, PhotoGalleryDataSource dataSource)
            : base(albumView)
        {
            _albumView = albumView;
            _dataSource = dataSource;
        }

        /// <inheritdoc />
        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            _dataSource.ChangeAsset(_dataSource.AllAssets[indexPath.Row]);

            // animations and stuff
            base.ItemSelected(collectionView, indexPath);
        }

        /// <inheritdoc />
        public override void Scrolled(UIScrollView scrollView)
        {
            if (Equals(scrollView, _albumView.CollectionView))
                _dataSource.UpdateCachedAssets();
        }
    }
}