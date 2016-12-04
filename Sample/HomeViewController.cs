using System;
using System.IO;
using Chafu;
using Cirrious.FluentLayouts.Touch;
using CoreFoundation;
using Foundation;
using UIKit;

namespace Sample
{
    public class HomeViewController : UIViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Title = "Chafu";

            var deleteAll = new UIBarButtonItem(UIBarButtonSystemItem.Trash) { TintColor = Configuration.BackgroundColor };


            NavigationController.NavigationBar.BarTintColor = Configuration.TintColor;
            NavigationController.NavigationBar.TintColor = Configuration.BaseTintColor;
            NavigationItem.RightBarButtonItem = deleteAll;

            View.BackgroundColor = Configuration.BackgroundColor;

            Configuration.CropImage = true;

            var imageView = new UIImageView
            {
                BackgroundColor = UIColor.Black,
                AccessibilityLabel = "SelectedImage"
            };

            var urlLabel = new UILabel
            {
                AccessibilityLabel = "UrlLabel",
                TextColor = UIColor.White,
                Font = UIFont.SystemFontOfSize(10),
                Lines = 4
            };

            var pickerButton = new UIButton(UIButtonType.System)
            {
                BackgroundColor = Configuration.TintColor,
                TintColor = UIColor.Black,
                AccessibilityLabel = "PickImage"
            };
            pickerButton.SetTitle("Pick Image", UIControlState.Normal);

            var albumButton = new UIButton(UIButtonType.System)
            {
                BackgroundColor = Configuration.TintColor,
                TintColor = UIColor.Black,
                AccessibilityLabel = "ShowAlbum"
            };
            albumButton.SetTitle("Show Album", UIControlState.Normal);

            var chafu = new ChafuViewController { HasVideo = true };
            chafu.ImageSelected += (sender, image) =>
            {
                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    imageView.Image = image;
                });

                urlLabel.Text = CopyImageToLocalFolder(image);
            };
            chafu.VideoSelected += (sender, videoUrl) =>
            {
                urlLabel.Text = videoUrl.AbsoluteString;
                CopyVideoToLocalFolder(videoUrl);
            };
            chafu.Closed += (sender, e) =>
            {
                /* do stuff on closed */

            };

            pickerButton.TouchUpInside += (sender, args) =>
            {
                NavigationController.PresentModalViewController(chafu, true);
            };

            var albumViewController = new AlbumViewController
            {
                LazyDataSource = (view, size, mediaTypes) =>
                    new LocalFilesDataSource(view, size, mediaTypes) { ImagesPath = TempPath() },
                LazyDelegate = (view, source) => new LocalFilesDelegate(view, (LocalFilesDataSource)source),
                ShowExtraButton = true,
                ShowDoneButton = false,
                ShowDeleteButton = true
            };

            albumViewController.Extra += (sender, args) =>
            {
                albumViewController.Dismiss();
                NavigationController.PresentModalViewController(chafu, true);
            };

            albumViewController.ImageSelected += (sender, image) =>
            {
                imageView.Image = image;
            };

            albumButton.TouchUpInside += (sender, args) =>
            {
                // Test InitialSelectedImage by selecting random path
                albumViewController.InitialSelectedImagePath = GetRandomPath();

                NavigationController.PresentModalViewController(albumViewController, true);
            };

            deleteAll.Clicked += (sender, args) =>
            {
                DeleteAllStuff();
                ((LocalFilesDataSource)albumViewController.AlbumDataSource)?.UpdateImageSource(TempPath());
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
                imageView.Above(urlLabel, 10),

                urlLabel.AtLeftOf(View, 5),
                urlLabel.AtRightOf(View, 5),
                urlLabel.Above(pickerButton, 10),

                pickerButton.AtLeftOf(View, 50),
                pickerButton.AtRightOf(View, 50),
                pickerButton.Height().EqualTo(50),
                pickerButton.Above(albumButton, 20),

                albumButton.AtLeftOf(View, 50),
                albumButton.AtRightOf(View, 50),
                albumButton.Height().EqualTo(50),
                albumButton.AtBottomOf(View, 10f)
                );
        }

        private string GetRandomPath()
        {
            var dirPath = TempPath();
            if (!Directory.Exists(dirPath))
                return null;

            var files = Directory.GetFiles(dirPath);
            var random = new Random().Next(files.Length - 1);
            return files[random];
        }

        private string CopyImageToLocalFolder(UIImage image)
        {
            var dirPath = TempPath();
            var fileName = $"{Guid.NewGuid().ToString("N")}.jpg";
            var tempPath = Path.Combine(dirPath, fileName);

            DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
            {
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                using (var stream = image.AsJPEG().AsStream())
                using (var fileStream = File.Create(tempPath))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }
            });

            return tempPath;
        }

        private void CopyVideoToLocalFolder(NSUrl videoUrl)
        {
            DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
            {
                var dirPath = TempPath();
                var fileName = $"{Guid.NewGuid().ToString("N")}.mov";
                var tempPath = Path.Combine(dirPath, fileName);

                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                File.Copy(videoUrl.RelativePath, tempPath);
            });
        }

        private void DeleteAllStuff()
        {
            DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
            {
                var dirPath = TempPath();
                if (!Directory.Exists(dirPath)) return;

                Directory.Delete(dirPath, true);
                Directory.CreateDirectory(dirPath);
            });
        }

        public string TempPath()
        {
            var ret = Path.Combine(Path.GetTempPath(), "Chafu");
            return ret;
        }
    }
}