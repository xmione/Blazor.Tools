using Moq;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using System.Reflection.Emit;
using System.Reflection;
using Blazor.Tools.BlazorBundler.Utilities.Assemblies;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Blazor.Tools.BlazorBundler.Entities;
using static Blazor.Tools.BlazorBundler.Utilities.Assemblies.AssemblyGenerator;
using System.Management;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Reflection.PortableExecutable;
using DocumentFormat.OpenXml.Office2016.Excel;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels;

namespace Blazor.Tools.BlazorBundler.Tests
{
    [TestClass]
    public class TypeCreatorTests : SampleData, IDisposable
    {
        private Mock<ITypeCreator> _typeCreatorMock = default!;
        private AssemblyName _assemblyName = default!;
        private PersistedAssemblyBuilder _assemblyBuilder = default!;
        private ModuleBuilder _testModuleBuilder = default!;
        private Type _vmType;
        private FieldBuilder _employeeListField;
        private FieldBuilder _contextProviderField;
        private Type _employeeType;

        [TestInitialize]
        public void TestInit()
        {
            string contextAssemblyName = "Blazor.Tools.BlazorBundler.Interfaces";
            // Initialize the mock object
            _typeCreatorMock = new Mock<ITypeCreator>();
            _assemblyName = new AssemblyName(contextAssemblyName);
            _assemblyBuilder = new PersistedAssemblyBuilder(_assemblyName, typeof(object).Assembly); 
            _testModuleBuilder = _assemblyBuilder.DefineDynamicModule(contextAssemblyName);

            // SetI up default mock behavior
            _typeCreatorMock.Setup(tc => tc.DefineAssemblyName(It.IsAny<string>(), It.IsAny<string>())).Returns(_assemblyName);
            _typeCreatorMock.Setup(tc => tc.DefineAssemblyBuilder(It.IsAny<AssemblyName>(), It.IsAny<AssemblyBuilderAccess>()))
                .Returns(_assemblyBuilder);
            _typeCreatorMock.Setup(tc => tc.DefineModuleBuilder(It.IsAny<PersistedAssemblyBuilder>(), It.IsAny<string>()))
                .Returns(_testModuleBuilder);

        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Cleanup resources after each test
            // Optionally, remove any temporary files or resources if needed
        }

        [TestMethod]
        public void DefineAssemblyName_ShouldReturn_AssemblyName()
        {
            // Arrange
            string contextAssemblyName = "Blazor.Tools.BlazorBundler.Interfaces";
            string version = "3.1.2.0";

            // Act
            var assemblyName = _typeCreatorMock.Object.DefineAssemblyName(contextAssemblyName, version);

            // Assert
            Assert.AreEqual(_assemblyName, assemblyName);
            _typeCreatorMock.Verify(tc => tc.DefineAssemblyName(contextAssemblyName, version), Times.Once);
        }

        [TestMethod]
        public void DefineAssemblyBuilder_ShouldReturn_AssemblyBuilder()
        {
            string contextAssemblyName = "Blazor.Tools.BlazorBundler.Interfaces";

            // Arrange
            AssemblyName assemblyName = new AssemblyName(contextAssemblyName);
            AssemblyBuilderAccess assemblyAccess = AssemblyBuilderAccess.Run;

            _typeCreatorMock.Setup(tc => tc.DefineAssemblyBuilder(assemblyName, assemblyAccess)).Returns(_assemblyBuilder);

            // Act
            var assemblyBuilder = _typeCreatorMock.Object.DefineAssemblyBuilder(assemblyName, assemblyAccess);

            // Assert
            Assert.AreEqual(_assemblyBuilder, assemblyBuilder);
            _typeCreatorMock.Verify(tc => tc.DefineAssemblyBuilder(assemblyName, assemblyAccess), Times.Once);
        }

        [TestMethod]
        public void DefineModule_ShouldReturn_ModuleBuilder()
        {
            // Act
            var moduleBuilder = _typeCreatorMock.Object.DefineModuleBuilder(_assemblyBuilder, "TestModule");

            // Assert
            Assert.AreEqual(_testModuleBuilder, moduleBuilder);
            _typeCreatorMock.Verify(tc => tc.DefineModuleBuilder(_assemblyBuilder, "TestModule"), Times.Once);
        }

        [TestMethod]
        public void DefineInterfaceType_ShouldReturn_InterfaceType()
        {
            // Arrange
            string fullyQualifiedName = "TestNamespace.ITestInterface`1[[TestNamespace.TestClass, TestAssembly, Version=1.0.0.0]]";

            _typeCreatorMock.Setup(tc => tc.DefineInterfaceType(It.IsAny<ModuleBuilder>(), It.IsAny<string>()))
                .Returns(typeof(IDisposable)); // You can return any expected interface type for the mock.

            // Act
            var createdType = _typeCreatorMock.Object.DefineInterfaceType(_testModuleBuilder, fullyQualifiedName);

            // Assert
            Assert.AreEqual(typeof(IDisposable), createdType); // Check for the type returned from the mock
            _typeCreatorMock.Verify(tc => tc.DefineInterfaceType(_testModuleBuilder, fullyQualifiedName), Times.Once);
        }

        [TestMethod]
        public void DefineInterfaceType_Mock_Test()
        {
            try
            {
                // Arrange
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

                // Create a dummy type that matches the fully qualified name
                string baseTypeName = $"Blazor.Tools.BlazorBundler.Interfaces.IViewModel";
                TypeBuilder typeBuilder = moduleBuilder.DefineType(baseTypeName, TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

                // Define generic parameters for the type
                string[] genericParamNames = new[] { "T1", "T2" };
                GenericTypeParameterBuilder[] genericParameters = typeBuilder.DefineGenericParameters(genericParamNames);
                for (int i = 0; i < genericParameters.Length; i++)
                {
                    genericParameters[i].SetGenericParameterAttributes(GenericParameterAttributes.None);
                }

                // Create the type
                Type dummyType = typeBuilder.CreateTypeInfo().AsType();

                // Configure the mock to return this dummy type
                _typeCreatorMock.Setup(m => m.DefineInterfaceType(moduleBuilder, fullyQualifiedName)).Returns(dummyType);

                // Act
                Type resultType = _typeCreatorMock.Object.DefineInterfaceType(moduleBuilder, fullyQualifiedName);

                // Assert
                Assert.IsNotNull(resultType, "The result type should not be null.");
                Assert.AreEqual(dummyType, resultType, "The result type should match the dummy type.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Create failed with exception: {ex.Message}");
                AppLogger.HandleError(ex);
            }
        }

        [TestMethod]
        public void DynamicType_Test()
        {
            var dynamicTypeMatchTest = new DynamicTypeMatchTest();
            Assert.IsTrue(dynamicTypeMatchTest.IsMatched());
            Assert.IsTrue(dynamicTypeMatchTest.IsAssignable());
        }

        [TestMethod]
        public void CreateAnInstanceOfDynamicType()
        {
            var contextAssemblyName = "Blazor.Tools.BlazorBundler";
            var employeeVMTypeName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels.EmployeeVM";
            var dllPath = Path.Combine(Path.GetTempPath(), $"{contextAssemblyName}.dll");

            // Define a dynamic assembly and module
            AssemblyName assemblyName = new AssemblyName(contextAssemblyName);
            //AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            PersistedAssemblyBuilder assemblyBuilder = new PersistedAssemblyBuilder(assemblyName, typeof(object).Assembly);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(contextAssemblyName);

            // Define a dynamic type
            TypeBuilder typeBuilder = moduleBuilder.DefineType(employeeVMTypeName, TypeAttributes.Public);

            // Define a constructor
            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            ILGenerator constructorIl = constructorBuilder.GetILGenerator();
            constructorIl.Emit(OpCodes.Ret);

            // Create the type
            Type employeeVMType = typeBuilder.CreateType();

            assemblyBuilder.Save(dllPath);

            // Load the assembly
            var assembly = Assembly.LoadFile(dllPath);

            // Get Employee VM Type
            employeeVMType = assembly.GetType(employeeVMTypeName)!;

            // Create an instance of the dynamically created type
            object instance = Activator.CreateInstance(employeeVMType)!;

            // Use the instance (e.g., check its type)
            Console.WriteLine($"Created instance of: {instance.GetType().FullName}");
        }

        [TestMethod]
        public void Create_Bundler_Test()
        {
            var contextAssemblyName = "Blazor.Tools.BlazorBundler";
            var employeeTypeNameSpace = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models";
            var interfaceNameSpace = "Blazor.Tools.BlazorBundler.Interfaces";
            var employeeTypeName = "Employee";
            var version = "3.1.2.0";
            var employeeFullyQualifiedTypeName = $"{employeeTypeNameSpace}.{employeeTypeName}";
            var employeeVMNameSpace = employeeTypeNameSpace.Replace("Models", "ViewModels");
            var employeeVMTypeName = employeeTypeName + "VM";
            var employeeVMFullyQualifiedTypeName = $"{employeeVMNameSpace}.{employeeVMTypeName}";
            var dllPath = Path.Combine(Path.GetTempPath(), $"{contextAssemblyName}.dll" );
            var typeCreator = new TypeCreator();
            var assemblyName = typeCreator.DefineAssemblyName(contextAssemblyName, version);
            typeCreator.DefineAssemblyBuilder(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = typeCreator.DefineModuleBuilder(moduleName: contextAssemblyName);
            TypeBuilder typeBuilder = default!;
            TypeBuilder tbBeforeCreate = default!;

            var modelProperties = typeCreator.GetDataTableColumnDefinitions(EmployeeDataTable)
                .Where(i => i.TableName == employeeTypeName)
                .Select(p => new PropertyDefinition
                {
                    Name = p.ColumnName,
                    Type = p.ColumnType
                }).ToList() ?? default!; 

            _employeeType = typeCreator.CreateType(
                ref moduleBuilder,
                out typeBuilder,
                out tbBeforeCreate,
                typeName: employeeFullyQualifiedTypeName,
                typeAttributes: TypeAttributes.Public | TypeAttributes.Class,
                properties: modelProperties
                );

            var iValidatableObjectType = typeof(IValidatableObject);
            var iValidatableTypeMethods = iValidatableObjectType
                .GetMethods()
                .Select(i => new MethodDefinition
                { 
                    Name = i.Name,
                    Attributes = i.Attributes,
                    ReturnType = i.ReturnType,
                    ParameterTypes = Array.ConvertAll(i.GetParameters(), p => p.ParameterType)
                });

            var iValidatableObjectCreatedType = typeCreator.CreateType(
                ref moduleBuilder,
                out typeBuilder,
                out tbBeforeCreate,
                typeName: iValidatableObjectType?.FullName ?? default!,
                typeAttributes: TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract,
                methodDefinitions: iValidatableTypeMethods
                );

            var iCloneableType = typeof(ICloneable<>);
            var iCloneableTypeMethods = iCloneableType
                .GetMethods()
                .Select(i => new MethodDefinition
                {
                    Name = i.Name,
                    Attributes = i.Attributes,
                    ReturnType = i.ReturnType,
                    ParameterTypes = Array.ConvertAll(i.GetParameters(), p => p.ParameterType)
                });

            var iCloneableProperties = iCloneableType.GetProperties()?.Select(p => new PropertyDefinition
            {
                Name = p.Name,
                Type = p.PropertyType
            }).ToList() ?? default!;

            var iCloneableCreatedType = typeCreator.CreateType(
                ref moduleBuilder,
                out typeBuilder,
                out tbBeforeCreate,
                typeName: iCloneableType?.FullName ?? default!,
                typeAttributes: TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract,
                methodDefinitions: iCloneableTypeMethods,
                properties: iCloneableProperties,
                genericParameterNames: new[] { "T" }
                );

            var iModelExtendedPropertiesType = typeof(IModelExtendedProperties);

            var iModelExtendedProperties = iModelExtendedPropertiesType
                .GetProperties()?
                .Select(p => new PropertyDefinition
                {
                    Name = p.Name,
                    Type = p.PropertyType
                }).ToList() ?? default!;

            //// Define the IModelExtendedProperties interface
            //typeBuilder = moduleBuilder.DefineType(
            //    iModelExtendedPropertiesType.FullName!,
            //    TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

            //foreach (var prop in iModelExtendedProperties)
            //{
            //    typeBuilder = typeCreator.DefineInterfaceProperty(typeBuilder, prop.Name, prop.Type);
            //}

            //var iModelExtendedPropertiesCreatedType = typeBuilder.CreateType();

            var iModelExtendedPropertiesCreatedType = typeCreator.CreateType(
                ref moduleBuilder,
                out typeBuilder,
                out tbBeforeCreate,
                typeName: $"{interfaceNameSpace}.IModelExtendedProperties",
                typeAttributes: TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract,
                properties: iModelExtendedProperties
                );

            //// Define the IEmployee interface
            //typeBuilder = moduleBuilder.DefineType(
            //    $"{interfaceNameSpace}.IModelExtendedProperties",
            //    TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

            //// Define properties for the interface
            //foreach (var prop in iModelExtendedProperties)
            //{
            //    typeBuilder = typeCreator.DefineInterfaceProperty(typeBuilder, prop.Name, prop.Type);
            //}

            //// Create the interface type
            //var iModelExtendedPropertiesConstructedType = typeBuilder.CreateType();

            var iViewModelType = typeof(IViewModel<,>);
            var iViewModelCreatedType = typeCreator.CreateType(
                ref moduleBuilder,
                out typeBuilder,
                out tbBeforeCreate,
                typeName: $"{interfaceNameSpace}.IViewModel",
                typeAttributes: TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract,
                interfaces: new Type[] { iModelExtendedPropertiesCreatedType },
                genericParameterNames: new[] { "TModel", "TIModel" },  // Define <TModel, TIModel>
                defineMethodsAction: tb => DefineViewModelMethods(typeCreator, tb)
                );
            
            var iViewModelConstructedGenericType = iViewModelCreatedType.MakeGenericType(_employeeType, iModelExtendedPropertiesCreatedType);

            //var genericType = typeof(List<>).MakeGenericType(iViewModelType);
            //var removeMethod = genericType.GetMethod("Remove", BindingFlags.Instance | BindingFlags.Public, Type.DefaultBinder, new[] { iViewModelType }, null) ?? default!;
            var iImplementations = new List<(Type InterfaceType, bool isTypeUsed)>()
            {
                new (iValidatableObjectCreatedType, false),
                new (iCloneableCreatedType, true),
                new (iViewModelConstructedGenericType, false),
            };

            _vmType = typeCreator.CreateType
                (
                ref moduleBuilder,
                out typeBuilder,
                out tbBeforeCreate,
                typeName: employeeVMFullyQualifiedTypeName,
                typeAttributes: TypeAttributes.Public | TypeAttributes.Class,
                baseType: _employeeType,
                //interfaces: new Type[] { iValidatableObjectCreatedType, iCloneableCreatedType, iViewModelType, iModelExtendedPropertiesConstructedType },
                properties: modelProperties,
                defineFieldsAction: tb => DefineFields(typeCreator, tb),
                defineConstructorsAction: tb => DefineConstructors(typeCreator, tb, _employeeType),
                defineMethodsAction: tb => DefineMethods(typeCreator, tb, iViewModelType, _employeeType, iModelExtendedPropertiesCreatedType),
                iImplementations: iImplementations
                );

            //DefineMethodsAsync(typeCreator, tbBeforeCreate, iViewModelType, _employeeType, iModelExtendedPropertiesConstructedType);
            // Save the assembly to a file
            typeCreator.Save(dllPath);

            //var assembly = Assembly.LoadFile(dllPath);
            //_vmType = assembly.GetType(employeeVMFullyQualifiedTypeName)!;

            //object employeeInstance = Activator.CreateInstance(_employeeType)!;
            //object employeeVMInstance = Activator.CreateInstance(_vmType)!;

            //// Use reflection to set the _employees field with List<EmployeeVM>
            //FieldInfo employeesField = typeBuilder.GetField("_employees", BindingFlags.NonPublic | BindingFlags.Instance)!;
            //Type listOfEmployeeVMType = typeof(List<>).MakeGenericType(_vmType);
            //object listOfEmployeeVMInstance = Activator.CreateInstance(listOfEmployeeVMType)!;
            //employeesField.SetValue(employeeVMInstance, listOfEmployeeVMInstance);

            ////typeBuilder = DefineConstructorsAsync(typeCreator, typeBuilder, _employeeType);
            //typeBuilder = DefineMethodsAsync(typeCreator, typeBuilder, iViewModelType, _employeeType, iModelExtendedPropertiesConstructedType);

        }

        [TestMethod]
        public void DefineIModelExtendedPropertiesType_Test()
        {
            // Define assembly and module
            var contextAssemblyName = "Blazor.Tools.BlazorBundler";
            var interfaceAssemblyName = "Blazor.Tools.BlazorBundler.Interfaces";
            var iModelExtendedPropertiesTypeName = "IModelExtendedProperties";
            var version = "3.1.2.0";
            var iModelExtendedPropertiesFullyQualifiedTypeName = $"{interfaceAssemblyName}.{iModelExtendedPropertiesTypeName}";
            var dllPath = Path.Combine(Path.GetTempPath(), $"{contextAssemblyName}.dll");

            var tc = new TypeCreator();
            tc.DefineAssemblyName(contextAssemblyName, version);
            var assemblyBuilder = tc.DefineAssemblyBuilder();
            var moduleBuilder = tc.DefineModuleBuilder();

            // Define the IModelExtendedProperties interface
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                iModelExtendedPropertiesFullyQualifiedTypeName,
                TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

            // Define properties for the interface
            typeBuilder = tc.DefineInterfaceProperty(typeBuilder, "RowID", typeof(int));
            typeBuilder = tc.DefineInterfaceProperty(typeBuilder, "IsEditMode", typeof(bool));
            typeBuilder = tc.DefineInterfaceProperty(typeBuilder, "IsVisible", typeof(bool));
            typeBuilder = tc.DefineInterfaceProperty(typeBuilder, "StartCell", typeof(int));
            typeBuilder = tc.DefineInterfaceProperty(typeBuilder, "EndCell", typeof(int));
            typeBuilder = tc.DefineInterfaceProperty(typeBuilder, "IsFirstCellClicked", typeof(bool));

            // Create the interface type
            Type iModelExtendedPropertiesType = typeBuilder.CreateType();

            // Retrieve and print properties (interfaces don't have actual properties, so this will only show the declared signatures)
            var methods = iModelExtendedPropertiesType.GetMethods();
            foreach (var method in methods)
            {
                AppLogger.WriteInfo($"Method: {method.Name}, Return Type: {method.ReturnType}");
            }

            if (File.Exists(dllPath))
            {
                File.Delete(dllPath);
            }

            assemblyBuilder.Save(dllPath);
        }

        [TestMethod]
        public void CreateEmployeeType_Test()
        {
            /// Define assembly and module
            var contextAssemblyName = "Blazor.Tools.BlazorBundler";
            var modelTypeAssemblyName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models.Employee";
            var employeeTypeName = "Employee";
            var version = "3.1.2.0";
            var employeeFullyQualifiedTypeName = $"{modelTypeAssemblyName}.{employeeTypeName}";
            var dllPath = Path.Combine(Path.GetTempPath(), $"{contextAssemblyName}.dll");

            var tc = new TypeCreator();
            tc.DefineAssemblyName(contextAssemblyName, version);
            var assemblyBuilder = tc.DefineAssemblyBuilder();
            var moduleBuilder = tc.DefineModuleBuilder();

            // Define the Employee class
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                employeeFullyQualifiedTypeName,
                TypeAttributes.Public | TypeAttributes.Class);

            // Define auto-property fields
            typeBuilder = tc.DefineAutoProperty(typeBuilder, "ID", typeof(int));
            typeBuilder = tc.DefineAutoProperty(typeBuilder, "FirstName", typeof(string));
            typeBuilder = tc.DefineAutoProperty(typeBuilder, "MiddleName", typeof(string));
            typeBuilder = tc.DefineAutoProperty(typeBuilder, "LastName", typeof(string));
            typeBuilder = tc.DefineAutoProperty(typeBuilder, "DateOfBirth", typeof(DateOnly));
            typeBuilder = tc.DefineAutoProperty(typeBuilder, "CountryID", typeof(int));

            // Create the type
            Type employeeType = typeBuilder.CreateType();

            var methods = employeeType.GetMethods();
            foreach (var method in methods)
            {
                AppLogger.WriteInfo($"Method: {method.Name}, Return Type: {method.ReturnType}");
            }

            if (File.Exists(dllPath))
            {
                File.Delete(dllPath);
            }

            assemblyBuilder.Save(dllPath);
        }

        [TestMethod]
        public void CreateIEmployeeType_Test()
        {
            // Define assembly and module
            var contextAssemblyName = "Blazor.Tools.BlazorBundler";
            var interfaceAssemblyName = "Blazor.Tools.BlazorBundler.Interfaces";
            var iEmployeeTypeName = "IEmployee";
            var version = "3.1.2.0";
            var iEmployeeFullyQualifiedTypeName = $"{interfaceAssemblyName}.{iEmployeeTypeName}";
            var dllPath = Path.Combine(Path.GetTempPath(), $"{contextAssemblyName}.dll");

            var tc = new TypeCreator();
            tc.DefineAssemblyName(contextAssemblyName, version);
            var assemblyBuilder = tc.DefineAssemblyBuilder();
            var moduleBuilder = tc.DefineModuleBuilder();

            // Define the IEmployee interface
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                iEmployeeFullyQualifiedTypeName,
                TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

            // Define properties for the interface
            typeBuilder = tc.DefineInterfaceProperty(typeBuilder, "ID", typeof(int));
            typeBuilder = tc.DefineInterfaceProperty(typeBuilder, "FirstName", typeof(string));
            typeBuilder = tc.DefineInterfaceProperty(typeBuilder, "MiddleName", typeof(string));
            typeBuilder = tc.DefineInterfaceProperty(typeBuilder, "LastName", typeof(string));
            typeBuilder = tc.DefineInterfaceProperty(typeBuilder, "DateOfBirth", typeof(DateOnly));
            typeBuilder = tc.DefineInterfaceProperty(typeBuilder, "CountryID", typeof(int));

            // Create the interface type
            Type iEmployeeType = typeBuilder.CreateType();

            // Retrieve and print properties (interfaces don't have actual properties, so this will only show the declared signatures)
            var methods = iEmployeeType.GetMethods();
            foreach (var method in methods)
            {
                AppLogger.WriteInfo($"Method: {method.Name}, Return Type: {method.ReturnType}");
            }

            if (File.Exists(dllPath))
            {
                File.Delete(dllPath);
            }

            assemblyBuilder.Save(dllPath);
        }

        [TestMethod]
        public void CreateIViewModelInterface_Test()
        {
            // Define assembly and module
            AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");

            // Define the IViewModel<TModel, TIModel> interface
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                "Blazor.Tools.BlazorBundler.Interfaces.IViewModel`2", // `2 indicates two generic parameters
                TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

            // Define generic parameters TModel and TIModel
            GenericTypeParameterBuilder[] genericParams = typeBuilder.DefineGenericParameters("TModel", "TIModel");

            // Define methods in the interface
            DefineInterfaceMethod(typeBuilder, "ToNewModel", genericParams[0]); // TModel ToNewModel();
            DefineInterfaceMethod(typeBuilder, "ToNewIModel", genericParams[1]); // TIModel ToNewIModel();
            DefineInterfaceTaskMethod(typeBuilder, "FromModel", typeof(Task<>).MakeGenericType(typeBuilder), genericParams[0]); // Task<IViewModel<TModel, TIModel>> FromModel(TModel model);
            DefineInterfaceTaskMethod(typeBuilder, "SetEditMode", typeof(Task<>).MakeGenericType(typeBuilder), typeof(bool)); // Task<IViewModel<TModel, TIModel>> SetEditMode(bool isEditMode);
            DefineInterfaceTaskMethod(typeBuilder, "SaveModelVM", typeof(Task<>).MakeGenericType(typeBuilder)); // Task<IViewModel<TModel, TIModel>> SaveModelVM();
            DefineInterfaceTaskMethod(typeBuilder, "SaveModelVMToNewModelVM", typeof(Task<>).MakeGenericType(typeBuilder)); // Task<IViewModel<TModel, TIModel>> SaveModelVMToNewModelVM();

            // Define method with IEnumerable<IViewModel<TModel, TIModel>> as return type and parameter
            Type iEnumerableViewModelType = typeof(IEnumerable<>).MakeGenericType(typeBuilder);
            DefineInterfaceTaskMethod(typeBuilder, "AddItemToList", typeof(Task<>).MakeGenericType(iEnumerableViewModelType), iEnumerableViewModelType);
            DefineInterfaceTaskMethod(typeBuilder, "UpdateList", typeof(Task<>).MakeGenericType(iEnumerableViewModelType), iEnumerableViewModelType, typeof(bool));
            DefineInterfaceTaskMethod(typeBuilder, "DeleteItemFromList", typeof(Task<>).MakeGenericType(iEnumerableViewModelType), iEnumerableViewModelType);

            // Create the interface type
            Type iViewModelType = typeBuilder.CreateType();

            // Output the methods in the created interface
            var methods = iViewModelType.GetMethods();
            foreach (var method in methods)
            {
                AppLogger.WriteInfo($"Method: {method.Name}, Return Type: {method.ReturnType}");
            }
        }
        
        private ModuleBuilder CreateEmployeeType(ModuleBuilder moduleBuilder)
        {
            var contextAssemblyName = "Blazor.Tools.BlazorBundler";
            var modelTypeAssemblyName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models";
            var iModelTypeAssemblyName = "Blazor.Tools.BlazorBundler.Interfaces";
            var modelTypeName = "Employee";
            var iModelTypeName = "IEmployee";
            var version = "3.1.2.0";

            var tc = new TypeCreator(contextAssemblyName, modelTypeAssemblyName, modelTypeName, iModelTypeAssemblyName, iModelTypeName, version, AssemblyBuilderAccess.Run );

            // Define the Employee class
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models.Employee",
                TypeAttributes.Public | TypeAttributes.Class);

            // Define fields
            FieldBuilder idField = typeBuilder.DefineField("ID", typeof(int), FieldAttributes.Public);
            FieldBuilder firstNameField = typeBuilder.DefineField("FirstName", typeof(string), FieldAttributes.Public);
            FieldBuilder middleNameField = typeBuilder.DefineField("MiddleName", typeof(string), FieldAttributes.Public);
            FieldBuilder lastNameField = typeBuilder.DefineField("LastName", typeof(string), FieldAttributes.Public);
            FieldBuilder dateOfBirthField = typeBuilder.DefineField("DateOfBirth", typeof(DateOnly), FieldAttributes.Public);
            FieldBuilder countryIdField = typeBuilder.DefineField("CountryID", typeof(int), FieldAttributes.Public);

            // Define properties
            DefineClassProperty(typeBuilder, "ID", typeof(int));
            DefineClassProperty(typeBuilder, "FirstName", typeof(string));
            DefineClassProperty(typeBuilder, "MiddleName", typeof(string));
            DefineClassProperty(typeBuilder, "LastName", typeof(string));
            DefineClassProperty(typeBuilder, "DateOfBirth", typeof(DateOnly));
            DefineClassProperty(typeBuilder, "CountryID", typeof(int));

            // Create the type
            var employeeType = typeBuilder.CreateType();

            // Retrieve and print properties
            var properties = employeeType.GetProperties();
            foreach (var prop in properties)
            {
                AppLogger.WriteInfo($"Property: {prop.Name}, Type: {prop.PropertyType}");
            }

            return moduleBuilder;
        }

        private ModuleBuilder CreateIViewModelType(ModuleBuilder moduleBuilder, string typeName)
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
            var iViewModelCreatedType = typeBuilder.CreateTypeInfo().AsType();

            return moduleBuilder;
        }

        private void DefineInterfaceMethod(TypeBuilder typeBuilder, string methodName, Type returnType)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName,
                MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                returnType, Type.EmptyTypes);
        }

        private void DefineInterfaceTaskMethod(TypeBuilder typeBuilder, string methodName, Type returnType, params Type[] parameterTypes)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName,
                MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                returnType, parameterTypes);
        }

        private void DefineClassProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = typeBuilder.DefineField($"_{propertyName.ToLower()}", propertyType, FieldAttributes.Private);
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            MethodBuilder getPropMethodBuilder = typeBuilder.DefineMethod($"get_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                propertyType, Type.EmptyTypes);

            ILGenerator getIl = getPropMethodBuilder.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMethodBuilder = typeBuilder.DefineMethod($"set_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                null, new Type[] { propertyType });

            ILGenerator setIl = setPropMethodBuilder.GetILGenerator();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMethodBuilder);
            propertyBuilder.SetSetMethod(setPropMethodBuilder);
        }
         
        // Method to define the iViewModelMethods of IViewModel<TModel, TIModel> interface
        private TypeBuilder DefineViewModelMethods(TypeCreator typeCreator, TypeBuilder typeBuilder)
        {
            // Define the return type and method signature for each method

            // Method: TModel ToNewModel()
            typeBuilder = typeCreator.DefineMethod(typeBuilder, "ToNewModel", typeof(object), Type.EmptyTypes);

            // Method: TIModel ToNewIModel()
            typeBuilder = typeCreator.DefineMethod(typeBuilder, "ToNewIModel", typeof(object), Type.EmptyTypes);

            // Method: Task<IViewModel<TModel, TIModel>> FromModel(TModel model)
            typeBuilder = typeCreator.DefineMethod(typeBuilder, "FromModel", typeof(Task<>).MakeGenericType(typeBuilder), new[] { typeof(object) });

            // Method: Task<IViewModel<TModel, TIModel>> SetEditMode(bool isEditMode)
            typeBuilder = typeCreator.DefineMethod(typeBuilder, "SetEditMode", typeof(Task<>).MakeGenericType(typeBuilder), new[] { typeof(bool) });

            // Method: Task<IViewModel<TModel, TIModel>> SaveModelVM()
            typeBuilder = typeCreator.DefineMethod(typeBuilder, "SaveModelVM", typeof(Task<>).MakeGenericType(typeBuilder), Type.EmptyTypes);

            // Method: Task<IViewModel<TModel, TIModel>> SaveModelVMToNewModelVM()
            typeBuilder = typeCreator.DefineMethod(typeBuilder, "SaveModelVMToNewModelVM", typeof(Task<>).MakeGenericType(typeBuilder), Type.EmptyTypes);

            // Method: Task<IEnumerable<IViewModel<TModel, TIModel>>> AddItemToList(IEnumerable<IViewModel<TModel, TIModel>> modelVMList)
            typeBuilder = typeCreator.DefineMethod(typeBuilder, "AddItemToList", typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(typeBuilder)), new[] { typeof(IEnumerable<>).MakeGenericType(typeBuilder) });

            // Method: Task<IEnumerable<IViewModel<TModel, TIModel>>> UpdateList(IEnumerable<IViewModel<TModel, TIModel>> modelVMList, bool isAdding)
            typeBuilder = typeCreator.DefineMethod(typeBuilder, "UpdateList", typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(typeBuilder)), new[] { typeof(IEnumerable<>).MakeGenericType(typeBuilder), typeof(bool) });

            // Method: Task<IEnumerable<IViewModel<TModel, TIModel>>> DeleteItemFromList(IEnumerable<IViewModel<TModel, TIModel>> modelVMList)
            typeBuilder = typeCreator.DefineMethod(typeBuilder, "DeleteItemFromList", typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(typeBuilder)), new[] { typeof(IEnumerable<>).MakeGenericType(typeBuilder) });

            return typeBuilder;
        }

        public TypeBuilder DefineFields(TypeCreator typeCreator, TypeBuilder typeBuilder)
        {
            //_employeeListField = typeBuilder.DefineField("_employees", typeof(object), FieldAttributes.Private);
            //_contextProviderField = typeBuilder.DefineField("_contextProvider", typeof(IContextProvider), FieldAttributes.Private);

            // Define the field for the contextProvider only once
            var employeesType = typeof(List<>).MakeGenericType(typeBuilder)!;
            var employeesTypeName = employeesType.FullName!;
            var contextProviderType = typeof(IContextProvider)!;
            var contextProviderTypeName = contextProviderType.FullName!;
            _employeeListField = typeCreator.DefineField(employeesTypeName, typeBuilder,"_employees", employeesType);
            _contextProviderField = typeCreator.DefineField(contextProviderTypeName, typeBuilder, "_contextProvider", contextProviderType);

            return typeBuilder;
        }

        public TypeBuilder DefineConstructors(TypeCreator typeCreator, TypeBuilder typeBuilder, Type modelType)
        {

            //_employeeListField = typeBuilder.DefineField("_employees", typeof(List<>).MakeGenericType(typeBuilder), FieldAttributes.Private);
            //_contextProviderField = typeBuilder.DefineField("_contextProvider", typeof(IContextProvider), FieldAttributes.Private);

            // Define constructors
            // Parameterless constructor
            typeBuilder = typeCreator.DefineConstructor("parameterless", typeBuilder, Type.EmptyTypes, ilg =>
            {
                // Call the base constructor
                ConstructorInfo baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Call, baseConstructor);

                // Step 1: Initialize _employees 
                ilg.Emit(OpCodes.Ldarg_0);
                var employeeListConstructor = typeof(List<>).MakeGenericType(typeof(object)).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Newobj, employeeListConstructor);
                ilg.Emit(OpCodes.Stfld, _employeeListField);

                // Step 2: Initialize _contextProvider
                ilg.Emit(OpCodes.Ldarg_0);
                var contextProviderConstructor = typeof(ContextProvider).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Newobj, contextProviderConstructor);
                ilg.Emit(OpCodes.Stfld, _contextProviderField); // Store it in the _contextProvider field

                // Step 3: Initialize other fields (IModelExtendedProperties fields)
                if (typeCreator.Fields != null)
                {
                    foreach ((string typeName, FieldBuilder fieldBuilder) in typeCreator.Fields)
                    {
                        if (typeName.Contains("IModelExtendedProperties"))
                        {
                            ilg.Emit(OpCodes.Ldarg_0);
                            ilg.Emit(OpCodes.Ldc_I4_0); // Initialize int/boolean fields with 0/false
                            ilg.Emit(OpCodes.Stfld, fieldBuilder); // Store default values in fields
                        }
                    }
                }

                // Complete constructor by emitting return instruction
                ilg.Emit(OpCodes.Ret);

                Console.WriteLine("Constructor for EmployeeVM defined.");
            });

            // Constructor with IContextProvider parameter
            typeBuilder = typeCreator.DefineConstructor("with_IContextProvider", typeBuilder, new[] { typeof(IContextProvider) }, ilg =>
            {
                ConstructorInfo baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Ldarg_0); // Load 'this'
                ilg.Emit(OpCodes.Call, baseConstructor); // Call base constructor

                // Initialize _employeeList as List<object> (since EmployeeVM is not yet available)
                ilg.Emit(OpCodes.Ldarg_0); // Load 'this'
                ConstructorInfo objectListConstructor = typeof(List<object>).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Newobj, objectListConstructor); // Create new List<object>
                ilg.Emit(OpCodes.Stfld, _employeeListField); // Store in _employeeList field

                // SetI _contextProvider field
                ilg.Emit(OpCodes.Ldarg_0); // Load 'this'
                ilg.Emit(OpCodes.Ldarg_1); // Load the constructor parameter (IContextProvider)
                ilg.Emit(OpCodes.Stfld, _contextProviderField); // Store in _contextProvider field

                // Initialize other fields (e.g., IModelExtendedProperties)
                if (typeCreator.Fields != null)
                {
                    foreach ((string typeName, FieldBuilder fieldBuilder) in typeCreator.Fields)
                    {
                        if (typeName.Contains("IModelExtendedProperties"))
                        {
                            ilg.Emit(OpCodes.Ldarg_0); // Load 'this'
                            ilg.Emit(OpCodes.Ldc_I4_0); // Load the default value (0 for int, false for bool)
                            ilg.Emit(OpCodes.Stfld, fieldBuilder); // Store in the field
                        }
                    }
                }

                ilg.Emit(OpCodes.Ret); // Return from constructor
                Console.WriteLine("Constructor with IContextProvider parameter defined.");
            });

            // Constructor with IContextProvider and Employee parameter
            typeBuilder = typeCreator.DefineConstructor("with_IContextProvider_Employee", typeBuilder, new[] { typeof(IContextProvider), modelType }, ilg =>
            {
                ConstructorInfo baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Call, baseConstructor);

                // SetI contextProvider field
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ldarg_1);
                ilg.Emit(OpCodes.Stfld, _contextProviderField);

                // SetI _modelProperties from Employee model
                ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                ilg.Emit(OpCodes.Ldarg_2); // Load Employee model

                if (typeCreator.Properties != null)
                {
                    foreach ((string typeName, PropertyBuilder prop) in typeCreator.Properties)
                    {
                        if (modelType.FullName == typeName)
                        {
                            if (prop.CanWrite)
                            {
                                ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                                ilg.Emit(OpCodes.Ldarg_2); // Load Employee model
                                ilg.Emit(OpCodes.Callvirt, prop.GetGetMethod()!); // Get property value
                                ilg.Emit(OpCodes.Callvirt, prop.GetSetMethod()!); // SetI property value
                            }

                        }

                    }
                }

                ilg.Emit(OpCodes.Ret);

                Console.WriteLine("Constructor with IContextProvider and Employee parameter defined.");
            });

            // Constructor with IContextProvider and EmployeeVM parameter
            typeBuilder = typeCreator.DefineConstructor("with_IContextProvider_EmployeeVM", typeBuilder, new[] { typeof(IContextProvider), typeBuilder }, ilg =>
            {
                ConstructorInfo baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                ilg.Emit(OpCodes.Call, baseConstructor); // Call base class constructor

                // SetI _contextProvider field
                ilg.Emit(OpCodes.Ldarg_0);  // Load "this"
                ilg.Emit(OpCodes.Ldarg_1);  // Load contextProvider
                ilg.Emit(OpCodes.Stfld, _contextProviderField);  // Store contextProvider in _contextProvider

                // SetI properties from modelVM
                if (typeCreator.Properties != null)
                {
                    foreach ((string typeName, PropertyBuilder prop) in typeCreator.Properties)
                    {
                        // Skip the cast if we're dealing with IModelExtendedProperties or similar
                        bool isIModelProperty = typeName.Contains("IModelExtendedProperties");
                        bool isEmployeeVMProperty = typeName == modelType.FullName;

                        if ((isIModelProperty || isEmployeeVMProperty) && prop.CanWrite)
                        {
                            // Load "this" (target object)
                            ilg.Emit(OpCodes.Ldarg_0);

                            // Load modelVM (the second argument passed into the constructor)
                            ilg.Emit(OpCodes.Ldarg_2);  // Load modelVM

                            // Call the getter on modelVM
                            ilg.Emit(OpCodes.Callvirt, prop.GetGetMethod()!);  // Call modelVM's getter for the property

                            // Call the setter on the target object
                            ilg.Emit(OpCodes.Callvirt, prop.GetSetMethod()!);  // SetI the target's property
                        }
                    }
                }

                ilg.Emit(OpCodes.Ret);  // Return from constructor

                Console.WriteLine("Constructor with IContextProvider and EmployeeVM parameter defined.");
            });

            //// Verify all constructors
            //var _vmType = modelVMClassBuilder.CreateType(); // Create the final VM class

            //// Log all constructors to ensure they are defined
            //var constructors = _vmType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //foreach (var ctor in constructors)
            //{
            //    var parameters = ctor.GetParameters();
            //    Console.WriteLine($"Constructor: {ctor.Name}, Parameters: {string.Join(", ", parameters.Select(p => p.ParameterType.Name))}");
            //}

            return typeBuilder;
        }

        public TypeBuilder DefineMethods(TypeCreator typeCreator, TypeBuilder typeBuilder, Type iViewModelType, Type modelType, Type iModelType)
        {
            typeBuilder = typeCreator.DefineMethod(typeBuilder, "Clone", typeBuilder, Type.EmptyTypes, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // 1. Create a new instance of EmployeeVM using its constructor (with _contextProvider)
                ConstructorInfo employeeVMConstructor = typeCreator?.Constructors?.FirstOrDefault(c=> c.ConstructorName == "parameterless").ConstructorBuilder!; 
                FieldInfo contextProviderField = typeCreator?.Fields?.FirstOrDefault(f => f.TypeName == "_contextProvider").FieldBuilder!; // Get _contextProvider field

                // Load 'this' to access _contextProvider and push it on the evaluation stack
                ilg.Emit(OpCodes.Ldarg_0); // Load 'this'
                ilg.Emit(OpCodes.Ldfld, contextProviderField); // Load _contextProvider field

                // Call the constructor of EmployeeVM (passing _contextProvider)
                ilg.Emit(OpCodes.Newobj, employeeVMConstructor); // Create a new EmployeeVM

                // 2. Store the new instance in a local variable
                LocalBuilder cloneInstance = ilg.DeclareLocal(typeBuilder); // Declare local variable for new EmployeeVM
                ilg.Emit(OpCodes.Stloc, cloneInstance); // Store the new instance in the local variable

                // 3. Assign properties from 'this' to the clone instance
                // The following properties are assigned: IsEditMode, IsVisible, IsFirstCellClicked, StartCell, EndCell, RowID, ID, FirstName, MiddleName, LastName, DateOfBirth, CountryID

                if (typeCreator?.Properties != null)
                {
                    foreach ((string typeName, PropertyBuilder prop) in typeCreator.Properties)
                    {
                        if (typeName == modelType.FullName || typeName == iModelType.FullName)
                        {
                            if (prop.CanWrite)
                            {
                                ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                                ilg.Emit(OpCodes.Ldarg_2); // Load Employee model
                                ilg.Emit(OpCodes.Callvirt, prop.GetGetMethod()!); // Get property value
                                ilg.Emit(OpCodes.Callvirt, prop.GetSetMethod()!); // SetI property value
                            }

                        }

                    }
                }

                // 4. Return the cloned instance
                ilg.Emit(OpCodes.Ldloc, cloneInstance); // Load the cloned instance
                ilg.Emit(OpCodes.Ret); // Return the cloned instance
            });

            typeBuilder = typeCreator.DefineMethod(typeBuilder, "SetList", typeof(void), new[] { typeof(List<EmployeeVM>) }, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Load 'this' onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_0); // Load 'this'

                // Load the 'items' argument onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_1); // Load items (List<EmployeeVM>)

                // SetI the _employees field with the 'items' argument
                FieldInfo employeesField = typeCreator?.Fields?.FirstOrDefault(f => f.TypeName == "_employees").FieldBuilder!; 
                ilg.Emit(OpCodes.Stfld, employeesField); // Store 'items' into _employees

                // Return from the method
                ilg.Emit(OpCodes.Ret);
            });

            // Define the Validate method dynamically
            typeBuilder = typeCreator.DefineMethod(typeBuilder, "Validate", typeof(IEnumerable<ValidationResult>), new[] { typeof(ValidationContext) }, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Declare labels for branching
                Label notNullLabel = ilg.DefineLabel(); // If _employees is not null
                Label endOfMethod = ilg.DefineLabel(); // End of method (yield break)

                // Load 'this' and check if _employees is null
                FieldInfo employeesField = typeCreator?.Fields?.FirstOrDefault(f => f.TypeName == "_employees").FieldBuilder!;
                ilg.Emit(OpCodes.Ldarg_0); // Load 'this'
                ilg.Emit(OpCodes.Ldfld, employeesField); // Load _employees field

                // If _employees is not null, branch to notNullLabel
                ilg.Emit(OpCodes.Brtrue_S, notNullLabel); // If _employees != null, continue

                // _employees is null: return early (yield break)
                ilg.Emit(OpCodes.Br, endOfMethod); // Jump to end

                // Mark the point where _employees is not null
                ilg.MarkLabel(notNullLabel);

                // Placeholder for additional validation logic (like calling AlreadyExists)
                // ...

                // End of method (exit point)
                ilg.MarkLabel(endOfMethod);
                ilg.Emit(OpCodes.Ret);
            });

            typeBuilder = typeCreator.DefineMethod(typeBuilder, "AlreadyExists", typeof(bool), new[] { typeof(string), typeof(int) }, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Declare local variables
                LocalBuilder alreadyExists = ilg.DeclareLocal(typeof(bool)); // bool alreadyExists
                LocalBuilder foundItem = ilg.DeclareLocal(typeBuilder); // EmployeeVM foundItem

                // Initialize alreadyExists = false
                ilg.Emit(OpCodes.Ldc_I4_0); // Push 'false' onto the stack
                ilg.Emit(OpCodes.Stloc, alreadyExists); // Store it in alreadyExists

                // Check if 'name' is not null
                Label nameNotNullLabel = ilg.DefineLabel();
                ilg.Emit(OpCodes.Ldarg_1); // Load 'name'
                ilg.Emit(OpCodes.Brtrue_S, nameNotNullLabel); // Branch if 'name' != null

                // If 'name' is null, return false
                ilg.Emit(OpCodes.Ldloc, alreadyExists); // Load alreadyExists
                ilg.Emit(OpCodes.Ret); // Return alreadyExists (false)

                // Mark the point where 'name' is not null
                ilg.MarkLabel(nameNotNullLabel);

                // Load _employees and call FirstOrDefault

                MethodInfo firstOrDefaultMethod = typeof(Enumerable).GetMethods()
                .Where(m => m.Name == "FirstOrDefault" && m.GetParameters().Length == 2)
                .Where(m =>
                {
                    var parameters = m.GetParameters();
                    // Ensure the second parameter is a Func<,>
                    return parameters[1].ParameterType.IsGenericType &&
                           parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>);
                })
                .Single()
                .MakeGenericMethod(typeBuilder);  // Apply your generic type

                // Create a lambda expression for the FirstOrDefault call
                // p => p.FirstName == name && p.ID != currentItemId
                MethodInfo firstNameGetter = typeCreator?.Properties?.FirstOrDefault(p => p.TypeName == "FirstName").PropertyBuilder?.GetGetMethod()!;
                MethodInfo idGetter = typeCreator?.Properties?.FirstOrDefault(p => p.TypeName == "ID").PropertyBuilder?.GetGetMethod()!;
                FieldInfo employeesField = typeCreator?.Fields?.FirstOrDefault(f => f.TypeName == "_employees").FieldBuilder!;

                // Load _employees
                ilg.Emit(OpCodes.Ldarg_0); // Load 'this'
                ilg.Emit(OpCodes.Ldfld, employeesField); // Load _employees

                // Push lambda expression to the evaluation stack
                // You'd need to emit IL code for the expression "p => p.FirstName == name && p.ID != currentItemId"
                // (Details of this lambda emission can be complex and depends on how you're generating dynamic expressions)

                // Call FirstOrDefault on _employees
                ilg.Emit(OpCodes.Call, firstOrDefaultMethod);

                // Check if foundItem != null
                ilg.Emit(OpCodes.Stloc, foundItem); // Store the result into foundItem
                ilg.Emit(OpCodes.Ldloc, foundItem); // Load foundItem onto the evaluation stack
                Label setAlreadyExistsTrue = ilg.DefineLabel();
                ilg.Emit(OpCodes.Brtrue_S, setAlreadyExistsTrue); // If foundItem != null, branch to set alreadyExists = true

                // Mark the label for setting alreadyExists to true
                ilg.MarkLabel(setAlreadyExistsTrue);
                ilg.Emit(OpCodes.Ldc_I4_1); // Load 'true'
                ilg.Emit(OpCodes.Stloc, alreadyExists); // SetI alreadyExists = true

                // Return alreadyExists
                ilg.Emit(OpCodes.Ldloc, alreadyExists); // Load alreadyExists
                ilg.Emit(OpCodes.Ret); // Return alreadyExists
            });
            
            // Define the ToNewModel
            typeBuilder = typeCreator.DefineMethod(typeBuilder, "ToNewModel", modelType, Type.EmptyTypes, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Declare a local variable to hold the new instance of Employee
                LocalBuilder employeeLocal = ilg.DeclareLocal(modelType); // Assume modelType is Employee

                // Find constructor of Employee
                ConstructorInfo employeeConstructor = modelType.GetConstructor(Type.EmptyTypes)
                    ?? throw new InvalidOperationException($"No parameterless constructor found for type {modelType.Name}");

                // Emit code to create a new instance of Employee and store it in local variable
                ilg.Emit(OpCodes.Newobj, employeeConstructor);
                ilg.Emit(OpCodes.Stloc, employeeLocal);

                if (typeCreator?.Properties != null)
                {
                    foreach ((string typeName, PropertyBuilder prop) in typeCreator.Properties)
                    {
                        if (typeName == modelType.FullName && prop.CanWrite)
                        {
                            // Load the new Employee instance onto the stack
                            ilg.Emit(OpCodes.Ldloc, employeeLocal);
                            // Load 'this' (the current instance) onto the stack
                            ilg.Emit(OpCodes.Ldarg_0);
                            ilg.Emit(OpCodes.Callvirt, prop.GetGetMethod()!); // Get property value
                            ilg.Emit(OpCodes.Callvirt, prop.GetSetMethod()!); // SetI property value

                        }

                    }
                }

                // Load the new Employee instance onto the stack and return it
                ilg.Emit(OpCodes.Ldloc, employeeLocal);
                ilg.Emit(OpCodes.Ret);
            });

            typeBuilder = typeCreator.DefineMethod(typeBuilder, "ToNewIModel", iModelType, Type.EmptyTypes, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Get the constructor of the type (with IContextProvider)
                ConstructorInfo? constructor = typeCreator?.Constructors?.FirstOrDefault(c => c.ConstructorName == "with_IContextProvider").ConstructorBuilder;
                if (constructor == null)
                {
                    throw new InvalidOperationException($"No constructor with IContextProvider found for type {_vmType.Name}");
                }

                // Load 'this._contextProvider' (load the current instance's context provider)
                ilg.Emit(OpCodes.Ldarg_0); // Load 'this'
                ilg.Emit(OpCodes.Ldfld, _contextProviderField); // Load _contextProvider field

                // Call the constructor to create a new instance of EmployeeVM with contextProvider
                ilg.Emit(OpCodes.Newobj, constructor); // Create new EmployeeVM with _contextProvider
                LocalBuilder newInstance = ilg.DeclareLocal(typeBuilder); // Declare a local variable to hold the new instance
                ilg.Emit(OpCodes.Stloc, newInstance); // Store the new instance in the local variable

                // SetI properties on the new instance (from the current instance)
                foreach ((string typeName, PropertyBuilder prop) in typeCreator?.Properties!)
                {
                    if ((typeName == iModelType.FullName || typeName == modelType.FullName) && prop.CanWrite)
                    {
                        // Load the new EmployeeVM instance onto the stack
                        ilg.Emit(OpCodes.Ldloc, newInstance); // Load new instance

                        // Load 'this' (the current instance) onto the stack
                        ilg.Emit(OpCodes.Ldarg_0);
                        ilg.Emit(OpCodes.Callvirt, prop.GetGetMethod()!); // Get property value
                        ilg.Emit(OpCodes.Callvirt, prop.GetSetMethod()!); // SetI property value
                    }
                }

                // Load the new instance onto the stack and return it
                ilg.Emit(OpCodes.Ldloc, newInstance); // Load new instance
                ilg.Emit(OpCodes.Ret); // Return the new instance
            });

            // Define the method in the dynamic class builder
            typeBuilder = typeCreator.DefineMethod(typeBuilder, "FromModel", typeof(Task<>).MakeGenericType(iViewModelType), new[] { modelType }, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Load 'this' onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_0);

                // Load the model argument onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_1);

                // Get the 'FromModel' method from the IViewModel<TModel, TIModel> interface
                var method = iViewModelType.GetMethod("FromModel");

                if (method == null)
                {
                    throw new InvalidOperationException($"No method found with name 'FromModel' in type {typeBuilder.Name}");
                }

                // Call the 'FromModel' method
                ilg.Emit(OpCodes.Callvirt, method);

                // Convert the result to Task<IViewModel<TModel, TIModel>>
                ilg.Emit(OpCodes.Castclass, typeof(Task<>).MakeGenericType(iViewModelType));

                // Return the result
                ilg.Emit(OpCodes.Ret);
            });

            typeBuilder = typeCreator.DefineMethod(typeBuilder, "SetEditMode", typeof(Task<>).MakeGenericType(iViewModelType), new[] { typeof(bool) }, new[] { "isEditMode" }, (ilg, localBuilder) =>
            {
                // Load 'this' onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_0);

                // Load the isEditMode argument onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_1);

                // SetI the IsEditMode property
                var isEditModeItem = typeCreator.Properties?.FirstOrDefault(p => p.PropertyBuilder.Name.Contains("IsEditMode"));
                var isEditModeProperty = isEditModeItem?.PropertyBuilder;
                if (isEditModeProperty == null)
                {
                    throw new InvalidOperationException("IsEditMode property not found.");
                }

                ilg.Emit(OpCodes.Callvirt, isEditModeProperty?.GetSetMethod() ?? default!);

                // Emit async completion
                var completedTaskProperty = typeof(Task).GetProperty(nameof(Task.CompletedTask));
                if (completedTaskProperty == null)
                {
                    throw new InvalidOperationException("CompletedTask property not found.");
                }

                ilg.Emit(OpCodes.Call, completedTaskProperty?.GetGetMethod() ?? default!);
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ret);
            });

            typeBuilder = typeCreator.DefineMethod(typeBuilder, "SaveModelVM", typeof(Task<>).MakeGenericType(iViewModelType), Type.EmptyTypes, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Load 'this' onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_0);

                // SetI the IsEditMode property to false
                var isEditModeItem = typeCreator.Properties?.FirstOrDefault(p => p.PropertyBuilder.Name.Contains("IsEditMode"));
                var isEditModeProperty = isEditModeItem?.PropertyBuilder;
                if (isEditModeProperty == null)
                {
                    throw new InvalidOperationException("IsEditMode property not found.");
                }

                ilg.Emit(OpCodes.Ldc_I4_0);
                ilg.Emit(OpCodes.Callvirt, isEditModeProperty?.GetSetMethod() ?? default!);

                // Emit async completion
                var completedTaskProperty = typeof(Task).GetProperty(nameof(Task.CompletedTask));
                if (completedTaskProperty == null)
                {
                    throw new InvalidOperationException("CompletedTask property not found.");
                }

                ilg.Emit(OpCodes.Call, completedTaskProperty?.GetGetMethod() ?? default!);
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ret);
            });

            typeBuilder = typeCreator.DefineMethod(typeBuilder, "SaveModelVMToNewModelVM", typeof(Task<>).MakeGenericType(iViewModelType), Type.EmptyTypes, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Load 'this' onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_0);

                // Get all defined constructors
                var constructors = typeCreator.GetDefinedConstructors();

                foreach (var (name, ctor, parameters) in constructors)
                {
                    Console.WriteLine($"Constructor: {name}, Parameters: {string.Join(", ", parameters.Select(p => p.Name))}");
                }

                // Identify the constructor with IContextProvider
                var constructor = constructors.FirstOrDefault(c =>
                    c.ParameterTypes.Length == 1 && c.ParameterTypes[0] == typeof(IContextProvider));

                if (constructor == default)
                {
                    throw new InvalidOperationException("Constructor with IContextProvider not found.");
                }

                // Create a new instance of modelType using the identified constructor
                ilg.Emit(OpCodes.Ldarg_1); // Load the IContextProvider argument
                ilg.Emit(OpCodes.Newobj, constructor.ConstructorBuilder); // Call the constructor to create a new instance

                // Store the new instance in a local variable
                var newInstance = ilg.DeclareLocal(modelType);
                ilg.Emit(OpCodes.Stloc, newInstance);

                // Load _modelProperties from 'this' and set them on the new instance
                if (typeCreator.Properties != null)
                {
                    foreach ((string typeName, PropertyBuilder prop) in typeCreator.Properties)
                    {
                        if (new[] { modelType.FullName, iModelType.FullName }.Contains(typeName) && prop.CanWrite)
                        {
                            // Load the new instance
                            ilg.Emit(OpCodes.Ldloc, newInstance);

                            // Load property value from 'this'
                            ilg.Emit(OpCodes.Ldarg_0);
                            ilg.Emit(OpCodes.Callvirt, prop?.GetGetMethod() ?? default!);

                            // SetI the property on the new instance
                            ilg.Emit(OpCodes.Callvirt, prop?.GetSetMethod() ?? default!);
                        }
                    }

                    // Emit async completion
                    var completedTaskProperty = typeof(Task).GetProperty(nameof(Task.CompletedTask));
                    if (completedTaskProperty == null)
                    {
                        throw new InvalidOperationException("CompletedTask property not found.");
                    }

                    ilg.Emit(OpCodes.Call, completedTaskProperty?.GetGetMethod() ?? default!);

                    // Load the new instance and return it as Task<IViewModel<TModel, TIModel>>
                    ilg.Emit(OpCodes.Ldloc, newInstance);
                    ilg.Emit(OpCodes.Ret);
                }
            });

            typeBuilder = typeCreator.DefineMethod(typeBuilder, "AddItemToList", typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(iViewModelType)), new[] { typeof(IEnumerable<>).MakeGenericType(iViewModelType) }, new[] { "modelVMList" }, (ilg, localBuilder) =>
            {
                // Load the modelVMList argument
                ilg.Emit(OpCodes.Ldarg_1);

                // Create a list from the argument
                var toListMethod = typeof(Enumerable).GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(iViewModelType) ?? default!;
                ilg.Emit(OpCodes.Call, toListMethod);

                // Add 'this' to the list
                ilg.Emit(OpCodes.Ldloc_0);
                ilg.Emit(OpCodes.Ldarg_0);
                var method = iViewModelType.GetMethod("FromModel");

                if (method == null)
                {
                    throw new InvalidOperationException($"No method found with name 'FromModel' in type {typeBuilder.Name}");
                }
                ilg.Emit(OpCodes.Callvirt, method);

                // Emit async completion
                var completedTaskProperty = typeof(Task).GetProperty(nameof(Task.CompletedTask));
                if (completedTaskProperty == null)
                {
                    throw new InvalidOperationException("CompletedTask property not found.");
                }

                ilg.Emit(OpCodes.Call, completedTaskProperty?.GetGetMethod() ?? default!);
                ilg.Emit(OpCodes.Ldloc_0);
                ilg.Emit(OpCodes.Ret);
            });

            typeBuilder = typeCreator.DefineMethod(typeBuilder, "UpdateList", typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(iViewModelType)), new[] { typeof(IEnumerable<>).MakeGenericType(iViewModelType), typeof(bool) }, new[] { "modelVMList", "isAdding" }, (ilg, localBuilder) =>
            {
                // Convert modelVMList to List
                ilg.Emit(OpCodes.Ldarg_1);
                var toListMethod = typeof(Enumerable).GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(iViewModelType) ?? default!;
                ilg.Emit(OpCodes.Call, toListMethod);

                // Store the list in a local variable
                var listLocal = ilg.DeclareLocal(typeof(List<>).MakeGenericType(iViewModelType));
                ilg.Emit(OpCodes.Stloc, listLocal);

                // If isAdding, remove 'this' from the list
                var addLabel = ilg.DefineLabel();
                ilg.Emit(OpCodes.Ldarg_2);  // Load isAdding argument
                ilg.Emit(OpCodes.Brtrue_S, addLabel); // If true, jump to adding logic

                // Remove 'this' from the list
                //ilg.Emit(OpCodes.Ldloc, listLocal);
                //ilg.Emit(OpCodes.Ldarg_0); // Load 'this'
                //var genericType = typeof(List<>).MakeGenericType(iViewModelType);
                //var removeMethod = genericType.GetMethod("Remove", new[] { iViewModelType }) ?? default!;
                //ilg.Emit(OpCodes.Callvirt, removeMethod);

                // Assign list back to modelVMList (mimics modelVMList = list)
                ilg.MarkLabel(addLabel); // Add logic starts here
                ilg.Emit(OpCodes.Ldloc, listLocal); // Load the modified list
                ilg.Emit(OpCodes.Starg_S, 1); // Store it back to the modelVMList argument

                // Emit async completion
                var completedTaskProperty = typeof(Task).GetProperty(nameof(Task.CompletedTask));
                if (completedTaskProperty == null)
                {
                    throw new InvalidOperationException("CompletedTask property not found.");
                }

                ilg.Emit(OpCodes.Call, completedTaskProperty?.GetGetMethod() ?? default!);
                ilg.Emit(OpCodes.Ret);
            });

            typeBuilder = typeCreator.DefineMethod(typeBuilder, "DeleteItemFromList", typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(iViewModelType)), new[] { typeof(IEnumerable<>).MakeGenericType(iViewModelType) }, new[] { "modelVMList" }, (ilg, localBuilder) =>
            {
                // Load the modelVMList argument
                ilg.Emit(OpCodes.Ldarg_1);

                // Convert to list
                var toListMethod = typeof(Enumerable).GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(iViewModelType) ?? default!;
                ilg.Emit(OpCodes.Call, toListMethod);

                //// Remove 'this' from the list
                //ilg.Emit(OpCodes.Ldloc_0);
                //ilg.Emit(OpCodes.Ldarg_0);
                //var removeMethod = typeof(List<>).MakeGenericType(iViewModelType).GetMethod("Remove") ?? default!;
                //ilg.Emit(OpCodes.Callvirt, removeMethod);

                // Emit async completion
                var completedTaskProperty = typeof(Task).GetProperty(nameof(Task.CompletedTask));
                if (completedTaskProperty == null)
                {
                    throw new InvalidOperationException("CompletedTask property not found.");
                }

                ilg.Emit(OpCodes.Call, completedTaskProperty.GetGetMethod() ?? default!);
                ilg.Emit(OpCodes.Ldloc_0);
                ilg.Emit(OpCodes.Ret);
            });

            return typeBuilder;
        }

        public void Dispose()
        {
            // Cleanup resources
        }
    }
}
