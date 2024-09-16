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
            // Extract the base type name
            string baseTypeName = GetBaseTypeName(fullyQualifiedName);

            // Define the generic parameters if present
            TypeBuilder tb = mb.DefineType(baseTypeName, TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

            // Extract and define generic parameters if needed
            DefineGenericParameters(tb, fullyQualifiedName);

            // Create the type
            return tb.CreateType();
        }

        private string GetBaseTypeName(string fullyQualifiedName)
        {
            int backtickIndex = fullyQualifiedName.IndexOf('`');
            if (backtickIndex != -1)
            {
                return fullyQualifiedName.Substring(0, backtickIndex);
            }
            return fullyQualifiedName;
        }

        private void DefineGenericParameters(TypeBuilder tb, string fullyQualifiedName)
        {
            int backtickIndex = fullyQualifiedName.IndexOf('`');
            if (backtickIndex != -1)
            {
                string genericParams = fullyQualifiedName.Substring(backtickIndex + 1);
                string[] genericParamNames = genericParams.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

                // Define generic parameters for the type
                GenericTypeParameterBuilder[] genericParameters = tb.DefineGenericParameters(genericParamNames);
                for (int i = 0; i < genericParameters.Length; i++)
                {
                    genericParameters[i].SetGenericParameterAttributes(GenericParameterAttributes.None);
                    // Additional configuration of generic parameters can be done here
                }
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

            string fullyQualifiedName = $"{contextAssemblyName}.IViewModel`2[[{modelTypeAssemblyName}.{modelTypeName}, {modelTypeAssemblyName}, Version={version}, Culture=neutral, PublicKeyToken=null],[{iModelTypeAssemblyName}.{iModelTypeName}, {iModelTypeAssemblyName}, Version={version}, Culture=neutral, PublicKeyToken=null]]";

            var tc = new TypeCreator();
            Type createdType = tc.DefineInterfaceType(moduleBuilder, fullyQualifiedName);

            Console.WriteLine($"Created Type FullName: {createdType.FullName}");
            Console.WriteLine($"Created Type Name: {createdType.Name}");
        }
    }
}
