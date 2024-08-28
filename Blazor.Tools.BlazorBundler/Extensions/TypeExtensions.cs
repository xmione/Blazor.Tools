using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }

}
