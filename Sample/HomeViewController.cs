using Cirrious.FluentLayouts.Touch;
using Fusuma;
using UIKit;

namespace Sample
{
    public class HomeViewController : UIViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Title = "Fusuma";

            var imageView = new UIImageView();

            var albumButton = new UIButton(UIButtonType.System);
            albumButton.SetTitle("Album", UIControlState.Normal);
            albumButton.TouchUpInside +=
                (sender, args) => NavigationController.PushViewController(new AlbumViewController(), true);

            var cameraButton = new UIButton(UIButtonType.System);
            cameraButton.SetTitle("Camera", UIControlState.Normal);
            cameraButton.TouchUpInside +=
                (sender, args) => NavigationController.PushViewController(new CameraViewController(), true);

            var videoButton = new UIButton(UIButtonType.System);
            videoButton.SetTitle("Video", UIControlState.Normal);
            videoButton.TouchUpInside +=
                (sender, args) => NavigationController.PushViewController(new VideoViewController(), true);

            var fusumaViewController = new FusumaViewController {HasVideo = true};
            fusumaViewController.ImageSelected += (sender, image) => imageView.Image = image;
            var fullPickerButton = new UIButton(UIButtonType.System);
            fullPickerButton.SetTitle("Full Picker", UIControlState.Normal);
            fullPickerButton.TouchUpInside += (sender, args) =>
            {
                NavigationController.PushViewController(fusumaViewController, true);
            };

            Add(imageView);
            Add(albumButton);
            Add(cameraButton);
            Add(videoButton);

            View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();

            View.AddConstraints(
                imageView.AtTopOf(View, 70),
                imageView.AtLeftOf(View, 10),
                imageView.AtRightOf(View, 10),
                imageView.Height().EqualTo(200),

                albumButton.Below(imageView, 10),
                albumButton.WithSameWidth(imageView),
                albumButton.Height().EqualTo(40),

                cameraButton.Below(albumButton, 10),
                cameraButton.WithSameWidth(imageView),
                cameraButton.Height().EqualTo(40),

                videoButton.Below(cameraButton, 10),
                videoButton.WithSameWidth(imageView),
                videoButton.Height().EqualTo(40),

                fullPickerButton.Below(videoButton, 10),
                fullPickerButton.WithSameWidth(imageView),
                fullPickerButton.Height().EqualTo(40)
                );
        }
    }
}