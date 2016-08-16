using Cirrious.FluentLayouts.Touch;
using Chafu;
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

            var imageView = new UIImageView();
            imageView.BackgroundColor = UIColor.Black;

            var fusumaViewController = new ChafuViewController {HasVideo = true};
            fusumaViewController.ImageSelected += (sender, image) => imageView.Image = image;

            var pickerButton = new UIButton(UIButtonType.System) {
                BackgroundColor = Configuration.TintColor,
                TintColor = UIColor.Black
            };
            pickerButton.SetTitle("Pick Image", UIControlState.Normal);
            pickerButton.TouchUpInside += (sender, args) =>
            {
				NavigationController.PresentModalViewController (fusumaViewController, true);
            };

            Add(imageView);
			Add(pickerButton);

            View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();

            EdgesForExtendedLayout = UIRectEdge.None;

            View.AddConstraints(
                imageView.Width().EqualTo().HeightOf(imageView),
                imageView.AtTopOf(View, 5),
                imageView.AtLeftOf(View, 5),
                imageView.AtRightOf(View, 5),

                pickerButton.Below(imageView, 50),
                pickerButton.AtLeftOf(View, 50),
                pickerButton.AtRightOf(View, 50),
                pickerButton.Height().EqualTo(70)
                );
        }
    }
}