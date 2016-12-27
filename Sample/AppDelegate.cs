using System.IO;
using System.Linq;
using Foundation;
using UIKit;

namespace Sample
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
#if DEBUG
            Xamarin.Calabash.Start();
#endif

            // create a new window instance based on the screen size
            Window = new UIWindow(UIScreen.MainScreen.Bounds);

            // If you have defined a root view controller, set it here:
            Window.RootViewController = new UINavigationController(new HomeViewController());

            // make the window visible
            Window.MakeKeyAndVisible();

            return true;
        }

        [Export("ensureImages")]
        public void EnsureImages()
        {
            var rice = UIImage.FromBundle("DefaultImages/rice.jpg");
            var corn = UIImage.FromBundle("DefaultImages/corn.jpg");
            foreach (var image in new UIImage[] { rice, corn })
            {
                image.SaveToPhotosAlbum((img, error) =>
                {

                });
            }
        }

        [Export("ensureLocalImages")]
        public void EnsureLocalImages()
        {
            var tempPath = TempPath();

            var files = Directory.GetFiles(tempPath);

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName.Contains("__test_1") ||
                    fileName.Contains("__test_2"))
                    return;
            }

            var rice = UIImage.FromBundle("DefaultImages/rice.jpg");
            var corn = UIImage.FromBundle("DefaultImages/corn.jpg");
            var bundleImages = new UIImage[] { rice, corn };

            var index = 0;
            foreach (var image in bundleImages)
            {
                using (var data = image.AsPNG())
                using (var imageStream = data.AsStream())
                using (var fileStream = new FileStream(
                    Path.Combine(tempPath, $"__test_{index++}.png"), FileMode.Create))
                {
                    CopyStream(imageStream, fileStream);
                }
            }
        }

        public static void CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        public static string TempPath()
        {
            var ret = Path.Combine(Path.GetTempPath(), "Chafu");

            if (!Directory.Exists(ret))
                Directory.CreateDirectory(ret);

            return ret;
        }
    }
}
