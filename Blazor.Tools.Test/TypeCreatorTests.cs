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

            // Set up default mock behavior
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
                AppLogger.HandleException(ex);
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

            //var iModelExtendedPropertiesConstructedType = typeCreator.CreateType(
            //    ref moduleBuilder,
            //    out typeBuilder,
            //    typeName: $"{interfaceNameSpace}.IModelExtendedProperties",
            //    typeAttributes: TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract,
            //    properties: iModelExtendedProperties
            //    );

            // Define the IEmployee interface
            typeBuilder = moduleBuilder.DefineType(
                $"{interfaceNameSpace}.IModelExtendedProperties",
                TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

            // Define properties for the interface
            foreach (var prop in iModelExtendedProperties)
            {
                typeBuilder = typeCreator.DefineInterfaceProperty(typeBuilder, prop.Name, prop.Type);
            }

            // Create the interface type
            var iModelExtendedPropertiesConstructedType = typeBuilder.CreateType();

            var iViewModelType = typeof(IViewModel<,>);
            var iViewModelCreatedType = typeCreator.CreateType(
                ref moduleBuilder,
                out typeBuilder,
                typeName: $"{interfaceNameSpace}.IViewModel",
                typeAttributes: TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract,
                interfaces: new Type[] { iModelExtendedPropertiesConstructedType },
                genericParameterNames: new[] { "TModel", "TIModel" },  // Define <TModel, TIModel>
                defineMethodsAction: tb => DefineViewModelMethods(typeCreator, tb)
                );
            
            var iViewModelConstructedGenericType = iViewModelCreatedType.MakeGenericType(_employeeType, iModelExtendedPropertiesConstructedType);

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
                typeName: employeeVMFullyQualifiedTypeName,
                typeAttributes: TypeAttributes.Public | TypeAttributes.Class,
                baseType: _employeeType,
                //interfaces: new Type[] { iValidatableObjectCreatedType, iCloneableCreatedType, iViewModelType, iModelExtendedPropertiesConstructedType },
                properties: modelProperties,
                defineFieldsAction: tb => DefineFields(tb),
                defineConstructorsAction: tb => DefineConstructors(typeCreator, tb, _employeeType),
                //defineMethodsAction: tb => DefineMethods(typeCreator, tb, iViewModelType, _employeeType, iModelExtendedPropertiesConstructedType),
                iImplementations: iImplementations
                );

            // Save the assembly to a file
            typeCreator.Save(dllPath);

            var assembly = Assembly.LoadFile(dllPath);
            _vmType = assembly.GetType(employeeVMFullyQualifiedTypeName)!;

            object employeeInstance = Activator.CreateInstance(_employeeType)!;
            object employeeVMInstance = Activator.CreateInstance(_vmType)!;

            // Use reflection to set the _employees field with List<EmployeeVM>
            FieldInfo employeesField = typeBuilder.GetField("_employees", BindingFlags.NonPublic | BindingFlags.Instance)!;
            Type listOfEmployeeVMType = typeof(List<>).MakeGenericType(_vmType);
            object listOfEmployeeVMInstance = Activator.CreateInstance(listOfEmployeeVMType)!;
            employeesField.SetValue(employeeVMInstance, listOfEmployeeVMInstance);

            //typeBuilder = DefineConstructors(typeCreator, typeBuilder, _employeeType);
            typeBuilder = DefineMethods(typeCreator, typeBuilder, iViewModelType, _employeeType, iModelExtendedPropertiesConstructedType);

        }

        [TestMethod]
        public void DefineInterfaceType_Test()
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

            var expectedType = typeof(IViewModel<Employee, IModelExtendedProperties>);
            string expectedTypeFullName = expectedType.FullName ?? string.Empty;

            var tc = new TypeCreator(contextAssemblyName, modelTypeAssemblyName, modelTypeName, iModelTypeAssemblyName, iModelTypeName, version, AssemblyBuilderAccess.Run);
            
            Type createdType = tc.DefineInterfaceType(moduleBuilder, expectedTypeFullName);

            Debug.WriteLine($"Expected FullName: {expectedTypeFullName}");
            Debug.WriteLine($"Created Type FullName: {createdType.AssemblyQualifiedName}");

            //Assert.AreEqual(expectedTypeFullName, createdType.AssemblyQualifiedName);
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
            // Define fields
            //FieldBuilder idField = typeBuilder.DefineField("_id", typeof(int), FieldAttributes.Public);
            //FieldBuilder firstNameField = typeBuilder.DefineField("_firstName", typeof(string), FieldAttributes.Public);
            //FieldBuilder middleNameField = typeBuilder.DefineField("_middleName", typeof(string), FieldAttributes.Public);
            //FieldBuilder lastNameField = typeBuilder.DefineField("_lastName", typeof(string), FieldAttributes.Public);
            //FieldBuilder dateOfBirthField = typeBuilder.DefineField("_dateOfBirth", typeof(DateOnly), FieldAttributes.Public);
            //FieldBuilder countryIdField = typeBuilder.DefineField("_countryID", typeof(int), FieldAttributes.Public);

            // Define properties
            //DefineClassProperty(typeBuilder, "ID", typeof(int));
            //DefineClassProperty(typeBuilder, "FirstName", typeof(string));
            //DefineClassProperty(typeBuilder, "MiddleName", typeof(string));
            //DefineClassProperty(typeBuilder, "LastName", typeof(string));
            //DefineClassProperty(typeBuilder, "DateOfBirth", typeof(DateOnly));
            //DefineClassProperty(typeBuilder, "CountryID", typeof(int));

            // Create the type
            Type employeeType = typeBuilder.CreateType();

            var methods = employeeType.GetMethods();
            foreach (var method in methods)
            {
                AppLogger.WriteInfo($"Method: {method.Name}, Return Type: {method.ReturnType}");
            }

            //// Retrieve and print properties
            //var properties = employeeType.GetProperties();
            //foreach (var prop in properties)
            //{
            //    AppLogger.WriteInfo($"Property: {prop.Name}, Type: {prop.PropertyType}");
            //}

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

        public TypeBuilder DefineFields(TypeBuilder typeBuilder)
        {
            // Define the field for the contextProvider only once
            _employeeListField = typeBuilder.DefineField("_employees", typeof(List<>).MakeGenericType(typeBuilder), FieldAttributes.Private);
            //_employeeListField = typeBuilder.DefineField("_employees", typeof(object), FieldAttributes.Private);
            _contextProviderField = typeBuilder.DefineField("_contextProvider", typeof(IContextProvider), FieldAttributes.Private);

            return typeBuilder;
        }

        public TypeBuilder DefineConstructors(TypeCreator typeCreator, TypeBuilder typeBuilder, Type modelType)
        {

            //_employeeListField = typeBuilder.DefineField("_employees", typeof(List<>).MakeGenericType(typeBuilder), FieldAttributes.Private);
            //_contextProviderField = typeBuilder.DefineField("_contextProvider", typeof(IContextProvider), FieldAttributes.Private);

            // Define constructors
            // Parameterless constructor
            typeBuilder = typeCreator.DefineConstructor(typeBuilder, Type.EmptyTypes, ilg =>
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
                if (typeCreator.AddedFields != null)
                {
                    foreach ((string typeName, FieldBuilder fieldBuilder) in typeCreator.AddedFields)
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
            typeBuilder = typeCreator.DefineConstructor(typeBuilder, new[] { typeof(IContextProvider) }, ilg =>
            {
                ConstructorInfo baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Call, baseConstructor);

                // Set _employeeList field
                ilg.Emit(OpCodes.Ldarg_0);
                var employeeListConstructor = typeof(List<>).MakeGenericType(typeof(object)).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Newobj, employeeListConstructor);
                ilg.Emit(OpCodes.Stfld, _employeeListField);

                // Set _contextProvider field
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ldarg_1);
                ilg.Emit(OpCodes.Stfld, _contextProviderField);

                // Step 3: Initialize other fields (IModelExtendedProperties fields)
                if (typeCreator.AddedFields != null)
                {
                    foreach ((string typeName, FieldBuilder fieldBuilder) in typeCreator.AddedFields)
                    {
                        if (typeName.Contains("IModelExtendedProperties"))
                        {
                            ilg.Emit(OpCodes.Ldarg_0);
                            ilg.Emit(OpCodes.Ldc_I4_0); // Initialize int/boolean fields with 0/false
                            ilg.Emit(OpCodes.Stfld, fieldBuilder); // Store default values in fields
                        }
                    }
                }

                ilg.Emit(OpCodes.Ret);

                Console.WriteLine("Constructor with IContextProvider parameter defined.");
            });

            // Constructor with IContextProvider and Employee parameter
            typeBuilder = typeCreator.DefineConstructor(typeBuilder, new[] { typeof(IContextProvider), modelType }, ilg =>
            {
                ConstructorInfo baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Call, baseConstructor);

                // Set contextProvider field
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ldarg_1);
                ilg.Emit(OpCodes.Stfld, _contextProviderField);

                // Set _modelProperties from Employee model
                ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                ilg.Emit(OpCodes.Ldarg_2); // Load Employee model

                if (typeCreator.AddedProperties != null)
                {
                    foreach ((string typeName, PropertyBuilder prop) in typeCreator.AddedProperties)
                    {
                        if (modelType.FullName == typeName)
                        {
                            if (prop.CanWrite)
                            {
                                ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                                ilg.Emit(OpCodes.Ldarg_2); // Load Employee model
                                ilg.Emit(OpCodes.Callvirt, prop.GetGetMethod()!); // Get property value
                                ilg.Emit(OpCodes.Callvirt, prop.GetSetMethod()!); // Set property value
                            }

                        }

                    }
                }

                ilg.Emit(OpCodes.Ret);

                Console.WriteLine("Constructor with IContextProvider and Employee parameter defined.");
            });

            // Constructor with IContextProvider and EmployeeVM parameter
            typeBuilder = typeCreator.DefineConstructor(typeBuilder, new[] { typeof(IContextProvider), typeBuilder }, ilg =>
            {
                ConstructorInfo baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Call, baseConstructor);

                // Set contextProvider field
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ldarg_1);
                ilg.Emit(OpCodes.Stfld, _contextProviderField);

                // Set _modelProperties from EmployeeVM modelVM
                ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                ilg.Emit(OpCodes.Ldarg_2); // Load EmployeeVM modelVM

                if (typeCreator.AddedProperties != null)
                {
                    foreach ((string typeName, PropertyBuilder prop) in typeCreator.AddedProperties)
                    {
                        var isValidType = typeName == modelType.FullName || typeName.Contains("IModelExtendedProperties");
                        if (isValidType && prop.CanWrite)
                        {
                            ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                            ilg.Emit(OpCodes.Ldarg_2); // Load EmployeeVM modelVM
                            ilg.Emit(OpCodes.Callvirt, prop.GetGetMethod()!); // Get property value
                            ilg.Emit(OpCodes.Callvirt, prop.GetSetMethod()!); // Set property value
                        }
                    }
                }

                ilg.Emit(OpCodes.Ret);

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
            // Define methodDefinitions dynamically
            typeBuilder = typeCreator.DefineMethod(typeBuilder, "ToNewModel", modelType, Type.EmptyTypes, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Check for parameterless constructor
                ConstructorInfo constructor = modelType.GetConstructor(Type.EmptyTypes)
                    ?? throw new InvalidOperationException($"No parameterless constructor found for type {modelType.Name}");

                // Emit IL code to call the constructor
                ilg.Emit(OpCodes.Newobj, constructor);
                ilg.Emit(OpCodes.Ret);
            });

            typeBuilder = typeCreator.DefineMethod(typeBuilder, "ToNewIModel", iModelType, Type.EmptyTypes, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Get the constructor of modelType
                ConstructorInfo? constructor = modelType.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                {
                    throw new InvalidOperationException($"No parameterless constructor found for type {modelType.Name}");
                }

                // Emit IL to create a new instance of modelType
                ilg.Emit(OpCodes.Newobj, constructor); // Create new instance of modelType
                LocalBuilder localBuilderInstance = ilg.DeclareLocal(modelType); // Declare a local variable for the instance
                ilg.Emit(OpCodes.Stloc, localBuilderInstance); // Store the instance in the local variable

                // Get _modelProperties of modelType
                if (typeCreator.AddedProperties != null)
                {
                    foreach ((string typeName, PropertyBuilder prop) in typeCreator.AddedProperties)
                    {
                        if (typeName == modelType.FullName && prop.CanWrite)
                        {
                            // Load the instance and the value to set
                            ilg.Emit(OpCodes.Ldloc, localBuilderInstance); // Load instance
                            ilg.Emit(OpCodes.Ldarg_0); // Load 'this'

                            // Load the value of the property from 'this'
                            if (prop != null)
                            {
                                ilg.Emit(OpCodes.Callvirt, prop?.GetGetMethod() ?? default!); // Call property getter
                                ilg.Emit(OpCodes.Callvirt, prop?.GetSetMethod() ?? default!); // Call property setter
                            }
                        }
                    }

                    // Return the new instance
                    ilg.Emit(OpCodes.Ldloc, localBuilderInstance);
                    ilg.Emit(OpCodes.Ret);
                }
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

                // Set the IsEditMode property
                var isEditModeItem = typeCreator.AddedProperties?.FirstOrDefault(p => p.PropertyBuilder.Name.Contains("IsEditMode"));
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

                // Set the IsEditMode property to false
                var isEditModeItem = typeCreator.AddedProperties?.FirstOrDefault(p => p.PropertyBuilder.Name.Contains("IsEditMode"));
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

                foreach (var (ctor, parameters) in constructors)
                {
                    Console.WriteLine($"Constructor: {ctor.Name}, Parameters: {string.Join(", ", parameters.Select(p => p.Name))}");
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
                if (typeCreator.AddedProperties != null)
                {
                    foreach ((string typeName, PropertyBuilder prop) in typeCreator.AddedProperties)
                    {
                        if (new[] { modelType.FullName, iModelType.FullName }.Contains(typeName) && prop.CanWrite)
                        {
                            // Load the new instance
                            ilg.Emit(OpCodes.Ldloc, newInstance);

                            // Load property value from 'this'
                            ilg.Emit(OpCodes.Ldarg_0);
                            ilg.Emit(OpCodes.Callvirt, prop?.GetGetMethod() ?? default!);

                            // Set the property on the new instance
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
