/*====================================================================================================
    Class Name  : TypeCreator
    Created By  : Solomio S. Sisante
    Created On  : September 15, 2024
    Purpose     : To provide a POC prototype for testing type creations.
  ====================================================================================================*/
using Blazor.Tools.BlazorBundler.Interfaces;
using System.Reflection;
using System.Reflection.Emit;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class TypeCreator : ITypeCreator
    {
        public Type DefineInterfaceType(ModuleBuilder mb, string fullyQualifiedName)
        {
            // Extract base type name and generic parameters
            string baseTypeName = GetBaseTypeName(fullyQualifiedName);
            string[] genericParams = GetGenericParameters(fullyQualifiedName);

            // Define the type with generic parameters
            TypeBuilder tb = mb.DefineType(baseTypeName, TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

            // Define the generic parameters
            DefineGenericParameters(tb, genericParams);

            // Create and return the type
            return tb.CreateType();
        }

        private string GetBaseTypeName(string fullyQualifiedName)
        {
            int backtickIndex = fullyQualifiedName.IndexOf('`');
            return backtickIndex != -1 ? fullyQualifiedName.Substring(0, backtickIndex) : fullyQualifiedName;
        }

        private string[] GetGenericParameters(string fullyQualifiedName)
        {
            int backtickIndex = fullyQualifiedName.IndexOf('`');
            if (backtickIndex != -1)
            {
                string genericParams = fullyQualifiedName.Substring(backtickIndex + 1).Trim('[', ']');
                return genericParams.Split(new[] { "],[" }, StringSplitOptions.None);
            }
            return Array.Empty<string>();
        }

        private void DefineGenericParameters(TypeBuilder tb, string[] genericParams)
        {
            if (genericParams.Length > 0)
            {
                string[] genericParamNames = new string[genericParams.Length];
                for (int i = 0; i < genericParams.Length; i++)
                {
                    genericParamNames[i] = $"T{i + 1}";
                }

                GenericTypeParameterBuilder[] genericParameters = tb.DefineGenericParameters(genericParamNames);

                // No constraints or attributes set for generic parameters
            }
        }

        public static void Create()
        {
            string contextAssemblyName = "Blazor.Tools.BlazorBundler.Interfaces";
            string modelTypeAssemblyName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models";
            string modelTypeName = "Employee";
            string iModelTypeAssemblyName = "Blazor.Tools.BlazorBundler.Interfaces";
            string iModelTypeName = "IModelExtendedProperties";
            string version = "1.0.0.0";

            AssemblyName assemblyName = new AssemblyName(contextAssemblyName);
            assemblyName.Version = new Version(version);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(contextAssemblyName);

            // Fully qualified name for the type
            string fullyQualifiedName = $"{contextAssemblyName}.IViewModel`2[[{modelTypeAssemblyName}.{modelTypeName}, {modelTypeAssemblyName}, Version={version}, Culture=neutral, PublicKeyToken=null],[{iModelTypeAssemblyName}.{iModelTypeName}, {iModelTypeAssemblyName}, Version={version}, Culture=neutral, PublicKeyToken=null]]";

            var tc = new TypeCreator();
            Type createdType = tc.DefineInterfaceType(moduleBuilder, fullyQualifiedName);

            Console.WriteLine($"Expected FullName: {fullyQualifiedName}");
            Console.WriteLine($"Created Type FullName: {createdType.FullName}");
        }
    }
}
