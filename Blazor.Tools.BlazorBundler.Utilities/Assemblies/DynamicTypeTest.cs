using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class DynamicTypeMatchTest
    {
        public static bool IsMatched()
        {
            // Define the types for generic parameters
            Type modelType = typeof(Entities.SampleObjects.Models.Employee);
            Type modelExtendedPropertiesType = typeof(IModelExtendedProperties);

            // Create the assembly and module
            AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            // Define the generic type
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                "Blazor.Tools.BlazorBundler.Interfaces.IViewModel`2",
                TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract,
                null
            );

            // Define the generic parameters
            GenericTypeParameterBuilder[] genericParamBuilders = typeBuilder.DefineGenericParameters("TModel", "TIModel");

            // Ensure generic parameters are correctly set up
            genericParamBuilders[0].SetGenericParameterAttributes(GenericParameterAttributes.None);
            genericParamBuilders[1].SetGenericParameterAttributes(GenericParameterAttributes.None);

            // Create the type
            Type createdType = typeBuilder.CreateTypeInfo().AsType();

            // Construct expected type definition
            string expectedFullName = $"Blazor.Tools.BlazorBundler.Interfaces.IViewModel`2[[{modelType.FullName}, {modelType.Assembly.GetName().Name}], [{modelExtendedPropertiesType.FullName}, {modelExtendedPropertiesType.Assembly.GetName().Name}]]";

            // Compare full names
            string createdTypeFullName = createdType.FullName;
            AppLogger.WriteInfo($"Expected Full Name: {expectedFullName}");
            AppLogger.WriteInfo($"Created Full Name: {createdTypeFullName}");

            // Check if they match
            bool match = createdTypeFullName == expectedFullName;
            AppLogger.WriteInfo(match ? "The types match." : "The types do not match.");
            return match;
        }
    }
}
