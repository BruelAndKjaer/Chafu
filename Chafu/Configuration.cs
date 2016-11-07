using UIKit;

namespace Chafu
{
    /// <summary>
    /// Configuration class for Chafu. Use this to change colors, images and options.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Gets or sets the color of the base tint. Used for bottom icons in picker, when not highlighted.
        /// </summary>
        /// <value>The color of the base tint.</value>
        public static UIColor BaseTintColor { get; set; } = UIColor.FromRGBA(255, 255, 255, 255);

        /// <summary>
        /// Gets or sets the tint color. Used for tinting icons.
        /// </summary>
        /// <value>The color of the tint.</value>
        public static UIColor TintColor { get; set; } = UIColor.FromRGBA(0, 0x96, 0x88, 255);

		/// <summary>
		/// Gets or sets the tint color of the delete button.
		/// </summary>
		/// <value>The color of the tint.</value>
		public static UIColor DeleteTintColor { get; set; } = UIColor.FromRGBA(230, 46, 52, 255);

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        /// <value>The color of the background.</value>
        public static UIColor BackgroundColor { get; set; } = UIColor.FromRGBA(0x21, 0x21, 0x21, 255);

        /// <summary>
        /// Gets or sets whether to Crop images into squares. Height = Width.
        /// </summary>
        /// <value><c>true</c> if crop image; otherwise, <c>false</c>.</value>
        public static bool CropImage { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to tint icons.
        /// </summary>
        /// <value><c>true</c> if tint icons; otherwise, <c>false</c>.</value>
        public static bool TintIcons { get; set; } = true;

        /// <summary>
        /// Gets or sets the camera roll title. The title in the Menu on top of picker.
        /// </summary>
        /// <value>The camera roll title. Default is "CAMERA ROLL".</value>
        public static string CameraRollTitle { get; set; } = "CAMERA ROLL";

        /// <summary>
        /// Gets or sets the camera title. The title in the Menu on top of picker.
        /// </summary>
        /// <value>The camera title. Default is "PHOTO".</value>
        public static string CameraTitle { get; set; } = "PHOTO";

        /// <summary>
        /// Gets or sets the video title. The title in the Menu on top of picker.
        /// </summary>
        /// <value>The video title. Default is "VIDEO".</value>
        public static string VideoTitle { get; set; } = "VIDEO";

        /// <summary>
        /// Gets or sets the mode order. Which item to show first in picker.
        /// </summary>
        /// <value>The mode order. Default is LibraryFirst</value>
        public static ModeOrder ModeOrder { get; set; } = ModeOrder.LibraryFirst;

        /// <summary>
        /// Gets or sets the flash on image.
        /// </summary>
        /// <value>The flash on image.</value>
        public static UIImage FlashOnImage { get; set; }

        /// <summary>
        /// Gets or sets the flash off image.
        /// </summary>
        /// <value>The flash off image.</value>
        public static UIImage FlashOffImage { get; set; }

        /// <summary>
        /// Gets or sets the flip image. Flip camera icon.
        /// </summary>
        /// <value>The flip image.</value>
        public static UIImage FlipImage { get; set; }

        /// <summary>
        /// Gets or sets the shutter image.
        /// </summary>
        /// <value>The shutter image.</value>
        public static UIImage ShutterImage { get; set; }

        /// <summary>
        /// Gets or sets whether to show back camera first.
        /// </summary>
        /// <value><c>true</c> back camera shown first; <c>false</c>, front camera shown first.</value>
        public static bool ShowBackCameraFirst { get; set; } = true;

        /// <summary>
        /// Gets or sets the video start image.
        /// </summary>
        /// <value>The video start image.</value>
        public static UIImage VideoStartImage { get; set; }

        /// <summary>
        /// Gets or sets the video stop image.
        /// </summary>
        /// <value>The video stop image.</value>
        public static UIImage VideoStopImage { get; set; }

        /// <summary>
        /// Gets or sets the album image.
        /// </summary>
        /// <value>The album image.</value>
        public static UIImage AlbumImage { get; set; }

        /// <summary>
        /// Gets or sets the camera image.
        /// </summary>
        /// <value>The camera image.</value>
        public static UIImage CameraImage { get; set; }

        /// <summary>
        /// Gets or sets the video image.
        /// </summary>
        /// <value>The video image.</value>
        public static UIImage VideoImage { get; set; }

        /// <summary>
        /// Gets or sets the check image.
        /// </summary>
        /// <value>The check image.</value>
        public static UIImage CheckImage { get; set; }

        /// <summary>
        /// Gets or sets the close image.
        /// </summary>
        /// <value>The close image.</value>
        public static UIImage CloseImage { get; set; }

        /// <summary>
        /// Gets or sets the extra image.
        /// </summary>
        /// <value>The extra image.</value>
        public static UIImage ExtraImage { get; set; }

		/// <summary>
		/// Gets or sets the delete button image.
		/// </summary>
		/// <value>The delete button image.</value>
		public static UIImage DeleteImage { get; set; }

        /// <summary>
        /// Gets or sets whether Statusbar should be hidden.
        /// </summary>
        /// <value><c>true</c> if prefer statusbar hidden; otherwise, <c>false</c>.</value>
        public static bool PreferStatusbarHidden { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show Camera viewfinder in Square mode.
        /// </summary>
        /// <value><c>true</c> if show square; otherwise, <c>false</c>.</value>
        public static bool ShowSquare { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to save newly taken photos with the camera to Photos Album.
        /// </summary>
        /// <value><c>true</c> if save to Photos Album; otherwise, <c>false</c>.</value>
        public static bool SaveToPhotosAlbum { get; set; } = false;

        /// <summary>
        /// Gets or sets the text of the camera preview, when no camera is present.
        /// </summary>
        /// <value>Value of the text to show, default is <c>NO CAMERA</c>.</value>
        public static string NoCameraText { get; set; } = "NO CAMERA";

        /// <summary>
        /// Gets or sets whether to record audio when recording video.
        /// </summary>
        /// <value><c>true</c> if record audio; otherwise, <c>false</c>.</value>
        public static bool RecordAudio { get; set; } = true;

        /// <summary>
        /// Gets or sets how many cells are allowed to be displayed on a row.
        /// 
        /// Default is 3.
        /// </summary>
        public static int NumberOfCells { get; set; } = 3;

        /// <summary>
        /// Get or set whether to show a Action Sheet when deleting a photo or video.
        /// 
        /// Default is <c>true</c>
        /// </summary>
        /// <value><c>true</c> if show action sheet; otherwise, <c>false</c>.</value>
        public static bool ShowActionSheetOnDelete { get; set; } = true;

        /// <summary>
        /// Get or set the title of the Action Sheet when deleting a photo.
        /// </summary>
        public static string DeletePhotoTitle { get; set; }

        /// <summary>
        /// Get or set the message of the Action Sheet when deleting a photo.
        /// </summary>
        public static string DeletePhotoMessage { get; set; }

        /// <summary>
        /// Get or set the title of the Action Sheet when deleting a video.
        /// </summary>
        public static string DeleteVideoTitle { get; set; }

        /// <summary>
        /// Get or set the message of the Action Sheet when deleting a video.
        /// </summary>
        public static string DeleteVideoMessage { get; set; }

        /// <summary>
        /// Get or set the title of the delete button when deleting a photo or video.
        /// </summary>
        public static string DeleteTitle { get; set; } = "Delete";

        /// <summary>
        /// Get or set the title of the cancel button when deleting a photo or video.
        /// </summary>
        public static string CancelTitle { get; set; } = "Cancel";

        /// <summary>
        /// Gets or sets whether camera detects faces.
        /// </summary>
        /// <value><c>true</c> if detect faces; otherwise, <c>false</c>.</value>
        public static bool DetectFaces { get; set; } = true;

        /// <summary>
        /// Gets or sets the border color of the detected face.
        /// </summary>
        /// <value>The color of the border. Defaults to <see cref="UIColor.White"/></value>
        public static UIColor DetectedFaceBorderColor { get; set; } = UIColor.White;

        /// <summary>
        /// Gets or sets the width of the border around the detected face.
        /// </summary>
        /// <value>The width of the border. Defaults to <c>2</c></value>
        public static int DetectedFaceBorderWidth { get; set; } = 2;

        /// <summary>
        /// Gets or sets the corner radius on the border around the detected face.
        /// </summary>
        /// <value>The detected corner radius. Defaults to <c>5</c></value>
        public static int DetectedFaceCornerRadius { get; set; } = 5;
    }
}
