<#
    C:\repo\Blazor.Tools\Scripts\delglobalpackages.ps1
#>

$globalNugetPackagesFolderPath = "${env:UserProfile}\.nuget\packages"
$folders = @(
    "blazor.tools.blazorbundler",
    "blazor.tools.blazorbundler.entities",
    "blazor.tools.blazorbundler.entities.sampleobjects.data",
    "blazor.tools.blazorbundler.entities.sampleobjects.models",
    "blazor.tools.blazorbundler.entities.sampleobjects.viewmodels",
    "blazor.tools.blazorbundler.extensions",
    "blazor.tools.blazorbundler.factories",
    "blazor.tools.blazorbundler.interfaces",
    "blazor.tools.blazorbundler.models",
    "blazor.tools.blazorbundler.sessionmanagement",
    "blazor.tools.blazorbundler.utilities"
     
)
Write-Host "==========================================================================================="
Write-Host "Deleting ${globalNugetPackagesFolderPath} folders..."
Write-Host "==========================================================================================="

# Loop through each path and remove bin and obj folders
foreach ($folder in $folders) {
    $nugetFolder = Join-Path $globalNugetPackagesFolderPath $folder 

    if (Test-Path $nugetFolder) {
        Write-Host "Deleting ${nugetFolder} folder..."
        Remove-Item -Path $nugetFolder -Recurse -Force
    }
}

