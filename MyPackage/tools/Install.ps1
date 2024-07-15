param($installPath, $toolsPath, $package, $project)

# Example: Create a new folder in the consuming project
$targetFolder = "$project\contentFiles"
if (!(Test-Path -Path $targetFolder)) {
    New-Item -ItemType Directory -Path $targetFolder
}

# Example: Write a new file in the consuming project
$targetFile = "$project\contentFiles\test.js"
Add-Content -Path $targetFile -Value "This is an example file created by the NuGet package."
