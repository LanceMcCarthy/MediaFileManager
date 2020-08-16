# MediaFileManager

Media File Manager is a UWP-packaged WPF application for easily naming media files that need a specific  naming paradigm ([see this 50 second video](https://youtu.be/5U7rmrJXwWw)).

This repo also serves as a real-world example using GitHub Actions to package and distribute a WPF application as a UWP-packaged distribution to the Microsoft Store *and* Azure Storage blob! 

| Branch       | Status                           |
|--------------|----------------------------------|
| master | ![WPF CI](https://github.com/LanceMcCarthy/MediaFileManager/workflows/WPF%20CI/badge.svg)|
| releases | ![WPF CD](https://github.com/LanceMcCarthy/MediaFileManager/workflows/WPF%20CD/badge.svg) |

### Installation Options

* [Microsoft Store](https://www.microsoft.com/en-us/p/media-file-manager/9pd3jfk7w5mb)
* [AppInstaller Web Site](https://dvlup.blob.core.windows.net/general-app-files/Installers/MediaFileManager/index.html)

## Getting Started - Video Files

### Stage 1 - Choose a root folder for the TV Show / Series

![Stage 1](https://user-images.githubusercontent.com/3520532/58042684-56a3ac80-7b09-11e9-84d2-960619c96316.png)

### Stage 2 - Select the Season, then the episodes

1. Select a Season(s) to load the episodes
2. Select the epsiodes to be renamed (keyboard shortcuts supported)
3. Select a portion of the filename that will be replaced 
4. Enter the replacement text

![SelectionText](https://dvlup.blob.core.windows.net/general-app-files/GIFs/RenamingSelection.gif)

5. Click the Rename button to start the operation

![Stage 2](https://user-images.githubusercontent.com/3520532/58042664-455aa000-7b09-11e9-98cd-11d3a62a2f65.png)

### Result

All the selected episodes have been renamed as long as there was text that matched the selected matching text in that file name.

![Result](https://user-images.githubusercontent.com/3520532/58042755-7f2ba680-7b09-11e9-858a-9d511c5bd6a5.png)
