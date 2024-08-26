using System.Reflection;

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
        public static Assembly LoadAssemblyFromDLLFile(string assemblyPath)
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
        public static Assembly LoadAssemblyFromName(string assemblyName)
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
        public static IEnumerable<string> GetProperties(Assembly assembly, string typeName, bool isInterface = false)
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
    }
}
