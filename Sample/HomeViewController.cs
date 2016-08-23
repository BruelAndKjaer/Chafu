using Chafu;
using Cirrious.FluentLayouts.Touch;
using UIKit;

namespace Sample
{
    public class HomeViewController : UIViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Title = "Chafu";

            NavigationController.NavigationBar.BarTintColor = Configuration.TintColor;
            NavigationController.NavigationBar.TintColor = Configuration.BaseTintColor;

            View.BackgroundColor = Configuration.BackgroundColor;

            var imageView = new UIImageView {BackgroundColor = UIColor.Black};

            var urlLabel = new UILabel();

            var chafu = new ChafuViewController { HasVideo = true};
            chafu.ImageSelected += (sender, image) => imageView.Image = image;
            chafu.VideoSelected += (sender, videoUrl) => urlLabel.Text = videoUrl.AbsoluteString;
            chafu.Closed += (sender, e) => { /* do stuff on closed */ };

            var pickerButton = new UIButton(UIButtonType.System) {
                BackgroundColor = Configuration.TintColor,
                TintColor = UIColor.Black
            };
            pickerButton.SetTitle("Pick Image", UIControlState.Normal);
            pickerButton.TouchUpInside += (sender, args) =>
            {
				NavigationController.PresentModalViewController (chafu, true);
            };

            var albumViewController = new AlbumViewController();
            albumViewController.ImageSelected += (sender, image) =>
            {
                imageView.Image = image;
            };
            
            var albumButton = new UIButton(UIButtonType.System)
            {
                BackgroundColor = Configuration.TintColor,
                TintColor = UIColor.Black
            };
            albumButton.SetTitle("Show Album", UIControlState.Normal);
            albumButton.TouchUpInside += (sender, args) =>
            {
                NavigationController.PresentModalViewController(albumViewController, true);
            };

            Add(imageView);
            Add(urlLabel);
			Add(pickerButton);
            Add(albumButton);

            View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();

            EdgesForExtendedLayout = UIRectEdge.None;

            View.AddConstraints(
                imageView.Width().EqualTo().HeightOf(imageView),
                imageView.AtTopOf(View, 5),
                imageView.AtLeftOf(View, 5),
                imageView.AtRightOf(View, 5),

                urlLabel.Below(imageView, 10),
                urlLabel.AtLeftOf(View, 5),
                urlLabel.AtRightOf(View, 5),

                pickerButton.Below(urlLabel, 50),
                pickerButton.AtLeftOf(View, 50),
                pickerButton.AtRightOf(View, 50),
                pickerButton.Height().EqualTo(70),

                albumButton.Below(pickerButton, 20),
                albumButton.AtLeftOf(View, 50),
                albumButton.AtRightOf(View, 50),
                albumButton.Height().EqualTo(70)
                );
        }
    }
}