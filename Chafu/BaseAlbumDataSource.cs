using System;
using UIKit;

namespace Chafu
{
    /// <summary>
    /// Base class used for DataSource
    /// </summary>
    public abstract class BaseAlbumDataSource : UICollectionViewDataSource
    {
        /// <summary>
        /// Event which triggers when permissions are denied for the Camera Roll
        /// </summary>
        public abstract event EventHandler CameraRollUnauthorized;

        /// <summary>
        /// Get a cropped version of the currently selected image
        /// </summary>
        /// <param name="onImage">Callback <see cref="Action{T}"/> with the cropped <see cref="UIImage"/></param>
        public abstract void GetCroppedImage (Action<UIImage> onImage);

        /// <summary>
        /// <see cref="MediaType"/> of the current selected item
        /// </summary>
        public abstract MediaType CurrentMediaType { get; set; }

        /// <summary>
        /// File system path of the current selected item
        /// </summary>
        public abstract string CurrentMediaPath { get; set; }

        /// <summary>
        /// Show the first item in this data source
        /// </summary>
        public abstract void ShowFirstImage();

        /// <summary>
        /// Delete the currently selected <see cref="MediaItem"/>
        /// </summary>
        /// <returns>Returns the deleted <see cref="MediaItem"/></returns>
		public abstract MediaItem DeleteCurrentMediaItem();
    }
}