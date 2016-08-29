using System;
using System.Collections.Generic;
using System.Linq;
using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using Photos;
using UIKit;

namespace Chafu
{
    public class PhotoGalleryDataSource : ChafuAlbumDataSource, IPHPhotoLibraryChangeObserver
    {
        private readonly AlbumView _albumView;
        private readonly CGSize _cellSize;
        private CGRect _previousPreheatRect = CGRect.Empty;
        private PHAsset _asset;

        public override event EventHandler CameraRollUnauthorized;

        public PHFetchResult Videos { get; set; }

        public PHFetchResult Images { get; set; }
        public List<PHAsset> AllAssets { get; } = new List<PHAsset>();
        public PHCachingImageManager ImageManager { get; private set; }

        public PhotoGalleryDataSource(AlbumView albumView, CGSize cellSize)
        {
            _albumView = albumView;
            _cellSize = cellSize != CGSize.Empty ? cellSize : new CGSize(100, 100);

            CheckPhotoAuthorization();

            var options = new PHFetchOptions
            {
                SortDescriptors = new[] {new NSSortDescriptor("creationDate", false)}
            };

            Images = PHAsset.FetchAssets(PHAssetMediaType.Image, options);
            Videos = PHAsset.FetchAssets(PHAssetMediaType.Video, options);

            AllAssets.AddRange(Images.OfType<PHAsset>());
            AllAssets.AddRange(Videos.OfType<PHAsset>());

            ShowFirstImage();

            PHPhotoLibrary.SharedPhotoLibrary.RegisterChangeObserver(this);
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = collectionView.DequeueReusableCell("AlbumViewCell", indexPath) as AlbumViewCell ??
                       new AlbumViewCell();

            if (ImageManager == null) return cell;

            if (cell.Tag != 0)
                ImageManager.CancelImageRequest((int)cell.Tag);

            var asset = AllAssets[(int)indexPath.Item];

            cell.IsVideo = asset.MediaType == PHAssetMediaType.Video;
            cell.Duration = asset.Duration;

            cell.Tag = ImageManager.RequestImageForAsset(asset, _cellSize, PHImageContentMode.AspectFill, null,
                (result, info) =>
                {
                    cell.Image = result;
                    cell.Tag = 0;
                });

            return cell;
        }

        public override nint NumberOfSections(UICollectionView collectionView) => 1;

        public override nint GetItemsCount(UICollectionView collectionView, nint section) => AllAssets?.Count ?? 0;

        public void PhotoLibraryDidChange(PHChange changeInstance)
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                var collctionChanges = changeInstance.GetFetchResultChangeDetails(Images);
                if (collctionChanges == null) return;

                Images = collctionChanges.FetchResultAfterChanges;

                var collectionView = _albumView.CollectionView;

                if (!collctionChanges.HasIncrementalChanges || collctionChanges.HasMoves)
                    collectionView.ReloadData();
                else
                {
                    collectionView.PerformBatchUpdates(() =>
                    {
                        var removedIndexes = collctionChanges.RemovedIndexes;
                        if ((removedIndexes?.Count ?? 0) != 0)
                            collectionView.DeleteItems(IndexPathsFromIndexSet(removedIndexes, 0));

                        var insertedIndexes = collctionChanges.InsertedIndexes;
                        if ((insertedIndexes?.Count ?? 0) != 0)
                            collectionView.InsertItems(IndexPathsFromIndexSet(insertedIndexes, 0));

                        var changedIndexes = collctionChanges.ChangedIndexes;
                        if ((changedIndexes?.Count ?? 0) != 0)
                            collectionView.ReloadItems(IndexPathsFromIndexSet(changedIndexes, 0));
                    }, null);
                }

                ResetCachedAssets();
            });
        }

        private void ResetCachedAssets()
        {
            ImageManager?.StopCaching();
            _previousPreheatRect = CGRect.Empty;
        }

        public void UpdateCachedAssets()
        {
            var collectionView = _albumView.CollectionView;

            var preheatRect = collectionView.Bounds;
            preheatRect = CGRect.Inflate(preheatRect, 0.0f, 0.5f*preheatRect.Height);

            var delta = Math.Abs(preheatRect.GetMidY() - _previousPreheatRect.GetMidY());
            if (delta > collectionView.Bounds.Height/3.0)
            {
                var addedIndexPaths = new List<NSIndexPath>();
                var removedIndexPaths = new List<NSIndexPath>();

                var rects = ComputeDifferenceBetweenRect(_previousPreheatRect, preheatRect);

                foreach (var rect in rects.Item1) {
                    var indexPaths = IndexPathsForElementsInRect(collectionView, rect);
                    addedIndexPaths.AddRange(indexPaths);
                }

                foreach (var rect in rects.Item2) {
                    var indexPaths = IndexPathsForElementsInRect(collectionView, rect);
                    removedIndexPaths.AddRange(indexPaths);
                }

                var assetsToStartCaching = AssetsAtIndexPaths(addedIndexPaths);
                var assetsToStopCaching = AssetsAtIndexPaths(removedIndexPaths);

                ImageManager?.StartCaching(assetsToStartCaching, _cellSize, PHImageContentMode.AspectFill,
                    null);
                ImageManager?.StopCaching(assetsToStopCaching, _cellSize, PHImageContentMode.AspectFill,
                    null);

                _previousPreheatRect = preheatRect;
            }
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

            if (newMaxY > oldMaxY) {
                var rectToAdd = new CGRect(newRect.X, oldMaxY, newRect.Width, newMaxY - oldMaxY);
                addedRects.Add(rectToAdd);
            }
            if (oldMinY > newMinY) {
                var rectToAdd = new CGRect(newRect.X, newMinY, newRect.Width, oldMinY - newMinY);
                addedRects.Add(rectToAdd);
            }
            if (newMaxY < oldMaxY) {
                var rectToRemove = new CGRect(newRect.X, newMaxY, newRect.Width, oldMaxY - newMaxY);
                removedRects.Add(rectToRemove);
            }
            if (oldMinY < newMinY) {
                var rectToRemove = new CGRect(newRect.X, oldMinY, newRect.Width, newMinY - oldMinY);
                removedRects.Add(rectToRemove);
            }

            return new Tuple<IEnumerable<CGRect>, IEnumerable<CGRect>>(addedRects, removedRects);
        }

        private static NSIndexPath[] IndexPathsFromIndexSet(NSIndexSet set, nint section)
        {
            var paths = new List<NSIndexPath>();

            set.EnumerateIndexes((nuint idx, ref bool stop) =>
            {
                paths.Add(NSIndexPath.FromItemSection((nint)idx, section));
            });

            return paths.ToArray();
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

        public void CheckPhotoAuthorization()
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
                        ImageManager = new PHCachingImageManager();
                        if (AllAssets != null && AllAssets.Any())
                            ChangeAsset(AllAssets.First());
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
            {
                DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
                {
                    var options = new PHImageRequestOptions { NetworkAccessAllowed = true };
                    var assetSize = new CGSize(asset.PixelWidth, asset.PixelHeight);

                    ImageManager?.RequestImageForAsset(asset, assetSize,
                        PHImageContentMode.AspectFill, options,
                        (result, info) =>
                        {
                            DispatchQueue.MainQueue.DispatchAsync(() =>
                            {
                                CurrentMediaType = ChafuMediaType.Image;
                                _albumView.ImageCropView.Hidden = false;
                                _albumView.MovieView.Hidden = true;
                                _albumView.ImageCropView.ImageSize = assetSize;
                                _albumView.ImageCropView.Image = result;
                            });
                        });
                });
            }
            else if (asset.MediaType == PHAssetMediaType.Video)
            {
                DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
                {
                    var options = new PHVideoRequestOptions { NetworkAccessAllowed = true };
                    ImageManager?.RequestAvAsset(asset, options,
                        (ass, mix, info) =>
                        {
                            DispatchQueue.MainQueue.DispatchAsync(() =>
                            {
                                CurrentMediaType = ChafuMediaType.Video;
                                _albumView.ImageCropView.Hidden = true;
                                _albumView.MovieView.Hidden = false;

                                var urlAsset = ass as AVUrlAsset;
                                if (urlAsset == null) return;
                                _albumView.MoviePlayerController.ContentUrl = urlAsset.Url;
                                _albumView.MoviePlayerController.PrepareToPlay();
                            });
                        });
                });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (PHPhotoLibrary.AuthorizationStatus == PHAuthorizationStatus.Authorized)
                    PHPhotoLibrary.SharedPhotoLibrary.UnregisterChangeObserver(this);

                _asset.Dispose();
                _asset = null;
            }

            base.Dispose(disposing);
        }

        public override void GetCroppedImage(Action<UIImage> onImage)
        {
			var scale = UIScreen.MainScreen.Scale;

            var view = _albumView.ImageCropView;

            var normalizedX = view.ContentOffset.X / view.ContentSize.Width;
            var normalizedY = view.ContentOffset.Y / view.ContentSize.Height;

            var normalizedWidth = view.Frame.Width / view.ContentSize.Width;
            var normalizedHeight = view.Frame.Height / view.ContentSize.Height;

            var cropRect = new CGRect(normalizedX, normalizedY, normalizedWidth, normalizedHeight);

            DispatchQueue.DefaultGlobalQueue.DispatchAsync (() => {
				var options = new PHImageRequestOptions {
					DeliveryMode = PHImageRequestOptionsDeliveryMode.HighQualityFormat,
					NetworkAccessAllowed = true,
					NormalizedCropRect = cropRect,
					ResizeMode = PHImageRequestOptionsResizeMode.Exact
				};

				var targetWidth = Math.Floor ((float)_asset.PixelWidth * cropRect.Width);
				var targetHeight = Math.Floor ((float)_asset.PixelHeight * cropRect.Height);
				var dimension = Math.Max (Math.Min (targetHeight, targetWidth), 1024 * scale);

				var targetSize = new CGSize (dimension, dimension);

				PHImageManager.DefaultManager.RequestImageForAsset (_asset, targetSize, PHImageContentMode.AspectFill,
                    options, (result, info) => DispatchQueue.MainQueue.DispatchAsync(() => onImage?.Invoke (result)));;
			});
        }

        public override ChafuMediaType CurrentMediaType { get; set; }
        public override void ShowFirstImage()
        {
            if (AllAssets.Any())
            {
                AllAssets.Sort((a, b) => (int)a.CreationDate.Compare(b.CreationDate));
                AllAssets.Reverse();

                ChangeAsset(AllAssets.First());
                _albumView.CollectionView.ReloadData();
                _albumView.CollectionView.SelectItem(NSIndexPath.FromRowSection(0, 0), false, UICollectionViewScrollPosition.None);
            }
        }
    }
}