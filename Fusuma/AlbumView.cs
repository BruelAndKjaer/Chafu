using System;
using CoreGraphics;
using Foundation;
using UIKit;

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
        public UIView ImageCropViewContainer { get; private set; }
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
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            CollectionViewConstraintHeight = NSLayoutConstraint.Create(CollectionView, NSLayoutAttribute.Height,
                NSLayoutRelation.Equal);
            CollectionViewConstraintHeight.Constant = 150;
            CollectionView.AddConstraint(CollectionViewConstraintHeight);

            var collectionViewWrapper = new UIView(new CGRect(0, 0, 400, 600)) { CollectionView };
            collectionViewWrapper.AccessibilityLabel = "CollectionViewWrapper";
            collectionViewWrapper.TranslatesAutoresizingMaskIntoConstraints = false;
            Add(collectionViewWrapper);
            collectionViewWrapper.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(CollectionView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal,
                    collectionViewWrapper, NSLayoutAttribute.Bottom, 1, 0),
                NSLayoutConstraint.Create(CollectionView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal,
                    collectionViewWrapper, NSLayoutAttribute.Trailing, 1, 0),
                NSLayoutConstraint.Create(CollectionView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal,
                    collectionViewWrapper, NSLayoutAttribute.Leading, 1, 0)
            });

            ImageCropView = new ImageCropView
            {
                Frame = new CGRect(0, 0, 400, 400),
                AccessibilityLabel = "ImageCropView",
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ImageCropView.AddConstraint(NSLayoutConstraint.Create(ImageCropView, NSLayoutAttribute.Width, NSLayoutRelation.Equal,
                ImageCropView, NSLayoutAttribute.Height, 1, 0));

            ImageCropViewContainer = new UIView(new CGRect(0, 50, 400, 400)) { ImageCropView };
            ImageCropViewContainer.BackgroundColor = UIColor.White;
            ImageCropViewContainer.AccessibilityLabel = "ImageCropViewContainer";
            ImageCropViewContainer.TranslatesAutoresizingMaskIntoConstraints = false;
            Add(ImageCropViewContainer);
            ImageCropViewContainer.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(ImageCropView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal,
                    ImageCropViewContainer, NSLayoutAttribute.Leading, 1, 0),
                NSLayoutConstraint.Create(ImageCropView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal,
                    ImageCropViewContainer, NSLayoutAttribute.Bottom, 1, 0),
                NSLayoutConstraint.Create(ImageCropView, NSLayoutAttribute.Top, NSLayoutRelation.Equal,
                    ImageCropViewContainer, NSLayoutAttribute.Top, 1, 0),
                NSLayoutConstraint.Create(ImageCropViewContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal,
                    ImageCropViewContainer, NSLayoutAttribute.Height, 1, 0),
                NSLayoutConstraint.Create(ImageCropView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal,
                    ImageCropViewContainer, NSLayoutAttribute.Trailing, 1, 0)
            });

			BackgroundColor = Configuration.BackgroundColor;

            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(ImageCropViewContainer, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Trailing, 1, 0),
                NSLayoutConstraint.Create(collectionViewWrapper, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Bottom, 1, 0),
                NSLayoutConstraint.Create(ImageCropViewContainer, NSLayoutAttribute.Leading, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Leading, 1, 0),
                NSLayoutConstraint.Create(collectionViewWrapper, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Trailing, 1, 0),
                NSLayoutConstraint.Create(collectionViewWrapper, NSLayoutAttribute.Top, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Top, 1, 0),
                ImageCropViewConstraintTop =
                    NSLayoutConstraint.Create(ImageCropViewContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal,
                        this, NSLayoutAttribute.Top, 1, 0),
                NSLayoutConstraint.Create(collectionViewWrapper, NSLayoutAttribute.Leading, NSLayoutRelation.Equal,
                    this, NSLayoutAttribute.Leading, 1, 0)
            });

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

            ImageCropViewContainer.Layer.ShadowColor = UIColor.Black.CGColor;
            ImageCropViewContainer.Layer.ShadowRadius = 30;
            ImageCropViewContainer.Layer.ShadowOpacity = 0.9f;
            ImageCropViewContainer.Layer.ShadowOffset = CGSize.Empty;

            CollectionView.BackgroundColor = Configuration.BackgroundColor;

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
                _cropBottomY = ImageCropViewContainer.Frame.Y + ImageCropViewContainer.Frame.Height;

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
                        (nfloat) Math.Max(_imageCropViewMinimalVisibleHeight - ImageCropViewContainer.Frame.Height,
                            currentPos.Y + _dragDiff - ImageCropViewContainer.Frame.Height);
                    CollectionViewConstraintHeight.Constant =
                        (nfloat) Math.Min(Frame.Height - _imageCropViewMinimalVisibleHeight,
                            Frame.Height - ImageCropViewConstraintTop.Constant - ImageCropViewContainer.Frame.Height);
                }
                else if (DragDirection == DragDirection.Down && currentPos.Y > _cropBottomY)
                {
                    ImageCropViewConstraintTop.Constant =
                        (nfloat) Math.Min(ImageCropViewOriginalConstraintTop,
                            currentPos.Y - ImageCropViewContainer.Frame.Height);
                    CollectionViewConstraintHeight.Constant =
                        (nfloat)
                            Math.Max(
                                Frame.Height - ImageCropViewOriginalConstraintTop - ImageCropViewContainer.Frame.Height,
                                Frame.Height - ImageCropViewConstraintTop.Constant - ImageCropViewContainer.Frame.Height);
                }
                else if (DragDirection == DragDirection.Stop && CollectionView.ContentOffset.Y < 0)
                {
                    DragDirection = DragDirection.Scroll;
                    _imaginaryCollectionViewOffsetStartPosY = currentPos.Y;
                }
                else if (DragDirection == DragDirection.Scroll)
                {
                    ImageCropViewConstraintTop.Constant = _imageCropViewMinimalVisibleHeight -
                                                          ImageCropViewContainer.Frame.Height + currentPos.Y -
                                                          _imaginaryCollectionViewOffsetStartPosY;
                    CollectionViewConstraintHeight.Constant =
                        (nfloat)
                            Math.Max(
                                Frame.Height - ImageCropViewOriginalConstraintTop - ImageCropViewContainer.Frame.Height,
                                Frame.Height - ImageCropViewConstraintTop.Constant - ImageCropViewContainer.Frame.Height);
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
                                                          ImageCropViewContainer.Frame.Height;
                    CollectionViewConstraintHeight.Constant = Frame.Height - _imageCropViewMinimalVisibleHeight;

                    AnimateNotify(0.3, 0, UIViewAnimationOptions.CurveEaseOut, LayoutIfNeeded, finished => { });
                    DragDirection = DragDirection.Down;
                }
                else
                {
                    // Get back to the original position
                    ImageCropView.Scrollable = true;

                    ImageCropViewConstraintTop.Constant = ImageCropViewOriginalConstraintTop;
                    CollectionViewConstraintHeight.Constant = Frame.Height - ImageCropViewOriginalConstraintTop -
                                                              ImageCropViewContainer.Frame.Height;
                    AnimateNotify(0.3, 0, UIViewAnimationOptions.CurveEaseOut, LayoutIfNeeded, finished => {});
                    DragDirection = DragDirection.Up;
                }
            }
        }

        public AlbumView() { CreateView(); }
        public AlbumView(IntPtr handle) : base(handle) { CreateView(); }
    }
}
