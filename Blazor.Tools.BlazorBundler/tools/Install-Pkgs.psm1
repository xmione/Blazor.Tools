<#
    File Name       : Install-Pkgs.psm1
    Purpose         : To automate the copying and installation of nuget packages.
    Created By      : Solomio S. Sisante
    Created On      : July 20, 2024
    Sample command  : 
                        $version = "3.0.8"
                        $userProfileName = "solom"
                        $sourcePath = "C:\Users\$userProfileName\.nuget\packages\blazor.tools.blazorbundler\$version"
                        $targetPath = "C:\repo\Blazor.Tools\Blazor.Tools\Blazor.Tools.csproj"
                        Install-Pkgs -SourcePath $sourcePath -TargetProjectPath $targetPath
#>

Function Install-Pkgs
{
    param($SourcePath, $TargetProjectPath)

    Write-Host "=============================================================="
    Write-Host "Running Install.psm1..."
    Write-Host "SourcePath: $SourcePath"
    Write-Host "TargetProjectPath: $TargetProjectPath"

    $packagesPath = "${SourcePath}\packages"
    $targetPath = Split-Path $TargetProjectPath

    Write-Host "packagesPath: $packagesPath"
    Write-Host "targetPath: $targetPath"
    Write-Host "=============================================================="

    function Copy-FileToProject {
        param(
            [string]$Source,
            [string]$Destination
        )

        # Check if the destination file already exists
        if (!(Test-Path -Path $Destination)) {
            # Create the directory structure if it doesn't exist
            $destinationDir = Split-Path -Path $Destination
            if (!(Test-Path -Path $destinationDir)) {
                New-Item -ItemType Directory -Path $destinationDir | Out-Null
            }

            # Copy the file
            Copy-Item -Path $Source -Destination $Destination
            Write-Host "Copied $Source to $Destination"
        } else {
            Write-Host "File $Destination already exists. Skipping."
        }
    }
    <#
    # Specify files to copy
    $filesToCopy = @(
        @{
            Source = Join-Path -Path $SourcePath -ChildPath "wwwroot\bundler\blazor-bootstrap\blazor.bootstrap.css"
            Destination = Join-Path -Path $targetPath -ChildPath "wwwroot\bundler\blazor-bootstrap\blazor.bootstrap.css"
        },
        @{
            Source = Join-Path -Path $SourcePath -ChildPath "wwwroot\bundler\blazor-bootstrap\blazor.bootstrap.js"
            Destination = Join-Path -Path $targetPath -ChildPath "wwwroot\bundler\blazor-bootstrap\blazor.bootstrap.js"
        },
        @{
            Source = Join-Path -Path $SourcePath -ChildPath "wwwroot\bundler\blazored-typeahead\blazored-typeahead.js"
            Destination = Join-Path -Path $targetPath -ChildPath "wwwroot\bundler\blazored-typeahead\blazored-typeahead.js"
        },
        @{
            Source = Join-Path -Path $SourcePath -ChildPath "wwwroot\bundler\bootstrap-icons\font\bootstrap-icons.min.css"
            Destination = Join-Path -Path $targetPath -ChildPath "wwwroot\bundler\bootstrap-icons\font\bootstrap-icons.min.css"
        },
        @{
            Source = Join-Path -Path $SourcePath -ChildPath "wwwroot\bundler\bootstrap-icons\font\fonts\bootstrap-icons.woff"
            Destination = Join-Path -Path $targetPath -ChildPath "wwwroot\bundler\bootstrap-icons\font\fonts\bootstrap-icons.woff"
        },
        @{
            Source = Join-Path -Path $SourcePath -ChildPath "wwwroot\bundler\bootstrap-icons\font\fonts\bootstrap-icons.woff2"
            Destination = Join-Path -Path $targetPath -ChildPath "wwwroot\bundler\bootstrap-icons\font\fonts\bootstrap-icons.woff2"
        },
        @{
            Source = Join-Path -Path $SourcePath -ChildPath "wwwroot\bundler\js\bootstrap.bundle.min.js"
            Destination = Join-Path -Path $targetPath -ChildPath "wwwroot\bundler\js\bootstrap.bundle.min.js"
        },
        @{
            Source = Join-Path -Path $SourcePath -ChildPath "wwwroot\bundler\js\site.js"
            Destination = Join-Path -Path $targetPath -ChildPath "wwwroot\bundler\js\site.js"
        },
        @{
            Source = Join-Path -Path $SourcePath -ChildPath "wwwroot\bundler\css\bundler.css"
            Destination = Join-Path -Path $targetPath -ChildPath "wwwroot\bundler\css\bundler.css"
        },
        @{
            Source = Join-Path -Path $SourcePath -ChildPath "tools\Uninstall.ps1"
            Destination = Join-Path -Path $targetPath -ChildPath "BlazorBundler\Uninstall.ps1"
        },
        @{
            Source = Join-Path -Path $SourcePath -ChildPath "README.md"
            Destination = Join-Path -Path $targetPath -ChildPath "BlazorBundler\README.md"
        }
    )

    # Loop through files and copy each to the targetPath
    foreach ($file in $filesToCopy) {
        Copy-FileToProject -Source $file['Source'] -Destination $file['Destination']
    }
    #> 
    # Specify packages to install
 
    $packagesToInstall = @(
        @{ PackageName = "blazor.bootstrap"; Version = "1.11.1" },
        @{ PackageName = "blazored.typeahead"; Version = "4.7.0" },
        @{ PackageName = "bogus"; Version = "35.6.0" },
        @{ PackageName = "closedxml"; Version = "0.102.3" },
        @{ PackageName = "dapper"; Version = "2.1.35" },
        @{ PackageName = "htmlagilitypack"; Version = "1.11.61" },
        @{ PackageName = "microsoft.aspnetcore.diagnostics.entityframeworkcore"; Version = "8.0.6" },
        @{ PackageName = "microsoft.aspnetcore.identity.entityframeworkcore"; Version = "8.0.6" },
        @{ PackageName = "microsoft.build"; Version = "17.10.4" },
        @{ PackageName = "microsoft.entityframeworkcore.sqlserver"; Version = "8.0.6" },
        @{ PackageName = "microsoft.entityframeworkcore.tools"; Version = "8.0.6" },
        @{ PackageName = "microsoft.ml"; Version = "3.0.1" },
        @{ PackageName = "microsoft.visualstudio.azure.containers.tools.targets"; Version = "1.21.0" },
        @{ PackageName = "newtonsoft.json"; Version = "13.0.3" },
        @{ PackageName = "system.configuration.configurationmanager"; Version = "8.0.0" },
        @{ PackageName = "system.data.oledb"; Version = "8.0.0" },
        @{ PackageName = "system.diagnostics.performancecounter"; Version = "8.0.0" }
    )
 
    # Install NuGet packages
    function Install-NuGetPackage {
        param(
            [string]$TargetProjectPath,
            [string]$PackageName,
            [string]$Version,
            [string]$PackagesPath
        )

        # dotnet add "C:\repo\Blazor.Tools\Blazor.Tools" package blazor.bootstrap --version 1.11.1 --source "C:\Users\solom\.nuget\packages\blazor.tools.blazorbundler\3.0.5\packages"
        Write-Host "dotnet add $targetPath package $PackageName --version $Version --source $PackagesPath"
        dotnet add $targetPath package $PackageName --version $Version --source $PackagesPath
    }

    # Loop through files and copy each to the targetPath

    foreach ($package in $packagesToInstall) {
        # Install Blazor.Bootstrap package
        Install-NuGetPackage -$TargetProjectPath $TargetProjectPath -PackageName $package['PackageName'] -Version $package['Version'] -PackagesPath $packagesPath
    }

    Write-Host "Completed copying files and installing packages"
}

Export-ModuleMember -Function Install-Pkgs