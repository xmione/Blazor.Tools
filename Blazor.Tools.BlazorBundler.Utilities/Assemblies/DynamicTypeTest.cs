using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Emit;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models;
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class DynamicTypeMatchTest
    {
        private Type _iViewModelCreatedType = default!;
        private Assembly _iViewModelAssembly = default!;
        private Type _iViewModelLoadedType = default!;
        private Type _createdEmployeeVMType = default!;
        private Type _iCloneableGenericType = default!;
        private Type _iViewModelGenericType = default!;
        private Type _iViewModelConstructedType = default!;
        private TypeBuilder _employeeVMTypeBuilder = default!;
        private Type _constructedIViewModelType = default!;

        public DynamicTypeMatchTest()
        {
            var contextAssemblyName = "Blazor.Tools.BlazorBundler";
            var assemblyFileName = $"{contextAssemblyName}.dll";
            var version = "3.1.2.0";
            var iViewModelTypeName = $"{contextAssemblyName}.IViewModel`2";
            var tempPath = Path.GetTempPath();
            var dllFilePath = $"{Path.Combine(tempPath, contextAssemblyName)}.dll";
            var employeeTypeAssemblyName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models";
            var employeeTypeName = $"{employeeTypeAssemblyName}.Employee";
            var iModelExtendedPropertiesTypeName = $"{contextAssemblyName}.IModelExtendedProperties";
            var employeeVMTypeName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels.EmployeeVM";
            // Define the types for generic parameters
            Type modelType = typeof(Employee);
            Type modelExtendedPropertiesType = typeof(IModelExtendedProperties);

            // Create the assembly and module
            AssemblyName assemblyName = new AssemblyName(contextAssemblyName) { Version = new Version(version) };
            PersistedAssemblyBuilder assemblyBuilder = new PersistedAssemblyBuilder(assemblyName, typeof(object).Assembly);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(contextAssemblyName);

            moduleBuilder = CreateIViewModelType(moduleBuilder, iViewModelTypeName);
            //moduleBuilder = CreateEmployeeType(moduleBuilder, employeeTypeName);
            moduleBuilder = CreateEmployeeVMType(moduleBuilder, employeeVMTypeName);

            // Save the assembly to disk
            assemblyBuilder.Save(dllFilePath);

            // Load the dll file assembly using dll path extension method 
            _iViewModelAssembly = Assembly.LoadFile(dllFilePath);
            _iViewModelLoadedType = _iViewModelAssembly.GetType(_iViewModelCreatedType.FullName ?? string.Empty) ?? throw new InvalidOperationException("Loaded type is null.");

        }
        public bool IsMatched()
        {
            // Construct expected type definition
            //var expectedType = typeof(IViewModel<Employee, IModelExtendedProperties>);
            var expectedType = typeof(IViewModel<,>);

            // Compare full names
            string createdTypeFullName = _iViewModelCreatedType.FullName ?? string.Empty;
            string loadedTypeFullName = _iViewModelLoadedType.FullName ?? string.Empty;
            AppLogger.WriteInfo($"Expected Full Name: {expectedType.FullName}");
            AppLogger.WriteInfo($"Created Full Name: {createdTypeFullName}");
            AppLogger.WriteInfo($"Loaded Full Name: {loadedTypeFullName}");

            _iViewModelLoadedType.DisplayTypeDifferences(expectedType);

            // Check if they match
            bool match = createdTypeFullName == expectedType.FullName && loadedTypeFullName == expectedType.FullName;
            AppLogger.WriteInfo(match ? "The types match." : "The types do not match.");

            // Construct the generic type with specific type arguments
            //_constructedIViewModelType = _iViewModelLoadedType.MakeGenericType(typeof(Employee), typeof(IModelExtendedProperties));

            //// Check if the constructed type matches the expected type
            //var expectedIViewModelType = typeof(IViewModel<Employee, IModelExtendedProperties>);
            //bool iViewModelMatch = _constructedIViewModelType == expectedIViewModelType;

            //expectedIViewModelType.DisplayTypeDifferences(_constructedIViewModelType);
            //match = match && iViewModelMatch;
            return match;
        }

        public bool IsAssignable()
        {
            bool isAssignableFrom = false;

            // Retrieve the created EmployeeVM type from the saved assembly
            Type loadedEmployeeVMType = _iViewModelAssembly.GetType(_createdEmployeeVMType.FullName ?? string.Empty)
                ?? throw new InvalidOperationException("Loaded EmployeeVM type is null.");

            // Check if EmployeeVM is assignable from IViewModel<Employee, IModelExtendedProperties>
            //isAssignableFrom = _constructedIViewModelType.IsAssignableFrom(employeeVMType);
            isAssignableFrom = _constructedIViewModelType.IsAssignableFrom(loadedEmployeeVMType);

            AppLogger.WriteInfo(isAssignableFrom ? "EmployeeVM is assignable from IViewModel<Employee, IModelExtendedProperties>." : "EmployeeVM is not assignable from IViewModel<Employee, IModelExtendedProperties>.");

            return isAssignableFrom;

        }
          
        private ModuleBuilder CreateIViewModelType(ModuleBuilder moduleBuilder, string typeName )
        {

            // Define the generic type
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                typeName,
                TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract,
                null
            );

            // Define the generic parameters (without earlier techniques that did not work)
            //GenericTypeParameterBuilder[] genericParamBuilders = typeBuilder.DefineGenericParameters(
            //    $"{employeeTypeName}",
            //    $"{iModelExtendedPropertiesTypeName}"
            //);

            GenericTypeParameterBuilder[] genericParamBuilders = typeBuilder.DefineGenericParameters(
                "TModel",
                $"TIModel"
            );

            // Create the type
            _iViewModelCreatedType = typeBuilder.CreateTypeInfo().AsType();

            
            return moduleBuilder;
        }

        public ModuleBuilder CreateEmployeeVMType(ModuleBuilder moduleBuilder, string typeName)
        {
            // Define the EmployeeVM class dynamically
            _employeeVMTypeBuilder = moduleBuilder.DefineType(
                typeName,
                TypeAttributes.Public,
                typeof(Employee)
            );

            // Implement IValidatableObject
            _employeeVMTypeBuilder.AddInterfaceImplementation(typeof(IValidatableObject));

            // Implement ICloneable<EmployeeVM>
            _iCloneableGenericType = typeof(ICloneable<>).MakeGenericType(_employeeVMTypeBuilder); // Do not create the type yet
            _employeeVMTypeBuilder.AddInterfaceImplementation(_iCloneableGenericType);

            // Ensure you're using the open generic type definition (i.e., not yet constructed)
            _iViewModelGenericType = typeof(IViewModel<,>);  // This is a generic type definition

            // Now construct the generic type with specific arguments
            _iViewModelConstructedType = _iViewModelGenericType.MakeGenericType(typeof(Employee), typeof(IModelExtendedProperties));

            // Implement the constructed type
            _employeeVMTypeBuilder.AddInterfaceImplementation(_iViewModelConstructedType);

            // Create the EmployeeVM type (done last)
            _createdEmployeeVMType = _employeeVMTypeBuilder.CreateTypeInfo().AsType();

            var interfaces = _createdEmployeeVMType.GetInterfaces();
            foreach (var iFace in interfaces)
            {
                var interfaceName = iFace.FullName ?? string.Empty;
                AppLogger.WriteInfo(interfaceName);
            }

            return moduleBuilder;
        }
    }
}
