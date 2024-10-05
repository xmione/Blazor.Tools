using Blazor.Tools.BlazorBundler.Components.Grid;
using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels;
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Assemblies;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Data;

namespace Blazor.Tools.Test
{
    [TestClass]
    public sealed class DataTableGridTests : SampleData
    {
        private DataTableGrid? _dataTableGrid;
        private BunitContext _testContext;
        private Mock<IDataTableGrid>? _dataTableGridMock;
        private Mock<IDynamicClassBuilder>? _dynamicClassBuilderMock;
        private Mock<IModelExtendedProperties>? _iModelExtendedProperties;
        private IRenderedComponent<DataTableGrid>? _dataTableGridComponent;
        private string? _tableName;
        private Type? _modelType;
        private Type? _modelVMType;
        private Type? _interfaceType;
        private object? _modelInstance;
        private object? _modelVMInstance;
        private string? _tempFolderPath;
        private string? _modelTempDllPath;
        private string? _modelVMTempDllPath;

        [TestInitialize]
        public void TestInit()
        {
            // Set up before each test method.

            _dataTableGrid = new DataTableGrid
            {
                Title = Title,
                DataSources = DataSources,
                SelectedTable = EmployeeDataTable,
                TableList = TableList,
                ModelsAssemblyName = ModelsAssemblyName,
                ViewModelsAssemblyName = ViewModelsAssemblyName,

            };

            _testContext = new BunitContext();
            // Initialize the mock objects
            _dataTableGridMock = new Mock<IDataTableGrid>();
            _dynamicClassBuilderMock = new Mock<IDynamicClassBuilder>();
            _iModelExtendedProperties = new Mock<IModelExtendedProperties>();
            
            // Define the open generic type and type arguments
            var openGenericType = typeof(IViewModel<,>);
            var typeArguments = new[] { typeof(Employee), typeof(IModelExtendedProperties) };

            // Create a mock for the closed generic type using the extension method
            var mock = new Mock<IViewModel<Employee, IModelExtendedProperties>>()
                .CreateMockForGenericType(typeArguments);

            // Register the mock service with the test context's dependency injection
            _testContext.Services.AddSingleton(_dataTableGridMock.Object);
            _testContext.Services.AddSingleton(_dynamicClassBuilderMock.Object);

            // Arrange
            // Set up the mock to return specific values or throw exceptions
            // Define the paths in the Temp folder
            _tempFolderPath = Path.GetTempPath();
            _modelTempDllPath = Path.Combine(_tempFolderPath, $"{ModelsAssemblyName}.dll");
            _modelVMTempDllPath = Path.Combine(_tempFolderPath, $"{ViewModelsAssemblyName}.dll");
            _modelType = typeof(Employee);
            var tiModelType = typeof(IModelExtendedProperties);
            Type iViewModelGenericType = typeof(IViewModel<,>);
            var iViewModelTypes = new Type[] { iViewModelGenericType.MakeGenericType(_modelType, tiModelType), tiModelType };

            _dataTableGridMock.Setup(m => m.CreateDynamicBundlerDLLAsync()).Returns(Task.CompletedTask);
            _dataTableGridMock.Setup(m => m.DefineConstructorsAsync(_dynamicClassBuilderMock.Object, _modelVMTempDllPath)).Returns(Task.CompletedTask);
            //_dataTableGridMock.Setup(m => m.DefineMethodsAsync(_dynamicClassBuilderMock.Object, _dynamicClassBuilderMock.Object.DynamicType)).Returns(Task.CompletedTask);
            //_dataTableGridMock.Setup(m => m.DefineTableColumnsAsync()).Returns(Task.CompletedTask);

            // Set up the dynamic class builder mock
            _dynamicClassBuilderMock.Setup(m => m.CreateClassFromDataTable(It.IsAny<DataTable>())).Verifiable();
            _dynamicClassBuilderMock.Setup(m => m.SaveAssembly(It.IsAny<string>(), It.IsAny<bool>())).Verifiable();
            _dynamicClassBuilderMock.Setup(m => m.DeleteAssembly()).Verifiable();

            _tableName = EmployeeDataTable.TableName;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Tear down after each test method.
            _testContext.Dispose();
        }


        [TestMethod]
        public async Task CreateDynamicBundlerDLL_Concrete_Test()
        {
            try
            {
                _dataTableGrid?.InitializeVariablesAsync();

                //protected override void BuildRenderTree(RenderTreeBuilder builder)
            }
            catch (Exception ex)
            {
                Assert.Fail($"Create failed with exception: {ex.Message}");
                AppLogger.HandleError(ex);
            }

            await Task.CompletedTask;
        }

        [TestMethod]
        public void DataTableGrid_Render_CorrectParameters()
        {
            // Arrange
            var dataTableGrid = _testContext.Render<DataTableGrid>(parameters => parameters
                .Add(p => p.Title, "Test Title")
                .Add(p => p.DataSources, DataSources)
                .Add(p => p.SelectedTable, EmployeeDataTable)
                .Add(p => p.ModelsAssemblyName, ModelsAssemblyName)
                .Add(p => p.ViewModelsAssemblyName, ViewModelsAssemblyName)
                .Add(p => p.AllowCellRangeSelection, true)
                .Add(p => p.TableList, TableList)
                .Add(p => p.HiddenColumnNames, HiddenEmployeeColumns)
                .Add(p => p.HeaderNames, EmployeeHeaderNames)
                .Add(p => p.ItemsChanged, OnItemsChanged)
            );

            // Act
            var renderedMarkup = dataTableGrid.Markup;

            // Assert
            dataTableGrid.Find("table").MarkupMatches("<table class=\"data-table-grid\">");
            Assert.IsTrue(renderedMarkup.Contains("Test Title"));
            // Add more assertions to verify other parameters
        }
    }
}
