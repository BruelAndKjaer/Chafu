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
            _dataSource.ChangeMediaItem(_dataSource.Files[indexPath.Row]);
			_dataSource.CurrentIndexPath = indexPath;

            base.ItemSelected(collectionView, indexPath);
        }
    }
}