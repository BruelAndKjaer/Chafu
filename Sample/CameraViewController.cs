using System;
using System.Collections.Generic;
using System.Text;
using Fusuma;
using UIKit;

namespace Sample
{
    public class CameraViewController : UIViewController
    {
        private CameraView _cameraView;
        private UIImageView _imageView;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "Camera";

            _cameraView = new CameraView {TranslatesAutoresizingMaskIntoConstraints = false};
            Add(_cameraView);

            _imageView = new UIImageView {TranslatesAutoresizingMaskIntoConstraints = false};
            Add(_imageView);
            View.BringSubviewToFront(_cameraView);

            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(_cameraView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Top, 1, 0),
                NSLayoutConstraint.Create(_cameraView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Bottom, 1, 0),
                NSLayoutConstraint.Create(_cameraView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Width, 1, 0),
                NSLayoutConstraint.Create(_cameraView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Leading, 1, 0),
                NSLayoutConstraint.Create(_imageView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Top, 1, 0),
                NSLayoutConstraint.Create(_imageView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Bottom, 1, 0),
                NSLayoutConstraint.Create(_imageView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Width, 1, 0),
                NSLayoutConstraint.Create(_imageView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Leading, 1, 0)
            });

            _cameraView.Initialize(OnImage);
        }

        private void OnImage(UIImage uiImage)
        {
            _imageView.Image = uiImage;
            _cameraView.RemoveFromSuperview();
        }
    }
}
