using System;
using Cirrious.FluentLayouts.Touch;
using UIKit;

namespace Chafu
{
    public class ChafuMenuView : UIView
    {
        public event EventHandler Closed;
        public event EventHandler Done;
        public event EventHandler Extra;

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

        public bool ExtraButtonHidden
        {
            get { return ExtraButton.Hidden; }
            set { ExtraButton.Hidden = value; }
        }

        public UIButton DoneButton { get; }
        public UIButton CloseButton { get; }
        public UIButton ExtraButton { get; }
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
                ContentEdgeInsets = new UIEdgeInsets(8, 8, 8, 8),
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

            ExtraButton = new UIButton
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                ContentMode = UIViewContentMode.ScaleToFill,
                Opaque = false,
                ContentEdgeInsets = new UIEdgeInsets(8, 8, 8, 8),
                AccessibilityLabel = "ExtraButton"
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
            Add(ExtraButton);
            Add(MenuTitle);

            this.AddConstraints(
                CloseButton.AtLeftOf(this, 8),
                CloseButton.AtTopOf(this, 8),
                CloseButton.Width().EqualTo().HeightOf(CloseButton),
                CloseButton.Height().EqualTo(40),

                MenuTitle.WithSameCenterY(this).Plus(2),
                MenuTitle.ToRightOf(CloseButton, 8),
                MenuTitle.Height().EqualTo(21),

                
                DoneButton.AtTopOf(this, 8),
                DoneButton.Width().EqualTo().HeightOf(DoneButton),
                DoneButton.Height().EqualTo(40),
                DoneButton.AtRightOf(this, 8),

                ExtraButton.ToRightOf(MenuTitle, 8),
                ExtraButton.Width().EqualTo().HeightOf(ExtraButton),
                ExtraButton.Height().EqualTo(40),
                ExtraButton.ToLeftOf(DoneButton, 8),
                ExtraButton.AtTopOf(this, 8));

            var checkImage = Configuration.CheckImage ?? UIImage.FromBundle("ic_check");
            var closeImage = Configuration.CloseImage ?? UIImage.FromBundle("ic_close");
            var extraImage = Configuration.ExtraImage ?? UIImage.FromBundle("ic_add");

            if (Configuration.TintIcons)
            {
                checkImage = checkImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                closeImage = closeImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                extraImage = extraImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                DoneButton.TintColor = Configuration.TintColor;
                CloseButton.TintColor = Configuration.TintColor;
                ExtraButton.TintColor = Configuration.TintColor;
            }

            CloseButton.SetImage(closeImage, UIControlState.Normal);
            CloseButton.SetImage(closeImage, UIControlState.Highlighted);
            CloseButton.SetImage(closeImage, UIControlState.Selected);

            DoneButton.SetImage(checkImage, UIControlState.Normal);

            ExtraButton.SetImage(extraImage, UIControlState.Normal);

            MenuTitle.TextColor = Configuration.BaseTintColor;

            CloseButton.TouchUpInside += OnClose;
            DoneButton.TouchUpInside += OnDone;
            ExtraButton.TouchUpInside += OnExtra;

            CloseButtonHidden = false;
            DoneButtonHidden = false;
            ExtraButtonHidden = true;
        }

        private void OnDone(object sender, EventArgs e)
        {
            Done?.Invoke(sender, e);
        }

        private void OnClose(object sender, EventArgs e)
        {
            Closed?.Invoke(sender, e);
        }

        private void OnExtra(object sender, EventArgs e)
        {
            Extra?.Invoke(sender, e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseButton.TouchUpInside -= OnClose;
                DoneButton.TouchUpInside -= OnDone;
                ExtraButton.TouchUpInside -= OnExtra;
            }

            base.Dispose(disposing);
        }
    }
}