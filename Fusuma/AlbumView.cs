using System;
using CoreGraphics;
using UIKit;
using Cirrious.FluentLayouts.Touch;

namespace Fusuma
{
    public class AlbumView : UIView
    {
        private CGPoint _dragStartPos = CGPoint.Empty;
        private nfloat _cropBottomY;
        private float _dragDiff = 20f;
        private float _imageCropViewMinimalVisibleHeight = 100;
        private nfloat _imaginaryCollectionViewOffsetStartPosY;

        public static readonly int ImageCropViewOriginalConstraintTop = 50;
        
        public ImageCropView ImageCropView { get; private set; }
        public NSLayoutConstraint ImageCropViewConstraintTop { get; private set; }
        public NSLayoutConstraint CollectionViewConstraintHeight { get; private set; }
        public NSLayoutConstraint CollectionViewConstraintTop { get; private set; }
        public UICollectionView CollectionView { get; private set; }

        public DragDirection DragDirection { get; set; }

        private void CreateView()
        {
            var collectionFlowLayout = new UICollectionViewFlowLayout
            {
                MinimumInteritemSpacing = 1,
                MinimumLineSpacing = 1,
                ItemSize = new CGSize(60, 60),
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

			Add (ImageCropView);

			BackgroundColor = Configuration.BackgroundColor;

            CollectionViewConstraintHeight = NSLayoutConstraint.Create(CollectionView, NSLayoutAttribute.Height,
                NSLayoutRelation.Equal);
            CollectionViewConstraintHeight.Constant = 150;
            CollectionView.AddConstraint(CollectionViewConstraintHeight);

            AddConstraints(new[]
            {
                ImageCropViewConstraintTop =
                    NSLayoutConstraint.Create(ImageCropView, NSLayoutAttribute.Top, NSLayoutRelation.Equal,
                        this, NSLayoutAttribute.Top, 1, 0),
                CollectionViewConstraintTop =
                    NSLayoutConstraint.Create(CollectionView, NSLayoutAttribute.Top, NSLayoutRelation.Equal,
                        ImageCropView, NSLayoutAttribute.Bottom, 1, 0)
            });

            this.AddConstraints(
                ImageCropView.Width().EqualTo().HeightOf(ImageCropView),
                ImageCropView.AtLeftOf(this),
                ImageCropView.AtRightOf(this),

                CollectionView.AtBottomOf(this),
                CollectionView.AtLeftOf(this),
                CollectionView.AtRightOf(this)
            );

            Hidden = true;

            CollectionView.RegisterClassForCell(typeof(AlbumViewCell), "AlbumViewCell");
        }

        public void Initialize(UICollectionViewDataSource dataSource, UICollectionViewDelegate @delegate)
        {
            Hidden = false;

            var panGesture = new UIPanGestureRecognizer(Panned)
            {
                ShouldRecognizeSimultaneously = (recognizer, gestureRecognizer) => true
            };
            AddGestureRecognizer(panGesture);

            CollectionViewConstraintHeight.Constant = Frame.Height - ImageCropView.Frame.Height -
                                                      ImageCropViewOriginalConstraintTop;
            ImageCropViewConstraintTop.Constant = 50;
            DragDirection = DragDirection.Up;

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

            if (sender.State == UIGestureRecognizerState.Began)
            {
                var view = sender.View;
                var loc = sender.LocationInView(view);
                var subView = view?.HitTest(loc, null);

                if (subView != null && subView.Equals(ImageCropView) &&
                    ImageCropViewConstraintTop.Constant == ImageCropViewOriginalConstraintTop)
                {
                    return;
                }

                _dragStartPos = currentPos;
                _cropBottomY = ImageCropView.Frame.Y + ImageCropView.Frame.Height;

                if (DragDirection == DragDirection.Stop)
                    DragDirection = ImageCropViewConstraintTop.Constant == ImageCropViewOriginalConstraintTop
                        ? DragDirection.Up
                        : DragDirection.Down;

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
            else if (sender.State == UIGestureRecognizerState.Changed)
            {
                if (DragDirection == DragDirection.Up && currentPos.Y < _cropBottomY - _dragDiff)
                {
                    ImageCropViewConstraintTop.Constant =
                        (nfloat) Math.Max(_imageCropViewMinimalVisibleHeight - ImageCropView.Frame.Height,
                            currentPos.Y + _dragDiff - ImageCropView.Frame.Height);
                    CollectionViewConstraintHeight.Constant =
                        (nfloat) Math.Min(Frame.Height - _imageCropViewMinimalVisibleHeight,
                            Frame.Height - ImageCropViewConstraintTop.Constant - ImageCropView.Frame.Height);
                }
                else if (DragDirection == DragDirection.Down && currentPos.Y > _cropBottomY)
                {
                    ImageCropViewConstraintTop.Constant =
                        (nfloat) Math.Min(ImageCropViewOriginalConstraintTop,
                            currentPos.Y - ImageCropView.Frame.Height);
                    CollectionViewConstraintHeight.Constant =
                        (nfloat)
                            Math.Max(
                                Frame.Height - ImageCropViewOriginalConstraintTop - ImageCropView.Frame.Height,
                                Frame.Height - ImageCropViewConstraintTop.Constant - ImageCropView.Frame.Height);
                }
                else if (DragDirection == DragDirection.Stop && CollectionView.ContentOffset.Y < 0)
                {
                    DragDirection = DragDirection.Scroll;
                    _imaginaryCollectionViewOffsetStartPosY = currentPos.Y;
                }
                else if (DragDirection == DragDirection.Scroll)
                {
                    ImageCropViewConstraintTop.Constant = _imageCropViewMinimalVisibleHeight -
                                                          ImageCropView.Frame.Height + currentPos.Y -
                                                          _imaginaryCollectionViewOffsetStartPosY;
                    CollectionViewConstraintHeight.Constant =
                        (nfloat)
                            Math.Max(
                                Frame.Height - ImageCropViewOriginalConstraintTop - ImageCropView.Frame.Height,
                                Frame.Height - ImageCropViewConstraintTop.Constant - ImageCropView.Frame.Height);
                }
            }
            else
            {
                _imageCropViewMinimalVisibleHeight = 0;

                if (sender.State == UIGestureRecognizerState.Ended && DragDirection == DragDirection.Stop)
                {
                    ImageCropView.Scrollable = true;
                    return;
                }

                if (currentPos.Y < _cropBottomY - _dragDiff &&
                    ImageCropViewConstraintTop.Constant != ImageCropViewOriginalConstraintTop)
                {
                    // The largest movement
                    ImageCropView.Scrollable = false;
                    ImageCropViewConstraintTop.Constant = _imageCropViewMinimalVisibleHeight -
                                                          ImageCropView.Frame.Height;
                    CollectionViewConstraintHeight.Constant = Frame.Height - _imageCropViewMinimalVisibleHeight;
                    CollectionViewConstraintTop.Constant = ImageCropViewOriginalConstraintTop;

                    AnimateNotify(0.3, 0, UIViewAnimationOptions.CurveEaseOut, LayoutIfNeeded, finished => { });
                    DragDirection = DragDirection.Down;
                }
                else
                {
                    // Get back to the original position
                    ImageCropView.Scrollable = true;

                    ImageCropViewConstraintTop.Constant = ImageCropViewOriginalConstraintTop;
                    CollectionViewConstraintHeight.Constant = Frame.Height - ImageCropViewOriginalConstraintTop -
                                                              ImageCropView.Frame.Height;
                    CollectionViewConstraintTop.Constant = 0;
                    AnimateNotify(0.3, 0, UIViewAnimationOptions.CurveEaseOut, LayoutIfNeeded, finished => {});
                    DragDirection = DragDirection.Up;
                }
            }
        }

        public AlbumView() { CreateView(); }
        public AlbumView(IntPtr handle) : base(handle) { CreateView(); }
    }
}
