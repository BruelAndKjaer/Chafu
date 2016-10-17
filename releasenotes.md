### New in 1.3.2

* Added option to record audio with video
* Rotate images and video correctly if device is rotated before recording video or shooting image

### New in 1.3.3

* Added possibility of deleting images and video

### New in 1.3.4

* Show less of Image/Video preview when it is pushed up
* Dim Image/Video preview when pushed up to prevent stealing visual focus
* Move the pan gesture out to entire view, instead of only CollectionView, which enables pan down to reveal preview
* Fix blank album view after accepting Photo Library permission
* Fix wrong aspect ration on album view cells
* Fix missing iOS 10 specific strings in Info.plist in sample
* Remove old asset specifications in csproj, fixes build on newer Xamarin versions

### New in 1.3.5

* Fix UnauthorizedAccessException in sample
* General cleanup

### New in 1.3.6

* Added callback with which MediaItem was deleted

### New in 1.4.0

* XML doc
* Fix crash when deleting videos
* Optional Action Sheet when deleting in Local Files

### New in 1.4.1

* Fill cells in AlbumView for videos instead of fitting

### New in 1.4.2

* Fix null reference exception in rare cases where UIImage is null when scaling