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
		public event EventHandler Deleted;

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

		public bool DeleteButtonHidden
		{
			get { return DeleteButton.Hidden; }
			set
			{
				if (DeleteButton.Hidden == value) return;

				DeleteButton.Hidden = value;
				UpdateConstraints();
			}
		}

        public UIButton DoneButton { get; }
        public UIButton CloseButton { get; }
        public UIButton ExtraButton { get; }
		public UIButton DeleteButton { get; }
        public UILabel MenuTitle { get; }

        public MenuView()
        {
            CloseButton = CreateButton("CloseButton");
            DoneButton = CreateButton("DoneButton");            
            ExtraButton = CreateButton("ExtraButton");
			DeleteButton = CreateButton("DeleteButton");

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
			Add(DeleteButton);

            var checkImage = Configuration.CheckImage ?? UIImage.FromBundle("ic_check");
            var closeImage = Configuration.CloseImage ?? UIImage.FromBundle("ic_close");
            var extraImage = Configuration.ExtraImage ?? UIImage.FromBundle("ic_add");
			var deleteImage = Configuration.DeleteImage ?? UIImage.FromBundle("ic_delete");

            if (Configuration.TintIcons)
            {
                checkImage = checkImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                closeImage = closeImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                extraImage = extraImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
				deleteImage = deleteImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                DoneButton.TintColor = Configuration.TintColor;
                CloseButton.TintColor = Configuration.TintColor;
                ExtraButton.TintColor = Configuration.TintColor;
				DeleteButton.TintColor = Configuration.DeleteTintColor;
            }

            CloseButton.SetImage(closeImage, UIControlState.Normal);
            CloseButton.SetImage(closeImage, UIControlState.Highlighted);
            CloseButton.SetImage(closeImage, UIControlState.Selected);

            DoneButton.SetImage(checkImage, UIControlState.Normal);

            ExtraButton.SetImage(extraImage, UIControlState.Normal);

			DeleteButton.SetImage(deleteImage, UIControlState.Normal);

            MenuTitle.TextColor = Configuration.BaseTintColor;

            CloseButton.TouchUpInside += OnClose;
            DoneButton.TouchUpInside += OnDone;
            ExtraButton.TouchUpInside += OnExtra;
			DeleteButton.TouchUpInside += OnDelete;

            CloseButtonHidden = false;
            DoneButtonHidden = false;
            ExtraButtonHidden = true;
			DeleteButtonHidden = true;
        }

		private UIButton CreateButton(string accessibilityLabel)
		{
			return new UIButton
			{
				TranslatesAutoresizingMaskIntoConstraints = false,
				LineBreakMode = UILineBreakMode.MiddleTruncation,
				VerticalAlignment = UIControlContentVerticalAlignment.Center,
				HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
				ContentMode = UIViewContentMode.ScaleToFill,
				Opaque = false,
				ContentEdgeInsets = new UIEdgeInsets(8, 8, 8, 8),
				AccessibilityLabel = accessibilityLabel
			};
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

		private void OnDelete(object sender, EventArgs e)
		{
			Deleted?.Invoke(sender, e);
		}

        public override void UpdateConstraints()
        {
            RemoveConstraints(Constraints);

            this.AddConstraints(
                CloseButton.AtLeftOf(this, 8),
                CloseButton.AtTopOf(this, 8),
                CloseButton.Width().EqualTo(40),
                CloseButton.Height().EqualTo(40),

				DeleteButton.ToRightOf(CloseButton, 8),
				DeleteButton.AtTopOf(this, 8),
				DeleteButton.Height().EqualTo(40),
				DeleteButtonHidden
					? DeleteButton.Width().EqualTo(0)
					: DeleteButton.Width().EqualTo(40),

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
				DeleteButton.TouchUpInside -= OnDelete;
            }

            base.Dispose(disposing);
        }
    }
}