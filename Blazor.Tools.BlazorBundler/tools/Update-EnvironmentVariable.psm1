
<#

# Import Module:

    Import-Module C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\tools\Update-Envs.psm1

# Remove Module:

    Remove-Module Update-Envs

# For Machine profile: 
    -IsUser $false
    
  For User profile:
    -IsUser $true

# Add an environment variable:   
    Update-EnvironmentVariable -Action Add -Name "Configuration" -Value "Release" -IsUser $false 

# Remove an environment variable:
    
    Update-EnvironmentVariable -Action Remove -Name "MyVariable"

#>

Function Update-EnvironmentVariable {
    param (
        [Parameter(Mandatory = $true)]
        [ValidateSet("Add", "Remove")]
        [string]$Action,

        [Parameter(Mandatory = $true)]
        [string]$Name,

        [string]$Value,

        [bool]$IsUser = $true
    )

    # Path to the user's PowerShell profile script
    $ProfilePath = $PROFILE

    # Ensure the profile script exists
    if (-not (Test-Path -Path $ProfilePath)) {
        Write-Output "Creating new Profile file $ProfilePath..."
        New-Item -ItemType File -Path $ProfilePath -Force
    }

    # Read the profile content
    $ProfileContent = Get-Content -Path $ProfilePath

    # Define the pattern to match the environment variable
    $Pattern = "^\`$env:" + [regex]::Escape($Name) + "\s*="

    if ($Action -eq "Add") {
        # Set the environment variable in the User scope (or use Machine for system-wide)
        if($IsUser){
            [System.Environment]::SetEnvironmentVariable($Name, $Value, [System.EnvironmentVariableTarget]::User)
        } else {
            [System.Environment]::SetEnvironmentVariable($Name, $Value, [System.EnvironmentVariableTarget]::Machine)
        }

        # Remove existing environment variable if it exists in the profile
        $FilteredContent = $ProfileContent | Where-Object { $_ -notmatch $Pattern }

        # Add the new environment variable to the profile
        $NewVariable = "`$env:$Name = `"$Value`""
        $FilteredContent += $NewVariable

        Write-Output "Adding environment variable $Name with value $Value to profile and current session."
    } elseif ($Action -eq "Remove") {
        # Remove the environment variable from the User scope
        if($IsUser){
            [System.Environment]::SetEnvironmentVariable($Name, $null, [System.EnvironmentVariableTarget]::User)
        } else {
            [System.Environment]::SetEnvironmentVariable($Name, $null, [System.EnvironmentVariableTarget]::Machine)
        }

        # Remove the environment variable from the profile
        $FilteredContent = $ProfileContent | Where-Object { $_ -notmatch $Pattern }

        Write-Output "Removing environment variable $Name from profile and current session."
    }

    # Write the updated content back to the profile script
    Set-Content -Path $ProfilePath -Value $FilteredContent

    Write-Output "Finished updating environment variables. A restart may be required to apply changes globally."
}
