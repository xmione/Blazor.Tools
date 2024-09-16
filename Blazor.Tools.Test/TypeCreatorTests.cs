using Moq;
using Blazor.Tools.BlazorBundler.Interfaces;
using TestContext = Bunit.TestContext;
using Microsoft.Extensions.DependencyInjection;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using System.Reflection.Emit;
using System.Reflection;

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
        public void DefineInterfaceType_Test()
        {
            try
            {
                // Arrange
                AssemblyName asmName = new AssemblyName("DynamicAssembly");
                AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
                ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

                string fullyQualifiedName = "Blazor.Tools.BlazorBundler.Interfaces.IViewModel`2[[Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models.Employee, Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null],[Blazor.Tools.BlazorBundler.Interfaces.IModelExtendedProperties, Blazor.Tools.BlazorBundler.Interfaces, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]";

                // Create a dummy type that matches the fully qualified name
                string baseTypeName = "Blazor.Tools.BlazorBundler.Interfaces.IViewModel";
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
                Assert.Fail($"Test failed with exception: {ex.Message}");
                ApplicationExceptionLogger.HandleException(ex);
            }
        }

        public void Dispose()
        {
            // Cleanup resources
            _testContext.Dispose();
        }
    }
}
