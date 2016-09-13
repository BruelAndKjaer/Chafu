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
    public class MediaItem
    {
        public ChafuMediaType MediaType { get; set; }
        public string Path { get; set; }
    }

    public class LocalFilesDataSource : BaseAlbumDataSource
    {
        private readonly AlbumView _albumView;
        private readonly CGSize _cellSize;
        public readonly List<MediaItem> Files = new List<MediaItem>();
        private string _imagesPath;

		public NSIndexPath CurrentIndexPath { get; set; }

        public override ChafuMediaType CurrentMediaType { get; set; }

        public string CurrentMediaPath { get; private set; }

        public string ImagesPath
        {
            get { return _imagesPath; }
            set {
                _imagesPath = value;
                UpdateImageSource(_imagesPath);
            }
        }

        public static string[] MovieFileExtensions { get; set; } = {".mov", ".mp4"};
        public static string[] ImageFileExtensions { get; set; } = {".jpg", ".jpeg", ".png"};

        public LocalFilesDataSource(AlbumView albumView, CGSize cellSize = default(CGSize))
        {
            _albumView = albumView;
            _cellSize = cellSize != CGSize.Empty ? cellSize : new CGSize(100, 100);
        }

        public void UpdateImageSource(string imagesPath)
        {
            if (string.IsNullOrEmpty(imagesPath))
                throw new ArgumentNullException(nameof(imagesPath));

            if (!Directory.Exists(imagesPath))
                throw new InvalidOperationException($"Cannot load images from non-existing Directory: {imagesPath}");

            var dirInfo = new DirectoryInfo(imagesPath);
            var fileInfo = dirInfo.GetFiles();
            var files = fileInfo.OrderByDescending(f => f.CreationTime).Select(f => f.FullName).Distinct();

            Files.Clear();
            var items = files.Select(file =>
            {
                if (MovieFileExtensions.Any(ex => file.ToLower().EndsWith(ex, StringComparison.Ordinal)))
                    return new MediaItem {MediaType = ChafuMediaType.Video, Path = "file://" + file};
                if (ImageFileExtensions.Any(ex => file.ToLower().EndsWith(ex, StringComparison.Ordinal)))
                    return new MediaItem {MediaType = ChafuMediaType.Image, Path = file};
                return null;
            }).Where(f => f != null);

            Files.AddRange(items);

            ShowFirstImage();
        }

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
                var sourceMovieUrl = new NSUrl(item.Path);
                using (var asset = AVAsset.FromUrl(sourceMovieUrl))
                using (var generator = AVAssetImageGenerator.FromAsset(asset))
                {
                    generator.AppliesPreferredTrackTransform = true;

                    var duration = asset.Duration.Seconds;
                    var time = new CMTime(Clamp(1, 0, (int)cell.Duration), 1);

                    CMTime actual;
                    NSError error;

                    using (var cgImage = generator.CopyCGImageAtTime(time, out actual, out error))
                    using (var uiImage = new UIImage(cgImage))
                    {
                        var scaledImage = uiImage.ScaleImage(_cellSize);

                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            if (cell.Tag == row)
                            {
                                cell.Duration = duration;
                                cell.Image = scaledImage;
                            }
                        });
                    }
                }

            });
        }

        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section) => Files?.Count ?? 0;

        public void ChangeImage(MediaItem item)
        {
            if (item?.Path == null) return;
            if (_albumView?.ImageCropView == null) return;

            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                if (CurrentMediaType == ChafuMediaType.Video && 
                    _albumView.MoviePlayerController.ContentUrl != null)
                {
                    _albumView.MoviePlayerController.Stop();
                }

                _albumView.ImageCropView.Image = null;
                _albumView.MoviePlayerController.ContentUrl = null;
                CurrentMediaPath = item.Path;
                CurrentMediaType = item.MediaType;

                if (item.MediaType == ChafuMediaType.Image)
                {
                    _albumView.ImageCropView.Hidden = false;
                    _albumView.MovieView.Hidden = true;

                    var image = UIImage.FromFile(item.Path);

                    _albumView.ImageCropView.ImageSize = image.Size;
                    _albumView.ImageCropView.Image = image;
                }
                else
                {
                    _albumView.ImageCropView.Hidden = true;
                    _albumView.MovieView.Hidden = false;

                    var sourceMovieUrl = new NSUrl(item.Path);

                    _albumView.MoviePlayerController.ContentUrl = sourceMovieUrl;
                    _albumView.MoviePlayerController.PrepareToPlay();
                }
            });
        }

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

        public override void ShowFirstImage()
        {
            if (!Files.Any()) return;

            ChangeImage(Files.First());
			CurrentIndexPath = NSIndexPath.FromRowSection(0, 0);
            _albumView?.CollectionView.ReloadData();
            _albumView?.CollectionView.SelectItem(CurrentIndexPath, false,
                UICollectionViewScrollPosition.None);
        }

		public override void DeleteCurrentMediaItem()
		{
			if (CurrentIndexPath == null) return;
			var mediaItem = Files.FirstOrDefault(f => f.Path == CurrentMediaPath);
			if (mediaItem == null) return;

			_albumView.CollectionView.PerformBatchUpdates(() =>
			{
				Files.Remove(mediaItem);
				_albumView.ImageCropView.Image = null;
				_albumView.ImageCropView.ImageSize = CGSize.Empty;
				_albumView.CollectionView.DeleteItems(new [] { CurrentIndexPath });
				File.Delete(CurrentMediaPath);

				CurrentMediaPath = null;
				CurrentIndexPath = null;
			}, null);

			ShowFirstImage();
		}

        public override event EventHandler CameraRollUnauthorized;
    }
}