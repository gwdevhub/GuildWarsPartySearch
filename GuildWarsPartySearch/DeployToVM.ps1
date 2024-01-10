Param(
	[string]$User,
	[string]$Address,
	[string]$KeyFile,
	[string]$PathToEnvFileFolder
)

if (!(Test-Path $KeyFile)){
	Write-Host Key file does not exist
	exit -1
}

ssh -o "StrictHostKeyChecking=no" -i $KeyFile "$User@$Address" "rm -rf deploy/; mkdir deploy"
scp -o "StrictHostKeyChecking=no" -r -i $KeyFile "$($PathToEnvFileFolder)/envfile" "$($User)@$($Address):deploy/"
ssh -o "StrictHostKeyChecking=no" -i $KeyFile "$User@$Address" 'sudo docker stop $(sudo docker ps -a -q)'
ssh -o "StrictHostKeyChecking=no" -i $KeyFile "$User@$Address" "sudo docker pull alex4991/guildwarspartysearch.server:latest; nohup sudo docker run --env-file deploy/envfile -p 80:80 -p 443:443 alex4991/guildwarspartysearch.server:latest > stdout.txt 2> stderr.txt &"
