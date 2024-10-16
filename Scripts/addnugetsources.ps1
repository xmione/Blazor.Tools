<#
    C:\repo\Blazor.Tools\Scripts\addnugetsources.ps1
    Hard Note: Adding a backslash at the end of the path would be catastrophic because it will treat it as an escaped quote \"
#>


Write-Host "==========================================================================================="
Write-Host "Adding required nuget sources..."
Write-Host "==========================================================================================="

#dotnet nuget add source "https://api.nuget.org/v3/index.json" -n "Nuget.org"

# You do not need to add this because this is automatic when installing VS. You can't remove it.
#dotnet nuget add source "C:\Program Files (x86)\Microsoft SDKs\NuGetPackages" -n "Microsoft Visual Studio Offline Packages"

dotnet nuget add source "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\packages" -n "BlazorBundlerPackages"
dotnet nuget add source "C:\repo\Blazor.Tools\packages" -n "Blazor.Tools Packages"

dotnet nuget list source
