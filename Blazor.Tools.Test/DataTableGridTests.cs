using Blazor.Tools.BlazorBundler.Components.Grid;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;

namespace Blazor.Tools.Test
{
    [TestClass]
    public sealed class DataTableGridTests : SampleData
    {
        private DataTableGrid? _dataTableGrid;

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

        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Tear down after each test method.
        }


        [TestMethod]
        public async Task CreateDynamicBundlerDLL_Concrete_Test()
        {
            try
            {
                _dataTableGrid?.InitializeVariables();

                //await DefineConstructors(_dynamicClassBuilderMock.Object, _modelTempDllPath);
                //await DefineMethods(_dynamicClassBuilderMock.Object, _modelType, tiModelType);

            }
            catch (Exception ex)
            {
                Assert.Fail($"Create failed with exception: {ex.Message}");
                ApplicationExceptionLogger.HandleException(ex);
            }

            await Task.CompletedTask;
        }
    }
}
