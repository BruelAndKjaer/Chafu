using System;
using CoreGraphics;
using UIKit;

namespace Chafu
{
    public abstract class ChafuAlbumDataSource : UICollectionViewDataSource
    {
		public abstract void GetCroppedImage (CGRect cropRect, Action<UIImage> onImage);
    }
}