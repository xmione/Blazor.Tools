/*====================================================================================================
    Class Name  : TypeCreator
    Created By  : Solomio S. Sisante
    Created On  : September 15, 2024
    Purpose     : To provide a POC prototype for testing type creations.
  ====================================================================================================*/
using System.Reflection;
using System.Reflection.Emit;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class TypeCreator
    {
        public static Type DefineInterfaceType(ModuleBuilder mb, string fullyQualifiedName)
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

        private static string GetBaseTypeName(string fullyQualifiedName)
        {
            int backtickIndex = fullyQualifiedName.IndexOf('`');
            if (backtickIndex != -1)
            {
                return fullyQualifiedName.Substring(0, backtickIndex);
            }
            return fullyQualifiedName;
        }

        private static void DefineGenericParameters(TypeBuilder tb, string fullyQualifiedName)
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

        public static void Test()
        {
            AssemblyName asmName = new AssemblyName("DynamicAssembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            string fullyQualifiedName = "Blazor.Tools.BlazorBundler.Interfaces.IViewModel`2[[Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models.Employee, Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null],[Blazor.Tools.BlazorBundler.Interfaces.IModelExtendedProperties, Blazor.Tools.BlazorBundler.Interfaces, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]";

            Type createdType = DefineInterfaceType(moduleBuilder, fullyQualifiedName);

            Console.WriteLine($"Created Type FullName: {createdType.FullName}");
            Console.WriteLine($"Created Type Name: {createdType.Name}");
        }
    }
}
