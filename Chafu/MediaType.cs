using System;
using UIKit;

namespace Chafu
{
    /// <summary>
    /// Enum used to determine the media type to display
    /// </summary>
    [Flags]
    public enum MediaType
    {
        /// <summary>
        /// Used for images
        /// </summary>
        Image = 1,
        /// <summary>
        /// Used for video
        /// </summary>
        Video = 1 << 1
    }
    
}