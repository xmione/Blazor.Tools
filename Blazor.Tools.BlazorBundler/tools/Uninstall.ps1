# Uninstall.ps1
# Sample Usage: .\Uninstall -ProjectName "Blazor.Tools.csproj"
param(
        [string]$ProjectName
    )
Write-Host "Executing CustomUninstall script"

$scriptsourceDir = Get-Item -Path (Get-Location)

# Get the parent directory of the current directory
$projectDir = Split-Path -Path $scriptsourceDir.FullName -Parent

# List of directories to delete
$directoriesToDelete = @(
    "wwwroot\bundler",
    "BlazorBundler"
)

# Function to remove items from .csproj file
function RemoveFromCsproj($csprojFile, $items) {
    [xml]$csprojXml = Get-Content $csprojFile

    # Select all ItemGroup nodes
    $itemGroups = $csprojXml.SelectNodes("//ItemGroup")

    foreach ($itemGroup in $itemGroups) {
        foreach ($item in $itemGroup.ChildNodes) {
            foreach ($itemToRemove in $items) {
                if ($item.Name -ne "ProjectReference" -and $item.Include -like "*$itemToRemove*") {
                    Write-Host "Removing $itemToRemove from .csproj"
                    $itemGroup.RemoveChild($item)
                }
            }
        }
    }

    $csprojXml.Save($csprojFile)
}

# Loop through directories and delete each recursively
foreach ($directory in $directoriesToDelete) {
    $fullPath = Join-Path $projectDir $directory
    if (Test-Path $fullPath -PathType Container) {
        Write-Host "Deleting directory: $fullPath"
        Remove-Item -Path $fullPath -Recurse -Force
    } else {
        Write-Host "Directory not found: $fullPath"
    }
}

# Define the .csproj file
$csprojFile = Join-Path $projectDir $ProjectName  # Replace with your actual .csproj filename e.g.: "Blazor.Tools.csproj"

# Call function to remove specified items from .csproj
RemoveFromCsproj $csprojFile $directoriesToDelete

Write-Host "CustomUninstall script completed"
