<#
    C:\repo\Blazor.Tools\Scripts\delnugetsources.ps1
#>

Write-Host "==========================================================================================="
Write-Host "Deleting nuget sources..."
Write-Host "==========================================================================================="

dotnet nuget remove source "Nuget.org"

#Note: You can't remove this global nuget package source because it is automatically installed by VS.
#dotnet nuget remove source "Microsoft Visual Studio Offline Packages"

dotnet nuget remove source "GlobalNugetPackages"
dotnet nuget remove source "Blazor.Tools Packages"
dotnet nuget remove source "BlazorBundler"
dotnet nuget remove source "BlazorBundlerPackages"
dotnet nuget remove source "MyPackage"

dotnet nuget list source

