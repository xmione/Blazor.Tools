param (
    [string]$PackagePath,
    [string]$DestinationPath = "$env:ProgramFiles\NuGet\Packages"
)

try {
    # Ensure PackageManagement and NuGet provider are available
    if (-not (Get-Module -ListAvailable -Name PackageManagement)) {
        Write-Host "Installing PackageManagement module..."
        Install-Module -Name PackageManagement -Force -SkipPublisherCheck
    }

    # This is not needed for local nuget packages
    <#
        if (-not (Get-PackageSource -Name 'Nuget.org')) {
            Write-Host "Registering Nuget.org package source..."
            Register-PackageSource -Name Nuget.org -ProviderName NuGet -Location "https://api.nuget.org/v3/index.json"
        }
    #>

    # Ensure the destination directory exists
    if (-not (Test-Path -Path $DestinationPath)) {
        Write-Host "Creating destination directory: $DestinationPath"
        New-Item -ItemType Directory -Path $DestinationPath -Force
    }

    cls
    Write-Host "Starting installation of package from $PackagePath to $DestinationPath"

    # Install the package
    Install-Package -Source $PackagePath -DestinationPath $DestinationPath -ProviderName NuGet -Force
    
    Write-Host "Completed the installation of package: $PackagePath"
} catch {
    Write-Error "An error occurred during the package installation: $_"
    exit 1
}
