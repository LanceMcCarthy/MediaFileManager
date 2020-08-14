Set-ExecutionPolicy RemoteSigned -Force

# ********* Install StoreBroker and import PowerShell Module *********
git clone https://github.com/Microsoft/StoreBroker.git 'D:\a\MediaFileManager\MediaFileManager\SBGitRoot\'
Import-Module -Force 'D:\a\MediaFileManager\MediaFileManager\SBGitRoot\StoreBroker'

# ********* Authenticate Store Broker *********
$username = $env:PartnerCenterClientId
$password = ConvertTo-SecureString $env:PartnerCenterClientSecret -AsPlainText -Force
$cred = New-Object System.Management.Automation.PSCredential ($username, $password)

Set-StoreBrokerAuthentication -TenantId $env:PartnerCenterTenantId -Credential $cred

# ********* Prepare Submission Package *********
$sbTempFolderPath = New-Item -Type Directory -Force -Path (Join-Path -Path 'D:\a\MediaFileManager\MediaFileManager\' -ChildPath 'SBTemp')
$appxUploadFilePath = "D:\a\MediaFileManager\MediaFileManager\src\MediaFileManager\PackageProject\StoreUploadPackages\PackageProject_" + $env:UWP_VERSION + "_" + $env:UwpBundlePlatform + "_bundle.appxupload"
          
$configFilePath = 'D:\a\MediaFileManager\MediaFileManager\.scripts\storeBrokerConfig.json'
$outName = 'submission.json'

New-SubmissionPackage -ConfigPath $configFilePath -AppxPath $appxUploadFilePath -OutPath $sbTempFolderPath -OutName $outName

# ********* UPDATE & COMMIT SUBMISSION *********
$originalSubmissionDataFilePath = 'D:\a\MediaFileManager\MediaFileManager\.scripts\submissionData.json'
$newSubmissionDataPath = Join-Path -Path $sbTempFolderPath -ChildPath 'submissionData.json'
$packagePath = 'D:\a\MediaFileManager\MediaFileManager\src\MediaFileManager\PackageProject\StoreUploadPackages\StoreUploadPackages.zip'

Update-ApplicationSubmission -ReplacePackages -AppId $env:PartnerCenterStoreId -SubmissionDataPath $originalSubmissionDataFilePath -PackagePath $packagePath -AutoCommit -Force