using Chafu;
using Foundation;
using UIKit;

namespace Sample
{
    public class LocalFilesDelegate : ChafuAlbumDelegate
    {
        private readonly LocalFilesDataSource _dataSource;

        public LocalFilesDelegate(AlbumView albumView, LocalFilesDataSource dataSource)
            : base(albumView)
        {
            _dataSource = dataSource;
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            _dataSource.ChangeImage(_dataSource.Images[indexPath.Row]);

            base.ItemSelected(collectionView, indexPath);
        }
    }
}