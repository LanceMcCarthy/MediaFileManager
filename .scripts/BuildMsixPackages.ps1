$env:Path += ";C:\Program Files\Microsoft Visual Studio\2022\Professional\Msbuild\Current\Bin"

Set-Location ..\MediaFileManager

// pull packages
dotnet restore "src\MediaFileManager\MediaFileManager.Desktop\MediaFileManager.Desktop.csproj" --configfile "src\nuget.config"

// x64 restore and build
msbuild.exe "src\MediaFileManager\MediaFileManager.Desktop\MediaFileManager.Desktop.csproj" /t:Restore /p:Configuration="Release" /p:RuntimeIdentifier="win-x64" /p:Platform="x64"

msbuild.exe "src\MediaFileManager\MediaFileManager.Desktop\MediaFileManager.Desktop.csproj" /p:Configuration="Release" /p:RuntimeIdentifier="win-x64" /p:Platform="x64" /p:UapMsixPackageBuildMode="SideloadOnly" /p:MsixPackageSigningEnabled="True" /p:PackageCertificateThumbprint=$env:LANCELOT_PFX_THUMBPRINT /p:PackageCertificatePassword=$env:LANCELOT_PFX_PASSWORD /p:GenerateAppxPackageOnBuild=true

// x86 restore and build
msbuild.exe "src\MediaFileManager\MediaFileManager.Desktop\MediaFileManager.Desktop.csproj" /t:Restore /p:Configuration="Release" /p:RuntimeIdentifier="win-x86" /p:Platform="x86"

msbuild.exe "src\MediaFileManager\MediaFileManager.Desktop\MediaFileManager.Desktop.csproj" /p:Configuration="Release" /p:RuntimeIdentifier="win-x86" /p:Platform="x86" /p:UapMsixPackageBuildMode="SideloadOnly" /p:MsixPackageSigningEnabled="True" /p:PackageCertificateThumbprint=$env:LANCELOT_PFX_THUMBPRINT /p:PackageCertificatePassword=$env:LANCELOT_PFX_PASSWORD /p:GenerateAppxPackageOnBuild=true
