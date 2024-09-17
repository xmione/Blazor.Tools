using Moq;
using Blazor.Tools.BlazorBundler.Interfaces;
using TestContext = Bunit.TestContext;
using Microsoft.Extensions.DependencyInjection;
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
        private TestContext _testContext = default!;
        private Mock<ITypeCreator> _typeCreatorMock = default!;

        [TestInitialize]
        public void TestInit()
        {
            // Initialize the bUnit test context
            _testContext = new TestContext();

            // Initialize the mock object
            _typeCreatorMock = new Mock<ITypeCreator>();

            // Register the mock service with the test context's dependency injection
            _testContext.Services.AddSingleton(_typeCreatorMock.Object);

           
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Cleanup resources after each test
            // Optionally, remove any temporary files or resources if needed
        }

        [TestMethod]
        public void Create_Test()
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

            //string expectedTypeFullName = $"{contextAssemblyName}.IViewModel`2[[{modelTypeAssemblyName}.{modelTypeName}, {modelTypeAssemblyName}, Version={version}, Culture=neutral, PublicKeyToken=null],[{iModelTypeAssemblyName}.{iModelTypeName}, {iModelTypeAssemblyName}, Version={version}, Culture=neutral, PublicKeyToken=null]]";
            var expectedType = typeof(IViewModel<Employee, IModelExtendedProperties>) ?? default!;
            var tc = new TypeCreator();
            string expectedTypeFullName = expectedType.FullName ?? default!;
            Type createdType = tc.DefineInterfaceType(moduleBuilder, expectedTypeFullName);

            Debug.WriteLine($"Expected FullName: {expectedTypeFullName}");
            Debug.WriteLine($"Created Type FullName: {createdType.FullName}");

            Assert.AreEqual(expectedTypeFullName, createdType.FullName);
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
            _testContext.Dispose();
        }
    }
}
