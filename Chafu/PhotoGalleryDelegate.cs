using Foundation;
using Photos;
using UIKit;

namespace Chafu
{
    public class PhotoGalleryDelegate : ChafuBaseGalleryDelegate
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
            _dataSource.ChangeImage(_dataSource.Images[indexPath.Row] as PHAsset);

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