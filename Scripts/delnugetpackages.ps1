<#
	delnugetpackages.ps1
#>

function Remove-AllFilesFromFolder {
    param (
        [string]$folderPath
    )

    if (-Not (Test-Path -Path $folderPath -PathType Container)) {
        Write-Error "The specified folder does not exist: $folderPath"
        return
    }

    # Get all files in the folder
    $files = Get-ChildItem -Path $folderPath -File

    if ($files.Count -eq 0) {
        Write-Output "No files found in the folder: $folderPath"
        return
    }

    foreach ($file in $files) {
        try {
            Remove-Item -Path $file.FullName -Force
            Write-Output "Deleted file: $($file.FullName)"
        } catch {
            Write-Error "Failed to delete file: $($file.FullName). Error: $_"
        }
    }
}

# Example usage
Remove-AllFilesFromFolder -folderPath "C:\repo\Blazor.Tools\packages"
