<#
    To test run:
                Import-Module C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\tools\Update-Envs.psm1
                Update-Envs -Configuration "Release" -MajorVersion 3 -MinorVersion 1 -PatchVersion 20 -RevisionVersion 0 -NugetApiKey $env:NugetApiKey -Publish $false -IsRelease $false -GitComment $GitComment -IsUser $false

    To remove:
                Remove-Module -Name Update-Envs

    To check if it exists:
                Get-Module -Name "Update-Envs"
#>

Function Update-Envs {
param(
    [Parameter(Mandatory=$true)]
    [string] $Configuration,
    [Parameter(Mandatory=$true)]
    [string] $MajorVersion,
    [Parameter(Mandatory=$true)]
    [string] $MinorVersion,
    [Parameter(Mandatory=$true)]
    [string] $PatchVersion,
    [Parameter(Mandatory=$true)]
    [string] $RevisionVersion,
    [Parameter(Mandatory=$true)]
    [string] $NugetApiKey,
    [Parameter(Mandatory=$true)]
    [bool] $Publish,
    [bool] $IsRelease = $false,
    [string] $GitComment = "Updated project with the latest changes",
    [bool] $IsUser = $true
)

    $solutionRoot = Get-Location
    $packageVersion = "${MajorVersion}.${MinorVersion}.${PatchVersion}"
    $assemblyVersion = "$packageVersion.$RevisionVersion"
    $fileVersion = "$packageVersion.$RevisionVersion"
    $changelogPath = "${solutionRoot}\Blazor.Tools.BlazorBundler\changelog_${packageVersion}.md"

    Write-Host "Configuration: $Configuration"
    Write-Host "MajorVersion: $MajorVersion"
    Write-Host "MinorVersion: $MinorVersion"
    Write-Host "PatchVersion: $PatchVersion"
    Write-Host "RevisionVersion: $RevisionVersion"
    Write-Host "NugetApiKey: $NugetApiKey"
    Write-Host "Publish: $Publish"
    Write-Host "IsRelease: $IsRelease"
    Write-Host "GitComment: $GitComment"

    Write-Host "packageVersion: $packageVersion"
    Write-Host "assemblyVersion: $assemblyVersion"
    Write-Host "fileVersion: $fileVersion"
    Write-Host "changelogPath: $changelogPath"

    Update-EnvironmentVariable -Action Add -Name "Configuration" -Value $Configuration -IsUser $IsUser
    Update-EnvironmentVariable -Action Add -Name "MajorVersion" -Value $MajorVersion -IsUser $IsUser
    Update-EnvironmentVariable -Action Add -Name "MinorVersion" -Value $MinorVersion -IsUser $IsUser
    Update-EnvironmentVariable -Action Add -Name "PatchVersion" -Value $PatchVersion -IsUser $IsUser
    Update-EnvironmentVariable -Action Add -Name "RevisionVersion" -Value $RevisionVersion -IsUser $IsUser
    Update-EnvironmentVariable -Action Add -Name "NugetApiKey" -Value $NugetApiKey -IsUser $IsUser
    Update-EnvironmentVariable -Action Add -Name "Publish" -Value $Publish -IsUser $IsUser
    Update-EnvironmentVariable -Action Add -Name "IsRelease" -Value $IsRelease -IsUser $IsUser
    Update-EnvironmentVariable -Action Add -Name "GitComment" -Value $GitComment -IsUser $IsUser
    Update-EnvironmentVariable -Action Add -Name "PackageVersion" -Value $packageVersion -IsUser $IsUser

    Update-EnvironmentVariable -Action Add -Name "AssemblyVersion" -Value $assemblyVersion -IsUser $IsUser
    Update-EnvironmentVariable -Action Add -Name "FileVersion" -Value $fileVersion -IsUser $IsUser
    Update-EnvironmentVariable -Action Add -Name "ChangelogPath" -Value $changelogPath -IsUser $IsUser
     
    if(-not $IsUser){
        Restart-Computer -Force
    }

}

Export-ModuleMember -Function Update-Envs
