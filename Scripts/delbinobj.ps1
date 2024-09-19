<#
    Deletes the bin and obj folders of the specified list of folders.
    To run: 

        [Script Folder]\delbinobj.ps1
        C:\repo\Blazor.Tools\Scripts\delbinobj.ps1

#>
# Define the list of paths where you want to delete bin and obj folders
$paths = @(
    "C:\repo\Blazor.Tools\Blazor.Tools\",
    "C:\repo\Blazor.Tools\Blazor.Tools.AppHost\",
    "C:\repo\Blazor.Tools\Blazor.Tools.AppHost.Test\",
    "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\",
    "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler.Entities\",
    "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data\",
    "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models\",
    "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels\",
    "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler.Extensions\",
    "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler.Factories\",
    "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler.Interfaces\",
    "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler.Models\",
    "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler.SessionManagement\",
    "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler.Utilities\",
    "C:\repo\Blazor.Tools\Blazor.Tools.ConsoleApp\",
    "C:\repo\Blazor.Tools\Blazor.Tools.ServiceDefaults\",
    "C:\repo\Blazor.Tools\Blazor.Tools.Test\",
    "C:\repo\Blazor.Tools\MyPackage\",
    "C:\repo\Blazor.Tools\PredictiveMaintenance\",
    "C:\repo\Blazor.Tools\PredictiveMaintenanceConsole\",
    "C:\repo\Blazor.Tools\TargetLibrary\"
)

# Loop through each path and remove bin and obj folders
foreach ($path in $paths) {
    $binFolder = Join-Path $path "bin"
    $objFolder = Join-Path $path "obj"

    if (Test-Path $binFolder) {
	Write-Host "Deleting bin folder at $binFolder..."
        Remove-Item -Recurse -Force $binFolder
        Write-Host "Deleted bin folder at $binFolder"
    } else {
        Write-Host "No bin folder found at $binFolder"
    }

    if (Test-Path $objFolder) {
	Write-Host "Deleting obj folder at $objFolder"
	Remove-Item -Recurse -Force $objFolder
        Write-Host "Deleted obj folder at $objFolder"
    } else {
        Write-Host "No obj folder found at $objFolder"
    }
}

Write-Host "Cleanup completed!"
