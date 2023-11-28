param(
    [Parameter(Mandatory=$true)]
    [string]$ClientId,

    [Parameter(Mandatory=$true)]
    [string]$ClientSecret,

    [Parameter(Mandatory=$true)]
    [string]$TenantId,

    [Parameter(Mandatory=$true)]
    [string]$StorageAccountName,

    [Parameter(Mandatory=$true)]
    [string]$ContainerName,

    [Parameter(Mandatory=$true)]
    [string]$SourceFolderPath
)

az login --service-principal -u $ClientId -p $ClientSecret --tenant $TenantId
az storage blob delete-batch --source $ContainerName --pattern '*' --account-name $StorageAccountName
az storage blob upload-batch --destination $ContainerName --source $SourceFolderPath --account-name $StorageAccountName