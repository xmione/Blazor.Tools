Function Cleanup-Tools {
    Write-Output "Running Cleanup-Tools module..."

    # Path to the user's PowerShell profile script
    $ProfilePath = $PROFILE

    # Check if the profile script exists
    if (Test-Path -Path $ProfilePath) {
        Write-Output "Cleaning up Profile file $ProfilePath..."

        # Read the profile content
        $ProfileContent = Get-Content -Path $ProfilePath

        # Define the pattern to match the import statements
        $Pattern = "Update-EnvironmentVariable.psm1|Print-Folder-Structure.psm1|Install-Pkgs.psm1|Uninstall-Pkgs.psm1|Cleanup-Tools.psm1|Get-EnvVars.psm1|Set-EnvVars.psm1"

        # Filter out the import statements
        $FilteredContent = $ProfileContent | Where-Object { $_ -notmatch $Pattern }

        # Write the filtered content back to the profile script
        Set-Content -Path $ProfilePath -Value $FilteredContent

        Write-Output "Finished cleaning up global module imports. Restart your PowerShell session to apply changes."
    } else {
        Write-Output "Profile file $ProfilePath does not exist. No cleanup needed."
    }
}

Export-ModuleMember -Function Cleanup-Tools
