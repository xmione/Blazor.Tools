param (
    [string]$PackagePath,
    [string]$DestinationPath = "$env:ProgramFiles\NuGet\Packages"
)

# Ensure PackageManagement and NuGet provider are available
if (-not (Get-Module -ListAvailable -Name PackageManagement)) {
    Install-Module -Name PackageManagement -Force -SkipPublisherCheck
}

if (-not (Get-PackageSource -Name 'Nuget.org')) {
    Register-PackageSource -Name Nuget.org -ProviderName NuGet -Location "https://api.nuget.org/v3/index.json"
}

# Ensure the destination directory exists
if (-not (Test-Path -Path $DestinationPath)) {
    New-Item -ItemType Directory -Path $DestinationPath -Force
}

# Install the package
Install-Package -Source $PackagePath -DestinationPath $DestinationPath -ProviderName NuGet -Force
