# BlazorBundler

BlazorBundler is a utility tool designed to simplify the process of bundling multiple packages, particularly for Blazor applications. This tool allows you to download and bundle essential files and dependencies, such as Bootstrap and Bootstrap Icons, to enhance your Blazor projects.

## Version Information
- **Package Version**: 3.0.4
- **Assembly Version**: 3.0.4.0
- **File Version**: 3.0.4.0

## Features

- **Automated Package Bundling**: Easily download and bundle multiple packages with a single command.
- **Bootstrap Integration**: Seamlessly include Bootstrap and Bootstrap Icons in your Blazor projects.
- **Asynchronous Downloads**: Efficiently download multiple files simultaneously for faster setup.
- Bundle:
    - Blazor.Bootstrap version 1.11.1
    - Blazored.Typeahead version 4.7.0
    - Bootstrap Icons version 1.11.3

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
- [changelog_3.0.2.md](https://github.com/xmione/Blazor.Tools/blob/master/Blazor.Tools.BlazorBundler/changelog_3.0.2.md)
- [changelog_3.0.3.md](https://github.com/xmione/Blazor.Tools/blob/master/Blazor.Tools.BlazorBundler/changelog_3.0.3.md)
- [changelog_3.0.4.md](https://github.com/xmione/Blazor.Tools/blob/master/Blazor.Tools.BlazorBundler/changelog_3.0.4.md)

