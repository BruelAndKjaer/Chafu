using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chafu;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Sample
{
    public class LocalFilesDataSource : ChafuAlbumDataSource
    {
        private readonly AlbumView _albumView;
        private readonly CGSize _cellSize;
        public readonly List<string> Images = new List<string>();

        public LocalFilesDataSource(AlbumView albumView, string imagesPath, CGSize cellSize = default(CGSize))
        {
            _albumView = albumView;
            _cellSize = cellSize != CGSize.Empty ? cellSize : new CGSize(100, 100);
            UpdateImageSource(imagesPath);
        }

        public void UpdateImageSource(string imagesPath)
        {
            if (string.IsNullOrEmpty(imagesPath))
                throw new ArgumentNullException(nameof(imagesPath));

            if (!Directory.Exists(imagesPath))
                throw new InvalidOperationException($"Cannot load images from non-existing Directory: {imagesPath}");

            var files = Directory.GetFiles(imagesPath);

            Images.Clear();
            Images.AddRange(files.Where(f => f.EndsWith(".jpg")));

            if (Images.Any())
            {
                ChangeImage(Images.First());
                _albumView?.CollectionView.ReloadData();
                _albumView?.CollectionView.SelectItem(NSIndexPath.FromRowSection(0, 0), false, UICollectionViewScrollPosition.None);
            }
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell("AlbumViewCell", indexPath) as AlbumViewCell ??
                       new AlbumViewCell();

            var path = Images[indexPath.Row];
            var image = UIImage.FromFile(path);
            cell.Image = image.ScaleImage(_cellSize);

            return cell;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section) => Images?.Count ?? 0;

        public void ChangeImage(string imagePath)
        {
            if (_albumView?.ImageCropView == null) return;

            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                _albumView.ImageCropView.Image = null;

                var image = UIImage.FromFile(imagePath);

                _albumView.ImageCropView.ImageSize = image.Size;
                _albumView.ImageCropView.Image = image;
            });
        }

        public override void GetCroppedImage(CGRect cropRect, Action<UIImage> onImage)
        {
            throw new NotImplementedException();
        }

        public override ChafuMediaType CurrentMediaType { get; set; }

        public override event EventHandler CameraRollUnauthorized;

        private static UIImage ScaledImage(UIImage image, CGSize size)
        {
            var scaledImage = image.ScaleImage(size);
            return scaledImage;
        }
    }
}