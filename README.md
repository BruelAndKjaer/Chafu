# Chafu

[![Build status](https://ci.appveyor.com/api/projects/status/k4nhuf35dnwr42av?svg=true)](https://ci.appveyor.com/project/Cheesebaron/chafu)
[![NuGet](https://img.shields.io/nuget/v/Chafu.svg?maxAge=2592000)](https://www.nuget.org/packages/Chafu/)

Chafu is a photo browser and camera library for Xamarin.iOS. It is heavily inspired from [Fusuma][1], which is a Swift library written by [ytakzk][2].

It has been tweaked for ease of use in a C# environment, all xibs converted to C# code and unnecessary wrapper views have been removed. The library
has been simplified and loads of unfixed Fusuma bugs and features have been fixed in this library.

## Preview
<img src="https://raw.githubusercontent.com/Cheesebaron/Chafu/master/images/sample.gif" width="340px">

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
Many thanks to [ytakzk][2] for his initial [Fusuma][1] implementation, which this library started as.

## What does Chafu mean?
Fusuma means bran in Japanese, Chafu in Japanese means chaff. Chaff is sometimes confused with bran.

## License
Chafu is licensed under the MIT License, see the LICENSE file for more information.

[1]: https://github.com/ytakzk/Fusuma
[2]: https://github.com/ytakzk
