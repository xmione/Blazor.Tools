<#
    FileName    : publish-packages.ps1
    Author      : Solomio S. Sisante
    Purpose     : To publish packages in packages folder to nuget.
    Date Created: September 24, 2024
    To run      :
                    Start-Process "powershell" -ArgumentList "-NoExit -Command `"C:\repo\Blazor.Tools\publish-packages -PackagesPath C:\repo\Blazor.Tools\Packages $Env:MY_NUGET_API_KEY -PackageVersion '3.1.2'`"" -Verb runAs
                    
                    or

                    C:\repo\Blazor.Tools\publish-packages -PackagesPath "C:\repo\Blazor.Tools\Packages" -NugetApiKey $Env:MY_NUGET_API_KEY -PackageVersion "3.1.2"
#>
param(
    [Parameter(Mandatory=$true)]
    [string] $PackagesPath,
    [Parameter(Mandatory=$true)]
    [string] $NugetApiKey,
    [Parameter(Mandatory=$true)]
    [string] $PackageVersion
)

Write-Host "PackagesPath: ${PackagesPath}"
Write-Host "NugetApiKey: ${NugetApiKey}"
Write-Host "PackageVersion: ${PackageVersion}"

Get-ChildItem -Path $PackagesPath -Filter "*${PackageVersion}.nupkg" | ForEach-Object {
    Write-Host "Publishing $PackagesPath..."
    dotnet nuget push $_.FullName --source "https://api.nuget.org/v3/index.json" --api-key $NugetApiKey
}