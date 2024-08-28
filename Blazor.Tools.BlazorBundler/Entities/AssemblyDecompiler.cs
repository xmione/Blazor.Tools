/*====================================================================================================
    Class Name  : AssemblyDecompiler
    Created By  : Solomio S. Sisante
    Created On  : August 28, 2024
    Purpose     : To manage decompiling of DLL classes.
  ====================================================================================================*/
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using System.Text;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class AssemblyDecompiler
    {
        private readonly string _assemblyPath;

        public AssemblyDecompiler(string assemblyPath)
        {
            _assemblyPath = assemblyPath;
        }

        public string DecompileType(string typeName)
        {
            var decompiledCode = string.Empty;
            using (var stream = new FileStream(_assemblyPath, FileMode.Open, FileAccess.Read))
            {
                var module = new PEFile("Assembly", stream);

                // Use a no-op assembly resolver
                var assemblyResolver = new AssemblyResolver();
                var typeSystem = new DecompilerTypeSystem(module, assemblyResolver);

                var decompilerSettings = new DecompilerSettings
                {
                    ShowDebugInfo = false,
                    ShowXmlDocumentation = false,
                    UseDebugSymbols = false,
                };

                var decompiler = new CSharpDecompiler(typeSystem, decompilerSettings);


                // Find the type to decompile
                var type = typeSystem.MainModule.TypeDefinitions
                    .FirstOrDefault(t => t.FullName == typeName);

                if (type == null)
                    throw new ArgumentException($"Type {typeName} not found in assembly.");

                decompiledCode = decompiler.DecompileTypeAsString(type.FullTypeName);
            }
            
            return decompiledCode;
        }

        public string DecompileMethod(string typeName, string methodName)
        {
            var decompiledMethod = string.Empty;

            // Decompile the type and clean up the code
            var decompiledCode = DecompileType(typeName);
            decompiledCode = CleanUpDecompiledCode(decompiledCode);

            // Extract the specific method from the decompiled code
            decompiledMethod = GetMethodCode(methodName, decompiledCode);

            if (string.IsNullOrEmpty(decompiledMethod))
                throw new ArgumentException($"Method {methodName} not found in type {typeName}.");

            return decompiledMethod;
        }

        // Helper method to extract the full method code
        private string GetMethodCode(string methodName, string contents)
        {
            // Split the content into lines for easier processing
            var lines = contents.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder methodCode = new StringBuilder();
            bool methodFound = false;
            int braceCount = 0;

            foreach (var line in lines)
            {
                // Check if the line contains the method name and hasn't started processing yet
                if (!methodFound && line.Contains(methodName) && IsMethodSignature(line, methodName))
                {
                    methodFound = true;
                }

                // Once the method is found, start adding lines to the methodCode
                if (methodFound)
                {
                    methodCode.AppendLine(line);

                    // Count the number of opening and closing braces
                    braceCount += line.Count(c => c == '{');
                    braceCount -= line.Count(c => c == '}');

                    // If we encounter an opening brace on a new line after the method name
                    if (braceCount == 0 && line.Contains("{"))
                    {
                        braceCount++;
                    }

                    // When braceCount reaches 0 after processing the method's body, we're done
                    if (braceCount == 0 && line.Contains("}"))
                    {
                        break;
                    }
                }
            }

            // If the method wasn't found, return an empty string
            if (!methodFound)
            {
                return string.Empty;
            }

            return methodCode.ToString();
        }

        // Helper method to determine if a line represents a method signature
        private bool IsMethodSignature(string line, string methodName)
        {
            // A basic check for a method signature: line should contain access modifiers and method name,
            // followed by parentheses indicating method parameters
            var methodSignaturePattern = $@"\b(public|private|protected|internal|static|async|virtual|override|sealed)?\s*[\w<>\[\]]+\s+{methodName}\s*\(.*\)";
            return System.Text.RegularExpressions.Regex.IsMatch(line.Trim(), methodSignaturePattern);
        }

        public static string CleanUpDecompiledCode(string decompiledCode)
        {
            // Simple removal of known unwanted attributes
            var cleanedCode = decompiledCode
                .Replace("[AsyncStateMachine(typeof(<SetEditMode>d__37))]", "")
                .Replace("[DebuggerStepThrough]", "")
                .Replace("System.Threading.Tasks.Task", "Task")
                .Replace("global::", "")
                .Replace("//IL_0007: Unknown result type (might be due to invalid IL or missing references)", "")
                .Replace("//IL_000c: Unknown result type (might be due to invalid IL or missing references)", "");

            // Additional cleanup if needed
            return cleanedCode;
        }

    }

    // Built-in implementation for AssemblyResolver
    public class AssemblyResolver : IAssemblyResolver
    {
        public PEFile? Resolve(IAssemblyReference reference)
        {
            // You can provide logic to resolve referenced assemblies here if needed
            return null; // Default behavior: no assembly resolution
        }

        public PEFile? ResolveModule(PEFile mainModule, string moduleName)
        {
            // You can provide logic to resolve modules here if needed
            return null; // Default behavior: no module resolution
        }

        public Task<PEFile?> ResolveAsync(IAssemblyReference reference)
        {
            return Task.FromResult(Resolve(reference));
        }

        public Task<PEFile?> ResolveModuleAsync(PEFile mainModule, string moduleName)
        {
            return Task.FromResult(ResolveModule(mainModule, moduleName));
        }
    }
}
