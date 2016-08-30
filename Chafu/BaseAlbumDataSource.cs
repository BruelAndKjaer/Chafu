using System;
using UIKit;

namespace Chafu
{
    public enum ChafuMediaType
    {
        Image,
        Video
    }

    public abstract class BaseAlbumDataSource : UICollectionViewDataSource
    {
        public abstract event EventHandler CameraRollUnauthorized;
        public abstract void GetCroppedImage (Action<UIImage> onImage);
        public abstract ChafuMediaType CurrentMediaType { get; set; }
        public abstract void ShowFirstImage();
    }
}