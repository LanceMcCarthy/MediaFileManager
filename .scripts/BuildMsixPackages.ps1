Write-Output "Setting msbuild Path"
$env:Path += ";C:\Program Files\Microsoft Visual Studio\2022\Professional\Msbuild\Current\Bin"

Write-Output "Determining paths to nuget.config and csproj..."
$currentDirectory = Get-Location
$repoRootFolder = Split-Path -Path $currentDirectory -Parent
$csprojFilePath = Join-Path -Path $repoRootFolder -ChildPath "src\MediaFileManager\MediaFileManager.Desktop\MediaFileManager.Desktop.csproj"
$nugetConfigFilePath = Join-Path -Path $repoRootFolder -ChildPath "src\MediaFileManager\MediaFileManager.Desktop\MediaFileManager.Desktop.csproj"

# pull packages
Write-Output "Invoking dotnet restore..."
dotnet restore $csprojFilePath --configfile $nugetConfigFilePath

# x64 restore and build
Write-Output "Restoring 64 bit dependencies..."
msbuild.exe $csprojFilePath /t:Restore /p:Configuration="Release" /p:RuntimeIdentifier="win-x64" /p:Platform="x64"

Write-Output "Building 64 bit MSIX package..."
msbuild.exe $csprojFilePath /p:Configuration="Release" /p:RuntimeIdentifier="win-x64" /p:Platform="x64" /p:UapMsixPackageBuildMode="SideloadOnly" /p:MsixPackageSigningEnabled="True" /p:PackageCertificateThumbprint=$env:LANCELOT_PFX_THUMBPRINT /p:PackageCertificatePassword=$env:LANCELOT_PFX_PASSWORD /p:GenerateAppxPackageOnBuild=true

# x86 restore and build
Write-Output "Restoring 64 bit dependencies..."
msbuild.exe $csprojFilePath /t:Restore /p:Configuration="Release" /p:RuntimeIdentifier="win-x86" /p:Platform="x86"

Write-Output "Building 64 bit MSIX package..."
msbuild.exe $csprojFilePath /p:Configuration="Release" /p:RuntimeIdentifier="win-x86" /p:Platform="x86" /p:UapMsixPackageBuildMode="SideloadOnly" /p:MsixPackageSigningEnabled="True" /p:PackageCertificateThumbprint=$env:LANCELOT_PFX_THUMBPRINT /p:PackageCertificatePassword=$env:LANCELOT_PFX_PASSWORD /p:GenerateAppxPackageOnBuild=true

Write-Output "Done! Check MediaFileManager.Desktop\bin\[ARCH]\Release\[TFM]\[RID]\AppPackages for msix files."
