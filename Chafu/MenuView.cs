using System;
using Cirrious.FluentLayouts.Touch;
using UIKit;

namespace Chafu
{
    public class MenuView : UIView
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
            set
            {
                if (DoneButton.Hidden == value) return;

                DoneButton.Hidden = value;
                UpdateConstraints();
            }
        }

        public bool CloseButtonHidden
        {
            get { return CloseButton.Hidden; }
            set { CloseButton.Hidden = value; }
        }

        public bool ExtraButtonHidden
        {
            get { return ExtraButton.Hidden; }
            set
            {
                if (ExtraButton.Hidden == value) return;

                ExtraButton.Hidden = value;
                UpdateConstraints();
            }
        }

        public UIButton DoneButton { get; }
        public UIButton CloseButton { get; }
        public UIButton ExtraButton { get; }
        public UILabel MenuTitle { get; }

        public MenuView()
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

        public override void UpdateConstraints()
        {
            RemoveConstraints(Constraints);

            this.AddConstraints(
                CloseButton.AtLeftOf(this, 8),
                CloseButton.AtTopOf(this, 8),
                CloseButton.Width().EqualTo(40),
                CloseButton.Height().EqualTo(40),

                MenuTitle.WithSameCenterY(this).Plus(2),
                MenuTitle.WithSameCenterX(this),
                MenuTitle.Height().EqualTo(21),

                DoneButton.AtTopOf(this, 8),
                DoneButton.Height().EqualTo(40),
                DoneButton.AtRightOf(this, 8),
                DoneButtonHidden
                    ? DoneButton.Width().EqualTo(0)
                    : DoneButton.Width().EqualTo(40),

                ExtraButton.ToRightOf(MenuTitle, 8),
                ExtraButton.Height().EqualTo(40),
                ExtraButton.ToLeftOf(DoneButton, 8),
                ExtraButton.AtTopOf(this, 8),
                ExtraButtonHidden
                    ? ExtraButton.Width().EqualTo(0)
                    : ExtraButton.Width().EqualTo(40)
                );

            base.UpdateConstraints();
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