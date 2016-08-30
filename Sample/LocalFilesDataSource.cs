using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AVFoundation;
using Chafu;
using CoreFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using UIKit;

namespace Sample
{
    public class MediaItem
    {
        public ChafuMediaType MediaType { get; set; }
        public string Path { get; set; }
    }

    public class LocalFilesDataSource : ChafuAlbumDataSource
    {
        private readonly AlbumView _albumView;
        private readonly CGSize _cellSize;
        public readonly List<MediaItem> Files = new List<MediaItem>();
        private string _imagesPath;

        public override ChafuMediaType CurrentMediaType { get; set; }

        public string ImagesPath
        {
            get { return _imagesPath; }
            set {
                _imagesPath = value;
                UpdateImageSource(_imagesPath);
            }
        }

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

            var files = Directory.GetFiles(imagesPath);

            Files.Clear();
            var items = files.Where(f => f.EndsWith(".jpg") || f.EndsWith(".mov")).Select(file =>
            {
                if (file.EndsWith(".mov"))
                    return new MediaItem {MediaType = ChafuMediaType.Video, Path = "file://" + file};
                if (file.EndsWith(".jpg"))
                    return new MediaItem { MediaType = ChafuMediaType.Image, Path = file };
                return null;
            });

            Files.AddRange(items);

            ShowFirstImage();
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell("AlbumViewCell", indexPath) as AlbumViewCell ??
                       new AlbumViewCell();

            var file = Files?[indexPath.Row];
            if (file?.Path == null) return cell;

            var row = indexPath.Row;

            cell.IsVideo = file.MediaType == ChafuMediaType.Video;
            cell.Image = null;
            cell.Tag = row;

            if (file.MediaType == ChafuMediaType.Image)
            {
                DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
                {
                    var image = UIImage.FromFile(file.Path);
                    var scaledImage = image.ScaleImage(_cellSize);

                    DispatchQueue.MainQueue.DispatchAsync(() =>
                    {
                        if (cell.Tag == row)
                            cell.Image = scaledImage;
                    });
                });
            }
            else
            {
                DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
                {
                    var sourceMovieUrl = new NSUrl(file.Path);
                    var asset = AVAsset.FromUrl(sourceMovieUrl);
                    var generator = AVAssetImageGenerator.FromAsset(asset);
                    var time = new CMTime(Clamp(1, 0, (int)cell.Duration), 1);
                    CMTime actual;
                    NSError error;
                    var cgImage = generator.CopyCGImageAtTime(time, out actual, out error);
                    var uiImage = new UIImage(cgImage);

                    DispatchQueue.MainQueue.DispatchAsync(() =>
                    {
                        if (cell.Tag == row)
                        {
                            cell.Duration = asset.Duration.Seconds;
                            cell.Image = uiImage;
                        }
                    });
                });
            }

            return cell;
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
                _albumView.ImageCropView.Image = null;
                _albumView.MoviePlayerController.ContentUrl = null;
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
            if (Files.Any())
            {
                ChangeImage(Files.First());
                _albumView?.CollectionView.ReloadData();
                _albumView?.CollectionView.SelectItem(NSIndexPath.FromRowSection(0, 0), false, UICollectionViewScrollPosition.None);
            }
        }

        public override event EventHandler CameraRollUnauthorized;
    }
}