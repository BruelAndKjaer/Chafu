using Foundation;
using UIKit;

namespace Chafu
{
    public abstract class ChafuAlbumDelegate : UICollectionViewDelegate
    {
        private readonly AlbumView _albumView;

        protected ChafuAlbumDelegate(AlbumView albumView)
        {
            _albumView = albumView;
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            UIView.AnimateNotify(0.2, 0.0, UIViewAnimationOptions.CurveEaseOut, () =>
            {
                _albumView.ImageCropView.Scrollable = true;
                _albumView.ImageCropViewConstraintTop.Constant = AlbumView.ImageCropViewOriginalConstraintTop;
                _albumView.CollectionViewConstraintHeight.Constant =
                    _albumView.Frame.Height - AlbumView.ImageCropViewOriginalConstraintTop -
                    _albumView.ImageCropView.Frame.Height;
                _albumView.CollectionViewConstraintTop.Constant = 0;
                _albumView.LayoutIfNeeded();
            },
            finished => { });

            _albumView.DragDirection = DragDirection.Up;
            _albumView.CollectionView.ScrollToItem(indexPath, UICollectionViewScrollPosition.Top, true);
        }
    }
}