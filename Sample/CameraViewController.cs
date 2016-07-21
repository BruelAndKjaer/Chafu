using System;
using System.Collections.Generic;
using System.Text;
using Fusuma;
using UIKit;

namespace Sample
{
    public class CameraViewController : UIViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "Camera";

            var cameraView = new CameraView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            Add(cameraView);

            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(cameraView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Top, 1, 0),
                NSLayoutConstraint.Create(cameraView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Bottom, 1, 0),
                NSLayoutConstraint.Create(cameraView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Width, 1, 0),
                NSLayoutConstraint.Create(cameraView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Leading, 1, 0)
            });

            cameraView.Initialize();
        }
    }
}
