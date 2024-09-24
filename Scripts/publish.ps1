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
    Write-Host "dotnet msbuild $solutionFile  /p:Configuration=$configuration /p:AssemblyVersion=$assemblyVersion /p:FileVersion=$fileVersion /p:Version=$packageVersion "
    Write-Host "==========================================================================================="
    # Update AssemblyVersion and FileVersion using the solution file
    dotnet msbuild $solutionFile  /p:Configuration=$configuration /p:AssemblyVersion=$assemblyVersion /p:FileVersion=$fileVersion /p:Version=$packageVersion
    <#
    Write-Host "Building project with the updated Configuration ($configuration) PackageVersion ($packageVersion), AssemblyVersion ($assemblyVersion) and FileVersion ($fileVersion)"
    # Update AssemblyVersion and FileVersion using the project file
    dotnet msbuild $projectFile  /p:Configuration=$configuration /p:AssemblyVersion=$assemblyVersion /p:FileVersion=$fileVersion /p:Version=$packageVersion
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
    $readmeContent =  Get-Content -Path "${changeLogDir}\readme.tpt" -Raw
    $readmeContent = $readmeContent -replace "\$packageVersion", $packageVersion
    $readmeContent = $readmeContent -replace "\$assemblyVersion", $assemblyVersion
    $readmeContent = $readmeContent -replace "\$fileVersion", $fileVersion

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

$SolutionRoot = Get-Location
$packageVersion = "3.1.3"
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

    # Check if the $nugetApiKey variable is empty
    if ([string]::IsNullOrWhiteSpace($nugetApiKey)) {
        throw "NuGet API Key environment variable is empty. In a terminal, run: `$Env:MY_NUGET_API_KEY = 'YOUR_API_KEY'"
    }
    
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
        Write-Host "docker build --build-arg BUILD_CONFIGURATION=$configuration NUGET_API_KEY=$nugetApiKey -t solomiosisante/blazor-bundler:latest ."
        Write-Host "==========================================================================================="

        # Dockerize
        #docker build -t solomiosisante/blazor-bundler:latest .
        docker build -p:Version=$packageVersion -p:FileVersion=$fileVersion -p:AssemblyVersion=$assemblyVersion --build-arg BUILD_CONFIGURATION=$configuration -t solomiosisante/blazor-bundler:latest .

        # Check the exit code of the msbuild command
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed with exit code $LASTEXITCODE"
        }

        Write-Host "==========================================================================================="
        Write-Host "Publishing the Docker image to Docker Hub..."
        Write-Host "docker push solomiosisante/blazor-bundler:latest"
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
        #dotnet nuget push packages/Blazor.Tools.BlazorBundler.$packageVersion.nupkg --source https://api.nuget.org/v3/index.json --api-key $nugetApiKey

        $command = "$SolutionRoot\publish-packages -PackagePath `"$packagesOutputFolderPath`" -NugetApiKey `"$nugetApiKey`" -PackageVersion `"$packageVersion`""
        Start-Process "powershell" -ArgumentList "-NoExit -Command `"$command`"" -Verb runAs
        
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

