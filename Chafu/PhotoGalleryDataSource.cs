using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using Photos;
using UIKit;

namespace Chafu
{
    public class PhotoGalleryDataSource : BaseAlbumDataSource, IPHPhotoLibraryChangeObserver
    {
        private readonly AlbumView _albumView;
        private readonly CGSize _cellSize;
        private CGRect _previousPreheatRect = CGRect.Empty;
        private PHAsset _asset;
        private readonly nfloat _scale;

        public override event EventHandler CameraRollUnauthorized;

        public PHFetchResult Videos { get; set; }
        public PHFetchResult Images { get; set; }
        public ObservableCollection<PHAsset> AllAssets { get; } = new ObservableCollection<PHAsset>();
        public PHCachingImageManager ImageManager { get; private set; }

        public PhotoGalleryDataSource(AlbumView albumView, CGSize cellSize)
        {
            _albumView = albumView;
            _cellSize = cellSize != CGSize.Empty ? cellSize : new CGSize(100, 100);
            _scale = UIScreen.MainScreen.Scale;

            CheckPhotoAuthorization(OnAuthorized);
        }

        private void OnAuthorized()
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                ImageManager = new PHCachingImageManager();

                var options = new PHFetchOptions
                {
                    SortDescriptors = new[] {new NSSortDescriptor("creationDate", false)}
                };

                Images = PHAsset.FetchAssets(PHAssetMediaType.Image, options);
                Videos = PHAsset.FetchAssets(PHAssetMediaType.Video, options);

                var assets = new List<PHAsset>();
                assets.AddRange(Images.OfType<PHAsset>());
                assets.AddRange(Videos.OfType<PHAsset>());
                foreach (var asset in assets.OrderByDescending(a => a.CreationDate.SecondsSinceReferenceDate))
                    AllAssets.Add(asset);

                ShowFirstImage();

                AllAssets.CollectionChanged += AssetsCollectionChanged;
                PHPhotoLibrary.SharedPhotoLibrary.RegisterChangeObserver(this);
            });
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell(AlbumViewCell.Key, indexPath) as AlbumViewCell ??
                       new AlbumViewCell();

            if (ImageManager == null) return cell;

            if (cell.Tag != 0)
                ImageManager.CancelImageRequest((int)cell.Tag);

            var asset = AllAssets[(int)indexPath.Item];

            cell.IsVideo = asset.MediaType == PHAssetMediaType.Video;
            cell.Duration = asset.Duration;

            cell.Tag = ImageManager.RequestImageForAsset(asset, _cellSize, PHImageContentMode.AspectFit, null,
                (result, info) => SetImageCellImage(cell, result));

            return cell;
        }

        private static void SetImageCellImage(AlbumViewCell cell, UIImage image)
        {
            cell.Image = image;
            cell.Tag = 0;
        }

        public override nint NumberOfSections(UICollectionView collectionView) => 1;

        public override nint GetItemsCount(UICollectionView collectionView, nint section) => AllAssets?.Count ?? 0;

        public void PhotoLibraryDidChange(PHChange changeInstance)
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                //var collectionView = _albumView.CollectionView;
                var imageCollectionChanges = changeInstance.GetFetchResultChangeDetails(Images);
                var videoCollctionChanges = changeInstance.GetFetchResultChangeDetails(Videos);

                if (imageCollectionChanges != null)
                    Images = AdjustAssets(Images, imageCollectionChanges);

                if (videoCollctionChanges != null)
                    Videos = AdjustAssets(Videos, videoCollctionChanges);
            });
        }

        private PHFetchResult AdjustAssets(PHFetchResult assets, PHFetchResultChangeDetails changes)
        {
            var before = assets;
            assets = changes.FetchResultAfterChanges;

            foreach (var asset in before.OfType<PHAsset>())
            {
                if (!assets.Contains(asset))
                    AllAssets.Remove(asset);
            }

            foreach (var asset in assets.OfType<PHAsset>().OrderBy(a => a.CreationDate.SecondsSinceReferenceDate))
            {
                if (!AllAssets.Contains(asset))
                    AllAssets.Insert(0, asset);
            }

            return assets;
        }

        private void AssetsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            var collectionView = _albumView.CollectionView;

            if (args.NewItems?.Count > 10 || args.OldItems?.Count > 10)
            {
                ReloadCollectionView(collectionView);
            }
            else if (args.Action == NotifyCollectionChangedAction.Move)
            {
                collectionView.PerformBatchUpdates(() =>
                {
                    var indexPaths = GetAllChangedIndexPaths(args.OldItems.Count,
                        args.NewItems.Count, args.OldStartingIndex, args.NewStartingIndex);
                    collectionView.ReloadItems(indexPaths);
                }, null);
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                collectionView.PerformBatchUpdates(() =>
                {
                    var indexPaths = GetIndexPaths(args.OldItems.Count, args.OldStartingIndex);
                    collectionView.DeleteItems(indexPaths);
                }, null);
            }
            else if (args.Action == NotifyCollectionChangedAction.Add)
            {
                collectionView.PerformBatchUpdates(() =>
                {
                    var indexPaths = GetIndexPaths(args.NewItems.Count, args.NewStartingIndex);
                    collectionView.InsertItems(indexPaths);
                }, null);
            }
            else
            {
                ReloadCollectionView(collectionView);
            }

            ResetCachedAssets();
        }

        private static void ReloadCollectionView(UICollectionView collectionView)
        {
            collectionView.PerformBatchUpdates(() => { }, null);
            collectionView.ReloadData();
        }

        private static NSIndexPath[] GetAllChangedIndexPaths(int oldCount, int newCount, int oldStartIndex,
            int newStartIndex)
        {
            var indexes = new NSIndexPath[oldCount + newCount];

            var startIndex = oldStartIndex;
            for (var i = 0; i < oldCount; i++)
                indexes[i] = NSIndexPath.FromRowSection(startIndex + i, 0);
            startIndex = newStartIndex;
            for (var i = 0; i < oldCount + newCount; i++)
                indexes[i] = NSIndexPath.FromRowSection(startIndex + i, 0);

            return indexes;
        }

        private static NSIndexPath[] GetIndexPaths(int itemCount, int startingIndex)
        {
            var indexPaths = new NSIndexPath[itemCount];
            for (var i = 0; i < indexPaths.Length; ++i)
                indexPaths[i] = NSIndexPath.FromRowSection(startingIndex + i, 0);

            return indexPaths;
        }

        private void ResetCachedAssets()
        {
            ImageManager?.StopCaching();
            _previousPreheatRect = CGRect.Empty;
        }

        public void UpdateCachedAssets()
        {
            var collectionView = _albumView.CollectionView;
            var preheatRect = CGRect.Inflate(collectionView.Bounds, 0.0f, 0.5f* collectionView.Bounds.Height);

            var delta = Math.Abs(preheatRect.GetMidY() - _previousPreheatRect.GetMidY());
            if (delta > collectionView.Bounds.Height/3.0)
            {
                var rects = ComputeDifferenceBetweenRect(_previousPreheatRect, preheatRect);

                var addedIndexPaths = GetIndexPathsForRects(collectionView, rects.Item1);
                var removedIndexPaths = GetIndexPathsForRects(collectionView, rects.Item2);
                var assetsToStartCaching = AssetsAtIndexPaths(addedIndexPaths);
                var assetsToStopCaching = AssetsAtIndexPaths(removedIndexPaths);

                ImageManager?.StartCaching(assetsToStartCaching, _cellSize, PHImageContentMode.AspectFill,
                    null);
                ImageManager?.StopCaching(assetsToStopCaching, _cellSize, PHImageContentMode.AspectFill,
                    null);

                _previousPreheatRect = preheatRect;
            }
        }

        private static IEnumerable<NSIndexPath> GetIndexPathsForRects(UICollectionView collectionView, IEnumerable<CGRect> rects)
        {
            var indexPaths = new List<NSIndexPath>();

            foreach (var rect in rects)
                indexPaths.AddRange(IndexPathsForElementsInRect(collectionView, rect));

            return indexPaths;
        }

        private static Tuple<IEnumerable<CGRect>, IEnumerable<CGRect>> ComputeDifferenceBetweenRect(
            CGRect oldRect, CGRect newRect)
        {
            if (!newRect.IntersectsWith(oldRect))
                return new Tuple<IEnumerable<CGRect>, IEnumerable<CGRect>>(new[] {newRect}, new[] {oldRect});

            var oldMaxY = oldRect.GetMaxY();
            var oldMinY = oldRect.GetMinY();
            var newMaxY = newRect.GetMaxY();
            var newMinY = newRect.GetMinY();

            var addedRects = new List<CGRect>();
            var removedRects = new List<CGRect>();

            if (newMaxY > oldMaxY)
                addedRects.Add(new CGRect(newRect.X, oldMaxY, newRect.Width, newMaxY - oldMaxY));
            if (oldMinY > newMinY)
                addedRects.Add(new CGRect(newRect.X, newMinY, newRect.Width, oldMinY - newMinY));
            if (newMaxY < oldMaxY)
                removedRects.Add(new CGRect(newRect.X, newMaxY, newRect.Width, oldMaxY - newMaxY));
            if (oldMinY < newMinY)
                removedRects.Add(new CGRect(newRect.X, oldMinY, newRect.Width, newMinY - oldMinY));

            return new Tuple<IEnumerable<CGRect>, IEnumerable<CGRect>>(addedRects, removedRects);
        }

        private static IEnumerable<NSIndexPath> IndexPathsForElementsInRect(UICollectionView collectionView, CGRect rect)
        {
            var allLayoutAttributes = collectionView?.CollectionViewLayout?.LayoutAttributesForElementsInRect(rect);
            if (allLayoutAttributes == null) return new NSIndexPath[0];
            if (allLayoutAttributes.Length == 0) return new NSIndexPath[0];

            return allLayoutAttributes.Select(attribute => attribute.IndexPath);
        }

        private PHAsset[] AssetsAtIndexPaths(IEnumerable<NSIndexPath> indexPaths)
        {
            if (indexPaths == null) return new PHAsset[0];
            var paths = indexPaths.ToArray();
            return !paths.Any() ? 
                new PHAsset[0] : 
                paths.Select(path => AllAssets[(int)path.Item]).ToArray();
        }

        public void CheckPhotoAuthorization(Action onAuthorized)
        {
            PHPhotoLibrary.RequestAuthorization(status =>
            {
                switch (status)
                {
                    case PHAuthorizationStatus.Restricted:
                    case PHAuthorizationStatus.Denied:
                        CameraRollUnauthorized?.Invoke(this, EventArgs.Empty);
                        break;
                    case PHAuthorizationStatus.Authorized:
                        onAuthorized?.Invoke();
                        break;
                }
            });
        }

        public void ChangeAsset(PHAsset asset)
        {
            if (asset == null) return;
            if (_albumView?.ImageCropView == null) return;

            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                _albumView.ImageCropView.Image = null;
                _asset = asset;
            });

            if (asset.MediaType == PHAssetMediaType.Image)
                ChangeImage(asset, ImageManager, new PHImageRequestOptions { NetworkAccessAllowed = true });
            else if (asset.MediaType == PHAssetMediaType.Video)
                ChangeVideo(asset, ImageManager, new PHVideoRequestOptions { NetworkAccessAllowed = true });
        }

        private void ChangeImage(PHAsset asset, PHImageManager imageManager, PHImageRequestOptions options)
        {
            var assetSize = new CGSize(asset.PixelWidth, asset.PixelHeight);

            DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
                imageManager?.RequestImageForAsset(asset, assetSize,
                    PHImageContentMode.AspectFill, options,
                    (result, info) =>
                        DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            CurrentMediaType = ChafuMediaType.Image;
                            _albumView.ImageCropView.Hidden = false;
                            _albumView.MovieView.Hidden = true;
                            _albumView.ImageCropView.ImageSize = assetSize;
                            _albumView.ImageCropView.Image = result;
                        })));
        }

        private void ChangeVideo(PHAsset asset, PHImageManager imageManager, PHVideoRequestOptions options)
        {
            DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
                imageManager?.RequestAvAsset(asset, options,
                    (ass, mix, info) =>
                        DispatchQueue.MainQueue.DispatchAsync(() => 
                        {
                            CurrentMediaType = ChafuMediaType.Video;
                            _albumView.ImageCropView.Hidden = true;
                            _albumView.MovieView.Hidden = false;

                            var urlAsset = ass as AVUrlAsset;
                            if (urlAsset == null) return;
                            _albumView.MoviePlayerController.ContentUrl = urlAsset.Url;
                            _albumView.MoviePlayerController.PrepareToPlay();
                        })));
        }

        public override void GetCroppedImage(Action<UIImage> onImage)
        {
            var cropRect = GetCropRect(_albumView.ImageCropView);

            DispatchQueue.DefaultGlobalQueue.DispatchAsync (() =>
            {
                var options = GetRequestOptions(cropRect);
                var targetSize = GetTargetSize(cropRect, _asset, _scale);

				PHImageManager.DefaultManager.RequestImageForAsset (_asset, targetSize, PHImageContentMode.AspectFill,
                    options, (result, info) => DispatchQueue.MainQueue.DispatchAsync(() => onImage?.Invoke (result)));
			});
        }

        private static PHImageRequestOptions GetRequestOptions(CGRect cropRect) =>
            new PHImageRequestOptions {
					DeliveryMode = PHImageRequestOptionsDeliveryMode.HighQualityFormat,
					NetworkAccessAllowed = true,
					NormalizedCropRect = cropRect,
					ResizeMode = PHImageRequestOptionsResizeMode.Exact
            };

        private static CGRect GetCropRect(UIScrollView view)
        {
            var normalizedX = view.ContentOffset.X / view.ContentSize.Width;
            var normalizedY = view.ContentOffset.Y / view.ContentSize.Height;

            var normalizedWidth = view.Frame.Width / view.ContentSize.Width;
            var normalizedHeight = view.Frame.Height / view.ContentSize.Height;

            var cropRect = new CGRect(normalizedX, normalizedY, normalizedWidth, normalizedHeight);

            return cropRect;
        }

        private static CGSize GetTargetSize(CGRect cropRect, PHAsset asset, nfloat scale)
        {
            var targetWidth = Math.Floor((float)asset.PixelWidth * cropRect.Width);
            var targetHeight = Math.Floor((float)asset.PixelHeight * cropRect.Height);
            var dimension = Math.Max(Math.Min(targetHeight, targetWidth), 1024 * scale);

            var targetSize = new CGSize(dimension, dimension);

            return targetSize;
        }

        public override ChafuMediaType CurrentMediaType { get; set; }

        public override void ShowFirstImage()
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                ChangeAsset(AllAssets.FirstOrDefault());
                _albumView.CollectionView.ReloadData();
                _albumView.CollectionView.SelectItem(NSIndexPath.FromRowSection(0, 0), false, UICollectionViewScrollPosition.None);
            });
        }

		public override MediaItem DeleteCurrentMediaItem()
		{
			throw new NotImplementedException();
		}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (PHPhotoLibrary.AuthorizationStatus == PHAuthorizationStatus.Authorized)
                    PHPhotoLibrary.SharedPhotoLibrary.UnregisterChangeObserver(this);

                _albumView.ClearPreview();

                _asset.Dispose();
                _asset = null;
            }

            base.Dispose(disposing);
        }
    }
}