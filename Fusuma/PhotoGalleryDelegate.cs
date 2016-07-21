using Foundation;
using Photos;
using UIKit;

namespace Fusuma
{
    public class PhotoGalleryDelegate : UICollectionViewDelegate
    {
        private readonly AlbumView _albumView;
        private readonly PhotoGalleryDataSource _dataSource;

        public PhotoGalleryDelegate(AlbumView albumView, PhotoGalleryDataSource dataSource)
        {
            _albumView = albumView;
            _dataSource = dataSource;
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            _dataSource.ChangeImage(_dataSource.Images[indexPath.Row] as PHAsset);

            _albumView.ImageCropView.Scrollable = true;
            _albumView.ImageCropViewConstraintTop.Constant = AlbumView.ImageCropViewOriginalConstraintTop;
            _albumView.CollectionViewConstraintHeight.Constant = _albumView.Frame.Height -
                AlbumView.ImageCropViewOriginalConstraintTop - _albumView.ImageCropViewContainer.Frame.Height;

            UIView.AnimateNotify(0.2, 0.0, UIViewAnimationOptions.CurveEaseOut, () => _albumView.LayoutIfNeeded(),
                finished => { });

            _albumView.DragDirection = DragDirection.Up;
            _albumView.CollectionView.ScrollToItem(indexPath, UICollectionViewScrollPosition.Top, true);
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            if (Equals(scrollView, _albumView.CollectionView))
                _dataSource.UpdateCachedAssets();
        }
    }
}