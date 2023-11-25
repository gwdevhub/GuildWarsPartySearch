param(
    [Parameter(Mandatory=$true)]
    [string]$ConnectionString,

    [Parameter(Mandatory=$true)]
    [string]$ContainerName,

    [Parameter(Mandatory=$true)]
    [string]$SourceFolderPath
)

az storage blob delete-batch --source $ContainerName --connection-string $ConnectionString --pattern '*'
az storage blob upload-batch --destination $ContainerName --source $SourceFolderPath --connection-string $ConnectionString