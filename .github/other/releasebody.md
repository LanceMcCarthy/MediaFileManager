# Instructions

This release contains both SideLoad and StoreUpload app packages for Media File Manager.

## Microsoft Store

You can install the app right from the Microsoft Store at any time - [See Media File Manager in the Microsoft Store](https://www.microsoft.com/en-us/p/media-file-manager/9pd3jfk7w5mb).

## Sideload

If you want to sideload the release, there are two ways to do it.

#### Option 1 [Easy] - AppInstaller (recommended)

This option provides automatic update support and is the easiest way to install an app.
 
1. Go to [Media File Manager installer website](https://dvlup.blob.core.windows.net/general-app-files/Installers/MediaFileManager/index.html) and click **Get the App** button.

![Screenshot of Get the App button](https://github.com/LanceMcCarthy/MediaFileManager/blob/main/.images/SideLoadPageSS.png)

> This option will automatically update your installation even though you're not using the Microsoft Store!

#### Option 2 [Advanced] - Powershell

1. Download and extract the `SideloadPackages.zip` file from the release attachments.
2. Navigate to the 'PackageProject.xxxx.Test/` subfolder.
3. Right-click on the `Install.ps1` file and select **Run with Powershell**

> You may get an extra step in which you are asked to trust the **Lancelot Certificate, LLC** certificate. Just click or enter Yes to continue. This is unlikely to happen as I am a trusted publisher and Microsoft has my code signing certificate.

## Support

If you have any problems, please [open an Issue](https://github.com/LanceMcCarthy/MediaFileManager/issues) so I can investigate and fix it for everyone to enjoy 😎.