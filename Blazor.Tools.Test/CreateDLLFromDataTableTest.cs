using Moq;
using TestContext = Bunit.TestContext;
using Microsoft.Extensions.DependencyInjection;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using Blazor.Tools.BlazorBundler.Utilities.Assemblies;
using System.Data;

namespace Blazor.Tools.BlazorBundler.Tests
{
    [TestClass]
    public class CreateDLLFromDataTableTest : SampleData, IDisposable
    {
        private TestContext _testContext = default!;
        private Mock<ICreateDLLFromDataTable> _createDLLFromDataTableMock = default!;
        private DataTable _dataTable = new DataTable();

        [TestInitialize]
        public void TestInit()
        {
            // Create a sample DataTable
            _dataTable = EmployeeDataTable;

            // Initialize the bUnit test context
            _testContext = new TestContext();

            // Initialize the mock object
            _createDLLFromDataTableMock = new Mock<ICreateDLLFromDataTable>();

            // Register the mock service with the test context's dependency injection
            _testContext.Services.AddSingleton(_createDLLFromDataTableMock.Object);

            _createDLLFromDataTableMock.Setup(m => m.BuildAndSaveAssembly(_dataTable));

        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Cleanup resources after each test
            // Optionally, remove any temporary files or resources if needed
        }

        [TestMethod]
        public void Create_Sample_DLL_Test()
        {
            try
            {
                
                // Use the actual class instead of a mock
                var cdft = new CreateDLLFromDataTable();

                // Delete file if exists
                if (File.Exists(cdft.DLLPath))
                {
                    File.Delete(cdft.DLLPath);
                }

                // Execute the method
                cdft.BuildAndSaveAssembly(_dataTable);

                // Check if the DLLPath is correctly set and the file exists
                Assert.IsTrue(File.Exists(cdft.DLLPath), "DLL file was not created successfully.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Test failed with exception: {ex.Message}");
                ApplicationExceptionLogger.HandleException(ex);
            }
        }
        
        [TestMethod]
        public void Assembly_With_Different_Namespaces_Test()
        {
            try
            {
                var employeeNameSpace = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models";
                var employeeTypeName = _dataTable.TableName;
                var iEmployeeNameSpace = "Blazor.Tools.BlazorBundler.Interfaces";
                var iEmployeeTypeName = $"I{employeeTypeName}";

                var fullyQualifiedEmployeeTypeName = $"{employeeNameSpace}.{employeeTypeName}";
                var fullyQualifiedIEmployeeTypeName = $"{iEmployeeNameSpace}.{iEmployeeTypeName}";

                var lastIndex = employeeNameSpace.LastIndexOf('.');
                var contextAssemblyName = employeeNameSpace[..lastIndex];

                // Use the actual class instead of a mock
                var cdft = new CreateDLLFromDataTable(employeeNameSpace, employeeTypeName, iEmployeeNameSpace, iEmployeeTypeName);

                // Delete file if exists
                if (File.Exists(cdft.DLLPath))
                {
                    File.Delete(cdft.DLLPath);
                }

                // Execute the method
                cdft.BuildAndSaveAssembly(_dataTable);

                // Check if the DLLPath is correctly set and the file exists
                Assert.IsTrue(File.Exists(cdft.DLLPath), "DLL file was not created successfully.");

                // Check if Context assembly name is correct
                Assert.IsTrue(cdft.ContextAssemblyName == contextAssemblyName, $"Context assembly name {cdft.ContextAssemblyName} was not correct. Expecting {contextAssemblyName}");

                // Check if concrete class type is assignable to interface type and vice-versa
                using (var assembly = DisposableAssembly.LoadFile(cdft.DLLPath))
                {
                    Type concreteType = assembly.Assembly.GetType(fullyQualifiedEmployeeTypeName) ?? default!;
                    Type interfaceType = assembly.Assembly.GetType(fullyQualifiedIEmployeeTypeName) ?? default!;
                    var isConcreteTypeAssignableToInterfaceType = interfaceType.IsAssignableFrom(concreteType);
                    var isInterfaceTypeAssignableToConcreteType = interfaceType.IsAssignableFrom(concreteType);

                    Assert.IsTrue(isConcreteTypeAssignableToInterfaceType, $"Concrete type {concreteType} is not assignable to {interfaceType}");
                    Assert.IsTrue(isInterfaceTypeAssignableToConcreteType, $"Interface type {interfaceType} is not assignable to {concreteType}");
                }
            }
            catch (Exception ex)
            {
                ApplicationExceptionLogger.HandleException(ex);
                Assert.Fail($"Test failed with exception: {ex.Message}");
            }
        }


        public void Dispose()
        {
            // Cleanup resources
            _testContext.Dispose();
        }
    }
}
