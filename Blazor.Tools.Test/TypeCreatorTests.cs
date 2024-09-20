using Moq;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using System.Reflection.Emit;
using System.Reflection;
using Blazor.Tools.BlazorBundler.Utilities.Assemblies;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models;
using System.Diagnostics;

namespace Blazor.Tools.BlazorBundler.Tests
{
    [TestClass]
    public class TypeCreatorTests : SampleData, IDisposable
    {
        private Mock<ITypeCreator> _typeCreatorMock = default!;
        private AssemblyName _testAssemblyName = default!;
        private AssemblyBuilder _testAssemblyBuilder = default!;
        private ModuleBuilder _testModuleBuilder = default!;

        [TestInitialize]
        public void TestInit()
        {
            string contextAssemblyName = "Blazor.Tools.BlazorBundler.Interfaces";
            // Initialize the mock object
            _typeCreatorMock = new Mock<ITypeCreator>();
            _testAssemblyName = new AssemblyName(contextAssemblyName);
            _testAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(_testAssemblyName, AssemblyBuilderAccess.Run);
            _testModuleBuilder = _testAssemblyBuilder.DefineDynamicModule(contextAssemblyName);

            // Set up default mock behavior
            _typeCreatorMock.Setup(tc => tc.DefineAssemblyName(It.IsAny<string>(), It.IsAny<string>())).Returns(_testAssemblyName);
            _typeCreatorMock.Setup(tc => tc.DefineAssemblyBuilder(It.IsAny<AssemblyName>(), It.IsAny<AssemblyBuilderAccess>()))
                .Returns(_testAssemblyBuilder);
            _typeCreatorMock.Setup(tc => tc.DefineModule(It.IsAny<AssemblyBuilder>(), It.IsAny<string>()))
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
            string version = "1.0.0.0";

            // Act
            var assemblyName = _typeCreatorMock.Object.DefineAssemblyName(contextAssemblyName, version);

            // Assert
            Assert.AreEqual(_testAssemblyName, assemblyName);
            _typeCreatorMock.Verify(tc => tc.DefineAssemblyName(contextAssemblyName, version), Times.Once);
        }


        [TestMethod]
        public void DefineAssemblyBuilder_ShouldReturn_AssemblyBuilder()
        {
            string contextAssemblyName = "Blazor.Tools.BlazorBundler.Interfaces";

            // Arrange
            AssemblyName assemblyName = new AssemblyName(contextAssemblyName);
            AssemblyBuilderAccess assemblyAccess = AssemblyBuilderAccess.Run;

            _typeCreatorMock.Setup(tc => tc.DefineAssemblyBuilder(assemblyName, assemblyAccess)).Returns(_testAssemblyBuilder);

            // Act
            var assemblyBuilder = _typeCreatorMock.Object.DefineAssemblyBuilder(assemblyName, assemblyAccess);

            // Assert
            Assert.AreEqual(_testAssemblyBuilder, assemblyBuilder);
            _typeCreatorMock.Verify(tc => tc.DefineAssemblyBuilder(assemblyName, assemblyAccess), Times.Once);
        }

        [TestMethod]
        public void DefineModule_ShouldReturn_ModuleBuilder()
        {
            // Act
            var moduleBuilder = _typeCreatorMock.Object.DefineModule(_testAssemblyBuilder, "TestModule");

            // Assert
            Assert.AreEqual(_testModuleBuilder, moduleBuilder);
            _typeCreatorMock.Verify(tc => tc.DefineModule(_testAssemblyBuilder, "TestModule"), Times.Once);
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
            Assert.IsTrue(DynamicTypeMatchTest.IsMatched());
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
            // Define assembly and module
            AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");

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
            Type employeeType = typeBuilder.CreateType();

            // Retrieve and print properties
            var properties = employeeType.GetProperties();
            foreach (var prop in properties)
            {
                AppLogger.WriteInfo($"Property: {prop.Name}, Type: {prop.PropertyType}");
            }
        }

        [TestMethod]
        public void CreateIEmployeeType_Test()
        {
            // Define assembly and module
            AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");

            // Define the IEmployee interface
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models.IEmployee",
                TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

            // Define properties for the interface
            DefineInterfaceProperty(typeBuilder, "ID", typeof(int));
            DefineInterfaceProperty(typeBuilder, "FirstName", typeof(string));
            DefineInterfaceProperty(typeBuilder, "MiddleName", typeof(string));
            DefineInterfaceProperty(typeBuilder, "LastName", typeof(string));
            DefineInterfaceProperty(typeBuilder, "DateOfBirth", typeof(DateOnly));
            DefineInterfaceProperty(typeBuilder, "CountryID", typeof(int));

            // Create the interface type
            Type iEmployeeType = typeBuilder.CreateType();

            // Retrieve and print properties (interfaces don't have actual properties, so this will only show the declared signatures)
            var methods = iEmployeeType.GetMethods();
            foreach (var method in methods)
            {
                AppLogger.WriteInfo($"Method: {method.Name}, Return Type: {method.ReturnType}");
            }
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

        private static void DefineInterfaceMethod(TypeBuilder typeBuilder, string methodName, Type returnType)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName,
                MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                returnType, Type.EmptyTypes);
        }

        private static void DefineInterfaceTaskMethod(TypeBuilder typeBuilder, string methodName, Type returnType, params Type[] parameterTypes)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName,
                MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                returnType, parameterTypes);
        }

        private static void DefineClassProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = typeBuilder.DefineField($"_{propertyName.ToLower()}", propertyType, FieldAttributes.Private);
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod($"get_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                propertyType, Type.EmptyTypes);

            ILGenerator getIl = getPropMthdBldr.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod($"set_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                null, new Type[] { propertyType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }

        private static void DefineInterfaceProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            // Define the getter method signature
            MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod($"get_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Abstract | MethodAttributes.Virtual,
                propertyType, Type.EmptyTypes);

            // Define the setter method signature
            MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod($"set_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Abstract | MethodAttributes.Virtual,
                null, new Type[] { propertyType });

            // Attach getter and setter to the property
            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }

        public void Dispose()
        {
            // Cleanup resources
        }
    }
}
