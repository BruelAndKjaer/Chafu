# Chafu

Chafu is a photo browser and camera library for Xamarin.iOS. It is heavily inspired from [Fusuma][1], which is a Swift library written by [ytakzk][2].
It has been tweaked for ease of use in a C# environment, and all views have been converted from xibs to typed out programatically.
A lot of unecessary wrapping of views has been removed to simplify the code in the library + loads of bugfixes and missing features have been implemented.

## Preview
<img src="https://raw.githubusercontent.com/Cheesebaron/Chafu/master/images/fusuma.gif" width="340px">

## Features

- [x] UIImagePickerController alternative
- [x] Camera roll
- [x] Camera for capturing both photos and video
- [x] Cropping of photos and video into squares
- [x] Toggling of flash when capturing photos and video
- [x] Supports front and back cameras
- [x] Customizable

## Installation

Install from NuGet

> Install-Package Chafu

## Usage

Add a `using chafu;` in the top of your class.

```
var chafu = new ChafuViewController();
chafu.HasVideo = true; // add video tab
PresentViewController(chafu, true);
```

## Events

```
// When image is selected or captured with camera
chafu.ImageSelected += (sender, image) => imageView.Image = image;

// When video is captured with camera
chafu.VideoSelected += (sender, videoUrl) => urlLabel.Text = videoUrl.AbsoluteString;

// When ViewController is dismissed
chafu.Closed += (sender, e) => { /* do stuff on closed */ };

// When permissions to access camera roll are denied by the user
chafu.CameraRollUnauthorized += (s, e) => { /* do stuff when Camera Roll is unauthorized */ };

// when permissions to access camera are denied by the user
chafu.CameraUnauthorized += (s, e) => { /* do stuff when Camera is unauthorized */ };
```

## Customization

All customization happens through the static `Configuration` class.

```
Configuration.BaseTintColor = UIColor.White;
Configuration.TintColor = UIColor.Red;
Configuration.BackgroundColor = UIColor.Cyan;
Configuration.CropImage = false;
Configuration.TintIcons = true;
// etc...
```

Explore the class for more configuration.

## Thanks to
Many thanks to [ytakzk][2] for his initial [Fusuma][1] implementation, which this component initially started as.

## What does chafu mean?
Fusuma means bran in Japanese, Chafu in Japanese means chaff. Chaff is sometimes confused with bran.

## License
Chafu is licensed under the MIT License, see the LICENSE file for more information.

[1]: https://github.com/ytakzk/Fusuma
[2]: https://github.com/ytakzk