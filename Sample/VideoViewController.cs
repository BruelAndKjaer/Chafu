using Foundation;
using Fusuma;
using UIKit;

namespace Sample
{
    public class VideoViewController : UIViewController
    {
        private VideoView _videoView;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "Camera";

            _videoView = new VideoView { TranslatesAutoresizingMaskIntoConstraints = false };
            Add(_videoView);


            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(_videoView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Top, 1, 0),
                NSLayoutConstraint.Create(_videoView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Bottom, 1, 0),
                NSLayoutConstraint.Create(_videoView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Width, 1, 0),
                NSLayoutConstraint.Create(_videoView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View,
                    NSLayoutAttribute.Leading, 1, 0)
            });

            _videoView.Initialize(OnVideo);
        }

        private void OnVideo(NSUrl obj)
        {
            UIApplication.SharedApplication.OpenUrl(obj);
        }
    }
}
