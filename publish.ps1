﻿#============================================================================================
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
#       .\publish -Publish $false -IsRelease $true
#
#   4.2. To build project and compose README and Change log files and publish to docker hub 
#       and nuget.org, run:
#
#       .\publish -Publish $true -IsRelease $true -GitComment "Updated project with the latest changes"
#
#============================================================================================

param(
    [Parameter(Mandatory=$true)]
    [bool] $Publish,
    [bool] $IsRelease = $false,
    [string] $GitComment = "Update project with the latest changes"
)

$PackageVersion = "3.1.1"
$AssemblyVersion = "$PackageVersion.0"
$FileVersion = "$PackageVersion.0"
$nugetApiKey = $Env:MY_NUGET_API_KEY
$changelogPath = "Blazor.Tools.BlazorBundler/changelog_$PackageVersion.md"

# Determine the configuration based on the IsRelease parameter
$Configuration = if ($IsRelease) { "Release" } else { "Debug" }

# Update project file - using dotnet msbuild
$projectFile = "Blazor.Tools.BlazorBundler/Blazor.Tools.BlazorBundler.csproj"

Write-Host "Building project with the updated PackageVersion ($PackageVersion), AssemblyVersion ($AssemblyVersion) and FileVersion ($FileVersion)"
# Update AssemblyVersion and FileVersion
dotnet msbuild $projectFile  /p:Configuration=$Configuration /p:AssemblyVersion=$AssemblyVersion /p:FileVersion=$FileVersion 

Write-Host "Packing the project..."
# Pack the project
dotnet pack -c $Configuration /p:PackageVersion=$PackageVersion /p:PackageReleaseNotesFile=$changelogPath -v detailed /p:NoDefaultExcludes=true

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
- Added missing project references.

### Revisions
- None.
"@

# Save change log to file
Write-Host "Saving change log to file $changelogPath"
Set-Content -Path $changelogPath -Value $changelogContent

# List all change log files in the directory
$changeLogFiles = Get-ChildItem -Filter "Blazor.Tools.BlazorBundler/changelog_*.md" | Sort-Object

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
    - Bogus version 35.6.0
    - Bootstrap Icons version 1.11.3
    - Dapper version 2.1.35
    - HtmlAgilityPack version 1.11.61
    - Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore version 8.0.6
    - Microsoft.AspNetCore.Identity.EntityFrameworkCore version 8.0.6
    - Microsoft.Build version 17.10.4
    - Microsoft.EntityFrameworkCore.SqlServer version 8.0.6
    - Microsoft.EntityFrameworkCore.Tools version 8.0.6
    - Microsoft.ML version 3.0.1
    - Microsoft.VisualStudio.Azure.Containers.Tools.Targets version 1.21.0
    - Newtonsoft.Json version 13.0.3    

## Installation

You can install BlazorBundler via NuGet Package Manager, Package Manager Console or from a terminal:

### Nuget Package Manager
- Search for Blazor.Tools.BlazorBundler in nuget.org and click Install.

### Package Manager Console
```
Install-Package Blazor.Tools.BlazorBundler
```

### Terminal console
```
dotnet add package Blazor.Tools.BlazorBundler
```

## After-Install Setup Commands

Note: After installing the package, you have to manually run the Install-Pkgs module file to install required nuget packages.
      First replace the values of the `$userProfileName` and `$targetPath` variables.
      
      `$userProfileName` should contain your Windows UserProfile Name
      `$sourcePath` should not be changed
      `$targetPath` should contain the full path of your project file

### Open PowerShell and run: 

```````
    `$version` = "$PackageVersion"
    `$userProfileName` = "solom"
    `$sourcePath` = "C:\Users\`$userProfileName`\.nuget\packages\blazor.tools.blazorbundler\`$version`"
    `$targetPath` = "C:\repo\Blazor.Tools\Blazor.Tools\Blazor.Tools.csproj"
    Install-Pkgs -SourcePath `$sourcePath` -TargetProjectPath `$targetPath`
```````
### Setup your App.razor stylesheets and javascripts

Add these to your <head> section:

```````
    <link rel="stylesheet" href="bootstrap/bootstrap.min.css" />
    <link rel="stylesheet" href="bundler/bootstrap-icons/font/bootstrap-icons.min.css" />
    <link rel="stylesheet" href="bundler/blazor-bootstrap/blazor.bootstrap.css" />
    <link rel="stylesheet" href="bundler/css/bundler.css" />
    
    <!-- This is the <ASSEMBLY_NAME>.styles.css file-->
    <link rel="stylesheet" href="Blazor.Tools.styles.css" /> 

    <script src="bundler/blazored-typeahead/blazored-typeahead.js"></script>
    <script src="bundler/js/site.js"></script>
    
    <!-- It needs to be in InteractiveServer mode to work -->
    <HeadOutlet @rendermode="@InteractiveServer" />
```````

Add these to your <body> section:
```````
    <!-- Once again, this is needed -->
    <Routes @rendermode="@InteractiveServer" />
    
    <!-- The script initializes the Blazor runtime, which is essential for a Blazor application to run -->
    <script src="_framework/blazor.web.js"></script>

    <!-- Use local Bootstrap JS -->
    <script src="bundler/js/bootstrap.bundle.min.js"></script>
    <script src="bundler/blazor-bootstrap/blazor.bootstrap.js"></script>
```````

### Add this to your Program.cs file:
```
builder.Services.AddBlazorBootstrap();
```

## Uninstallation

First, uninstall the package from the Nuget Package Manager, Package Manager Console or from a terminal.

### Nuget Package Manager 
- Search Blazor.Tools.BlazorBundler and uninstall it.

### Package Manager Console

```
Uninstall-Package Blazor.Tools.BlazorBundler
```

### Terminal console

```
dotnet remove package Blazor.Tools.BlazorBundler
```

## Clean-up
Note: After uninstalling the package, you have to manually run the Uninstall module file to uninstall the packages.
      First replace the values of the `$userProfileName` and `$targetPath` variables.

      `$projectPath` should contain the path to your project folder
      `$projectName` should contain the name of your project

Open PowerShell and run:

```````
    `$projectPath` = "C:\repo\Blazor.Tools\Blazor.Tools\"
    `$projectName` = "Blazor.Tools.csproj"
    Uninstall-Pkgs -ProjectPath  `$projectPath` -ProjectName `$projectName`
```````

## Change Logs
"@

$changeLogsUrl = "https://github.com/xmione/Blazor.Tools/blob/master/Blazor.Tools.BlazorBundler/"
# Append change log files to README.md
Write-Host "Appending change log files to README.md..."
$readmeContent += "`n"
foreach ($file in $changeLogFiles) {
    $fileName = $file.Name
    $fileUrl = "$changeLogsUrl$fileName"
    $link = "- [$($file.Name)]($($fileUrl))`n"
    Write-Host "Generated link: $link"
    $readmeContent += $link
}

# Save updated README.md
$readmePath = "Blazor.Tools.BlazorBundler/README.md"
Write-Host "Saving updated $readmePath..."
Set-Content -Path $readmePath -Value $readmeContent

# Save updated readme.txt
$readmePath = "Blazor.Tools.BlazorBundler/readme.txt"
Write-Host "Saving updated $readmePath..."
Set-Content -Path $readmePath -Value $readmeContent

<# Run the following codes only if boolean parameter Publish is true #>
if($Publish -eq $true)
{

    Write-Host "Pushing changes to GitHub Repository..."
    git add .
    git commit -m $GitComment
    git push
    #git push origin master
    
    Write-Host "Copying the project files..."

    Write-Host "Dockerizing the project solomiosisante/blazor-bundler:latest..."
    # Dockerize
    docker build -t solomiosisante/blazor-bundler:latest .

    Write-Host "Publishing the Docker image to Docker Hub..."
    # Publish the Docker image to Docker Hub (replace with your publish command)
    docker push solomiosisante/blazor-bundler:latest

    Write-Host "Publishing the Package to nuget.org..."
    # Publish the package to nuget.org (replace with your publish command)
    dotnet nuget push packages/Blazor.Tools.BlazorBundler.$PackageVersion.nupkg --source https://api.nuget.org/v3/index.json --api-key $nugetApiKey
}

[console]::beep(777,7777)  