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
                ApplicationExceptionLogger.HandleException(ex);
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


        public void Dispose()
        {
            // Cleanup resources
            _testContext.Dispose();
        }
    }
}
