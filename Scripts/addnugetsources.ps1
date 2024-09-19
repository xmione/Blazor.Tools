<#
    addnugetsources.ps1
#>
dotnet nuget add source "https://api.nuget.org/v3/index.json" -n "Nuget.org"
#dotnet nuget add source "C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\" -n "Microsoft Visual Studio Offline Packages"
# dotnet nuget add source "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\bin\Debug" -n "BlazorBundler"
dotnet nuget add source "C:\repo\Blazor.Tools\packages" -n "Blazor.Tools Packages"

dotnet build
dotnet nuget list source
