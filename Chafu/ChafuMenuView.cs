using System;
using Cirrious.FluentLayouts.Touch;
using UIKit;

namespace Chafu
{
    public class ChafuMenuView : UIView
    {
        public event EventHandler Closed;
        public event EventHandler Done;

        public string Title
        {
            get { return MenuTitle.Text; }
            set { MenuTitle.Text = value; }
        }

        public bool DoneButtonHidden
        {
            get { return DoneButton.Hidden; }
            set { DoneButton.Hidden = value; }
        }

        public bool CloseButtonHidden
        {
            get { return CloseButton.Hidden; }
            set { CloseButton.Hidden = value; }
        }

        public UIButton DoneButton { get; }
        public UIButton CloseButton { get; }
        public UILabel MenuTitle { get; }

        public ChafuMenuView()
        {
            CloseButton = new UIButton
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                ContentMode = UIViewContentMode.ScaleToFill,
                Opaque = false,
                ContentEdgeInsets = new UIEdgeInsets(6, 6, 6, 6),
                AccessibilityLabel = "CloseButton"
            };

            DoneButton = new UIButton
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                ContentMode = UIViewContentMode.ScaleToFill,
                Opaque = false,
                ContentEdgeInsets = new UIEdgeInsets(8, 8, 8, 8),
                AccessibilityLabel = "DoneButton"
            };

            MenuTitle = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentMode = UIViewContentMode.Left,
                TextAlignment = UITextAlignment.Center,
                UserInteractionEnabled = false,
                Opaque = false,
                BaselineAdjustment = UIBaselineAdjustment.AlignBaselines,
                AdjustsFontSizeToFitWidth = false,
                LineBreakMode = UILineBreakMode.TailTruncation,
                Text = Configuration.CameraRollTitle,
                TextColor = UIColor.White,
                AccessibilityLabel = "MenuTitle"
            };

            Add(CloseButton);
            Add(DoneButton);
            Add(MenuTitle);

            this.AddConstraints(
                CloseButton.AtLeftOf(this, 8),
                CloseButton.AtTopOf(this, 8),
                CloseButton.Width().EqualTo(40),
                CloseButton.Height().EqualTo(40),

                MenuTitle.WithSameCenterY(this).Plus(2),
                MenuTitle.ToRightOf(CloseButton, 8),
                MenuTitle.Height().EqualTo(21),

                DoneButton.ToRightOf(MenuTitle, 8),
                DoneButton.AtTopOf(this, 8),
                DoneButton.Width().EqualTo(40),
                DoneButton.Height().EqualTo(40),
                DoneButton.AtRightOf(this, 8));

            var checkImage = Configuration.CheckImage ?? UIImage.FromBundle("ic_check");
            var closeImage = Configuration.CloseImage ?? UIImage.FromBundle("ic_close");

            if (Configuration.TintIcons)
            {
                checkImage = checkImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                closeImage = closeImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                DoneButton.TintColor = Configuration.TintColor;
                CloseButton.TintColor = Configuration.TintColor;
            }

            CloseButton.SetImage(closeImage, UIControlState.Normal);
            CloseButton.SetImage(closeImage, UIControlState.Highlighted);
            CloseButton.SetImage(closeImage, UIControlState.Selected);

            DoneButton.SetImage(checkImage, UIControlState.Normal);

            MenuTitle.TextColor = Configuration.BaseTintColor;

            CloseButton.TouchUpInside += OnClose;
            DoneButton.TouchUpInside += OnDone;
        }

        private void OnDone(object sender, EventArgs e)
        {
            Done?.Invoke(sender, e);
        }

        private void OnClose(object sender, EventArgs e)
        {
            Closed?.Invoke(sender, e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseButton.TouchUpInside -= OnClose;
                DoneButton.TouchUpInside -= OnDone;
            }

            base.Dispose(disposing);
        }
    }
}