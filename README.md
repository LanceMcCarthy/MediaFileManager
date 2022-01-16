# MediaFileManager

Media File Manager is a UWP-packaged WPF application for easily naming media files that need a specific  naming paradigm ([see this 50 second video](https://youtu.be/5U7rmrJXwWw)) .

| Workflow | Status | Installation Options |
|--------|--------|-----|
| Development | [![Development](https://github.com/LanceMcCarthy/MediaFileManager/actions/workflows/ci_dev.yml/badge.svg)](https://github.com/LanceMcCarthy/MediaFileManager/actions/workflows/ci_dev.yml) | na |
| Main | ![Main](https://github.com/LanceMcCarthy/MediaFileManager/workflows/Main/badge.svg) | n/a |
| Prerelease  | ![Release (sideload)](https://github.com/LanceMcCarthy/MediaFileManager/workflows/Release%20(sideload)/badge.svg) | [AppInstaller (sideload)](https://dvlup.blob.core.windows.net/general-app-files/Installers/MediaFileManager/index.html) |
| Microsoft Store | ![Release (Microsoft Store)](https://github.com/LanceMcCarthy/MediaFileManager/workflows/Release%20(Microsoft%20Store)/badge.svg) | [Microsoft Store](https://www.microsoft.com/en-us/p/media-file-manager/9pd3jfk7w5mb) |

> This repository also serves as a real-world example using GitHub Actions to build and distribute a WPF application to the Microsoft Store and MSIX AppInstaller via Azure Storage.

## Video File Operations

### Stage 1 - Choose a root folder for the TV Show / Series

![Stage 1](https://user-images.githubusercontent.com/3520532/58042684-56a3ac80-7b09-11e9-84d2-960619c96316.png)

### Stage 2 - Select the Season, then the Episodes to rename

1. Select a Season(s) to load the episodes.
2. Select the Episodes to be renamed (keyboard shortcuts supported).
3. Select a portion of the filename that will be replaced.
4. Enter the replacement text
5. Click the `Rename` button to start the operation

Here is a GIF of step 4's smooth auto-populate operation:

![SelectionText](https://dvlup.blob.core.windows.net/general-app-files/GIFs/RenamingSelection.gif)

Here's a screenshot of all the steps:

![Stage 2](https://user-images.githubusercontent.com/3520532/58042664-455aa000-7b09-11e9-98cd-11d3a62a2f65.png)

### Result

All the selected episodes have been renamed as long as there was text that matched the selected matching text in that file name.

![Result](https://user-images.githubusercontent.com/3520532/58042755-7f2ba680-7b09-11e9-858a-9d511c5bd6a5.png)

## AudioBook File Operations

### Stage 1 - Opening the author folder

Similar to video files, you can choose a folder to open. In the case of an audio book, that is usually the Author.

![open author folder](https://user-images.githubusercontent.com/3520532/90906130-2cd9d500-e39f-11ea-9182-580479d9eb7d.png)

### Stage 2 - Setting Tags

Next, you select the sub-folder that contains the book's files and do the tag setting.

1. Select the folder containing the book's files
2. Select all the files for that book
3. Type in a the `Album Name` (book's title) and `Artist Name` (book's author).
4. Select the checkboxes to set what mp3 tag data you want to set
5. Click the `Update Tags` button to save the information to the selected files.

![audio book file tag operations](https://user-images.githubusercontent.com/3520532/90906831-3a438f00-e3a0-11ea-8103-b59272d9b7d6.png)
