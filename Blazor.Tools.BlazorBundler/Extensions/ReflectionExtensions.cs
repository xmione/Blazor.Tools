using System.Reflection;
using System.Text;
using MethodBody = System.Reflection.MethodBody;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class ReflectionExtensions
    {

        /// <summary>
        /// This extension method loads a specific assembly from its .dll assembly file.
        /// </summary>
        /// <param name="assemblyPath">The full path of the dll assembly file. 
        /// Example: "C:\repo\AccSol\AccSol.Interfaces\bin\Release\AccSol.Interfaces.dll".</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>Assembly</returns>
        public static Assembly LoadAssemblyFromDLLFile(this string assemblyPath)
        {
            // Load the assembly
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            if (assembly == null)
            {
                throw new ArgumentException($"Assembly '{assemblyPath}' could not be loaded.");
            }
            
            return assembly;
        }

        /// <summary>
        /// This extension method loads a specific assembly from its assembly name.
        /// </summary>
        /// <param name="assemblyName">The assembly name string. Example: "AccSol.Interfaces".
        /// The assembly (e.g: "AccSol.Interfaces") is assumed already added as project or dll reference.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>Assembly</returns>
        public static Assembly LoadAssemblyFromName(this string assemblyName)
        {
            // Load the assembly
            Assembly assembly = Assembly.Load(assemblyName);
            if (assembly == null)
            {
                throw new ArgumentException($"Assembly '{assemblyName}' could not be loaded.");
            }
            
            return assembly;
        }

        /// <summary>
        /// This extension method invokes the specified method.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="typeName"></param>
        /// <param name="methodName"></param>
        public static void InvokeMethod(this Assembly assembly, string typeName, string methodName)
        {
            // Get the type and method info
            Type type = assembly?.GetType(typeName) ?? default!;
            if (type != null)
            {
                MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static) ?? default!;

                // Invoke the method
                method.Invoke(null, null);
            }
            
        }

        /// <summary>
        /// Gets the PropertyInfo of an object.
        /// </summary>
        /// <param name="obj">The object to get the property infos from.</param>
        /// <returns>PropertyInfo[]? that contains all the object's properties.</returns>
        public static PropertyInfo[]? GetProperties(this object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var properties = obj.GetType()
                      .GetProperties(BindingFlags.Public | BindingFlags.Instance);

            return properties;
        }
        
        /// <summary>
        /// Gets the property names of an object.
        /// </summary>
        /// <param name="obj">The object to get the property names from.</param>
        /// <returns>IEnumerable<string> that contains all the object's property names.</returns>
        public static IEnumerable<string> GetPropertyNames(this object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var properties = obj.GetType()
                      .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                      .Select(p => p.Name);

            return properties;
        }

        /// <summary>
        /// Gets the value of a specific property by its name.
        /// </summary>
        /// <param name="obj">The object to get the property value from.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the property.</returns>
        public static object? GetProperty(this object obj, string propertyName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));
            }

            var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                throw new ArgumentException($"Property '{propertyName}' not found on '{obj.GetType().Name}'.", nameof(propertyName));
            }

            object? propertyValue = property.GetValue(obj);
            return propertyValue;
        }

        /// <summary>
        /// This extension method gets properties from a specific assembly name and class or interface.
        /// </summary>
        /// <param name="assemblyName">The assembly name string. Example: "AccSol.Interfaces".
        /// The assembly (e.g: "AccSol.Interfaces") is assumed already added as project or dll reference.</param>
        /// <param name="typeName">The type name string. Example: "IClient" </param>
        /// <param name="isInterface">If value is true, gets the properties from an interface.
        /// If value is false, gets the properties from a class. Default value is false.</param>
        /// <returns>IEnumerable<string> that contains all the object's property names.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static IEnumerable<string> GetProperties(this Assembly assembly, string typeName, bool isInterface = false)
        {
            var fullTypeName = string.Join(".", assembly.FullName, typeName);
            // Find the type (class or interface)
            Type? type = assembly?.GetTypes()?.FirstOrDefault(t => t.FullName == typeName);
            if (type == null)
            {
                throw new ArgumentException($"Type '{typeName}' not found in assembly '{assembly?.FullName}'.");
            }

            // Check if the type matches the expected kind (interface or class)
            if (isInterface && !type.IsInterface)
            {
                throw new ArgumentException($"Type '{typeName}' is not an interface.");
            }
            if (!isInterface && type.IsInterface)
            {
                throw new ArgumentException($"Type '{typeName}' is not a class.");
            }

            // Get properties of the type
            var properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name); 
            
            return properties;
        }

        /// <summary>
        /// This static method gets the list of interface full names and short names.
        /// </summary>
        /// <param name="assembly">The Assembly instance.</param>
        /// <returns>IEnumerable<Tuple<string, string>></string> - The list of interface full name and short name.</returns>
        public static IEnumerable<Tuple<string, string>> GetAssemblyInterfaceNames(this Assembly assembly)
        {
            // Get all types defined in the assembly
            Type[] types = assembly.GetTypes();

            // Filter the types to get only interfaces
            Type[] interfaces = Array.FindAll(types, t => t.IsInterface);

            // List to hold the interface names
            List<Tuple<string, string>> interfaceNames = new List<Tuple<string, string>>();

            // Add the full name and short name of each interface to the list
            foreach (Type iFace in interfaces)
            {
                interfaceNames.Add(new Tuple<string, string>(iFace.FullName ?? default!, iFace.Name));
            }

            return interfaceNames;
        }

        public static string GetILCode(this Type type, string methodName)
        {
            MethodInfo methodInfo = type.GetMethod(methodName)
                ?? throw new ArgumentException("Method not found.", nameof(methodName));

            MethodBody methodBody = methodInfo.GetMethodBody()
                ?? throw new InvalidOperationException("Method body is not available.");

            byte[] ilCode = methodBody.GetILAsByteArray()
                ?? throw new InvalidOperationException("IL code is not available.");

            string ilCodeString = string.Join(" ", ilCode.Select(b => $"{b:X2}"));

            // Get the return type and format it correctly using alias
            string returnType = FormatType(methodInfo.ReturnType);

            // Format method parameters using alias
            var parameters = methodInfo.GetParameters();
            string parametersString = string.Join(", ", parameters
                .Select(p => $"{FormatType(p.ParameterType)} {p.Name}"));

            // Return the formatted IL code with method signature
            StringBuilder formattedILCode = new StringBuilder();
            formattedILCode.AppendLine($"public {returnType} {methodInfo.Name}({parametersString})");
            formattedILCode.AppendLine("{");
            formattedILCode.AppendLine($"    // IL code");
            formattedILCode.AppendLine($"    {ilCodeString}");
            formattedILCode.AppendLine("}");

            return formattedILCode.ToString();
        }

        public static byte[] GetAssemblyBytes(this Assembly assembly)
        {
            byte[] bytes = null!;
            try 
            {
                using (var memoryStream = new MemoryStream())
                {
                    var module = assembly.GetModules()[0];
                    var moduleName = module.FullyQualifiedName;
                    var moduleData = File.ReadAllBytes(moduleName);
                    memoryStream.Write(moduleData, 0, moduleData.Length);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }

            return bytes;
            
        }

        private static string FormatType(Type type)
        {
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var genericArguments = type.GetGenericArguments();
                var genericArgumentsStr = string.Join(", ", genericArguments.Select(FormatType));
                return $"{genericTypeDefinition.Name.Split('`')[0]}<{genericArgumentsStr}>";
            }

            var typeName = type.ToAliasType();
            return typeName;
        }


        //public static string GetMethodCode(this Type type, string methodName)
        //{
        //    if (type == null) throw new ArgumentNullException(nameof(type));
        //    if (string.IsNullOrWhiteSpace(methodName)) throw new ArgumentException("Method name cannot be null or empty.", nameof(methodName));

        //    var assembly = Assembly.GetAssembly(type);
        //    if (assembly == null) throw new InvalidOperationException("Unable to get assembly.");

        //    // Create a temporary assembly file to handle dynamically created assembly
        //    var assemblyBytes = GetAssemblyBytes(assembly);
        //    if (assemblyBytes == null || assemblyBytes.Length == 0) return null;

        //    using var assemblyStream = new MemoryStream(assemblyBytes);
        //    var module = ModuleDefinition.ReadModule(assemblyStream);

        //    // Use Mono.Cecil to analyze the module
        //    var methodDefinition = module.Types
        //        .SelectMany(t => t.Methods)
        //        .FirstOrDefault(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));

        //    if (methodDefinition == null) return null;

        //    // Use a decompiler to get the method code
        //    var decompiler = new CSharpDecompiler(module.FileName, new DecompilerSettings());
        //    var decompiledCode = decompiler.DecompileWholeModuleAsString();

        //    // Find and return the method code
        //    var methodStartIndex = decompiledCode.IndexOf($"public {methodName}", StringComparison.OrdinalIgnoreCase);
        //    if (methodStartIndex == -1) return null;

        //    var methodCode = decompiledCode.Substring(methodStartIndex);
        //    var methodEndIndex = methodCode.IndexOf('}', methodStartIndex);
        //    if (methodEndIndex == -1) return null;

        //    return methodCode.Substring(0, methodEndIndex + 1);
        //}

        //public static byte[] GetAssemblyBytes(this Assembly assembly)
        //{
        //    if (assembly == null) throw new ArgumentNullException(nameof(assembly));

        //    if (assembly is AssemblyBuilder assemblyBuilder)
        //    {
        //        // Handle dynamically created assembly
        //        string tempFilePath = Path.GetTempFileName() + ".dll";
        //        try
        //        {
        //            // Save the dynamically created assembly to a temporary file
        //            assemblyBuilder.Save(tempFilePath);

        //            // Read and return the bytes from the temporary file
        //            return File.ReadAllBytes(tempFilePath);
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new InvalidOperationException("Failed to get assembly bytes.", ex);
        //        }
        //        finally
        //        {
        //            // Clean up the temporary file
        //            if (File.Exists(tempFilePath))
        //            {
        //                File.Delete(tempFilePath);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // Handle statically loaded assemblies
        //        try
        //        {
        //            // Get the assembly bytes directly from the module
        //            using (var stream = new MemoryStream())
        //            {
        //                // Get the raw assembly bytes from the loaded assembly
        //                foreach (var module in assembly.GetModules())
        //                {
        //                    using (var moduleStream = module.ModuleHandle.GetStream())
        //                    {
        //                        moduleStream.CopyTo(stream);
        //                    }
        //                }

        //                return stream.ToArray();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new InvalidOperationException("Failed to get assembly bytes.", ex);
        //        }
        //    }
        //}
    }
}
