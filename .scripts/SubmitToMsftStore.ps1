$uploadPackage = "D:\a\MediaFileManager\MediaFileManager\src\MediaFileManager\PackageProject\StoreUploadPackages\PackageProject_" + $env:UWP_VERSION + "_" + $env:UwpBundlePlatform + "_bundle.appxupload"
$sbTempFolderPath = New-Item -Type Directory -Force -Path (Join-Path -Path 'D:\a\MediaFileManager\MediaFileManager\' -ChildPath 'SBTemp')
$imagesFolderPath = 'D:\a\MediaFileManager\MediaFileManager\src\MediaFileManager\PackageProject\Images'

Set-ExecutionPolicy RemoteSigned -Force

# ********* Install StoreBroker, import module and Authenticate *********
git clone https://github.com/Microsoft/StoreBroker.git 'D:\a\MediaFileManager\MediaFileManager\SBGitRoot\'
Import-Module -Force 'D:\a\MediaFileManager\MediaFileManager\SBGitRoot\StoreBroker'
$username = $env:PartnerCenterClientId
$password = ConvertTo-SecureString $env:PartnerCenterClientSecret -AsPlainText -Force
$cred = New-Object System.Management.Automation.PSCredential ($username, $password)
Set-StoreBrokerAuthentication -TenantId $env:PartnerCenterTenantId -Credential $cred

# DEBUGGING1 Get submission data
$app = Get-Application -AppId '9PD3JFK7W5MB'
$sub = Get-ApplicationSubmission -AppId '9PD3JFK7W5MB' -SubmissionId $($app.lastPublishedApplicationSubmission.id)
$subJsonFilePath = New-TemporaryFile
ConvertTo-Json -InputObject $sub -Depth 20 | Out-File -FilePath $subJsonFilePath          

# DEBUGGING2 - upload config file to Azure blob for debug (SAS token only valid for a couple hours)
$sas = '?sv=2019-12-12&ss=b&srt=c&sp=rwlac&se=2020-08-14T23:56:11Z&st=2020-08-14T15:56:11Z&spr=https&sig=EaLFhgTYmCj0iFFbfm6G9GMbOGTvjUUYl3Di5N0AbHM%3D'
$headers = @{ 
    'x-ms-blob-type' = 'BlockBlob' 
}

$uri1 = "https://dvlup.blob.core.windows.net/ci-cd/broker-output/" + (Get-Item $subJsonFilePath).Name + $sas
Invoke-RestMethod -Method Put -Headers $headers -InFile $subJsonFilePath -Uri $uri1

# ********* Generate SBconfig file *********
#$sbConfigFilePath = 'D:\a\MediaFileManager\MediaFileManager\SBTemp\SBConfig.json'
$sbConfigFilePath = Join-Path -Path $sbTempFolderPath -ChildPath 'SBConfig.json'
New-StoreBrokerConfigFile -AppId '9PD3JFK7W5MB' -Path $sbConfigFilePath

# DEBUGGING3 - upload config file to Azure blob for debug
$uri2 = "https://dvlup.blob.core.windows.net/ci-cd/broker-output/" + (Get-Item $sbConfigFilePath).Name + $sas
Invoke-RestMethod -Method Put -Headers $headers -InFile $sbConfigFilePath -Uri $uri2

# ********* NEW SUBMISSION *********
#$sbSubmissionDataFilePath = 'D:\a\MediaFileManager\MediaFileManager\SBTemp\submission.json'
$sbSubmissionDataFilePath = Join-Path -Path $sbTempFolderPath -ChildPath 'submission.json'
$sbSubmissionDataFileName = (Get-Item $sbSubmissionDataFilePath).Name
New-SubmissionPackage -ConfigPath $sbConfigFilePath -ImagesRootPath $imagesFolderPath -AppxPath $uploadPackage -OutPath $sbTempFolderPath -OutName $sbSubmissionDataFileName

# DEBUGGING4
$uri3 = "https://dvlup.blob.core.windows.net/ci-cd/broker-output/" + (Get-Item $sbSubmissionDataFilePath).Name + $sas
Invoke-RestMethod -Method Put -Headers $headers -InFile $sbSubmissionDataFilePath -Uri $uri3

# ********* UPDATE & COMMIT SUBMISSION *********
Update-ApplicationSubmission -ReplacePackages -AppId $env:PartnerCenterStoreId -SubmissionDataPath $sbSubmissionDataFilePath -PackagePath $uploadPackage -AutoCommit -Force