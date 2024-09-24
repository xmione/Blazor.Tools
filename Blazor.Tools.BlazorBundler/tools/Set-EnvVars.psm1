<#
    To test run:
                Import-Module C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\tools\Set-EnvVars.psm1
                Set-EnvVars -MajorVersion 3 -MinorVersion 1 -PatchVersion 8 -RevisionVersion 0 -NugetApiKey "YOUR_API_Key" -Publish $false -IsRelease $false -GitComment "Updated project with the latest changes"

    To remove:
                Remove-Module -Name Set-EnvVars

    To check if it exists:
                Get-Module -Name "Set-EnvVars"
#>

Function Set-EnvVars {
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
    [string] $NugetApiKey,
    [Parameter(Mandatory=$true)]
    [bool] $Publish,
    [bool] $IsRelease = $false,
    [string] $GitComment = "Updated project with the latest changes"
)

    $solutionRoot = Get-Location
    $packageVersion = "${MajorVersion}.${MinorVersion}.${PatchVersion}"
    $assemblyVersion = "$packageVersion.$RevisionVersion"
    $fileVersion = "$packageVersion.$RevisionVersion"
    $changelogPath = "${solutionRoot}\Blazor.Tools.BlazorBundler\changelog_${packageVersion}.md"

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

    Set-Item -Path "Env:Configuration" -Value $Configuration
    Set-Item -Path "Env:MajorVersion" -Value $MajorVersion
    Set-Item -Path "Env:MinorVersion" -Value $MinorVersion
    Set-Item -Path "Env:PatchVersion" -Value $PatchVersion
    Set-Item -Path "Env:RevisionVersion" -Value $RevisionVersion
    Set-Item -Path "Env:NugetApiKey" -Value $NugetApiKey
    Set-Item -Path "Env:Publish" -Value $Publish
    Set-Item -Path "Env:IsRelease" -Value $IsRelease
    Set-Item -Path "Env:GitComment" -Value $GitComment

    Set-Item -Path "Env:AssemblyVersion" -Value $assemblyVersion
    Set-Item -Path "Env:FileVersion" -Value $fileVersion
    Set-Item -Path "Env:ChangelogPath" -Value $changelogPath
     
}

Export-ModuleMember -Function Set-EnvVars
