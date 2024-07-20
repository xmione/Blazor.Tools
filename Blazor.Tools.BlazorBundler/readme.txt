# BlazorBundler

BlazorBundler is a utility tool designed to simplify the process of bundling multiple packages, particularly for Blazor applications. This tool allows you to download and bundle essential files and dependencies, such as Bootstrap and Bootstrap Icons, to enhance your Blazor projects.

## Version Information
- **Package Version**: 3.0.7
- **Assembly Version**: 3.0.7.0
- **File Version**: 3.0.7.0

## Features

- **Automated Package Bundling**: Easily download and bundle multiple packages with a single command.
- **Bootstrap Integration**: Seamlessly include Bootstrap and Bootstrap Icons in your Blazor projects.
- **Asynchronous Downloads**: Efficiently download multiple files simultaneously for faster setup.
- Bundle:
    - Blazor.Bootstrap version 1.11.1
    - Blazored.Typeahead version 4.7.0
    - Bogus version 35.6.0
    - Bootstrap Icons version 1.11.3
    - Dapper version 2.1.35
    - HtmlAgilityPack version 1.11.61
    - Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore version 8.0.6
    - Microsoft.AspNetCore.Identity.EntityFrameworkCore version 8.0.6
    - Microsoft.Build version 17.10.4
    - Microsoft.EntityFrameworkCore.SqlServer version 8.0.6
    - Microsoft.EntityFrameworkCore.Tools version 8.0.6
    - Microsoft.ML version 3.0.1
    - Microsoft.VisualStudio.Azure.Containers.Tools.Targets version 1.21.0
    - Newtonsoft.Json version 13.0.3    

## Installation

You can install BlazorBundler via NuGet Package Manager, Package Manager Console or from a terminal:

### Nuget Package Manager
- Search for Blazor.Tools.BlazorBundler in nuget.org.


`
dotnet add package Blazor.Tools.BlazorBundler
`

### Package Manager Console
`
Install-Package Blazor.Tools.BlazorBundler
`

### Terminal console
`
dotnet add package Blazor.Tools.BlazorBundler
`

## Install Pre-requesites
Note: After installing the package, you have to manually run the Install.ps1 file to install required nuget packages.

In the BlazorBundler folder, run: 
    .\Install.ps1 -SourcePath "C:\Users\{user}\.nuget\packages\blazor.tools.blazorbundler\{version}" -TargetProjectPath "{TargetProjectPath}"
    .\Install.ps1 -SourcePath "C:\Users\solom\.nuget\packages\blazor.tools.blazorbundler\3.0.5" -TargetProjectPath "C:\repo\Blazor.Tools\Blazor.Tools\Blazor.Tools.csproj"

## Setup your App.razor stylesheets and javascripts

Add these to your <head> section:
`
    <link rel="stylesheet" href="bootstrap/bootstrap.min.css" />
    <link rel="stylesheet" href="bootstrap-icons/font/bootstrap-icons.min.css" />
    <link rel="stylesheet" href="blazor-bootstrap/blazor.bootstrap.css" />

    <script src="blazored-typeahead/blazored-typeahead.js"></script>
    <script src="js/site.js"></script>
`

Add these to your <body> section:
`
    <script src="js/bootstrap.bundle.min.js"></script>
    <script src="blazor-bootstrap/blazor.bootstrap.js"></script>
`
    
## Uninstallation

First, uninstall the package from the Nuget Package Manager, Package Manager Console or from a terminal.

### Nuget Package Manager 
- Search Blazor.Tools.BlazorBundler and uninstall it.

### Package Manager Console
`
Uninstall-Package Blazor.Tools.BlazorBundler
`

### Terminal console
`
dotnet remove package Blazor.Tools.BlazorBundler
`

There is an Uninstall.ps1 file you can run from the /BlazorBundler folder.


## Change Logs
- [changelog_3.0.7.md](https://github.com/xmione/Blazor.Tools/blob/master/Blazor.Tools.BlazorBundler/changelog_3.0.7.md)

