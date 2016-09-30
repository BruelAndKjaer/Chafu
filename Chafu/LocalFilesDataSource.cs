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
        private readonly AlbumView _albumView;
        private readonly CGSize _cellSize;
        private string _imagesPath;

        /// <summary>
        /// Backing <see cref="List{T}"/> of <see cref="MediaItem"/>
        /// </summary>
        public List<MediaItem> Files { get; } = new List<MediaItem>();

        /// <summary>
        /// Get or set the <see cref="NSIndexPath"/> of currently selected item
        /// </summary>
		public NSIndexPath CurrentIndexPath { get; set; }


        /// <inheritdoc />
        public override ChafuMediaType CurrentMediaType { get; set; }

        /// <inheritdoc />
        public override string CurrentMediaPath { get; set; }

        /// <summary>
        /// Get or set the path of where the data source looks for images and videos
        /// </summary>
        public string ImagesPath
        {
            get { return _imagesPath; }
            set {
                _imagesPath = value;
                UpdateImageSource(_imagesPath);
            }
        }

        /// <summary>
        /// Get or set the accepted extensions for video files.
        /// 
        /// Defaults to .mov and .mp4
        /// </summary>
        public static string[] MovieFileExtensions { get; set; } = {".mov", ".mp4"};

        /// <summary>
        /// Get or set the accepted extensions for image files.
        /// 
        /// Defaults to .jpg, .jpeg and .png
        /// </summary>
        public static string[] ImageFileExtensions { get; set; } = {".jpg", ".jpeg", ".png"};

        /// <summary>
        /// Creates a new data source for showing images and video from a local folder
        /// </summary>
        /// <param name="albumView"><see cref="AlbumView"/> this data source is feeding</param>
        /// <param name="cellSize">Optional: <see cref="CGSize"/> with the desired cell size. Defaults to 100x100.</param>
        public LocalFilesDataSource(AlbumView albumView, CGSize cellSize = default(CGSize))
        {
            _albumView = albumView;
            _cellSize = cellSize != CGSize.Empty ? cellSize : new CGSize(100, 100);
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

            ShowFirstImage();
        }

        private IEnumerable<string> GetOrderedFiles(string imagesPath)
        {
            if (string.IsNullOrEmpty(imagesPath))
                throw new ArgumentNullException(nameof(imagesPath));
            if (!Directory.Exists(imagesPath))
                throw new InvalidOperationException($"Cannot load images from non-existing Directory: {imagesPath}");

            var dirInfo = new DirectoryInfo(imagesPath);
            var fileInfo = dirInfo.GetFiles();
            return fileInfo.OrderByDescending(f => f.CreationTime).Select(f => f.FullName);
        }

        private static MediaItem GetMediaItem(string file)
        {
            if (MovieFileExtensions.Any(ex => file.ToLower().EndsWith(ex, StringComparison.Ordinal)))
                return new MediaItem { MediaType = ChafuMediaType.Video, Path = "file://" + file };
            if (ImageFileExtensions.Any(ex => file.ToLower().EndsWith(ex, StringComparison.Ordinal)))
                return new MediaItem { MediaType = ChafuMediaType.Image, Path = file };
            return null;
        }


        /// <inheritdoc />
        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell(AlbumViewCell.Key, indexPath) as AlbumViewCell ??
                       new AlbumViewCell();

            var file = Files?[indexPath.Row];
            if (file?.Path == null) return cell;

            var row = indexPath.Row;

            cell.IsVideo = file.MediaType == ChafuMediaType.Video;
            cell.Image = null;
            cell.Tag = row;

            if (file.MediaType == ChafuMediaType.Image)
                SetImageCell(cell, file, row);
            else
                SetVideoCell(cell, file, row);

            return cell;
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

        private static UIImage GetVideoThumbnail(AVAssetImageGenerator generator, double seconds, CGSize cellSize)
        {
            var time = new CMTime(Clamp(1, 0, (int)seconds), 1);

            CMTime actual;
            NSError error;

            using (var cgImage = generator.CopyCGImageAtTime(time, out actual, out error))
            using (var uiImage = new UIImage(cgImage))
            {
                var scaledImage = uiImage.ScaleImage(cellSize);
                return scaledImage;
            }
        }

        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }

        /// <inheritdoc />
        public override nint GetItemsCount(UICollectionView collectionView, nint section) => Files?.Count ?? 0;

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

                _albumView.ImageCropView.Image = null;
                _albumView.MoviePlayerController.ContentUrl = null;
                CurrentMediaPath = item.Path;
                CurrentMediaType = item.MediaType;

                if (item.MediaType == ChafuMediaType.Image)
                    ChangeImage(item, _albumView);
                else
                    ChangeVideo(item, _albumView);
            });
        }

        private static void ChangeImage(MediaItem item, AlbumView albumView)
        {
            albumView.ImageCropView.Hidden = false;
            albumView.MovieView.Hidden = true;

            var image = UIImage.FromFile(item.Path);

            albumView.ImageCropView.ImageSize = image.Size;
            albumView.ImageCropView.Image = image;
        }

        private static void ChangeVideo(MediaItem item, AlbumView albumView)
        {
            albumView.ImageCropView.Hidden = true;
            albumView.MovieView.Hidden = false;

            var sourceMovieUrl = new NSUrl(item.Path);

            albumView.MoviePlayerController.ContentUrl = sourceMovieUrl;
            albumView.MoviePlayerController.PrepareToPlay();
        }

        private void StopVideo()
        {
            if (CurrentMediaType != ChafuMediaType.Video) return;
            _albumView.StopVideo();
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
                    {
                        onImage?.Invoke(result);
                    });
                }
                UIGraphics.EndImageContext();
            });
        }

        /// <inheritdoc />
        public override void ShowFirstImage()
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                ChangeMediaItem(Files.FirstOrDefault());
                CurrentIndexPath = NSIndexPath.FromRowSection(0, 0);
                _albumView?.CollectionView.ReloadData();
                _albumView?.CollectionView.SelectItem(CurrentIndexPath, false,
                    UICollectionViewScrollPosition.None);
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
				_albumView.CollectionView.DeleteItems(new [] { CurrentIndexPath });

			    var path = CurrentMediaPath;
			    if (path.StartsWith("file://"))
			        path = path.Substring(7);

                File.Delete(path);

				CurrentMediaPath = null;
				CurrentIndexPath = null;
			}, null);

			ShowFirstImage();

		    return mediaItem;
		}

        private MediaItem MediaItemFromPath(string path)
        {
            return string.IsNullOrEmpty(path) ? null : Files.FirstOrDefault(f => f.Path == path);
        }

        /// <inheritdoc />
        public override event EventHandler CameraRollUnauthorized;
    }
}