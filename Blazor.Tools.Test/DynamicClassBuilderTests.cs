using Bunit;
using Moq;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Components.Grid;
using TestContext = Bunit.TestContext;
using Microsoft.Extensions.DependencyInjection;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;

namespace Blazor.Tools.BlazorBundler.Tests
{
    [TestClass]
    public class DynamicBundlerTests : SampleData, IDisposable
    {
        private TestContext _testContext = default!;
        private Mock<IDataTableGrid> _dataTableGridMock = default!;
        private Mock<IDynamicClassBuilder> _dynamicClassBuilderMock = default!;
        private IRenderedComponent<DataTableGrid> _dataTableGridComponent = default!;

        [TestInitialize]
        public void TestInit()
        {
            // Initialize the bUnit test context
            _testContext = new TestContext();

            // Initialize the mock object
            _dataTableGridMock = new Mock<IDataTableGrid>();
            _dynamicClassBuilderMock = new Mock<IDynamicClassBuilder>();

            // Register the mock service with the test context's dependency injection
            _testContext.Services.AddSingleton(_dataTableGridMock.Object);
            _testContext.Services.AddSingleton(_dynamicClassBuilderMock.Object);

            // Arrange
            // Set up the mock to return specific values or throw exceptions
            _dataTableGridMock.Setup(m => m.CreateDynamicBundlerDLL()).Returns(Task.CompletedTask);
            //_dynamicClassBuilderMock.Setup(m => m.CreateDynamicBundlerDLL()).Returns(Task.CompletedTask);
            
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Cleanup resources after each test
            // Optionally, remove any temporary files or resources if needed
        }

        [TestMethod]
        public async Task CreateDataTableGrid_Test()
        {
            /*
             <DataTableGrid  Title="@EmployeeDataTable.TableName"
                SelectedTable="@EmployeeDataTable" 
                ModelsAssemblyName="@ModelsAssemblyName"
                ViewModelsAssemblyName="@ViewModelsAssemblyName"
                HiddenColumnNames="@HiddenEmployeeColumns" 
                DataSources="@DataSources"
                ItemsChanged="@OnItemsChanged" 
                AllowCellRangeSelection="true"
                TableList="@TableList" 
                />

             */

            try
            {
                // Act: Call the method that interacts with IDynamicClassBuilder
                _dataTableGridComponent = _testContext.RenderComponent<DataTableGrid>(parameters => parameters
                    .Add(p => p.Title, EmployeeDataTable.TableName) // Set component parameters using bUnit's parameter helper
                    .Add(p => p.SelectedTable, EmployeeDataTable)
                    .Add(p => p.ModelsAssemblyName, ModelsAssemblyName)
                    .Add(p => p.ViewModelsAssemblyName, ViewModelsAssemblyName)
                    .Add(p => p.HiddenColumnNames, HiddenEmployeeColumns)
                    .Add(p => p.DataSources, DataSources)
                    .Add(p => p.ItemsChanged, OnItemsChanged)
                    .Add(p => p.AllowCellRangeSelection, true)
                    .Add(p => p.TableList, TableList)
                );

                await _dataTableGridComponent.Instance.CreateDynamicBundlerDLL();

                // Assert
                // Verify that CreateDynamicBundlerDLL was called
                _dataTableGridMock.Verify(m => m.CreateDynamicBundlerDLL(), Times.Once);
            }
            catch (Exception ex) 
            {
                Assert.Fail();
                ApplicationExceptionLogger.HandleException(ex);
            }
            
        }

        [TestMethod]
        public async Task CreateDynamicBundlerDLL_Test()
        {
            /*
             <DataTableGrid  Title="@EmployeeDataTable.TableName"
                SelectedTable="@EmployeeDataTable" 
                ModelsAssemblyName="@ModelsAssemblyName"
                ViewModelsAssemblyName="@ViewModelsAssemblyName"
                HiddenColumnNames="@HiddenEmployeeColumns" 
                DataSources="@DataSources"
                ItemsChanged="@OnItemsChanged" 
                AllowCellRangeSelection="true"
                TableList="@TableList" 
                />

             */
            // Act: Call the method that interacts with IDynamicClassBuilder
            _dataTableGridComponent = _testContext.RenderComponent<DataTableGrid>(parameters => parameters
                .Add(p => p.Title, EmployeeDataTable.TableName) // Set component parameters using bUnit's parameter helper
                .Add(p => p.SelectedTable, EmployeeDataTable) 
                .Add(p => p.ModelsAssemblyName, ModelsAssemblyName) 
                .Add(p => p.ViewModelsAssemblyName, ViewModelsAssemblyName) 
                .Add(p => p.HiddenColumnNames, HiddenEmployeeColumns) 
                .Add(p => p.DataSources, DataSources) 
                .Add(p => p.ItemsChanged, OnItemsChanged) 
                .Add(p => p.AllowCellRangeSelection, true) 
                .Add(p => p.TableList, TableList) 
            );

            await _dataTableGridComponent.Instance.CreateDynamicBundlerDLL();

            // Assert
            // Verify that CreateDynamicBundlerDLL was called
            _dataTableGridMock.Verify(m => m.CreateDynamicBundlerDLL(), Times.Once);

            // Additional assertions to validate the behavior
        }

        public void Dispose()
        {
            // Cleanup resources
            _testContext.Dispose();
        }
    }
}
