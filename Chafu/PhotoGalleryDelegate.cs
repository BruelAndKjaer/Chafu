using Foundation;
using Photos;
using UIKit;

namespace Chafu
{
    public class PhotoGalleryDelegate : ChafuAlbumDelegate
    {
        private readonly AlbumView _albumView;
        private readonly PhotoGalleryDataSource _dataSource;

        public PhotoGalleryDelegate(AlbumView albumView, PhotoGalleryDataSource dataSource)
            : base(albumView)
        {
            _albumView = albumView;
            _dataSource = dataSource;
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            _dataSource.ChangeAsset(_dataSource.AllAssets[indexPath.Row]);

            // animations and stuff
            base.ItemSelected(collectionView, indexPath);
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            if (Equals(scrollView, _albumView.CollectionView))
                _dataSource.UpdateCachedAssets();
        }
    }
}