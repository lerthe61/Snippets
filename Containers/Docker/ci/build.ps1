<#
.Synopsis
	Building set of containers and push them into private registry
#>

param(
	# [parameter(Mandatory=$true)]
	# [String]
	# $registry = "tempml.azurecr.io"
)

#$registry = "tempml.azurecr.io"

# Stop in case of error
$ErrorActionPreference = "Stop"

# Prepare new version
$rawContent = Get-Content -Path $PSScriptRoot'\currentVersion.txt'
$version = [int]$rawContent
$newVersion = $version+1

Write-Host 'Building containers with version: ' $newVersion

# Building 'Receiver'
$populateLabel = 'receiver:v'+$newVersion
Write-Host 'Building '$populateLabel' ...'
docker build -t $populateLabel -f .\receiverDockerFile .
Write-Host "Complete`n"

# Push 'Receiver' into registry
# docker tag $extractorLabel $registry/$populateLabel
# docker push $registry/$populateLabel

# Preserve new version for next builds
Set-Content -Path $PSScriptRoot'\currentVersion.txt' -Force $newVersion