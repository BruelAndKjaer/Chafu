using System;
using System.Collections.Generic;
using System.Text;
using CoreGraphics;
using Fusuma;
using UIKit;

namespace Sample
{
    public class AlbumViewController : UIViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Title = "Hello!";

            View.AccessibilityLabel = "Root";

            var albumView = new AlbumView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                AccessibilityLabel = "AlbumView"
            };
            albumView.CollectionView.RegisterClassForCell(typeof(AlbumViewCell), "AlbumViewCell");

            var dataSource = new PhotoGalleryDataSource(albumView, new CGSize(100, 100));
            var @delegate = new PhotoGalleryDelegate(albumView, dataSource);

            View.Add(albumView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(albumView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Top, 1, 0),
                NSLayoutConstraint.Create(albumView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Bottom, 1, 0),
                NSLayoutConstraint.Create(albumView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Width, 1, 0),
                NSLayoutConstraint.Create(albumView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Leading, 1, 0)
            });

            albumView.Initialize(dataSource, @delegate);
        }
    }
}
