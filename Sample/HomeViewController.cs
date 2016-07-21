using CoreGraphics;
using UIKit;

namespace Sample
{
    public class HomeViewController : UIViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Title = "Fusuma";

            var albumButton = new UIButton(UIButtonType.System);
            albumButton.SetTitle("Album", UIControlState.Normal);
            albumButton.Frame = new CGRect(10, 70, 310, 50);
            albumButton.TouchUpInside +=
                (sender, args) => NavigationController.PushViewController(new AlbumViewController(), true);

            var cameraButton = new UIButton(UIButtonType.System);
            cameraButton.SetTitle("Camera", UIControlState.Normal);
            cameraButton.Frame = new CGRect(10, 130, 310, 50);
            cameraButton.TouchUpInside +=
                (sender, args) => NavigationController.PushViewController(new CameraViewController(), true);

            Add(albumButton);
            Add(cameraButton);
        }
    }
}