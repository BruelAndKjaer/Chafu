using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.iOS;

namespace Chafu.UITest
{
    [TestFixture]
    public class Tests
    {
        iOSApp app;

        [SetUp]
        public void BeforeEachTest()
        {
            // TODO: If the iOS app being tested is included in the solution then open
            // the Unit Tests window, right click Test Apps, select Add App Project
            // and select the app projects that should be tested.
            //
            // The iOS project should have the Xamarin.TestCloud.Agent NuGet package
            // installed. To start the Test Cloud Agent the following code should be
            // added to the FinishedLaunching method of the AppDelegate:
            //
            //    #if ENABLE_TEST_CLOUD
            //    Xamarin.Calabash.Start();
            //    #endif
            app = ConfigureApp
                .iOS
                // TODO: Update this path to point to your iOS app and uncomment the
                // code if the app is not included in the solution.
                //.AppBundle ("../../../iOS/bin/iPhoneSimulator/Debug/Chafu.UITest.iOS.app")
                .StartApp();
        }

        [Test]
        public void AppLaunches()
        {
            app.Screenshot("App Launched");

            var selectedImage = app.WaitForElement(t => t.Marked("SelectedImage"));
            var urlLabel = app.WaitForElement(t => t.Marked("UrlLabel"));
            var pickImage = app.WaitForElement(t => t.Marked("PickImage"));
            var showAlbum = app.WaitForElement(t => t.Marked("ShowAlbum"));

            Assert.True(selectedImage.Count() == 1);
            Assert.True(urlLabel.Count() == 1);
            Assert.True(pickImage.Count() == 1);
            Assert.True(showAlbum.Count() == 1);
        }

        [Test]
        public void PickImageTest()
        {
            var pickImage = app.WaitForElement(t => t.Marked("PickImage"));
            Assert.True(pickImage.Count() == 1);

            app.Tap(a => a.Marked("PickImage"));
            app.Screenshot("Album View");

            var collectionView =
                app.WaitForElement(a => a.Class("UICollectionView").Marked("CollectionView"));

            Assert.True(collectionView.Count() == 1);

            var cells = app.WaitForElement(a => a.Class("Chafu_AlbumViewCell"));

            Assert.True(cells.Count() >= 0);

            if (cells.Count() > 0)
            {
                // probably sim or device with images in gallery

                app.Tap(a => a.Class("Chafu_AlbumViewCell"));
                app.Screenshot("Image Selected");

                app.Tap(a => a.Marked("DoneButton"));
                app.Screenshot("Back to start, selected image");

                app.WaitForElement(a => a.Marked("UrlLabel"));

                var text = app.Query(a => a.Marked("UrlLabel"));

                Assert.IsNotEmpty(text);
            }
        }

        [Test]
        public void TakeNewImageTest()
        {
            var pickImage = app.WaitForElement(t => t.Marked("PickImage"));
            Assert.True(pickImage.Count() == 1);

            app.Tap(a => a.Marked("PickImage"));
            app.Screenshot("Album View");

            app.Tap(a => a.Marked("PhotoButton"));
            app.Screenshot("Camera View");

            try
            {
                // simulator has no camera
                var noCamera = app.WaitForElement(a => a.Marked("NoCamera"));

                if (noCamera != null && noCamera.Count() == 1)
                {
                    return;
                }
            }
            catch { }

            app.Tap(a => a.Marked("ShutterButton"));
            app.Screenshot("New Picture");

            app.WaitForElement(a => a.Marked("UrlLabel"));
            var text = app.Query(a => a.Marked("UrlLabel"));

            Assert.IsNotEmpty(text);
        }

        [Test]
        public void TakeNewVideoTest()
        {
            var pickImage = app.WaitForElement(t => t.Marked("PickImage"));
            Assert.True(pickImage.Count() == 1);

            app.Tap(a => a.Marked("PickImage"));
            app.Screenshot("Album View");

            app.Tap(a => a.Marked("VideoButton"));
            app.Screenshot("Video View");

            try
            {
                // simulator has no camera
                var noCamera = app.WaitForElement(a => a.Marked("NoCamera"));

                if (noCamera != null && noCamera.Count() == 1)
                {
                    return;
                }
            }
            catch { }

            app.Tap(a => a.Marked("ShutterButton"));
            app.Screenshot("Record Video");

            Thread.Sleep(1000);

            app.Tap(a => a.Marked("ShutterButton"));
            app.Screenshot("New Video");

            app.WaitForElement(a => a.Marked("UrlLabel"));
            var text = app.Query(a => a.Marked("UrlLabel"));

            Assert.IsNotEmpty(text);
        }


    }
}
