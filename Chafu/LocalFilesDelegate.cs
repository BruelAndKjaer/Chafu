using Foundation;
using UIKit;

namespace Chafu
{
    public class LocalFilesDelegate : BaseAlbumDelegate
    {
        private readonly LocalFilesDataSource _dataSource;

        public LocalFilesDelegate(AlbumView albumView, LocalFilesDataSource dataSource)
            : base(albumView)
        {
            _dataSource = dataSource;
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            _dataSource.ChangeImage(_dataSource.Files[indexPath.Row]);

            base.ItemSelected(collectionView, indexPath);
        }
    }
}