<#
    C:\repo\Blazor.Tools\Scripts\delnugetsources.ps1
#>
dotnet nuget remove source "Nuget.org"
dotnet nuget remove source "Microsoft Visual Studio Offline Packages"
dotnet nuget remove source "GlobalNugetPackages"
dotnet nuget remove source "Blazor.Tools Packages"
dotnet nuget remove source "BlazorBundler"
dotnet nuget remove source "BlazorBundlerPackages"
dotnet nuget remove source "MyPackage"

dotnet nuget list source

