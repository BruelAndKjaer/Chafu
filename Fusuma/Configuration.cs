using UIKit;

namespace Fusuma
{
    public static class Configuration
    {
        public static UIColor BaseTintColor { get; set; } = UIColor.FromRGBA(255, 255, 255, 255);
        public static UIColor TintColor { get; set; } = UIColor.FromRGBA(0, 0x96, 0x88, 255);
        public static UIColor BackgroundColor { get; set; } = UIColor.FromRGBA(0x21, 0x21, 0x21, 255);
        public static bool CropImage { get; set; } = true;
        public static bool TintIcons { get; set; } = true;
        public static string CameraRollTitle { get; set; } = "CAMERA ROLL";
        public static string CameraTitle { get; set; } = "PHOTO";
        public static string VideoTitle { get; set; } = "VIDEO";
        public static ModeOrder ModeOrder { get; set; } = ModeOrder.LibraryFirst;
        public static UIImage FlashOnImage { get; set; }
        public static UIImage FlashOffImage { get; set; }
        public static UIImage FlipImage { get; set; }
        public static UIImage ShutterImage { get; set; }
        public static bool ShowBackCameraFirst { get; set; } = true;
        public static UIImage VideoStartImage { get; set; }
        public static UIImage VideoStopImage { get; set; }
        public static UIImage AlbumImage { get; set; }
        public static UIImage CameraImage { get; set; }
        public static UIImage VideoImage { get; set; }
        public static UIImage CheckImage { get; set; }
        public static UIImage CloseImage { get; set; }
        public static bool PreferStatusbarHidden { get; set; } = true;
    }
}
