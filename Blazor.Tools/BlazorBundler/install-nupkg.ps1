param (
    [string]$PackagePaths,
    [string]$SourcePath,
    [string]$SourceName = "BlazorBundlerPackages",
    [string]$DestinationPath = "$env:ProgramFiles\NuGet\Packages",
    [string]$TargetProjectPath
)

try {
    # Convert the semicolon-separated paths into an array
    $PackagePathsArray = $PackagePaths -split ';'

    Write-Host "PackagePathsArray: $PackagePathsArray"
    # Ensure the destination directory exists
    if (-not (Test-Path -Path $DestinationPath)) {
        New-Item -ItemType Directory -Path $DestinationPath -Force
    }

    Write-Host "SourcePath: $SourcePath"
    # Check if the NuGet source already exists
    #$existingSource = nuget sources list | ForEach-Object { $_.Trim() } | Where-Object { $_ -eq $SourcePath }

    #if (-not $existingSource) {
    #    Write-Host "Adding NuGet source $SourceName..."
    #    nuget sources add -Name $SourceName -Source $SourcePath
    #} else {
    #    Write-Host "NuGet source $SourceName already exists."
    #}

    foreach ($packagePath in $PackagePathsArray) {
        Write-Host "PackagePath: $packagePath"

        # Extract the package name and version from the package path using regex
        $packageFileName = [System.IO.Path]::GetFileNameWithoutExtension($packagePath)
         if ($packageFileName -match '^(.*?)(?:\.|$)(\d+\.\d+\.\d+.*)$') {
            $packageName = $matches[1]
            $packageVersion = $matches[2]
        } else {
            Write-Error "Invalid package file name format: $packageFileName"
            continue
        }

        # Install the package using dotnet CLI
        # dotnet add "C:\repo\Blazor.Tools\Blazor.Tools" package blazor.bootstrap --version 1.11.1 --source "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\packages"
        # dotnet add "C:\repo\Blazor.Tools\Blazor.Tools" package $PackagePath --version $Version --source $SourcePath
        Write-Host "Installing package $packageName version $packageVersion from source $SourcePath..."
        dotnet add $TargetProjectPath package $packageName --version $packageVersion --source $SourcePath
    }

     # Pause after each package installation
     # Read-Host "Press Enter to continue to the next package"

    <# No need to do this but I'm gonna keep this code for future use
        # Remove the NuGet source only if it was added by this script
        if (-not $existingSource) {
            Write-Host "Removing NuGet source $SourceName..."
            nuget sources remove -Name $SourceName
        }
    #>

    Write-Host "Completed the installation of $SourcePath package"
} catch {
    Write-Error "An error occurred during the package installation: $_"
    exit 1
}