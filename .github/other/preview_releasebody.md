This prerelease contains the appinstaller file that you can use instead of the Microsoft Store.

## Installation
Chose one of the following options to install this release.

### A) Easy Option - AppInstaller

This option provides automatic update support and is the fastest and easiest way.
 
1. Open [Media File Manager](https://dvlup.blob.core.windows.net/general-app-files/Installers/MediaFileManager/index.html) web page.
2. Expand the **Additional Links** section.
  - ![expand additional links](https://user-images.githubusercontent.com/3520532/172673610-ada746e2-8f2f-4418-b79e-7a4723f8aee5.png)
3. Right-click on the **App Installer File** link and select **Save-As** option.
  - ![save as](https://user-images.githubusercontent.com/3520532/172675376-c80a44b3-bc93-405c-b377-caffc41a9bf7.png)
4. Run the saved `PackageProject.appinstaller` file

### B) Advanced Option - Powershell

1. Download and extract the `SideloadPackages.zip` file from the release attachments.
2. Navigate to the 'PackageProject.xxxx.Test/` sub-folder.
3. Right-click on the `Install.ps1` file and select **Run with Powershell**

> You may get an extra step in which you are asked to trust the **Lancelot Certificate, LLC** certificate. Just click or enter Yes to continue. This is unlikely to happen as I am a trusted publisher and Microsoft has my code signing certificate.

## Support

If you have any problems, please [open an Issue](https://github.com/LanceMcCarthy/MediaFileManager/issues).