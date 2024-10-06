/*============================================================================================
    Created By: Solomio S. Sisante
    Created On: September 15, 2024
    Objective : To Test TypeCreator class
    File Name : TestTypeCreator.csx
    How to Use: In the C# Interactive window, run this command: 
                
                #load "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\Scripts\TestTypeCreator.csx"

  ============================================================================================*/

#r "System"
#r "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\9.0.0-rc.1.24431.7\System.Console.dll"  
#r "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\9.0.0-rc.1.24431.7\System.Runtime.dll"
#r "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\9.0.0-rc.1.24431.7\System.Reflection.Emit.dll"  
#r "C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler.Utilities\bin\Debug\net9.0\Blazor.Tools.BlazorBundler.Utilities.dll"

using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Reflection.Emit;
using Blazor.Tools.BlazorBundler.Utilities.Assemblies;

Console.OutputEncoding = System.Text.Encoding.Unicode;
Console.InputEncoding = System.Text.Encoding.Unicode;
Console.WriteLine("Starting the Test...");
TypeCreator.Test();
Console.WriteLine("The test has completed.");
