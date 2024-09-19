# BlazorBundler

BlazorBundler is a utility tool designed to simplify the process of bundling multiple packages, particularly for Blazor applications. This tool allows you to download and bundle essential files and dependencies, such as Bootstrap and Bootstrap Icons, to enhance your Blazor projects.

## Version Information
- **Package Version**: 3.1.2
- **Assembly Version**: 3.1.2.0
- **File Version**: 3.1.2.0

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
- Search for Blazor.Tools.BlazorBundler in nuget.org and click Install.

### Package Manager Console
`
Install-Package Blazor.Tools.BlazorBundler
`

### Terminal console
`
dotnet add package Blazor.Tools.BlazorBundler
`

## After-Install Setup Commands

Note: After installing the package, you have to manually run the Install-Pkgs module file to install required nuget packages.
      First replace the values of the $userProfileName and $targetPath variables.
      
      $userProfileName should contain your Windows UserProfile Name
      $sourcePath should not be changed
      $targetPath should contain the full path of your project file

### Open PowerShell and run: 

```
    $version = "3.1.2"
    $userProfileName = "solom"
    $sourcePath = "C:\Users\$userProfileName\.nuget\packages\blazor.tools.blazorbundler\$version"
    $targetPath = "C:\repo\Blazor.Tools\Blazor.Tools\Blazor.Tools.csproj"
    Install-Pkgs -SourcePath $sourcePath -TargetProjectPath $targetPath
```
### Setup your App.razor stylesheets and javascripts

Add these to your <head> section:

```
    <link rel="stylesheet" href="bootstrap/bootstrap.min.css" />
    <link rel="stylesheet" href="bundler/bootstrap-icons/font/bootstrap-icons.min.css" />
    <link rel="stylesheet" href="bundler/blazor-bootstrap/blazor.bootstrap.css" />
    <link rel="stylesheet" href="bundler/css/bundler.css" />
    
    <!-- This is the <ASSEMBLY_NAME>.styles.css file-->
    <link rel="stylesheet" href="Blazor.Tools.styles.css" /> 

    <script src="bundler/blazored-typeahead/blazored-typeahead.js"></script>
    <script src="bundler/js/site.js"></script>
    
    <!-- It needs to be in InteractiveServer mode to work -->
    <HeadOutlet @rendermode="@InteractiveServer" />
```

Add these to your <body> section:
```
    <!-- Once again, this is needed -->
    <Routes @rendermode="@InteractiveServer" />
    
    <!-- The script initializes the Blazor runtime, which is essential for a Blazor application to run -->
    <script src="_framework/blazor.web.js"></script>

    <!-- Use local Bootstrap JS -->
    <script src="bundler/js/bootstrap.bundle.min.js"></script>
    <script src="bundler/blazor-bootstrap/blazor.bootstrap.js"></script>
```

### Add this to your Program.cs file:
`
builder.Services.AddBlazorBootstrap();
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

## Clean-up
Note: After uninstalling the package, you have to manually run the Uninstall module file to uninstall the packages.
      First replace the values of the $userProfileName and $targetPath variables.

      $projectPath should contain the path to your project folder
      $projectName should contain the name of your project

Open PowerShell and run:

```
    $projectPath = "C:\repo\Blazor.Tools\Blazor.Tools\"
    $projectName = "Blazor.Tools.csproj"
    Uninstall-Pkgs -ProjectPath  $projectPath -ProjectName $projectName
```

## Change Logs
- [changelog_3.1.0.md](https://github.com/xmione/Blazor.Tools/blob/master/Blazor.Tools.BlazorBundler/changelog_3.1.0.md)
- [changelog_3.1.1.md](https://github.com/xmione/Blazor.Tools/blob/master/Blazor.Tools.BlazorBundler/changelog_3.1.1.md)
- [changelog_3.1.2.md](https://github.com/xmione/Blazor.Tools/blob/master/Blazor.Tools.BlazorBundler/changelog_3.1.2.md)

