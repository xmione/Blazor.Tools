using Bunit;
using Moq;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Components.Grid;
using TestContext = Bunit.TestContext;
using Microsoft.Extensions.DependencyInjection;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using Blazor.Tools.BlazorBundler.Utilities.Assemblies;
using BlazorBootstrap;
using System.Reflection;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models;
using System.Data;
using Blazor.Tools.BlazorBundler.Extensions;
using DocumentFormat.OpenXml.EMMA;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels;

namespace Blazor.Tools.BlazorBundler.Tests
{
    [TestClass]
    public class DynamicBundlerTests : SampleData, IDisposable
    {
        private TestContext _testContext = default!;
        private Mock<IDataTableGrid> _dataTableGridMock = default!;
        private Mock<IDynamicClassBuilder> _dynamicClassBuilderMock = default!;
        private Mock<IModelExtendedProperties> _iModelExtendedProperties;
        private IRenderedComponent<DataTableGrid> _dataTableGridComponent = default!;
        private string _tableName;
        private Type? _modelType;
        private Type? _modelVMType;
        private Type? _interfaceType;
        private object? _modelInstance;
        private object? _modelVMInstance;
        private string _tempFolderPath;
        private string _modelTempDllPath;
        private string _modelVMTempDllPath;

        [TestInitialize]
        public void TestInit()
        {
            // Initialize the bUnit test context
            _testContext = new TestContext();

            // Initialize the mock object
            _dataTableGridMock = new Mock<IDataTableGrid>();
            _dynamicClassBuilderMock = new Mock<IDynamicClassBuilder>();
            _iModelExtendedProperties = new Mock<IModelExtendedProperties>();
            // Define the open generic type and type arguments
            var openGenericType = typeof(IViewModel<,>);
            var typeArguments = new[] { typeof(Employee), typeof(EmployeeVM) };

            // Create a mock for the closed generic type using the extension method
            var mock = new Mock<IViewModel<Employee, EmployeeVM>>()
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
            
            var tiModelType = typeof(IModelExtendedProperties);
            Type iViewModelGenericType = typeof(IViewModel<,>);
            var iViewModelTypes = new Type[] { iViewModelGenericType.MakeGenericType(_modelType, tiModelType), tiModelType };

            _dataTableGridMock.Setup(m => m.CreateDynamicBundlerDLL()).Returns(Task.CompletedTask);
            _dataTableGridMock.Setup(m => m.DefineConstructors(_dynamicClassBuilderMock.Object,_modelVMTempDllPath)).Returns(Task.CompletedTask);
            //_dataTableGridMock.Setup(m => m.DefineMethods(_dynamicClassBuilderMock.Object, _dynamicClassBuilderMock.Object.DynamicType)).Returns(Task.CompletedTask);
            //_dataTableGridMock.Setup(m => m.DefineTableColumns()).Returns(Task.CompletedTask);

            // Set up the dynamic class builder mock
            _dynamicClassBuilderMock.Setup(m => m.CreateClassFromDataTable(It.IsAny<DataTable>())).Verifiable();
            _dynamicClassBuilderMock.Setup(m => m.SaveAssembly(It.IsAny<string>(), It.IsAny<bool>())).Verifiable();
            _dynamicClassBuilderMock.Setup(m => m.DeleteAssembly()).Verifiable();

            _tableName = EmployeeDataTable.TableName;
            
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
            try
            {
                // Setup the mock to simulate behavior
                _dynamicClassBuilderMock.Setup(m => m.CreateClassFromDataTable(EmployeeDataTable)).Verifiable();
                _dynamicClassBuilderMock.Setup(m => m.SaveAssembly(It.IsAny<string>(), It.IsAny<bool>())).Verifiable();

                // Create an instance of DynamicClassBuilder for TModel
                _dynamicClassBuilderMock.Object.CreateClassFromDataTable(EmployeeDataTable);
                _dynamicClassBuilderMock.Object.SaveAssembly();

                _modelType = _dynamicClassBuilderMock.Object.DynamicType;

                if (_modelType == null)
                {
                    Assert.Fail("Failed to retrieve the type from the model instance.");
                }

                // Define and create an instance of DynamicClassBuilder for TModelVM
                var tiModelType = typeof(IModelExtendedProperties);
                Type iViewModelGenericType = typeof(IViewModel<,>);
                var iViewModelTypes = new Type[] { iViewModelGenericType.MakeGenericType(_modelType, tiModelType), tiModelType };

                // Setup mock for methods
                _dynamicClassBuilderMock.Setup(m => m.CreateClassFromDataTable(EmployeeDataTable)).Verifiable();
                _dynamicClassBuilderMock.Setup(m => m.SaveAssembly(It.IsAny<string>(), It.IsAny<bool>())).Verifiable();

                _dynamicClassBuilderMock.Object.CreateClassFromDataTable(EmployeeDataTable);
                //await DefineConstructors(_dynamicClassBuilderMock.Object, _modelTempDllPath);
                //await DefineMethods(_dynamicClassBuilderMock.Object, _modelType, tiModelType);

                var vmAssembly = _dynamicClassBuilderMock.Object.Assembly;
                _modelVMType = _dynamicClassBuilderMock.Object.DynamicType;

                if (_modelVMType == null)
                {
                    Assert.Fail("Failed to retrieve the type from the model view instance.");
                }

                var tIModelTypeFullName = tiModelType.FullName;
                _interfaceType = _modelVMType.GetInterface(tIModelTypeFullName);

                if (_interfaceType == null)
                {
                    Assert.Fail("Interface type not found.");
                }

                // Verify the dynamic class and its implementation
                _modelInstance = Activator.CreateInstance(_modelType);
                _modelVMInstance = Activator.CreateInstance(_modelVMType);

                var iViewModelType = vmAssembly?.GetTypes()
                    .FirstOrDefault(t => t.FullName?.Contains("IViewModel") ?? false);

                Type specificIViewModelType = iViewModelGenericType.MakeGenericType(_modelType, tiModelType);

                var implementsViewModelInterface = iViewModelType != null && iViewModelType.IsAssignableFrom(_modelVMType);

                Assert.IsTrue(implementsViewModelInterface, "The ViewModel does not implement the expected IViewModel interface.");

                if (implementsViewModelInterface)
                {
                    var viewModelInterfaceInstance = _modelVMInstance as dynamic;

                    Assert.IsNotNull(viewModelInterfaceInstance, "Failed to cast to the interface type.");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Create failed with exception: {ex.Message}");
                ApplicationExceptionLogger.HandleException(ex);
            }
        }

        private Task DefineConstructors(DynamicClassBuilder builder, string assemblyFilePath)
        {
            // Define constructors here
            return Task.CompletedTask;
        }

        private Task DefineMethods(DynamicClassBuilder builder, Type modelType, Type tiModelType)
        {
            // Define methods here
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // Cleanup resources
            _testContext.Dispose();
        }
    }
}
