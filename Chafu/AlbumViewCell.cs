using System;
using UIKit;

namespace Chafu
{
    public sealed class AlbumViewCell : UICollectionViewCell
    {
        private UIImageView _imageView;

        public override bool Selected
        {
            get
            {
                return base.Selected;
            }
            set
            {
                base.Selected = value;
                Layer.BorderColor = value ? Configuration.TintColor.CGColor : UIColor.Clear.CGColor;
                Layer.BorderWidth = value ? 2 : 0;
            }
        }

        public UIImage Image
        {
            get { return _imageView.Image; }
            set { _imageView.Image = value; }
        }

        public AlbumViewCell()
        {
            CreateView();
        }

        public AlbumViewCell(IntPtr handle) : base(handle)
        {
            CreateView();
        }

        private void CreateView()
        {
            _imageView = new UIImageView { TranslatesAutoresizingMaskIntoConstraints = false };
            Add(_imageView);

            var bottomConstraint = NSLayoutConstraint.Create(_imageView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal,
                this, NSLayoutAttribute.Bottom, 1, 0);
            var leadingConstraint = NSLayoutConstraint.Create(_imageView, NSLayoutAttribute.Leading,
                NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading, 1, 0);
            var trailingConstraint = NSLayoutConstraint.Create(_imageView, NSLayoutAttribute.Trailing,
                NSLayoutRelation.Equal, this, NSLayoutAttribute.Trailing, 1, 0);
            var topConstraint = NSLayoutConstraint.Create(_imageView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,
                NSLayoutAttribute.Top, 1, 0);

            AddConstraints(new[] { bottomConstraint, topConstraint, leadingConstraint, trailingConstraint });
        }
    }
}
