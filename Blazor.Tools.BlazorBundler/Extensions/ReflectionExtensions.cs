using System.Reflection;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Gets the property names of an object.
        /// </summary>
        /// <param name="obj">The object to get the property names from.</param>
        /// <returns>IEnumerable<string> that contains all the object's property names.</returns>
        public static IEnumerable<string> GetProperties(this object obj)
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

            return property.GetValue(obj);
        }

        /// <summary>
        /// This extension method gets properties from a specific assembly name and class or interface.
        /// </summary>
        /// <param name="assemblyName">The assembly name string. Example: "AccSol.Interfaces".</param>
        /// <param name="typeName">The type name string. Example: "IClient" </param>
        /// <param name="isInterface">If value is true, gets the properties from an interface.
        /// If value is false, gets the properties from a class. Default value is false.</param>
        /// <returns>IEnumerable<string> that contains all the object's property names.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static IEnumerable<string> GetProperties(string assemblyName, string typeName, bool isInterface = false)
        {
            // Load the assembly
            Assembly assembly = Assembly.Load(assemblyName);
            if (assembly == null)
            {
                throw new ArgumentException($"Assembly '{assemblyName}' could not be loaded.");
            }

            var fullTypeName = string.Join(assemblyName, ".", typeName);
            // Find the type (class or interface)
            Type? type = assembly?.GetTypes()?.FirstOrDefault(t => t.FullName == typeName);
            if (type == null)
            {
                throw new ArgumentException($"Type '{typeName}' not found in assembly '{assemblyName}'.");
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
    }

}
