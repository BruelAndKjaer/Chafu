using System;
using CoreGraphics;
using UIKit;

namespace Chafu
{
    public abstract class ChafuAlbumDataSource : UICollectionViewDataSource
    {
        public abstract event EventHandler CameraRollUnauthorized;
        public abstract void GetCroppedImage (CGRect cropRect, Action<UIImage> onImage);
    }
}