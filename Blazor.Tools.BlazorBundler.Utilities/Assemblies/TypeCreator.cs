/*====================================================================================================
    Class Name  : TypeCreator
    Created By  : Solomio S. Sisante
    Created On  : September 15, 2024
    Purpose     : To provide a POC prototype for testing type creations.
  ====================================================================================================*/
using Blazor.Tools.BlazorBundler.Interfaces;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class TypeCreator : ITypeCreator
    {
        private string _contextAssemblyName;
        private string _modelTypeAssemblyName;
        private string _modelTypeName;
        private string _iModelTypeAssemblyName;
        private string _iModelTypeName;
        private string _version;
        private AssemblyName _assemblyName;
        private AssemblyBuilderAccess _assemblyBuilderAccess;
        private AssemblyBuilder _assemblyBuilder;
        private string _moduleName;
        private ModuleBuilder _moduleBuilder;

        public TypeCreator(string contextAssemblyName, string modelTypeAssemblyName, string modelTypeName, string iModelTypeAssemblyName, string iModelTypeName, string version, AssemblyBuilderAccess assemblyBuilderAccess) 
        {
            _contextAssemblyName = contextAssemblyName;
            _modelTypeAssemblyName = modelTypeAssemblyName;
            _modelTypeName = modelTypeName;
            _iModelTypeAssemblyName = iModelTypeAssemblyName;
            _iModelTypeName = iModelTypeName;
            _version = version;
            _assemblyName = default!;
            _assemblyBuilderAccess = assemblyBuilderAccess;
            _assemblyBuilder = default!;
            _moduleName = default!;
            _moduleBuilder = default!;
        }

        public AssemblyName DefineAssemblyName(string contextAssemblyName = null!, string version = null!)
        {
            _contextAssemblyName = contextAssemblyName ?? _contextAssemblyName;
            _version = version ?? _version;

            AssemblyName assemblyName = new AssemblyName(_contextAssemblyName);
            assemblyName.Version = new Version(_version);
            _assemblyName = assemblyName;

            return assemblyName;
        }
        
        public AssemblyBuilder DefineAssemblyBuilder(AssemblyName? assemblyName = null, AssemblyBuilderAccess? assemblyBuilderAccess = null)
        {
            _assemblyName = assemblyName ?? _assemblyName;
            _assemblyBuilderAccess = assemblyBuilderAccess ?? _assemblyBuilderAccess;

            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(_assemblyName, _assemblyBuilderAccess);
            _assemblyBuilder = assemblyBuilder;

            return assemblyBuilder;
        }

        public ModuleBuilder DefineModule(AssemblyBuilder assemblyBuilder = null!, string moduleName = null!)
        {
            _assemblyBuilder = assemblyBuilder ?? _assemblyBuilder;
            _moduleName = moduleName ?? _moduleName;
            ModuleBuilder moduleBuilder = _assemblyBuilder.DefineDynamicModule(_moduleName);
            _moduleBuilder = moduleBuilder;

            return moduleBuilder;
        }

        public Type DefineInterfaceType(ModuleBuilder moduleBuilder = null!, string fullyQualifiedName = null!)
        {
            _moduleBuilder = moduleBuilder ?? _moduleBuilder;

            // Extract base type name and generic parameters
            string baseTypeName = GetBaseTypeName(fullyQualifiedName);
            string[] genericParams = GetGenericParameters(fullyQualifiedName);

            // Define the type with generic parameters
            TypeBuilder tb = _moduleBuilder.DefineType(baseTypeName, TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

            // Define the actual generic parameters with their type constraints
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
                string genericParamsSection = fullyQualifiedName.Substring(backtickIndex + 1).Trim();
                if (genericParamsSection.StartsWith("[[") && genericParamsSection.EndsWith("]]"))
                {
                    string paramsContent = genericParamsSection.Substring(2, genericParamsSection.Length - 4);
                    return paramsContent.Split(new[] { "],[" }, StringSplitOptions.None);
                }
            }
            return Array.Empty<string>();
        }

        private void DefineGenericParameters(TypeBuilder tb, string[] genericParams)
        {
            if (genericParams.Length > 0)
            {
                // Define the generic parameters
                string[] genericParamNames = new string[genericParams.Length];
                for (int i = 0; i < genericParams.Length; i++)
                {
                    genericParamNames[i] = $"T{i + 1}";
                }

                // Define the generic parameters on the type builder
                GenericTypeParameterBuilder[] genericTypeParameters = tb.DefineGenericParameters(genericParamNames);

                // Define constraints or additional attributes if needed
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

            var typeCreator = new TypeCreator(contextAssemblyName, modelTypeAssemblyName, modelTypeName, iModelTypeAssemblyName, iModelTypeName, version, AssemblyBuilderAccess.Run);
            typeCreator.DefineAssemblyName();
            typeCreator.DefineAssemblyBuilder();
            typeCreator.DefineModule(moduleName: contextAssemblyName);

            // Fully qualified name for the type
            string fullyQualifiedName = $"{contextAssemblyName}.IViewModel`2[[{modelTypeAssemblyName}.{modelTypeName}, {modelTypeAssemblyName}, Version={version}, Culture=neutral, PublicKeyToken=null],[{iModelTypeAssemblyName}.{iModelTypeName}, {iModelTypeAssemblyName}, Version={version}, Culture=neutral, PublicKeyToken=null]]";

            Type createdType = typeCreator.DefineInterfaceType(fullyQualifiedName: fullyQualifiedName);

            Console.WriteLine($"Expected FullName: {fullyQualifiedName}");
            Console.WriteLine($"Created Type FullName: {createdType.FullName}");
        }
    }
}
