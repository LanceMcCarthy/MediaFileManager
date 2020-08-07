# MediaFileManager

| Branch       | Status |
|--------------|--------|
| master | ![WPF CI](https://github.com/LanceMcCarthy/MediaFileManager/workflows/WPF%20CI/badge.svg)|
| releases | ![WPF CD](https://github.com/LanceMcCarthy/MediaFileManager/workflows/WPF%20CD/badge.svg) |

Media File Manager is powerful media file naming tool, made with WPF and distributed as a UWP package for safe and easy installation. [watch a quick video demo](https://www.screencast.com/t/84UQ7Vkv) (< 1 minute).

Skip down to the [Installation section](./index#installation) for two different options; [Microsoft Store](./index#microsoft-store) or [SideLoad](./index#sideload).

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

# Installation

This release contains both SideLoad AppInstaller and StoreUpload app packages for v.${{ env.UWP_VERSION }} of Media File Manager.

If you have any problems, please [open an Issue](https://github.com/LanceMcCarthy/MediaFileManager/issues) so I can investigate and fix it for everyone to enjoy 😎.

## Microsoft Store

You can install the app right from the Microsoft Store at any time - [See Media File Manager in the Microsoft Store](https://www.microsoft.com/en-us/p/media-file-manager/9pd3jfk7w5mb).

## Sideload

If you want to sideload the release, there are a couple ways to do it.

### Option 1 - Easiest

1. Download and extract the `SideloadPackages.zip` file.
2. Double click on the .appinstaller file.

### Option 2 - Powershell

1. Download and extract the `SideloadPackages.zip` file
2. Navigate to the 'PackageProject_xxx_Test/` subfolder.
3. Right-click on the `Install.ps1` file and select **Run with Powershell**

> It is unlikely because I am a trusted publisher, but you may get an extra step in which you are asked to trust the **Lancelot Certificate, LLC** certificate. Just click or enter Yes to continue.
