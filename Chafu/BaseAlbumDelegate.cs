using Foundation;
using UIKit;

namespace Chafu
{
    /// <summary>
    /// Base class used for Album Delegate
    /// </summary>
    public abstract class BaseAlbumDelegate : UICollectionViewDelegate
    {
        private readonly AlbumView _albumView;

        /// <summary>
        /// Instantiate a AlbumDelegate
        /// </summary>
        /// <param name="albumView"><see cref="AlbumView"/> the delegate is attached to</param>
        protected BaseAlbumDelegate(AlbumView albumView)
        {
            _albumView = albumView;
        }

        /// <inheritdoc />
        public override void ItemSelected(
            UICollectionView collectionView, NSIndexPath indexPath)
        {
            UIView.AnimateNotify(0.2, 0.0, UIViewAnimationOptions.CurveEaseOut, () =>
            {
                _albumView.ImageCropView.Scrollable = true;
                _albumView.ImageCropView.Alpha = 1.0f;
                _albumView.MovieView.Alpha = 1.0f;
                _albumView.MovieViewConstraintTop.Constant =
                    _albumView.ImageCropViewConstraintTop.Constant =
                        AlbumView.ImageCropViewOriginalConstraintTop;
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