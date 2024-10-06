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
#       C:\repo\Blazor.Tools\Scripts\publish -MajorVersion 3 -MinorVersion 1 -PatchVersion 6 -RevisionVersion 0 -Publish $false -IsRelease $false
#       C:\repo\Blazor.Tools\Scripts\publish -MajorVersion 3 -MinorVersion 1 -PatchVersion 6 -RevisionVersion 0 -Publish $false -IsRelease $true
#
#   4.2. To build project and compose README and Change log files and publish to docker hub 
#       and nuget.org, run:
#
#       C:\repo\Blazor.Tools\Scripts\publish -MajorVersion 3 -MinorVersion 1 -PatchVersion 6 -RevisionVersion 0 -Publish $true -IsRelease $false -GitComment "Updated project with the latest Debug changes"
#       C:\repo\Blazor.Tools\Scripts\publish -MajorVersion 3 -MinorVersion 1 -PatchVersion 6 -RevisionVersion 0 -Publish $true -IsRelease $true -GitComment "Updated project with the latest Release changes"
#
#============================================================================================
param(
    [Parameter(Mandatory=$true)]
    [string] $MajorVersion,
    [Parameter(Mandatory=$true)]
    [string] $MinorVersion,
    [Parameter(Mandatory=$true)]
    [string] $PatchVersion,
    [Parameter(Mandatory=$true)]
    [string] $RevisionVersion,
    [Parameter(Mandatory=$true)]
    [bool] $Publish,
    [bool] $IsRelease = $false,
    [string] $GitComment = "Updated project with the latest changes"
)

Write-Host "MajorVersion: $MajorVersion"
Write-Host "MinorVersion: $MinorVersion"
Write-Host "PatchVersion: $PatchVersion"
Write-Host "RevisionVersion: $RevisionVersion"
Write-Host "Publish: $Publish"
Write-Host "IsRelease: $IsRelease"
Write-Host "GitComment: $GitComment"

<#==========================================================================================================
                        S T A R T  O F  F U N C T I O N  D E L A C R A T I O N S
===========================================================================================================#>

function RestoreNugetPackages {
    param(
        [string]$SolutionFile
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
    Write-Host " dotnet restore $SolutionFile --verbosity detailed"
    Write-Host "==========================================================================================="
    # Restore NuGet packages using solution file
    dotnet restore $SolutionFile --verbosity detailed
    <#
    Write-Host "Restoring NuGet packages using project file..."
    # Restore NuGet packages using project file
    dotnet restore $global:projectFile  
    #>

    # Check the exit code of the msbuild command
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE"
    }
}

function BuildSolution {
    param(
        [string]$Configuration, 
        [string]$PackageVersion, 
        [string]$AssemblyVersion, 
        [string]$FileVersion
    )
    <#
        To run:
                BuildSolution -configuration "Release" -packageVersion "3.1.3" -assemblyVersion "3.1.3" -fileVersion "3.1.3.0"
                dotnet msbuild $global:solutionFile /p:PackageVersion="3.1.3" /p:Configuration="Release" /p:AssemblyVersion="3.1.3" /p:FileVersion="3.1.3." /p:Version="3.1.2"
    #>
    

    Write-Host "==========================================================================================="
    Write-Host "Building solution with the updated Configuration ($Configuration) PackageVersion ($PackageVersion), AssemblyVersion ($AssemblyVersion) and FileVersion ($FileVersion)"
    Write-Host "dotnet msbuild $global:solutionFile  /p:Configuration=$Configuration /p:AssemblyVersion=$AssemblyVersion /p:FileVersion=$FileVersion /p:Version=$PackageVersion "
    Write-Host "==========================================================================================="
    # Update AssemblyVersion and FileVersion using the solution file. See (To run:) code above.
    dotnet msbuild $global:solutionFile /p:PackageVersion=$PackageVersion /p:Configuration=$Configuration /p:AssemblyVersion=$AssemblyVersion /p:FileVersion=$FileVersion /p:Version=$PackageVersion

    <#
    Write-Host "Building project with the updated Configuration ($Cconfiguration) PackageVersion ($PackageVersion), AssemblyVersion ($AssemblyVersion) and FileVersion ($FileVersion)"
    # Update AssemblyVersion and FileVersion using the project file
    dotnet msbuild $global:projectFile  /p:Configuration=$Configuration /p:AssemblyVersion=$AssemblyVersion /p:FileVersion=$FileVersion /p:Version=$PackageVersion
    #>

    # Check the exit code of the msbuild command
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE"
    }

}

function PackProject { 
    param(
        [string]$ProjectFile, 
        [string]$Configuration, 
        [string]$PackageVersion, 
        [string]$ChangeLogPath, 
        [string]$PackagesOutputFolderPath
    )
 
    Write-Host "==========================================================================================="
    Write-Host "Packing the project..."
    Write-Host "dotnet pack $ProjectFile -c $Configuration /p:PackageVersion=$PackageVersion /p:PackageReleaseNotesFile=$ChangelogPath -v detailed /p:NoDefaultExcludes=true --output $PackagesOutputFolderPath"
    Write-Host "==========================================================================================="
    # Pack the project
    # Test it in the terminal window
    # dotnet pack "Blazor.Tools.BlazorBundler/Blazor.Tools.BlazorBundler.csproj" -c "Debug" /p:PackageVersion="3.1.1" /p:PackageReleaseNotesFile="Blazor.Tools.BlazorBundler/changelog_3.1.1.md" -v detailed /p:NoDefaultExcludes=true --output "C:\repo\Blazor.Tools\packages"

    dotnet pack $ProjectFile -c $Configuration /p:PackageVersion=$PackageVersion /p:PackageReleaseNotesFile=$ChangelogPath -v detailed /p:NoDefaultExcludes=true --output $PackagesOutputFolderPath

    # Check the exit code of the msbuild command
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE"
    }
}

function SaveChangeLogAndReadMe {
    param(
        [string]$SolutionRoot, 
        [string]$PackageVersion, 
        [string]$AssemblyVersion, 
        [string]$FileVersion, 
        [string]$ChangelogPath
    )
    # Define the changelog directory and filter pattern
    $changelogDir = Join-Path -Path $SolutionRoot -ChildPath "Blazor.Tools.BlazorBundler"
    $filterPattern = "changelog_*.md"

    # List all change log files in the directory
    $changeLogFiles = Get-ChildItem -Path $changelogDir -Filter $filterPattern | Sort-Object

    # Generate Changelog with dynamic version information
    $changelogContent =  Get-Content -Path "${changelogDir}\changelogs.tpt" -Raw
    $changelogContent = $changelogContent -replace "$PackageVersion", $PackageVersion
    $changelogContent = $changelogContent -replace "$AssemblyVersion", $AssemblyVersion
    $changelogContent = $changelogContent -replace "$FileVersion", $FileVersion


    # Save change log to file
    Write-Host "==========================================================================================="
    Write-Host "Saving change log to file $ChangelogPath"
    Write-Host "Set-Content -Path $ChangelogPath -Value $changelogContent"
    Write-Host "==========================================================================================="
    Set-Content -Path $ChangelogPath -Value $changelogContent

    # Generate Markdown content for README.md with version information
    $readmeContent = Get-Content -Path "${changeLogDir}\readme.tpt" -Raw
    $readmeContent = $readmeContent -replace "$PackageVersion", $PackageVersion
    $readmeContent = $readmeContent -replace "$AssemblyVersion", $AssemblyVersion
    $readmeContent = $readmeContent -replace "$FileVersion", $FileVersion
    $branchName = git rev-parse --abbrev-ref HEAD

    $changeLogsUrl = "https://github.com/xmione/Blazor.Tools/blob/$branchName/Blazor.Tools.BlazorBundler/"
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

function Import-Modules {
    param (
        [Parameter(Mandatory=$true)]
        [string[]] $ModulePaths
    )

    foreach ($modulePath in $ModulePaths) {
        # Extract the module name from the path
        $moduleName = [System.IO.Path]::GetFileNameWithoutExtension($modulePath)

        # Check if the module is already imported
        if (-not (Get-Module -Name $moduleName -ListAvailable)) {
            Write-Host "Importing module: $moduleName from $modulePath"
            Import-Module $modulePath
        } else {
            Write-Host "Module $moduleName is already imported."
        }
    }
}

function Initialize
{
    param(
        [Parameter(Mandatory=$true)]
        [string] $MajorVersion,
        [Parameter(Mandatory=$true)]
        [string] $MinorVersion,
        [Parameter(Mandatory=$true)]
        [string] $PatchVersion,
        [Parameter(Mandatory=$true)]
        [string] $RevisionVersion,
        [Parameter(Mandatory=$true)]
        [bool] $Publish,
        [bool] $IsRelease = $false,
        [string] $GitComment = "Updated project with the latest changes"
    )

    Write-Host "MajorVersion: $MajorVersion"
    Write-Host "MinorVersion: $MinorVersion"
    Write-Host "PatchVersion: $PatchVersion"
    Write-Host "RevisionVersion: $RevisionVersion"
    Write-Host "Publish: $Publish"
    Write-Host "IsRelease: $IsRelease"
    Write-Host "GitComment: $GitComment"

    # Check the exit code of the msbuild command
    if ($LASTEXITCODE -ne 0) {
        throw "Initial failed with exit code $LASTEXITCODE"
    }

    $global:solutionRoot = Get-Location
    $global:majorVersion = $MajorVersion
    $global:minorVersion = $MinorVersion
    $global:patchVersion = $PatchVersion
    $global:revisionVersion = $RevisionVersion
    $global:packageVersion = "${MajorVersion}.${MinorVersion}.${PatchVersion}"
    $global:assemblyVersion = "$global:packageVersion.$RevisionVersion"
    $global:fileVersion = "$global:packageVersion.$RevisionVersion"
    $global:nugetApiKey = $Env:NugetApiKey
    $global:changelogPath = "${global:solutionRoot}\Blazor.Tools.BlazorBundler\changelog_${global:packageVersion}.md"

    # Check the exit code of the msbuild command
    if ($LASTEXITCODE -ne 0) {
        throw "Variable settings 1 failed with exit code $LASTEXITCODE"
    }

    # Determine the configuration based on the IsRelease parameter
    $global:configuration = if ($IsRelease) { "Release" } else { "Debug" }
    $global:solutionFile = "${global:solutionRoot}\Blazor.Tools.sln"
    $toolsFolderPath = "${global:solutionRoot}\Blazor.Tools.BlazorBundler\tools"
    $global:projectFile = "${global:solutionRoot}\Blazor.Tools.BlazorBundler\Blazor.Tools.BlazorBundler.csproj"
    $global:packagesOutputFolderPath = "${global:solutionRoot}\packages"
    $global:colors = @("Cyan", "Magenta", "Yellow", "Green")

    # Check the exit code of the msbuild command
    if ($LASTEXITCODE -ne 0) {
        throw "Variable settings 2 failed with exit code $LASTEXITCODE"
    }

    # Set environment variables
    $env:PackageVersion = $global:packageVersion
    $env:Configuration = $global:configuration
    $env:AssemblyVersion = $global:assemblyVersion
    $env:FileVersion = $global:fileVersion
    $env:NugetApiKey = $global:nugetApiKey
    $env:ChangelogPath = $global:changelogPath

    $modulePaths = @(
    "${toolsFolderPath}\Update-EnvironmentVariable.psm1",
    "${toolsFolderPath}\Print-Folder-Structure.psm1",
    "${toolsFolderPath}\Install-Pkgs.psm1",
    "${toolsFolderPath}\Uninstall-Pkgs.psm1",
    "${toolsFolderPath}\Cleanup-Tools.psm1",
    "${toolsFolderPath}\Get-EnvVars.psm1",
    "${toolsFolderPath}\Set-EnvVars.psm1"
)
    # Check the exit code of the msbuild command
    if ($LASTEXITCODE -ne 0) {
        throw "Environment variable settings failed with exit code $LASTEXITCODE"
    }
    
    Import-Modules -ModulePaths $modulePaths

    # Check the exit code of the msbuild command
    if ($LASTEXITCODE -ne 0) {
        throw "Import-Modules failed with exit code $LASTEXITCODE"
    }

    Set-Item -Path "Env:PackageVersion" -Value $global:packageVersion
    Set-Item -Path "Env:Configuration" -Value $global:configuration
    Set-Item -Path "Env:AssemblyVersion" -Value $global:assemblyVersion
    Set-Item -Path "Env:FileVersion" -Value $global:fileVersion
    Set-Item -Path "Env:NugetApiKey" -Value $global:nugetApiKey
    Set-Item -Path "Env:ChangelogPath" -Value $global:changelogPath

    # Check the exit code of the msbuild command
    if ($LASTEXITCODE -ne 0) {
        throw "Set-Item failed with exit code $LASTEXITCODE"
    }

    # Create the .env file
    $EnvFilePath = "$toolsFolderPath\.env"
    $EnvVars = @"
Configuration=$global:configuration
MajorVersion=$global:majorVersion
MinorVersion=$global:minorVersion
PatchVersion=$global:patchVersion
RevisionVersion=$global:revisionVersion
Publish=$Publish
IsRelease=$IsRelease
GitComment=$GitComment
PackageVersion=$global:packageVersion
AssemblyVersion=$global:assemblyVersion
FileVersion=$global:fileVersion
ChangelogPath=$global:changelogPath
"@
    Write-Output "Creating .env file at $EnvFilePath..."
    Set-Content -Path $EnvFilePath -Value $EnvVars

    if ($LASTEXITCODE -ne 0) {
        throw "Set-Content failed with exit code $LASTEXITCODE"
    }

}
<#==========================================================================================================
                        E N D  O F  F U N C T I O N  D E L A C R A T I O N S
===========================================================================================================#>
# Initialize error variable
$LASTEXITCODE=0
Initialize -MajorVersion $MajorVersion -MinorVersion $MinorVersion -PatchVersion $PatchVersion -RevisionVersion $RevisionVersion -Publish $Publish -IsRelease $IsRelease -GitComment $GitComment

Try {

    # Check if the $global:nugetApiKey variable is empty
    if ([string]::IsNullOrWhiteSpace($global:nugetApiKey)) {
        throw "NuGet API Key environment variable is empty. In a terminal, run: `$Env:NugetApiKey = 'YOUR_API_KEY'"
    }
    
    & "${global:solutionRoot}\Scripts\delnugetsources.ps1" -Verbose
    & "${global:solutionRoot}\Scripts\addnugetsources.ps1" -Verbose
    & "${global:solutionRoot}\Scripts\delnugetpackages.ps1" -Verbose
    & "${global:solutionRoot}\Scripts\delbinobj.ps1" -Verbose
    & "${global:solutionRoot}\Scripts\delglobalpackages.ps1" -Verbose

    RestoreNugetPackages -SolutionFile $global:solutionFile    
    BuildSolution -Configuration $global:configuration -PackageVersion $PackageVersion -AssemblyVersion $AssemblyVersion -FileVersion $FileVersion
    PackProject -ProjectFile $global:projectFile -Configuration $global:Configuration -PackageVersion $PackageVersion -ChangeLogPath $global:changeLogPath -PackagesOutputFolderPath $global:packagesOutputFolderPath   
    SaveChangeLogAndReadMe -SolutionRoot $global:solutionRoot -PackageVersion $PackageVersion -AssemblyVersion $AssemblyVersion -FileVersion $FileVersion -ChangeLogPath $global:changelogPath

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
        Write-Host "docker build --build-arg BUILD_CONFIGURATION=$global:configuration -t solomiosisante/blazor-bundler:latest ."
        Write-Host "==========================================================================================="

        # Dockerize
        #docker build -t solomiosisante/blazor-bundler:latest .
        docker build --build-arg BUILD_CONFIGURATION=$global:configuration -t solomiosisante/blazor-bundler:latest .

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

        # Clean Profile first for the imported Tools module
        Cleanup-Tools
        # Check the exit code of the msbuild command
        if ($LASTEXITCODE -ne 0) {
            throw "Cleanup-Tools failed with exit code $LASTEXITCODE"
        }

        Write-Host "==========================================================================================="
        Write-Host "Publishing the Package to nuget.org..."
        Write-Host "==========================================================================================="
        # Publish the package to nuget.org (replace with your publish command)
        #dotnet nuget push packages/Blazor.Tools.BlazorBundler.$global:packageVersion.nupkg --source https://api.nuget.org/v3/index.json --api-key $global:nugetApiKey
        
        $command = "$global:solutionRoot\publish-packages -PackagesPath `"$global:packagesOutputFolderPath`" -NugetApiKey `"$global:nugetApiKey`" -PackageVersion `"$PackageVersion`""
        Start-Process "powershell" -ArgumentList "-Command `"$command`"" -Verb runAs
        
        # Check the exit code of the msbuild command
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed with exit code $LASTEXITCODE"
        }

        # Note: Re-initialize: It is important to re-initialize the user profile for the imported modules to be available globally.
        Initialize -MajorVersion $MajorVersion -MinorVersion $MinorVersion -PatchVersion $PatchVersion -RevisionVersion $RevisionVersion -Publish $Publish -IsRelease $IsRelease -GitComment $GitComment
    }

}
Catch {
    Write-Host "An error occurred: $_"
    throw  # Stops script execution
}

[console]::beep(777,7777)  

