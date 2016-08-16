using System;
using CoreGraphics;
using UIKit;

namespace Fusuma
{
    public abstract class FusumaAlbumDataSource : UICollectionViewDataSource
    {
		public abstract void GetCroppedImage (CGRect cropRect, Action<UIImage> onImage);
    }
}