# List all files from inside specific cat file
$output = Test-FileCatalog -CatalogFilePath C:\Path\some.cat -Detailed
$output.CatalogItems.Keys | Sort-Object | ForEach-Object { Write-Host $_ }
