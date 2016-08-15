using System.Threading.Tasks;
using CoreGraphics;
using UIKit;

namespace Fusuma
{
    public abstract class FusumaAlbumDataSource : UICollectionViewDataSource
    {
        public abstract Task<UIImage> GetCroppedImage(CGRect cropRect);
    }
}