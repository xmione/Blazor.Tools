<#
    # Install Blazor.Bootstrap package
    Install-NuGetPackage -packageName "Blazor.Bootstrap" -version "1.11.1"  
#>

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
        Source = Join-Path -Path $installPath -ChildPath "wwwroot\bundler\blazor-bootstrap\blazor.bootstrap.css"
        Destination = Join-Path -Path $project -ChildPath "wwwroot\bundler\blazor-bootstrap\blazor.bootstrap.css"
    },
    @{
        Source = Join-Path -Path $installPath -ChildPath "wwwroot\bundler\blazor-bootstrap\blazor.bootstrap.js"
        Destination = Join-Path -Path $project -ChildPath "wwwroot\bundler\blazor-bootstrap\blazor.bootstrap.js"
    },
    @{
        Source = Join-Path -Path $installPath -ChildPath "wwwroot\bundler\blazored-typeahead\blazored-typeahead.js"
        Destination = Join-Path -Path $project -ChildPath "wwwroot\bundler\blazored-typeahead\blazored-typeahead.js"
    },
    @{
        Source = Join-Path -Path $installPath -ChildPath "wwwroot\bundler\bootstrap-icons\font\bootstrap-icons.min.css"
        Destination = Join-Path -Path $project -ChildPath "wwwroot\bundler\bootstrap-icons\font\bootstrap-icons.min.css"
    },
    @{
        Source = Join-Path -Path $installPath -ChildPath "wwwroot\bundler\bootstrap-icons\font\fonts\bootstrap-icons.woff"
        Destination = Join-Path -Path $project -ChildPath "wwwroot\bundler\bootstrap-icons\font\fonts\bootstrap-icons.woff"
    },
    @{
        Source = Join-Path -Path $installPath -ChildPath "wwwroot\bundler\bootstrap-icons\font\fonts\bootstrap-icons.woff2"
        Destination = Join-Path -Path $project -ChildPath "wwwroot\bundler\bootstrap-icons\font\fonts\bootstrap-icons.woff2"
    },
    @{
        Source = Join-Path -Path $installPath -ChildPath "wwwroot\bundler\js\bootstrap.bundle.min.js"
        Destination = Join-Path -Path $project -ChildPath "wwwroot\bundler\js\bootstrap.bundle.min.js"
    },
    @{
        Source = Join-Path -Path $installPath -ChildPath "wwwroot\bundler\js\site.js"
        Destination = Join-Path -Path $project -ChildPath "wwwroot\bundler\js\site.js"
    },
    @{
        Source = Join-Path -Path $installPath -ChildPath "tools\Uninstall.ps1"
        Destination = Join-Path -Path $project -ChildPath "BlazorBundler\Uninstall.ps1"
    },
    @{
        Source = Join-Path -Path $installPath -ChildPath "README.md"
        Destination = Join-Path -Path $project -ChildPath "BlazorBundler\README.md"
    }
)

# Loop through files and copy each to the project
foreach ($file in $filesToCopy) {
    Copy-FileToProject -SourcePath $file['Source'] -DestinationPath $file['Destination']
}

# Install NuGet packages
function Install-NuGetPackage {
    param(
        [string]$packageName,
        [string]$version
    )

    $dotnetArgs = "add package $packageName --version $version"
    Write-Host "Running: dotnet $dotnetArgs"
    dotnet $dotnetArgs
}
