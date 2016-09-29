using System;
using CoreGraphics;
using UIKit;
using Cirrious.FluentLayouts.Touch;
using MediaPlayer;

namespace Chafu
{
    public class AlbumView : UIView
    {
        private CGPoint _dragStartPos = CGPoint.Empty;
        private nfloat _cropBottomY;
        private float _dragDiff = 20f;
        private float _imageCropViewMinimalVisibleHeight = 50;
        private nfloat _imaginaryCollectionViewOffsetStartPosY;

        public static readonly int ImageCropViewOriginalConstraintTop = 0;

        public MPMoviePlayerController MoviePlayerController { get; private set; }
        public UIView MovieView { get; private set; }
        public ImageCropView ImageCropView { get; private set; }
        public NSLayoutConstraint ImageCropViewConstraintTop { get; private set; }
        public NSLayoutConstraint MovieViewConstraintTop { get; private set; }
        public NSLayoutConstraint CollectionViewConstraintHeight { get; private set; }
        public NSLayoutConstraint CollectionViewConstraintTop { get; private set; }
        public UICollectionView CollectionView { get; private set; }
        public CGSize CellSize { get; }

        public DragDirection DragDirection { get; set; }

        private void CreateView()
        {
            CreateControls();
            CreateConstraints();

            Hidden = true;
            CollectionView.RegisterClassForCell(typeof(AlbumViewCell), "AlbumViewCell");
        }

        private void CreateControls()
        {
            MoviePlayerController = new MPMoviePlayerController
            {
                AllowsAirPlay = false,
                ScalingMode = MPMovieScalingMode.AspectFit,
                ControlStyle = MPMovieControlStyle.Embedded,
                ShouldAutoplay = false,
                BackgroundColor = UIColor.Black
            };

            MovieView = MoviePlayerController.View;
            MovieView.TranslatesAutoresizingMaskIntoConstraints = false;
            MovieView.Hidden = true;

            Add(MovieView);

            var collectionFlowLayout = new UICollectionViewFlowLayout
            {
                MinimumInteritemSpacing = 1,
                MinimumLineSpacing = 1,
                ItemSize = CellSize,
                HeaderReferenceSize = new CGSize(0, 0),
                FooterReferenceSize = new CGSize(0, 0),
                SectionInset = new UIEdgeInsets(0, 0, 0, 0)
            };

            CollectionView = new UICollectionView(new CGRect(0, 450, 400, 150), collectionFlowLayout)
            {
                AccessibilityLabel = "CollectionView",
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Configuration.BackgroundColor
            };

            Add(CollectionView);

            ImageCropView = new ImageCropView
            {
                AccessibilityLabel = "ImageCropView",
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Configuration.BackgroundColor
            };

            Add(ImageCropView);

            BackgroundColor = Configuration.BackgroundColor;
        }

        private void CreateConstraints()
        {
            CollectionViewConstraintHeight = NSLayoutConstraint.Create(CollectionView, NSLayoutAttribute.Height,
                NSLayoutRelation.Equal);
            CollectionViewConstraintHeight.Constant = 0;
            CollectionView.AddConstraint(CollectionViewConstraintHeight);

            AddConstraints(new[]
            {
                ImageCropViewConstraintTop =
                    NSLayoutConstraint.Create(ImageCropView, NSLayoutAttribute.Top, NSLayoutRelation.Equal,
                        this, NSLayoutAttribute.Top, 1, 0),
                MovieViewConstraintTop =
                    NSLayoutConstraint.Create(MovieView, NSLayoutAttribute.Top, NSLayoutRelation.Equal,
                        this, NSLayoutAttribute.Top, 1, 0),
                CollectionViewConstraintTop =
                    NSLayoutConstraint.Create(CollectionView, NSLayoutAttribute.Top, NSLayoutRelation.Equal,
                        ImageCropView, NSLayoutAttribute.Bottom, 1, 0)
            });

            this.AddConstraints(
                ImageCropView.Height().EqualTo().WidthOf(ImageCropView),
                ImageCropView.AtLeftOf(this),
                ImageCropView.AtRightOf(this),

                MovieView.Height().EqualTo().WidthOf(MovieView),
                MovieView.AtLeftOf(this),
                MovieView.AtRightOf(this),

                CollectionView.AtBottomOf(this),
                CollectionView.AtLeftOf(this),
                CollectionView.AtRightOf(this)
            );
        }

        public void Initialize(UICollectionViewDataSource dataSource, UICollectionViewDelegate @delegate)
        {
            Hidden = false;

            var panGesture = new UIPanGestureRecognizer(Panned)
            {
                ShouldRecognizeSimultaneously = (recognizer, gestureRecognizer) => true
            };
            AddGestureRecognizer(panGesture);

            SetOriginalConstraints();

            ImageCropView.Layer.ShadowColor = UIColor.Black.CGColor;
            ImageCropView.Layer.ShadowRadius = 30;
            ImageCropView.Layer.ShadowOpacity = 0.9f;
            ImageCropView.Layer.ShadowOffset = CGSize.Empty;

            CollectionView.DataSource = dataSource;
            CollectionView.Delegate = @delegate;

            //CollectionView.ScrollToItem(NSIndexPath.FromItemSection(0,0),UICollectionViewScrollPosition.Top, false);
        }

        private void Panned(UIPanGestureRecognizer sender)
        {
            var currentPos = sender.LocationInView(this);
            var alpha = ImageCropView.Alpha;
            if (sender.State == UIGestureRecognizerState.Began)
                PannedBegan(sender, currentPos);
            else if (sender.State == UIGestureRecognizerState.Changed)
                PannedChanged(currentPos);
            else
                alpha = PannedOtherwise(sender, currentPos, alpha);

            AnimateNotify(0.3, 0, UIViewAnimationOptions.CurveEaseOut, () =>
            {
                ImageCropView.Alpha = alpha;
                MovieView.Alpha = alpha;
                LayoutIfNeeded();
            }, finished => { });
        }

        private void PannedBegan(UIPanGestureRecognizer sender, CGPoint currentPosition)
        {
            var view = sender.View;
            var loc = sender.LocationInView(view);
            var subView = view?.HitTest(loc, null);

            var isImageCropView = subView?.Equals(ImageCropView);

            if (isImageCropView.HasValue && isImageCropView.Value &&
                ImageCropViewConstraintTop.Constant == ImageCropViewOriginalConstraintTop)
            {
                return;
            }

            _dragStartPos = currentPosition;
            _cropBottomY = ImageCropView.Frame.Y + ImageCropView.Frame.Height;

            if (DragDirection == DragDirection.Stop)
            {
                DragDirection = ImageCropViewConstraintTop.Constant == ImageCropViewOriginalConstraintTop
                    ? DragDirection.Up
                    : DragDirection.Down;
            }

            if ((DragDirection == DragDirection.Up && _dragStartPos.Y < _cropBottomY + _dragDiff) ||
                (DragDirection == DragDirection.Down && _dragStartPos.Y > _cropBottomY))
            {
                DragDirection = DragDirection.Stop;
                ImageCropView.Scrollable = false;
            }
            else
            {
                ImageCropView.Scrollable = true;
            }
        }

        private void PannedChanged(CGPoint currentPosition)
        {
            if (DragDirection == DragDirection.Up && currentPosition.Y < _cropBottomY - _dragDiff)
            {
                MovieViewConstraintTop.Constant = ImageCropViewConstraintTop.Constant =
                    (nfloat)Math.Max(_imageCropViewMinimalVisibleHeight - ImageCropView.Frame.Height,
                        currentPosition.Y + _dragDiff - ImageCropView.Frame.Height);
                CollectionViewConstraintHeight.Constant =
                    (nfloat)Math.Min(Frame.Height - _imageCropViewMinimalVisibleHeight,
                        Frame.Height - ImageCropViewConstraintTop.Constant - ImageCropView.Frame.Height);
            }
            else if (DragDirection == DragDirection.Down && currentPosition.Y > _cropBottomY)
            {
                MovieViewConstraintTop.Constant = ImageCropViewConstraintTop.Constant =
                    (nfloat)Math.Min(ImageCropViewOriginalConstraintTop,
                        currentPosition.Y - ImageCropView.Frame.Height);
                CollectionViewConstraintHeight.Constant =
                    (nfloat)
                        Math.Max(
                            Frame.Height - ImageCropViewOriginalConstraintTop - ImageCropView.Frame.Height,
                            Frame.Height - ImageCropViewConstraintTop.Constant - ImageCropView.Frame.Height);
            }
            else if (DragDirection == DragDirection.Stop && CollectionView.ContentOffset.Y < 0)
            {
                DragDirection = DragDirection.Scroll;
                _imaginaryCollectionViewOffsetStartPosY = currentPosition.Y;
            }
            else if (DragDirection == DragDirection.Scroll)
            {
                MovieViewConstraintTop.Constant = ImageCropViewConstraintTop.Constant = _imageCropViewMinimalVisibleHeight -
                                                      ImageCropView.Frame.Height + currentPosition.Y -
                                                      _imaginaryCollectionViewOffsetStartPosY;
                CollectionViewConstraintHeight.Constant =
                    (nfloat)
                        Math.Max(
                            Frame.Height - ImageCropViewOriginalConstraintTop - ImageCropView.Frame.Height,
                            Frame.Height - ImageCropViewConstraintTop.Constant - ImageCropView.Frame.Height);
            }
        }

        private nfloat PannedOtherwise(UIGestureRecognizer sender, CGPoint currentPosition, nfloat alpha)
        {
            _imaginaryCollectionViewOffsetStartPosY = 0;

            if (sender.State == UIGestureRecognizerState.Ended && DragDirection == DragDirection.Stop)
            {
                ImageCropView.Scrollable = true;
                return alpha;
            }

            if (currentPosition.Y < _cropBottomY - _dragDiff &&
                ImageCropViewConstraintTop.Constant != ImageCropViewOriginalConstraintTop)
            {
                // The largest movement
                ImageCropView.Scrollable = false;
                alpha = 0.3f;
                MovieViewConstraintTop.Constant = ImageCropViewConstraintTop.Constant = _imageCropViewMinimalVisibleHeight -
                                                      ImageCropView.Frame.Height;
                CollectionViewConstraintHeight.Constant = Frame.Height - _imageCropViewMinimalVisibleHeight;
                CollectionViewConstraintTop.Constant = ImageCropViewOriginalConstraintTop;

                DragDirection = DragDirection.Down;
            }
            else
            {
                // Get back to the original position
                ImageCropView.Scrollable = true;
                alpha = 1.0f;
                SetOriginalConstraints();
            }

            return alpha;
        }

        private void SetOriginalConstraints()
        {
            MovieViewConstraintTop.Constant = ImageCropViewConstraintTop.Constant = ImageCropViewOriginalConstraintTop;
            CollectionViewConstraintHeight.Constant = Frame.Height - ImageCropViewOriginalConstraintTop -
                                                      ImageCropView.Frame.Height;
            CollectionViewConstraintTop.Constant = 0;
            DragDirection = DragDirection.Up;
        }

        public void ClearPreview()
        {
            ImageCropView.Image = null;
            ImageCropView.ImageSize = CGSize.Empty;

            StopVideo();
            MoviePlayerController.ContentUrl = null;
        }

        public void StopVideo()
        {
            if (MoviePlayerController?.ContentUrl == null) return;

            MoviePlayerController.Stop();
        }

        public AlbumView(CGSize cellSize)
        {
            CellSize = cellSize;
            CreateView();
        }
        public AlbumView(IntPtr handle) : base(handle) { }
    }
}
