using System;
using System.Collections.Generic;
using System.Text;
using CoreGraphics;
using UIKit;

namespace Fusuma
{
    public class CameraView : UIView
    {
        public CameraView(IntPtr handle) 
            : base(handle) { CreateView(); }
        public CameraView() { CreateView(); }

        private void CreateView()
        {
            ContentMode = UIViewContentMode.ScaleToFill;
            Frame = new CGRect(0,0, 400, 600);
            AutoresizingMask = UIViewAutoresizing.All;

            var previewContainer = new UIView
            {
                AccessibilityLabel = "PreviewContainer",
                Frame = new CGRect(0, 50, 400, 400),
                BackgroundColor = UIColor.Black,
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentMode = UIViewContentMode.ScaleToFill
            };
            Add(previewContainer);
            AddConstraint(NSLayoutConstraint.Create(previewContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal,
                previewContainer, NSLayoutAttribute.Width, 1, 0));

            var buttonContainer = new UIView
            {
                Frame = new CGRect(0, 450, 400, 150),
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentMode = UIViewContentMode.ScaleToFill,
                AccessibilityLabel = "ButtonContainer"
            };
            Add(buttonContainer);

            var shutterButton = new UIButton(new CGRect(166, 41, 68, 68))
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                AccessibilityLabel = "ShutterButton",
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                Opaque = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation
            };
            buttonContainer.Add(shutterButton);

            var heightConstraint = NSLayoutConstraint.Create(shutterButton, NSLayoutAttribute.Height,
                NSLayoutRelation.Equal);
            heightConstraint.Constant = 68;
            var widthConstraint = NSLayoutConstraint.Create(shutterButton, NSLayoutAttribute.Width,
                NSLayoutRelation.Equal);
            widthConstraint.Constant = 68;
            shutterButton.AddConstraints(new[] { heightConstraint, widthConstraint});

            var flipButton = new UIButton(new CGRect(15, 55, 40, 40))
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                AccessibilityLabel = "FlipButton",
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                Opaque = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation
            };
            buttonContainer.Add(flipButton);

            heightConstraint = NSLayoutConstraint.Create(shutterButton, NSLayoutAttribute.Height,
                NSLayoutRelation.Equal);
            heightConstraint.Constant = 40;
            widthConstraint = NSLayoutConstraint.Create(shutterButton, NSLayoutAttribute.Width,
                NSLayoutRelation.Equal);
            widthConstraint.Constant = 40;
            flipButton.AddConstraints(new[] { heightConstraint, widthConstraint });

            var flashButton = new UIButton(new CGRect(15, 55, 40, 40))
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                VerticalAlignment = UIControlContentVerticalAlignment.Center,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
                AccessibilityLabel = "FlashButton",
                ContentEdgeInsets = new UIEdgeInsets(2, 2, 2, 2),
                Opaque = false,
                LineBreakMode = UILineBreakMode.MiddleTruncation
            };
            buttonContainer.Add(flashButton);

            heightConstraint = NSLayoutConstraint.Create(shutterButton, NSLayoutAttribute.Height,
                NSLayoutRelation.Equal);
            heightConstraint.Constant = 40;
            widthConstraint = NSLayoutConstraint.Create(shutterButton, NSLayoutAttribute.Width,
                NSLayoutRelation.Equal);
            widthConstraint.Constant = 40;
            flashButton.AddConstraints(new[] { heightConstraint, widthConstraint });
        }


    }
}
