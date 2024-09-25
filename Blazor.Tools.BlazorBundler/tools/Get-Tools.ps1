<#
    To run:

    $command = "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\tools\Get-Tools.ps1"
    Start-Process "powershell" -ArgumentList "-NoExit -Command `"$command`"" -Verb runAs
#>

Write-Output "Running Get-Tools module..."

# Ensure execution policy is Unrestricted
#Set-ExecutionPolicy Unrestricted -Scope CurrentUser -Force -ErrorAction SilentlyContinue

$ScriptDir = Split-Path -parent $MyInvocation.MyCommand.Path

# Path to the user's PowerShell profile script
$ProfilePath = $PROFILE

# Remove the profile script if it exists
if (Test-Path -Path $ProfilePath) {
    Write-Output "Removing Profile file $ProfilePath..."
    Remove-Item -Path $ProfilePath -Force
}

# Ensure the profile directory exists
$ProfileDir = [System.IO.Path]::GetDirectoryName($ProfilePath)
if (-not (Test-Path -Path $ProfileDir)) {
    New-Item -ItemType Directory -Path $ProfileDir -Force
}

# Create the profile script if it does not exist
if (-not (Test-Path -Path $ProfilePath)) {
    Write-Output "Creating new Profile file $ProfilePath..."
    New-Item -ItemType File -Path $ProfilePath -Force
}

# Define the module import statements
$ModuleImports = @"
# Import custom modules
Import-Module '$ScriptDir\Update-EnvironmentVariable.psm1'
Import-Module '$ScriptDir\Print-Folder-Structure.psm1'
Import-Module '$ScriptDir\Install-Pkgs.psm1'
Import-Module '$ScriptDir\Uninstall-Pkgs.psm1'
Import-Module '$ScriptDir\Cleanup-Tools.psm1'
Import-Module '$ScriptDir\Get-EnvVars.psm1'
Import-Module '$ScriptDir\Set-EnvVars.psm1'
"@

# Remove existing import statements from the profile script
$ProfileContent = Get-Content -Path $ProfilePath
$Pattern = "Update-EnvironmentVariable.psm1|Print-Folder-Structure.psm1|Install-Pkgs.psm1|Uninstall-Pkgs.psm1|Cleanup-Tools.psm1|Get-EnvVars.psm1|Set-EnvVars.psm1"
$FilteredContent = $ProfileContent | Where-Object { $_ -notmatch $Pattern }

# Write the filtered content back to the profile script
Set-Content -Path $ProfilePath -Value $FilteredContent

# Add the new module import statements to the profile script
Add-Content -Path $ProfilePath -Value $ModuleImports


Write-Output "Finished setting up global module imports. Restart your PowerShell session to use them."
