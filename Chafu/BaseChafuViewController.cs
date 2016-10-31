using System;
using CoreGraphics;
using UIKit;

namespace Chafu
{
    /// <summary>
    /// Base UIViewController used for sharing common methods and properties
    /// </summary>
    public class BaseChafuViewController : UIViewController
    {
        /// <summary>
        /// Gets the album collectionview data source. Use <see cref="LazyDataSource"/> to create your own Data Source.
        /// </summary>
        /// <value>The album data source.</value>
        public BaseAlbumDataSource AlbumDataSource { get; set; }

        /// <summary>
        /// Gets the album delegate. Use <see cref="LazyDelegate"/> to create your own Delegate.
        /// </summary>
        /// <value>The album delegate.</value>
		public BaseAlbumDelegate AlbumDelegate { get; protected set; }

        /// <summary>
        /// Lazy initializer for the <see cref="AlbumDataSource"/>
        /// </summary>
        public Func<AlbumView, CGSize, MediaType, BaseAlbumDataSource> LazyDataSource { get; set; } =
            (view, size, mediaTypes) => new PhotoGalleryDataSource(view, size, mediaTypes);

        /// <summary>
        /// Lazy initializer for the <see cref="AlbumDelegate"/>
        /// </summary>
        public Func<AlbumView, BaseAlbumDataSource, BaseAlbumDelegate> LazyDelegate { get; set; } =
            (view, source) => new PhotoGalleryDelegate(view, (PhotoGalleryDataSource)source);

        /// <summary>
        /// Get or set the cell size. If not set, cell size will be calculated from 
        /// <see cref="Configuration.NumberOfCells"/> set in <see cref="Configuration"/>.
        /// </summary>
        public CGSize CellSize { get; set; }

        /// <summary>
        /// Calculate the cell size for the <see cref="AlbumView"/>.
        /// 
        /// Default behavior takes screen width and divides with
        /// <see cref="Configuration.NumberOfCells"/> set in <see cref="Configuration"/>.
        /// </summary>
        /// <returns>Returns a <see cref="CGSize"/> with cell size</returns>
        protected virtual CGSize CalculateCellSize()
        {
            var screenBounds = UIScreen.MainScreen.Bounds;
            var screenWidth = screenBounds.Width;
            if (Configuration.NumberOfCells <= 0)
                throw new InvalidOperationException(
                    "Cannot create cells for AlbumView, because Configuration.NumberOfCells is equal to or less than 0");

            // separators between cells
            var totalSeparators = Configuration.NumberOfCells - 1;
            var cellWidth = (screenWidth - totalSeparators) / Configuration.NumberOfCells;

            return new CGSize(cellWidth, cellWidth);
        }
    }
}
