# Instructions

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
