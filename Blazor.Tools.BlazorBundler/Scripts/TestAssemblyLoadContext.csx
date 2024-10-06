/*============================================================================================
    Created By: Solomio S. Sisante
    Created On: September 14, 2024
    Objective : To Test AssemblyLoadContext class
    File Name : TestAssemblyLoadContext.csx
    How to Use: In the C# Interactive window, run this command: 
                
                #load "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\Scripts\TestAssemblyLoadContext.csx"
  ============================================================================================*/

#r "System.Runtime.Loader" // Reference the necessary assembly
#r "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler.Extensions\bin\Debug\net8.0\Blazor.Tools.BlazorBundler.Extensions.dll"
using System;
using System.Reflection;
using System.Runtime.Loader;
using System.IO;
using System.Runtime.InteropServices;
using Blazor.Tools.BlazorBundler.Extensions;

string assemblyFilePath = Path.Combine(Path.GetTempPath(), "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models.dll"); // Provide the path to the assembly file
var assemblyLoadContext = new AssemblyLoadContext(null, true);  // Set isCollectible to true to enable unloading
var assembly = assemblyLoadContext.LoadFromAssemblyPath(assemblyFilePath ?? default!);

assemblyFilePath.IsFileInUse();

assemblyLoadContext.Unload();
assemblyFilePath.IsFileInUse();

assembly = null;
assemblyFilePath.IsFileInUse();

assemblyLoadContext = null;
assemblyFilePath.IsFileInUse();

Console.WriteLine("Force garbage collection to clean up");
// Force garbage collection to clean up
GC.Collect();
GC.WaitForPendingFinalizers();

// Check if the file is still in use
assemblyFilePath.IsFileInUse();

//// Define a custom AssemblyLoadContext
//public class DynamicAssemblyLoader : AssemblyLoadContext
//{
//    public DynamicAssemblyLoader() : base(true) { }

//    public Assembly LoadAssembly(string assemblyFilePath)
//    {
//        return LoadFromAssemblyPath(assemblyFilePath);
//    }
//}

//// Function to dynamically load the assembly and invoke a method
//public void LoadAndExecuteAssembly(string assemblyPath)
//{
//    var loader = new DynamicAssemblyLoader();
//    Assembly assembly = loader.LoadAssembly(assemblyPath);

//    Console.WriteLine($"Assembly loaded: {assembly.FullName}");

//    // Optionally, unload the context to release the assembly
//    loader.Unload();
//}

//// Test the functionality
//string assemblyFilePath = Path.Combine(Path.GetTempPath(), "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models.dll"); // Provide the path to the assembly file
//LoadAndExecuteAssembly(assemblyFilePath);

