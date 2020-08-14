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
$headers = @{ 'x-ms-blob-type' = 'BlockBlob' }

$azFileName1 = (Get-Item $subJsonFilePath).Name
$azStorageUriWithSas1 = "https://dvlup.blob.core.windows.net/ci-cd/broker-output/$($azFileName1)?st=2019-04-03T07%3A28%3A36Z&se=2019-04-03T07%3A28%3A36Z&sp=rwdl&sv=2018-03-28&sr=c&sig=Y3%2BBRkH5ivySba7qAFQ%2BnjF2HoVg0Lr4bjVPrKZh6mU%3D"
Invoke-RestMethod -Uri $azStorageUriWithSas1 -Method Put -Headers $headers -InFile $subJsonFilePath

# ********* Generate SBconfig file *********
#$sbConfigFilePath = 'D:\a\MediaFileManager\MediaFileManager\SBTemp\SBConfig.json'
$sbConfigFilePath = Join-Path -Path $sbTempFolderPath -ChildPath 'SBConfig.json'
New-StoreBrokerConfigFile -AppId '9PD3JFK7W5MB' -Path $sbConfigFilePath

# DEBUGGING3 - upload config file to Azure blob for debug
$azFileName2 = (Get-Item $sbConfigFilePath).Name
$azStorageUriWithSas2 = "https://dvlup.blob.core.windows.net/ci-cd/broker-output/$($azFileName2)?st=2019-04-03T07%3A28%3A36Z&se=2019-04-03T07%3A28%3A36Z&sp=rwdl&sv=2018-03-28&sr=c&sig=Y3%2BBRkH5ivySba7qAFQ%2BnjF2HoVg0Lr4bjVPrKZh6mU%3D"
Invoke-RestMethod -Uri $azStorageUriWithSas2 -Method Put -Headers $headers -InFile $sbConfigFilePath

# ********* NEW SUBMISSION *********
#$sbSubmissionDataFilePath = 'D:\a\MediaFileManager\MediaFileManager\SBTemp\submission.json'
$sbSubmissionDataFilePath = Join-Path -Path $sbTempFolderPath -ChildPath 'submission.json'
$sbSubmissionDataFileName = (Get-Item $sbSubmissionDataFilePath).Name
New-SubmissionPackage -ConfigPath $sbConfigFilePath -ImagesRootPath $imagesFolderPath -AppxPath $uploadPackage -OutPath $sbTempFolderPath -OutName $sbSubmissionDataFileName

# DEBUGGING4
$azFileName3 = (Get-Item $sbSubmissionDataFilePath).Name
$azStorageUriWithSas3 = "https://dvlup.blob.core.windows.net/ci-cd/broker-output/$($azFileName3)?st=2019-04-03T07%3A28%3A36Z&se=2019-04-03T07%3A28%3A36Z&sp=rwdl&sv=2018-03-28&sr=c&sig=Y3%2BBRkH5ivySba7qAFQ%2BnjF2HoVg0Lr4bjVPrKZh6mU%3D"
Invoke-RestMethod -Uri $azStorageUriWithSas3 -Method Put -Headers $headers -InFile $sbSubmissionDataFilePath

# ********* UPDATE & COMMIT SUBMISSION *********
Update-ApplicationSubmission -ReplacePackages -AppId $env:PartnerCenterStoreId -SubmissionDataPath $sbSubmissionDataFilePath -PackagePath $uploadPackage -AutoCommit -Force