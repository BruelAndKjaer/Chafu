using Foundation;
using UIKit;

namespace Chafu
{
    /// <summary>
    /// Delegate for Local Files
    /// </summary>
    public class LocalFilesDelegate : BaseAlbumDelegate
    {
        private readonly LocalFilesDataSource _dataSource;

        /// <summary>
        /// Create a new Delegate for Local Files
        /// </summary>
        /// <param name="albumView"><see cref="AlbumView"/> delegate attached to</param>
        /// <param name="dataSource"><see cref="LocalFilesDataSource"/> data source attached to</param>
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