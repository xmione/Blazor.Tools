using Moq;
using Blazor.Tools.BlazorBundler.Interfaces;
using TestContext = Bunit.TestContext;
using Microsoft.Extensions.DependencyInjection;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using System.Reflection.Emit;
using System.Reflection;
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
            _dataTable.Columns.Add("Id", typeof(int));
            _dataTable.Columns.Add("Name", typeof(string));
            _dataTable.Columns.Add("Age", typeof(int));

            // Initialize the bUnit test context
            _testContext = new TestContext();

            // Initialize the mock object
            _createDLLFromDataTableMock = new Mock<ICreateDLLFromDataTable>();

            // Register the mock service with the test context's dependency injection
            _testContext.Services.AddSingleton(_createDLLFromDataTableMock.Object);

            _createDLLFromDataTableMock.Setup(m => m.BuildAndSaveAssembly(_dataTable));
            _createDLLFromDataTableMock.Setup(m => m.CreateAndUseInstance());

        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Cleanup resources after each test
            // Optionally, remove any temporary files or resources if needed
        }

        [TestMethod]
        public void Run_Test()
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
                cdft.CreateAndUseInstance();

                // Check if the DLLPath is correctly set and the file exists
                Assert.IsTrue(File.Exists(cdft.DLLPath), "DLL file was not created successfully.");
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
