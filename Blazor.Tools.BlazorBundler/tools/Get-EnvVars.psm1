<#
    To test run:
                Import-Module C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\tools\Get-EnvVars.psm1
                Get-EnvVars

    To remove:
                Remove-Module -Name Get-EnvVars

    To check if it exists:
                Get-Module -Name "Get-EnvVars"
#>

Function Get-EnvVars {
    Write-Output "Getting Environment Variables..."
    Write-Output "Configuration: ${env:Configuration}"
    Write-Output "PackageVersion: ${env:PackageVersion}"
    Write-Output "AssemblyVersion: ${env:AssemblyVersion}"
    Write-Output "FileVersion: ${env:FileVersion}"
    Write-Output "NugetApiKey: ${env:NugetApiKey}"
    Write-Output "ChangelogPath: ${env:ChangelogPath}"
     
}

Export-ModuleMember -Function Get-EnvVars
