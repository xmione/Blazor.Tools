<#
    File Name       : Uninstall-Pkgs.psm1
    Purpose         : To automate the uninstallation of nuget packages.
    Created By      : Solomio S. Sisante
    Created On      : July 20, 2024
    Sample command  : 
                        $projectPath = "C:\repo\Blazor.Tools\Blazor.Tools\"
                        $projectName = "Blazor.Tools.csproj"
                        Uninstall-Pkgs -ProjectPath  $projectPath -$ProjectName $projectName
#>
Function Uninstall-Pkgs
{
    param(
            [string]$ProjectPath,
            [string]$ProjectName
        )
    Write-Host "Executing CustomUninstall script"

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

    # List of directories to delete
    $directoriesToDelete = @(
        "wwwroot\bundler",
        "BlazorBundler"
    )

    # Loop through directories and delete each recursively
    foreach ($directory in $directoriesToDelete) {
        $fullPath = Join-Path $ProjectPath $directory
        if (Test-Path $fullPath -PathType Container) {
            Write-Host "Deleting directory: $fullPath"
            Remove-Item -Path $fullPath -Recurse -Force
        } else {
            Write-Host "Directory not found: $fullPath"
        }
    }

    # Define the .csproj file
    $csprojFile = Join-Path $ProjectPath $ProjectName  # Replace with your actual .csproj filename e.g.: "Blazor.Tools.csproj"

    # Call function to remove specified items from .csproj
    RemoveFromCsproj $csprojFile $directoriesToDelete

    Cleanup-Tools
    Write-Host "CustomUninstall script completed"

}

Export-ModuleMember -Function Uninstall-Pkgs