/*====================================================================================================
    Class Name  : TypeExtensions
    Created By  : Solomio S. Sisante
    Created On  : September 2, 2024
    Purpose     : To provide a helper class for type extenstions.
  ====================================================================================================*/
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class TypeExtensions
    {
        // Mapping from alias type names to .NET type names
        private static readonly Dictionary<string, string> AliasToDotNetMap = new Dictionary<string, string>
        {
            { "bool", "Boolean" },
            { "byte", "Byte" },
            { "sbyte", "SByte" },
            { "char", "Char" },
            { "decimal", "Decimal" },
            { "double", "Double" },
            { "float", "Single" },
            { "int", "Int32" },
            { "uint", "UInt32" },
            { "long", "Int64" },
            { "ulong", "UInt64" },
            { "short", "Int16" },
            { "ushort", "UInt16" },
            { "object", "Object" },
            { "string", "String" }
        };

        // Reverse mapping for .NET to alias type names
        private static readonly Dictionary<string, string> DotNetToAliasMap = AliasToDotNetMap
            .ToDictionary(pair => pair.Value, pair => pair.Key);

        // Method to get the alias type name for a given .NET type name
        public static string ToAliasType(this Type type)
        {
            string typeName = type.Name;
            return DotNetToAliasMap.TryGetValue(typeName, out var aliasType) ? aliasType : typeName;
        }

        // Method to get the .NET type name for a given alias type name
        public static string ToDotNetType(this Type type)
        {
            string typeName = type.Name;
            return AliasToDotNetMap.TryGetValue(typeName, out var dotNetType) ? dotNetType : typeName;
        }

        public static string FormatType(this Type type)
        {
            return type.ToAliasType();
        }

        public static string[] ToAliasTypes(this Type[] types)
        {
            return types.Select(type => type.ToAliasType()).ToArray();
        }

        public static string[] ToDotNetTypes(this Type[] types)
        {
            return types.Select(type => type.ToDotNetType()).ToArray();
        }

        // Method to generate default value for a specified type
        public static object GenerateDefaultValue(this Type type)
        {
            // Special case for bool type to return lowercase false
            if (type == typeof(bool))
            {
                return false;
            }

            // Handle nullable bool types (e.g., bool?)
            if (Nullable.GetUnderlyingType(type) == typeof(bool))
            {
                return (bool?)false;
            }

            // Handle value types (including structs and enums)
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type) ?? default!;
            }

            // Handle nullable types by getting the underlying type and returning its default value
            if (Nullable.GetUnderlyingType(type) != null)
            {
                return null!;
            }

            // Handle special cases for common types
            if (type == typeof(string))
            {
                return string.Empty;
            }

            // If it's a reference type, the default value is null
            return null!;
        }

        public static string GenerateDefaultValueAsString(this Type type)
        {
            object defaultValue = type.GenerateDefaultValue();
            return defaultValue is bool boolValue ? boolValue.ToString().ToLower() : defaultValue?.ToString() ?? "null";
        }

        public static void DisplayTypeDifferences(this Type type1, Type type2)
        {
            bool isEqual = true;
            var type1Name = type1.Name;
            var type2Name = type2.Name;
            Debug.WriteLine(new string('=', 222));

            Debug.WriteLine("type1 FullName: " + type1.FullName);
            Debug.WriteLine("type2 FullName: " + type2.FullName);
            // Compare and print only the properties that differ
            if (type1.FullName != type2.FullName)
            {
               Debug.WriteLine("FullName differs:");
               isEqual = false;
            }

            if (type1.Assembly.FullName != type2.Assembly.FullName)
            {
               Debug.WriteLine("Assembly differs:");
               Debug.WriteLine("type1 Assembly: " + type1.Assembly.FullName);
               Debug.WriteLine("type2 Assembly: " + type2.Assembly.FullName);
               isEqual = false;
            }

            if (type1.Namespace != type2.Namespace)
            {
               Debug.WriteLine("Namespace differs:");
               Debug.WriteLine("type1 Namespace: " + type1.Namespace);
               Debug.WriteLine("type2 Namespace: " + type2.Namespace);
               isEqual = false;
            }

            if (type1.Module.Name != type2.Module.Name)
            {
               Debug.WriteLine("Module differs:");
               Debug.WriteLine("type1 Module: " + type1.Module.Name);
               Debug.WriteLine("type2 Module: " + type2.Module.Name);
               isEqual = false;
            }

            // Check if the types are loaded from different locations
            if (type1.Assembly.Location != type2.Assembly.Location)
            {
               Debug.WriteLine("Assembly Location differs:");
               Debug.WriteLine("type1 Assembly Location: " + type1.Assembly.Location);
               Debug.WriteLine("type2 Assembly Location: " + type2.Assembly.Location);
               isEqual = false;
            }

            // Check if the types are loaded in different contexts
            if (AssemblyLoadContext.GetLoadContext(type1.Assembly) != AssemblyLoadContext.GetLoadContext(type2.Assembly))
            {
               Debug.WriteLine("Assembly Load Context differs:");
               Debug.WriteLine("type1 Assembly Load Context: " + AssemblyLoadContext.GetLoadContext(type1.Assembly));
               Debug.WriteLine("type2 Assembly Load Context: " + AssemblyLoadContext.GetLoadContext(type2.Assembly));
               isEqual = false;
            }

            // Compare the runtime type handles
            if (!type1.TypeHandle.Equals(type2.TypeHandle))
            {
               Debug.WriteLine("TypeHandle differs:");
               Debug.WriteLine("type1 TypeHandle: " + type1.TypeHandle);
               Debug.WriteLine("type2 TypeHandle: " + type2.TypeHandle);
               isEqual = false;
            }

            // Check if either of the types are generic and compare generic arguments
            if (type1.IsGenericType != type2.IsGenericType ||
                !type1.GetGenericArguments().SequenceEqual(type2.GetGenericArguments()))
            {
               Debug.WriteLine("Generic Type Arguments differ:");
               Debug.WriteLine("type1 Generic Type Arguments: " + string.Join(", ", type1.GetGenericArguments().Cast<Type>()));
               Debug.WriteLine("type2 Generic Type Arguments: " + string.Join(", ", type2.GetGenericArguments().Cast<Type>()));
               isEqual = false;
            }

            // Check if either of the assemblies are loaded in reflection-only mode
            if (type1.Assembly.ReflectionOnly != type2.Assembly.ReflectionOnly)
            {
               Debug.WriteLine("ReflectionOnly mode differs:");
               Debug.WriteLine("type1 ReflectionOnly: " + type1.Assembly.ReflectionOnly);
               Debug.WriteLine("type2 ReflectionOnly: " + type2.Assembly.ReflectionOnly);
               isEqual = false;
            }

            // Compare custom attributes of both types
            var type1Attributes = type1.GetCustomAttributes(false);
            var type2Attributes = type2.GetCustomAttributes(false);

            if (!type1Attributes.SequenceEqual(type2Attributes))
            {
               Debug.WriteLine("Custom Attributes differ:");
               Debug.WriteLine("type1 Attributes: " + string.Join(", ", type1Attributes.Select(attr => attr.GetType().Name)));
               Debug.WriteLine("type2 Attributes: " + string.Join(", ", type2Attributes.Select(attr => attr.GetType().Name)));
               isEqual = false;
            }

            // Check if types are dynamically generated (like proxies or reflection emit types)
            if (type1.IsConstructedGenericType != type2.IsConstructedGenericType)
            {
               Debug.WriteLine("IsConstructedGenericType differs:");
               Debug.WriteLine("type1 IsConstructedGenericType: " + type1.IsConstructedGenericType);
               Debug.WriteLine("type2 IsConstructedGenericType: " + type2.IsConstructedGenericType);
               isEqual = false;
            }

            if (type1.IsDefined(typeof(CompilerGeneratedAttribute), false) != type2.IsDefined(typeof(CompilerGeneratedAttribute), false))
            {
               Debug.WriteLine("IsDefinedFromReflectionEmit differs:");
               Debug.WriteLine("type1 IsDefinedFromReflectionEmit: " + type1.IsDefined(typeof(CompilerGeneratedAttribute), false));
               Debug.WriteLine("type2 IsDefinedFromReflectionEmit: " + type2.IsDefined(typeof(CompilerGeneratedAttribute), false));
               isEqual = false;
            }

            if (isEqual)
            {
               Debug.WriteLine("The types are identical based on the checked properties.");
            }
            else
            {
               Debug.WriteLine("The types are not equal based on the differences shown above.");
            }

            Debug.WriteLine(new string('=', 222));
        }

        public static (string? Namespace, string? ClassName) GetNamespaceAndClassName(this Type type)
        {
            // Get the FullName of the type, if it's not null
            string fullName = type?.FullName ?? default!;
            if (string.IsNullOrEmpty(fullName))
            {
                throw new ArgumentException("Type does not have a FullName.");
            }

            // If the type is generic, strip off the generic parameters and assembly details
            if (type?.IsGenericType ?? false)
            {
                // This will give us something like: Blazor.Tools.BlazorBundler.Interfaces.IViewModel`2
                fullName = type.GetGenericTypeDefinition()?.FullName ?? default!;
            }

            // Split the FullName by the last dot to separate namespace and class name
            int lastDotIndex = fullName.LastIndexOf('.');
            if (lastDotIndex == -1)
            {
                // If no dot is found, it's a root-level type with no namespace
                return (null, fullName);
            }

            // Extract namespace and class name
            string ns = fullName.Substring(0, lastDotIndex);
            string className = fullName.Substring(lastDotIndex + 1);

            // Strip out generic arity (`2 in `IViewModel`2`) if present in class name
            int backtickIndex = className.IndexOf('`');
            if (backtickIndex != -1)
            {
                className = className.Substring(0, backtickIndex);
            }

            return (ns, className);
        }

    }
}
