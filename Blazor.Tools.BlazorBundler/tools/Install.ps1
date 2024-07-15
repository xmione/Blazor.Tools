param($installPath, $toolsPath, $package, $project)

function Copy-FileToProject {
    param(
        [string]$SourcePath,
        [string]$DestinationPath
    )

    # Check if the destination file already exists
    if (!(Test-Path -Path $DestinationPath)) {
        # Create the directory structure if it doesn't exist
        $destinationDir = Split-Path -Path $DestinationPath
        if (!(Test-Path -Path $destinationDir)) {
            New-Item -ItemType Directory -Path $destinationDir | Out-Null
        }

        # Copy the file
        Copy-Item -Path $SourcePath -Destination $DestinationPath
        Write-Host "Copied $SourcePath to $DestinationPath"
    } else {
        Write-Host "File $DestinationPath already exists. Skipping."
    }
}

# Example: Specify files to copy
$filesToCopy = @(
    @{
        Source = Join-Path -Path $installPath -ChildPath "wwwroot\blazorbootstrap\blazor.bootstrap.css"
        Destination = Join-Path -Path $project -ChildPath "wwwroot\blazorbootstrap\blazor.bootstrap.css"
    },
    @{
        Source = Join-Path -Path $installPath -ChildPath "wwwroot\blazorbootstrap\blazor.bootstrap.js"
        Destination = Join-Path -Path $project -ChildPath "wwwroot\blazorbootstrap\blazor.bootstrap.js"
    },
    @{
        Source = Join-Path -Path $installPath -ChildPath "wwwroot\blazored-typeahead\blazored-typeahead.js"
        Destination = Join-Path -Path $project -ChildPath "wwwroot\blazored-typeahead\blazored-typeahead.js"
    },
    @{
        Source = Join-Path -Path $installPath -ChildPath "wwwroot\bootstrap-icons\font\bootstrap-icons.min.css"
        Destination = Join-Path -Path $project -ChildPath "wwwroot\bootstrap-icons\font\bootstrap-icons.min.css"
    },
    @{
        Source = Join-Path -Path $installPath -ChildPath "wwwroot\bootstrap-icons\font\fonts\bootstrap-icons.woff"
        Destination = Join-Path -Path $project -ChildPath "wwwroot\bootstrap-icons\font\fonts\bootstrap-icons.woff"
    },
    @{
        Source = Join-Path -Path $installPath -ChildPath "wwwroot\bootstrap-icons\font\fonts\bootstrap-icons.woff2"
        Destination = Join-Path -Path $project -ChildPath "wwwroot\bootstrap-icons\font\fonts\bootstrap-icons.woff2"
    }
)

# Loop through files and copy each to the project
foreach ($file in $filesToCopy) {
    Copy-FileToProject -SourcePath $file['Source'] -DestinationPath $file['Destination']
}
