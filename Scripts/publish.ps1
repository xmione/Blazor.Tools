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
#       C:\repo\Blazor.Tools\Scripts\publish -Publish $false -IsRelease $false
#       C:\repo\Blazor.Tools\Scripts\publish -Publish $false -IsRelease $true
#
#   4.2. To build project and compose README and Change log files and publish to docker hub 
#       and nuget.org, run:
#
#       C:\repo\Blazor.Tools\Scripts\publish -Publish $true -IsRelease $false -GitComment "Updated project with the latest Debug changes"
#       C:\repo\Blazor.Tools\Scripts\publish -Publish $true -IsRelease $true -GitComment "Updated project with the latest Release changes"
#
#============================================================================================

param(
    [Parameter(Mandatory=$true)]
    [bool] $Publish,
    [bool] $IsRelease = $false,
    [string] $GitComment = "Update project with the latest changes"
)

<#==========================================================================================================
                        S T A R T  O F  F U N C T I O N  D E L A C R A T I O N S
===========================================================================================================#>

function RestoreNugetPackages {
    param(
        [string]$solutionFile
    )
# This command should only be manually run
    #Write-Host "Clearing nuget local cache..."
    #dotnet nuget locals all --clear
    <#
    Write-Host "==========================================================================================="
    Write-Host "Using Configuration ${Configuration}, cleaning the solution..."
    Write-Host "==========================================================================================="
    dotnet clean Blazor.Tools.sln

    Write-Host "==========================================================================================="
    Write-Host "Using Configuration ${Configuration}, building the solution..."
    Write-Host "==========================================================================================="
    dotnet build Blazor.Tools.sln
    #>
    Write-Host "==========================================================================================="
    Write-Host "Restoring NuGet packages using solution file..."
    Write-Host " dotnet restore $solutionFile --verbosity detailed"
    Write-Host "==========================================================================================="
    # Restore NuGet packages using solution file
    dotnet restore $solutionFile --verbosity detailed
    <#
    Write-Host "Restoring NuGet packages using project file..."
    # Restore NuGet packages using project file
    dotnet restore $projectFile  
    #>

    # Check the exit code of the msbuild command
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE"
    }
}

function BuildSolution {
    param(
        [string]$configuration, 
        [string]$packageVersion, 
        [string]$assemblyVersion, 
        [string]$fileVersion
    )
    Write-Host "==========================================================================================="
    Write-Host "Building solution with the updated Configuration ($configuration) PackageVersion ($packageVersion), AssemblyVersion ($assemblyVersion) and FileVersion ($fileVersion)"
    Write-Host "dotnet msbuild $solutionFile  /p:Configuration=$configuration /p:AssemblyVersion=$assemblyVersion /p:FileVersion=$fileVersion "
    Write-Host "==========================================================================================="
    # Update AssemblyVersion and FileVersion using the solution file
    dotnet msbuild $solutionFile  /p:Configuration=$configuration /p:AssemblyVersion=$assemblyVersion /p:FileVersion=$fileVersion 
    <#
    Write-Host "Building project with the updated Configuration ($configuration) PackageVersion ($packageVersion), AssemblyVersion ($assemblyVersion) and FileVersion ($fileVersion)"
    # Update AssemblyVersion and FileVersion using the project file
    dotnet msbuild $projectFile  /p:Configuration=$configuration /p:AssemblyVersion=$assemblyVersion /p:FileVersion=$fileVersion 
    #>

    # Check the exit code of the msbuild command
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE"
    }

}

function PackProject { 
    param(
        [string]$projectFile, 
        [string]$configuration, 
        [string]$packageVersion, 
        [string]$changeLogPath, 
        [string]$packagesOutputFolderPath
    )
 
    Write-Host "==========================================================================================="
    Write-Host "Packing the project..."
    Write-Host "==========================================================================================="
    # Pack the project
    # Test it in the terminal window
    # dotnet pack "Blazor.Tools.BlazorBundler/Blazor.Tools.BlazorBundler.csproj" -c "Debug" /p:PackageVersion="3.1.1" /p:PackageReleaseNotesFile="Blazor.Tools.BlazorBundler/changelog_3.1.1.md" -v detailed /p:NoDefaultExcludes=true --output "C:\repo\Blazor.Tools\packages"

    dotnet pack $projectFile -c $configuration /p:PackageVersion=$packageVersion /p:PackageReleaseNotesFile=$changelogPath -v detailed /p:NoDefaultExcludes=true --output $packagesOutputFolderPath

    # Check the exit code of the msbuild command
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE"
    }
}

function SaveChangeLogAndReadMe {
    param(
        [string]$SolutionRoot, 
        [string]$packageVersion, 
        [string]$assemblyVersion, 
        [string]$fileVersion, 
        [string]$changelogPath
    )
            
    # Generate Changelog with dynamic version information
    $changelogContent = @"
Version $packageVersion
-----------------------
Package Version: $packageVersion
Assembly Version: $assemblyVersion
File Version: $fileVersion

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
    Write-Host "==========================================================================================="
    Write-Host "Saving change log to file $changelogPath"
    Write-Host "==========================================================================================="
    Set-Content -Path $changelogPath -Value $changelogContent

    # Define the changelog directory and filter pattern
    $changelogDir = Join-Path -Path $SolutionRoot -ChildPath "Blazor.Tools.BlazorBundler"
    $filterPattern = "changelog_*.md"

    # List all change log files in the directory
    $changeLogFiles = Get-ChildItem -Path $changelogDir -Filter $filterPattern | Sort-Object

    # Generate Markdown content for README.md with version information
    $readmeContent = @"
# BlazorBundler

BlazorBundler is a utility tool designed to simplify the process of bundling multiple packages, particularly for Blazor applications. This tool allows you to download and bundle essential files and dependencies, such as Bootstrap and Bootstrap Icons, to enhance your Blazor projects.

## Version Information
- **Package Version**: $packageVersion
- **Assembly Version**: $assemblyVersion
- **File Version**: $fileVersion

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
    `$version` = "$packageVersion"
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
    Write-Host "==========================================================================================="
    Write-Host "Appending change log files to README.md..."
    Write-Host "==========================================================================================="
    $readmeContent += "`n"
    foreach ($file in $changeLogFiles) {
        $fileName = $file.Name
        $fileUrl = "$changeLogsUrl$fileName"
        $link = "- [$($file.Name)]($($fileUrl))`n"
        Write-Host "Generated link: $link"
        $readmeContent += $link
    }

    # Save updated README.md
    $readmePath = "${SolutionRoot}\Blazor.Tools.BlazorBundler\README.md"
    Write-Host "==========================================================================================="
    Write-Host "Saving updated $readmePath..."
    Write-Host "==========================================================================================="
    Set-Content -Path $readmePath -Value $readmeContent

    # Save updated readme.txt
    $readmePath = "${SolutionRoot}\Blazor.Tools.BlazorBundler\readme.txt"
    Write-Host "Saving updated $readmePath..."
    Set-Content -Path $readmePath -Value $readmeContent

}

<#==========================================================================================================
                        E N D  O F  F U N C T I O N  D E L A C R A T I O N S
===========================================================================================================#>

$SolutionRoot = "C:\repo\Blazor.Tools"
$packageVersion = "3.1.2"
$assemblyVersion = "$packageVersion.0"
$fileVersion = "$packageVersion.0"
$nugetApiKey = $Env:MY_NUGET_API_KEY
$changelogPath = "${SolutionRoot}\Blazor.Tools.BlazorBundler\changelog_$packageVersion.md"

# Determine the configuration based on the IsRelease parameter
$configuration = if ($IsRelease) { "Release" } else { "Debug" }

# Update project file - using dotnet msbuild
$solutionFile = "${SolutionRoot}\Blazor.Tools.sln"
$projectFile = "${SolutionRoot}\Blazor.Tools.BlazorBundler\Blazor.Tools.BlazorBundler.csproj"
$packagesOutputFolderPath = "${SolutionRoot}\packages"
$colors = @("Cyan", "Magenta", "Yellow", "Green")
Try {
    
    & "${SolutionRoot}\Scripts\delnugetsources.ps1" -Verbose
    & "${SolutionRoot}\Scripts\addnugetsources.ps1" -Verbose
    & "${SolutionRoot}\Scripts\delnugetpackages.ps1" -Verbose
    & "${SolutionRoot}\Scripts\delbinobj.ps1" -Verbose
    & "${SolutionRoot}\Scripts\delglobalpackages.ps1" -Verbose

    RestoreNugetPackages $solutionFile    
    BuildSolution $configuration $packageVersion $assemblyVersion $fileVersion
    PackProject $projectFile $configuration $packageVersion $changeLogPath $packagesOutputFolderPath   
    SaveChangeLogAndReadMe $SolutionRoot  $packageVersion  $assemblyVersion  $fileVersion  $changelogPath

    <# Run the following codes only if boolean parameter Publish is true #>
    if($Publish -eq $true)
    {
        Write-Host "==========================================================================================="
        Write-Host "Pushing changes to GitHub Repository..."
        Write-Host "==========================================================================================="
        git add .
        git commit -m $GitComment
        git push # pushes to current branch
        #git push origin master 
     
        Write-Host "==========================================================================================="
        Write-Host "Dockerizing the project solomiosisante/blazor-bundler:latest..."
        Write-Host "==========================================================================================="

        # Dockerize
        #docker build -t solomiosisante/blazor-bundler:latest .
        docker build --build-arg BUILD_CONFIGURATION=$configuration -t solomiosisante/blazor-bundler:latest .

        # Check the exit code of the msbuild command
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed with exit code $LASTEXITCODE"
        }

        Write-Host "==========================================================================================="
        Write-Host "Publishing the Docker image to Docker Hub..."
        Write-Host "==========================================================================================="
        # Publish the Docker image to Docker Hub (replace with your publish command)
        docker push solomiosisante/blazor-bundler:latest

        # Check the exit code of the msbuild command
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed with exit code $LASTEXITCODE"
        }

        Write-Host "==========================================================================================="
        Write-Host "Publishing the Package to nuget.org..."
        Write-Host "==========================================================================================="
        # Publish the package to nuget.org (replace with your publish command)
        dotnet nuget push packages/Blazor.Tools.BlazorBundler.$packageVersion.nupkg --source https://api.nuget.org/v3/index.json --api-key $nugetApiKey

        # Check the exit code of the msbuild command
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed with exit code $LASTEXITCODE"
        }
    }


}
Catch {
    Write-Host "An error occurred: $_"
    throw  # Stops script execution
}

[console]::beep(777,7777)  

