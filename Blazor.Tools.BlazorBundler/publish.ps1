#============================================================================================
# File Name: publish.ps1
# Purpose  : To automate the publishing of project to docker hub and nuget.org.
# Created By: Solomio S. Sisante
# Created On: July 13, 2024
#--------------------------------------------------------------------------------------------
# Do these steps before running the script:
# 1. Generate and copy API Key in nuget.org.
# 2. Run this command from Powershell terminal.
#       $Env:MY_NUGET_API_KEY="YOUR_API_KEY"  
#
# 3. Set the value of PackageVersion variable and the change log information.
#
# 4. Run this script from the same Powershell terminal. Make sure you are in the same folder.
#   Examples:
#   4.1. To build project and compose README and Change log files, run:
#
#       .\publish -Publish $false
#
#   4.2. To build project and compose README and Change log files and publish to docker hub 
#       and nuget.org, run:
#
#       .\publish -Publish $true
#
# 5. Process Steps:
#   5.1. Build first then commit and push to github then finally publish:
#       
#       .\publish -Publish $false
#       .\publish -Publish $true -GitComment "Update project with the latest changes"
#
#============================================================================================

param(
    [Parameter(Mandatory=$true)]
    [bool] $Publish,
    [string] $GitComment = "Update project with the latest changes"
)

$PackageVersion = "3.0.2"
$AssemblyVersion = "$PackageVersion.0"
$FileVersion = "$PackageVersion.0"
$nugetApiKey = $Env:MY_NUGET_API_KEY

# Update project file - using dotnet msbuild
$projectFile = "Blazor.Tools.BlazorBundler.csproj"

# Update AssemblyVersion and FileVersion
dotnet msbuild $projectFile  /p:Configuration=Release /p:AssemblyVersion=$AssemblyVersion /p:FileVersion=$FileVersion

# Generate Changelog with dynamic version information
$changelogContent = @"
Version $PackageVersion
-----------------------
Package Version: $PackageVersion
Assembly Version: $AssemblyVersion
File Version: $FileVersion

### Major Changes
- None

### Minor Changes
- None.

### Patches
- Added js folder with these files in wwwroot folder:
    bootstrap.bundle.min.js
    site.js

- Added cleanup scripts to uninstall files and remove them from the .csproj file

### Revisions
- None.
"@

# Save changelog to file
$changelogPath = "changelog_$PackageVersion.md"
Set-Content -Path $changelogPath -Value $changelogContent

# List all change log files in the directory
$changeLogFiles = Get-ChildItem -Filter "changelog_*.md" | Sort-Object

# Generate Markdown content for README.md with version information
$readmeContent = @"
# BlazorBundler

BlazorBundler is a utility tool designed to simplify the process of bundling multiple packages, particularly for Blazor applications. This tool allows you to download and bundle essential files and dependencies, such as Bootstrap and Bootstrap Icons, to enhance your Blazor projects.

## Version Information
- **Package Version**: $PackageVersion
- **Assembly Version**: $AssemblyVersion
- **File Version**: $FileVersion

## Features

- **Automated Package Bundling**: Easily download and bundle multiple packages with a single command.
- **Bootstrap Integration**: Seamlessly include Bootstrap and Bootstrap Icons in your Blazor projects.
- **Asynchronous Downloads**: Efficiently download multiple files simultaneously for faster setup.
- Bundle:
    - Blazor.Bootstrap version 1.11.1
    - Blazored.Typeahead version 4.7.0
    - Bootstrap Icons version 1.11.3

## Installation

You can install BlazorBundler via NuGet Package Manager, Package Manager Console or from a terminal:

#Nuget Package Manager
- Search for Blazor.Tools.BlazorBundler in nuget.org.


```
dotnet add package Blazor.Tools.BlazorBundler
```

#Package Manager Console
```
Install-Package Blazor.Tools.BlazorBundler
```

#Terminal console
```
dotnet add package Blazor.Tools.BlazorBundler
```

## Setup your App.razor stylesheets and javascripts

Add these to your <head> section:
```
    <link rel="stylesheet" href="bootstrap/bootstrap.min.css" />
    <link rel="stylesheet" href="bootstrap-icons/font/bootstrap-icons.min.css" />
    <link rel="stylesheet" href="blazor-bootstrap/blazor.bootstrap.css" />

    <script src="blazored-typeahead/blazored-typeahead.js"></script>
    <script src="js/site.js"></script>
```

Add these to your <body> section:
```
    <script src="js/bootstrap.bundle.min.js"></script>
    <script src="blazor-bootstrap/blazor.bootstrap.js"></script>
```
    
## Uninstallation

First, uninstall the package from the Nuget Package Manager, Package Manager Console or from a terminal.

#Nuget Package Manager 
- Search Blazor.Tools.BlazorBundler and uninstall it.

#Package Manager Console
```
Uninstall-Package Blazor.Tools.BlazorBundler
```

#Terminal console
```
dotnet remove package Blazor.Tools.BlazorBundler
```

There is an Uninstall.ps1 file you can run from the /BlazorBundler folder.


## Change Logs
"@

$changeLogsUrl = "https://github.com/xmione/Blazor.Tools/blob/master/Blazor.Tools.BlazorBundler/"
# Append change log files to README.md
$readmeContent += "`n"
foreach ($file in $changeLogFiles) {
    $fileName = $file.Name
    $fileUrl = "$changeLogsUrl$fileName"
    $link = "- [$($file.Name)]($($fileUrl))`n"
    Write-Host "Generated link: $link"
    $readmeContent += $link
}

# Save updated README.md
$readmePath = "README.md"
Set-Content -Path $readmePath -Value $readmeContent

<# Run the following codes only if boolean parameter Publish is true #>
if($Publish -eq $true)
{

    git add .
    git commit -m $GitComment
    git push origin master

    # Pack the project
    dotnet pack -c Release /p:PackageVersion=$PackageVersion /p:PackageReleaseNotesFile=$changelogPath -v detailed

    # Dockerize
    docker build -t solomiosisante/blazor-bundler:latest .

    # Publish the Docker image to Docker Hub (replace with your publish command)
    docker push solomiosisante/blazor-bundler:latest

    # Publish the package (replace with your publish command)
    dotnet nuget push bin/Release/Blazor.Tools.BlazorBundler.$PackageVersion.nupkg --source https://api.nuget.org/v3/index.json --api-key $nugetApiKey
}
