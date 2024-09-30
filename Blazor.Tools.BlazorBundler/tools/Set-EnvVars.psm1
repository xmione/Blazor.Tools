<#
    To test run:
                Import-Module C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\tools\Set-EnvVars.psm1
                Set-EnvVars -Configuration "Release" -MajorVersion 3 -MinorVersion 1 -PatchVersion 20 -RevisionVersion 0 -NugetApiKey $env:NugetApiKey -Publish $false -IsRelease $false -GitComment "- Updated the following packages:
	ICSharpCode.Decompiler version 9.0.0.7660-preview2
	Microsoft.AspNetCore.Components version 9.0.0-preview.7.24406.2
	Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore version 9.0.0-rc.1.24452.1
	Microsoft.AspNetCore.Identity.EntityFrameworkCore version 9.0.0-rc.1.24452.1

- Updated Install-Pkgs.psm1 that can be used to manually install packages."

    To remove:
                Remove-Module -Name Set-EnvVars

    To check if it exists:
                Get-Module -Name "Set-EnvVars"
#>

Function Set-EnvVars {
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

    Set-Item -Path env:Configuration -Value $Configuration
    Set-Item -Path env:MajorVersion -Value $MajorVersion
    Set-Item -Path env:MinorVersion -Value $MinorVersion
    Set-Item -Path env:PatchVersion -Value $PatchVersion
    Set-Item -Path env:RevisionVersion -Value $RevisionVersion
    Set-Item -Path env:NugetApiKey -Value $NugetApiKey
    Set-Item -Path env:Publish -Value $Publish
    Set-Item -Path env:IsRelease -Value $IsRelease
    Set-Item -Path env:GitComment -Value $GitComment
    Set-Item -Path env:PackageVersion -Value $packageVersion

    Set-Item -Path env:AssemblyVersion -Value $assemblyVersion
    Set-Item -Path env:FileVersion -Value $fileVersion
    Set-Item -Path env:ChangelogPath -Value $changelogPath
     
}

Export-ModuleMember -Function Set-EnvVars
