param(
    [string]$Architecture = "x86",
    [string]$SdkVersion = "10.0.19041.0",
    [string]$BundleFileName = "MediaFileManager.Desktop_2021.722.1.0_x86_x64.msixbundle",
    [string]$BundleVersion = "2021.722.1.0"
    )

$sdkToolsPath = Join-Path -Path "C:\Program Files (x86)\Windows Kits\10\bin" -ChildPath "\$SdkVersion\$Architecture\"
Write-Output $sdkToolsPath
$env:Path += ";$sdkToolsPath"

$currentDirectory = Get-Location
$repoRootFolder = Split-Path -Path $currentDirectory -Parent
$packagesSearchFolder = Join-Path -Path $repoRootFolder -ChildPath "\src\MediaFileManager\MediaFileManager.Desktop\bin"

$msixFiles = Get-ChildItem -Path $packagesSearchFolder -Recurse -Include MediaFileManager*.msix
$count = $msixFiles.count
Write-Output "Discovered $count MSIX files"

Write-Output "Copying MSIX files into a temporary folder for processing"
$tempMsixFolder = Join-Path -Path $env:TEMP -ChildPath "MsixFiles\"
New-Item -Path $tempMsixFolder -ItemType Directory -Force

Remove-Item "$tempMsixFolder\*.msix"


$msixFiles | ForEach-Object {
    if($_.FullName.Contains('\Dependencies\')){
        continue
    } else {
        Copy-Item -Path $_ -Destination $tempMsixFolder
        Write-Output "Successfully copied $_ to $tempMsixFolder"
    }
}

MakeAppx.exe bundle /bv $BundleVersion /d $tempMsixFolder /p $BundleFileName
