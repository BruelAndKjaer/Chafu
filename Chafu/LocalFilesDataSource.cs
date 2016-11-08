using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using UIKit;

namespace Chafu
{
    /// <summary>
    /// Data source for showing images and video from local folder
    /// </summary>
    public class LocalFilesDataSource : BaseAlbumDataSource
    {
        /// <inheritdoc />
        public override event EventHandler CameraRollUnauthorized;

        private readonly AlbumView _albumView;
        private readonly CGSize _cellSize;
        private string _imagesPath;
        private MediaType _mediaTypes;

        /// <summary>
        /// Backing <see cref="List{T}"/> of <see cref="MediaItem"/>
        /// </summary>
        public List<MediaItem> Files { get; } = new List<MediaItem>();

        /// <summary>
        /// Get or set the <see cref="NSIndexPath"/> of currently selected item
        /// </summary>
		public NSIndexPath CurrentIndexPath { get; set; }


        /// <inheritdoc />
        public override MediaType CurrentMediaType { get; set; }

        /// <inheritdoc />
        public override string CurrentMediaPath { get; set; }

        /// <summary>
        /// Get or set the path of where the data source looks for images and videos
        /// </summary>
        public string ImagesPath
        {
            get { return _imagesPath; }
            set
            {
                _imagesPath = value;
                UpdateImageSource(_imagesPath);
            }
        }

        /// <summary>
        /// Get or set the accepted extensions for video files.
        /// 
        /// Defaults to .mov and .mp4
        /// </summary>
        public static string[] MovieFileExtensions { get; set; } = { ".mov", ".mp4" };

        /// <summary>
        /// Get or set the accepted extensions for image files.
        /// 
        /// Defaults to .jpg, .jpeg and .png
        /// </summary>
        public static string[] ImageFileExtensions { get; set; } = { ".jpg", ".jpeg", ".png" };

        /// <summary>
        /// Get or set the initial path of the first selected image.
        /// 
        /// Default is null
        /// </summary>
        public string InitialSelectedImagePath { get; set; }

        /// <summary>
        /// Creates a new data source for showing images and video from a local folder
        /// </summary>
        /// <param name="albumView"><see cref="AlbumView"/> this data source is feeding</param>
        /// <param name="cellSize">Optional: <see cref="CGSize"/> with the desired cell size. 
        /// Defaults to 100x100.</param>
        /// <param name="mediaTypes"><see cref="MediaType"/> media types to show. 
        /// Default is both <see cref="MediaType.Image"/> and <see cref="MediaType.Video"/>.</param>
        public LocalFilesDataSource(AlbumView albumView, CGSize cellSize = default(CGSize),
            MediaType mediaTypes = MediaType.Image | MediaType.Video)
        {
            _albumView = albumView;
            _cellSize = cellSize != CGSize.Empty ? cellSize : new CGSize(100, 100);
            _mediaTypes = mediaTypes;
        }

        /// <summary>
        /// Update the path where to look for images and video
        /// </summary>
        /// <param name="imagesPath"></param>
        public void UpdateImageSource(string imagesPath)
        {
            var files = GetOrderedFiles(imagesPath);

            Files.Clear();
            var items = files.Select(GetMediaItem).Where(f => f != null);
            Files.AddRange(items);
        }

        /// <inheritdoc />
        public override UICollectionViewCell GetCell(UICollectionView collectionView,
            NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell(AlbumViewCell.Key, indexPath)
                as AlbumViewCell ?? new AlbumViewCell();

            var file = Files?[indexPath.Row];
            if (file?.Path == null) return cell;

            var row = indexPath.Row;

            cell.IsVideo = file.MediaType == MediaType.Video;
            cell.Image = null;
            cell.Tag = row;

            if (file.MediaType == MediaType.Image)
                SetImageCell(cell, file, row);
            else
                SetVideoCell(cell, file, row);

            return cell;
        }

        /// <inheritdoc />
        public override nint GetItemsCount(UICollectionView collectionView, nint section)
            => Files?.Count ?? 0;

        /// <summary>
        /// Change the the currently shown <see cref="MediaItem"/>
        /// </summary>
        /// <param name="item"><see cref="MediaItem"/> to change to</param>
        public void ChangeMediaItem(MediaItem item)
        {
            if (item?.Path == null) return;
            if (_albumView?.ImageCropView == null) return;

            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                StopVideo();

                _albumView.ClearPreview();
                CurrentMediaPath = item.Path;
                CurrentMediaType = item.MediaType;

                if (item.MediaType == MediaType.Image)
                    ChangeImage(item, _albumView);
                else
                    ChangeVideo(item, _albumView);
            });
        }

        /// <inheritdoc />
        public override void GetCroppedImage(Action<UIImage> onImage)
        {
            var view = _albumView?.ImageCropView;
            var image = view?.Image;

            if (image == null)
            {
                onImage?.Invoke(null);
                return;
            }

            var scale = UIScreen.MainScreen.Scale;
            var offset = view.ContentOffset;
            var size = view.Bounds.Size;

            DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
            {
                UIGraphics.BeginImageContextWithOptions(size, true, scale);

                using (var context = UIGraphics.GetCurrentContext())
                {
                    context.TranslateCTM(-offset.X, -offset.Y);
                    view.Layer.RenderInContext(context);

                    var result = UIGraphics.GetImageFromCurrentImageContext();

                    DispatchQueue.MainQueue.DispatchAsync(() =>
                        onImage?.Invoke(result));
                }
                UIGraphics.EndImageContext();
            });
        }

        /// <inheritdoc />
        public override void ShowFirstImage()
        {
            var itemAndIndex = GetItemAndIndex(Files, InitialSelectedImagePath);
            var item = itemAndIndex.Item1;
            var indexOfItem = itemAndIndex.Item2;

            if (item == null) return;

            ChangeMediaItem(item);

            var indexPath = NSIndexPath.FromRowSection(indexOfItem, 0);
            if (CurrentIndexPath != null && CurrentIndexPath.Length == indexPath.Length && CurrentIndexPath.Row == indexPath.Row &&
                CurrentIndexPath.Section == indexPath.Section)
                return; // no need to update if same selection

            CurrentIndexPath = indexPath;

            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                _albumView?.CollectionView.ReloadData();
                _albumView?.CollectionView.SelectItem(CurrentIndexPath, false,
                    UICollectionViewScrollPosition.None);
                _albumView?.CollectionView.ScrollToItem(CurrentIndexPath,
                    UICollectionViewScrollPosition.Top, false);   
            });
        }

        /// <inheritdoc />
		public override MediaItem DeleteCurrentMediaItem()
        {
            var mediaItem = MediaItemFromPath(CurrentMediaPath);
            if (mediaItem == null) return null;

            _albumView.CollectionView.PerformBatchUpdates(() =>
            {
                Files.Remove(mediaItem);

                _albumView.ClearPreview();
                _albumView.CollectionView.DeleteItems(new[] { CurrentIndexPath });

                var path = PathWithoutPrefix(CurrentMediaPath);
                File.Delete(path);

                if (CurrentMediaPath == InitialSelectedImagePath)
                    InitialSelectedImagePath = null;

                CurrentMediaPath = null;
                CurrentIndexPath = null;
            }, null);

            ShowFirstImage();

            return mediaItem;
        }

        private IEnumerable<string> GetOrderedFiles(string imagesPath)
        {
            if (string.IsNullOrEmpty(imagesPath))
                throw new ArgumentNullException(nameof(imagesPath));
            if (!Directory.Exists(imagesPath))
                throw new InvalidOperationException(
                    $"Cannot load images from non-existing Directory: {imagesPath}");

            var dirInfo = new DirectoryInfo(imagesPath);
            var fileInfo = dirInfo.GetFiles();
            return fileInfo.OrderByDescending(f => f.CreationTime).Select(f => f.FullName);
        }

        private MediaItem GetMediaItem(string file)
        {
            if (MovieFileExtensions.Any(ex =>
                file.ToLower().EndsWith(ex, StringComparison.Ordinal)) &&
                _mediaTypes.HasFlag(MediaType.Video))
                return new MediaItem
                {
                    MediaType = MediaType.Video,
                    Path = "file://" + file
                };
            if (ImageFileExtensions.Any(ex =>
                file.ToLower().EndsWith(ex, StringComparison.Ordinal)) &&
                _mediaTypes.HasFlag(MediaType.Image))
                return new MediaItem { MediaType = MediaType.Image, Path = file };
            return null;
        }

        private void SetImageCell(AlbumViewCell cell, MediaItem item, int row)
        {
            DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
            {
                using (var image = UIImage.FromFile(item.Path))
                {
                    var scaledImage = image.ScaleImage(_cellSize);

                    DispatchQueue.MainQueue.DispatchAsync(() =>
                    {
                        if (cell.Tag == row)
                            cell.Image = scaledImage;
                    });
                }
            });
        }

        private void SetVideoCell(AlbumViewCell cell, MediaItem item, int row)
        {
            DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
            {
                using (var asset = AVAsset.FromUrl(new NSUrl(item.Path)))
                using (var generator = AVAssetImageGenerator.FromAsset(asset))
                {
                    generator.AppliesPreferredTrackTransform = true;

                    var duration = asset.Duration.Seconds;
                    var scaledImage = GetVideoThumbnail(generator, duration, _cellSize);

                    DispatchQueue.MainQueue.DispatchAsync(() =>
                    {
                        if (cell.Tag != row) return;
                        cell.Duration = duration;
                        cell.Image = scaledImage;
                    });
                }
            });
        }

        private static UIImage GetVideoThumbnail(AVAssetImageGenerator generator,
            double seconds, CGSize cellSize)
        {
            var time = new CMTime(Clamp(1, 0, (int)seconds), 1);

            CMTime actual;
            NSError error;

            using (var cgImage = generator.CopyCGImageAtTime(time, out actual, out error))
            using (var uiImage = new UIImage(cgImage))
            {
                var scaledImage = uiImage.ScaleImage(cellSize,
                    UiKitExtensions.UIImageScaleMode.AspectFill,
                    UiKitExtensions.UIImageAlignment.Center, true);
                return scaledImage;
            }
        }

        private static int Clamp(int value, int min, int max)
            => value < min ? min : value > max ? max : value;

        private static void ChangeAlbumViewVisibility(MediaItem item, AlbumView albumView)
        {
            albumView.ImageCropView.Hidden = item.MediaType == MediaType.Video;
            albumView.MovieView.Hidden = item.MediaType == MediaType.Image;
        }

        private static void ChangeImage(MediaItem item, AlbumView albumView)
        {
            ChangeAlbumViewVisibility(item, albumView);

            var image = UIImage.FromFile(item.Path);

            albumView.ImageCropView.ImageSize = image.Size;
            albumView.ImageCropView.Image = image;
        }

        private static void ChangeVideo(MediaItem item, AlbumView albumView)
        {
            ChangeAlbumViewVisibility(item, albumView);

            var sourceMovieUrl = new NSUrl(item.Path);

            albumView.MoviePlayerController.ContentUrl = sourceMovieUrl;
            albumView.MoviePlayerController.PrepareToPlay();
        }

        private void StopVideo()
        {
            if (CurrentMediaType != MediaType.Video) return;
            _albumView.StopVideo();
        }

        private static Tuple<MediaItem, int> GetItemAndIndex(List<MediaItem> files, 
            string initialSelectedImagePath)
        {
            if (!files.Any()) return new Tuple<MediaItem, int>(null, 0);

            var item = GetItemFromInitialSelected(files, initialSelectedImagePath);
            if (item == null)
                item = files.FirstOrDefault();

            if (item == null)
                return new Tuple<MediaItem, int>(null, 0);

            var indexOfItem = files.LastIndexOf(item);

            return new Tuple<MediaItem, int>(item, indexOfItem);
        }

        private static MediaItem GetItemFromInitialSelected(List<MediaItem> files,
            string initialSelectedImagePath)
        {
            var item = !string.IsNullOrEmpty(initialSelectedImagePath)
                ? files.FirstOrDefault(f =>
                    NormalizePath(f.Path) == NormalizePath(initialSelectedImagePath) ||
                    NormalizePath(f.Path) == NormalizePath("file://" + initialSelectedImagePath))
                : files.FirstOrDefault();
            return item;
        }

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .ToUpperInvariant();
        }

        private static string PathWithoutPrefix(string path)
        {
            if (path.StartsWith("file://", StringComparison.Ordinal))
                path = path.Substring(7);
            return path;
        }

        private MediaItem MediaItemFromPath(string path)
            => string.IsNullOrEmpty(path) ?
               null :
               Files.FirstOrDefault(f => f.Path == path);
    }
}