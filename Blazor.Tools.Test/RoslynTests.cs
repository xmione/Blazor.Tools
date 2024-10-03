using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models;
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Assemblies;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Moq;
using System.Data;
using BlazorBootstrap;
using Bunit;
using Blazor.Tools.BlazorBundler.Components.Grid;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blazor.Tools.Test
{
    [TestClass]
    public sealed class RoslynTests
    {
        private BunitContext _dataTableGridContext;
        private BunitContext _tableGridContext;
        private BunitContext _tableGridInternalsContext;

        private Mock<IDataTableGrid>? _dataTableGridMock;
        private Mock<ITableGrid> _tableGridMock;
        private Mock<ITableGridInternals> _tableGridInternalsMock;

        private IRenderedComponent<DataTableGrid>? _dataTableGridComponent;
        private IRenderedComponent<TableGrid<IBase, IModelExtendedProperties>>? _tableGridComponent;
        private IRenderedComponent<TableGridInternals<IBase, IModelExtendedProperties>>? _tableGridInternalsComponent;

        private SampleData _sampleData;
        private DataTable? _selectedTableVM;
        private string _tableName = "table-name";
        private string _tableID = "table-id";
        private Type? _modelVMType;
        private IBase _modelInstance;
        private IViewModel<IBase, IModelExtendedProperties> _modelVMInstance;
        private Type _iModelExtendedPropertiesType;
        private Type _iViewModelType;
        private Type _tableGridType;
        private Type _modelType;
        private EventCallback<IEnumerable<IViewModel<IBase, IModelExtendedProperties>>> _itemsChangedCallback;

        [TestInitialize]
        public void TestInit()
        {
            // Set up before each test method.
            _dataTableGridContext = new BunitContext();
            _tableGridContext = new BunitContext();
            _tableGridInternalsContext = new BunitContext();

            // Initialize the mock objects
            _dataTableGridMock = new Mock<IDataTableGrid>();
            _tableGridMock = new Mock<ITableGrid>();
            _tableGridInternalsMock = new Mock<ITableGridInternals>();

            _sampleData = new SampleData();

            // Register the mock service with the test context's dependency injection
            _dataTableGridContext.Services.AddSingleton(_dataTableGridMock.Object);
            _tableGridContext.Services.AddSingleton(_tableGridMock.Object);
            _tableGridInternalsContext.Services.AddSingleton(_tableGridInternalsMock.Object);

            // Arrange
            // Set up the mock to return specific values or throw exceptions
            _dataTableGridMock.Setup(m => m.InitializeVariablesAsync()).Returns(Task.CompletedTask).Verifiable();
            _dataTableGridMock.Setup(m => m.RenderMainContentAsync(It.IsAny<RenderTreeBuilder>())).Returns(Task.CompletedTask).Verifiable();
            
            _tableGridMock.Setup(m => m.InitializeVariablesAsync()).Returns(Task.CompletedTask).Verifiable();
            _tableGridMock.Setup(m => m.RenderMainContentAsync(It.IsAny<RenderTreeBuilder>())).Returns(Task.CompletedTask).Verifiable();

            _tableGridInternalsMock.Setup(m => m.InitializeVariablesAsync()).Returns(Task.CompletedTask).Verifiable();
            _tableGridInternalsMock.Setup(m => m.RenderMainContentAsync(It.IsAny<RenderTreeBuilder>())).Returns(Task.CompletedTask).Verifiable();

            // Using a Predicate to Match Any Arguments
            //jsInterop.SetupVoid("StartCellClicked", _ => true);

            // Some more variable initializations
            _selectedTableVM = _sampleData.EmployeeDataTable?.Copy() ?? _selectedTableVM; // use this variable and do not change the parameter variable SelectedTable
            _tableName = _selectedTableVM?.TableName ?? _tableName!; //Employee
            _tableID = _tableName.ToLower();
            // Create an EventCallback for ItemsChanged
            _itemsChangedCallback = EventCallback.Factory.Create<IEnumerable<IViewModel<IBase, IModelExtendedProperties>>>(this, _sampleData.OnItemsChanged);

            // Setup JSInterop to handle the specific call
            var jsInterop = _dataTableGridContext.JSInterop;
            jsInterop.SetupVoid("StartCellClicked", true, _tableID);
            jsInterop.SetupVoid("scrollToBottom", $"{_tableID}-div");

            // Setup for TableGrid
            jsInterop = _tableGridContext.JSInterop;
            jsInterop.SetupVoid("StartCellClicked", true, _tableID);
            jsInterop.SetupVoid("scrollToBottom", $"{_tableID}-div");

            // Setup for TableGridInternals
            jsInterop = _tableGridInternalsContext.JSInterop;
            jsInterop.SetupVoid("StartCellClicked", true, _tableID);
            jsInterop.SetupVoid("scrollToBottom", _tableID);

        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Tear down after each test method.
        }

        [TestMethod]
        public void CreateBundlerDLL_Test()
        {

            // Define the paths in the Temp folder
            var tempFolderPath = Path.GetTempPath(); // Gets the system Temp directory
            string baseClassAssemblyName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models";
            string vmClassAssemblyName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels";
            string interfaceAssemblyName = "Blazor.Tools.BlazorBundler.Interfaces";
            string version = "3.1.20.0";
            string iModelExtendedPropertiesFullyQualifiedName = $"{interfaceAssemblyName}.IModelExtendedProperties";
            string iViewModelFullyQualifiedName = $"{interfaceAssemblyName}.IViewModel";
            string baseClassCode = string.Empty;
            string vmClassCode = string.Empty;

            string baseDLLPath = Path.Combine(tempFolderPath, $"{baseClassAssemblyName}.dll") ?? default!;

            string vmDllPath = Path.Combine(tempFolderPath, $"{vmClassAssemblyName}.dll") ?? default!;

            var vmClassName = $"{_tableName}VM";
            //Assembly vmClassAssembly = default!;

            var assemblyLocations = new List<string>
            {
                typeof(IBase).Assembly.Location
            };

            var usingStatements = new List<string>
            {
                "Blazor.Tools.BlazorBundler.Interfaces",
                "System"
            };
            
            using (var baseClassGenerator = new EntityClassDynamicBuilder(baseClassAssemblyName, _selectedTableVM!, assemblyLocations, usingStatements))
            {
                baseClassCode = baseClassGenerator.ToString();
                baseClassGenerator.EmitAssemblyToMemorySave(baseClassAssemblyName, version, baseDLLPath, baseClassCode);
                //baseClassGenerator.LoadAssembly();
                _modelType = baseClassGenerator?.ClassType!;

                //baseClassCode = baseClassCode.RemoveLines("using System");

                using (var vmClassGenerator = new ViewModelClassGenerator(vmClassAssemblyName, _modelType))
                {
                    vmClassGenerator.CreateFromDataTable(_selectedTableVM!);

                    vmClassCode = vmClassGenerator.ToString();
                    vmClassGenerator.EmitAssemblyToMemorySave(vmClassAssemblyName, version, vmDllPath, baseClassCode, vmClassCode);
                    //vmClassGenerator.LoadAssembly();
                    _modelVMType = vmClassGenerator?.ClassType!;

                    // Create an instance of the dynamically generated type
                    var dynamicInstance = (IViewModel<IBase, IModelExtendedProperties>)Activator.CreateInstance(_modelVMType)!;

                    // Use reflection to set the FirstName property
                    var firstNameProperty = _modelVMType.GetProperty("FirstName");
                    if (firstNameProperty != null)
                    {
                        firstNameProperty.SetValue(dynamicInstance, "John");
                    }

                    // Create a mock of the interface implemented by TestVM using Moq
                    var mockDynamicInstance = new Mock<IViewModel<IBase, IModelExtendedProperties>>();
                    mockDynamicInstance.SetupGet(x => x.IsEditMode).Returns(true);

                    // Assert that the FirstName property is set correctly using reflection
                    var firstNameValue = firstNameProperty?.GetValue(dynamicInstance);
                    Assert.AreEqual("John", firstNameValue);

                    //_modelType = null;
                    while (baseDLLPath.IsFileInUse())
                    {
                        //baseDLLPath.KillLockingProcesses();

                        Thread.Sleep(1000);
                    }

                    while (vmDllPath.IsFileInUse())
                    {
                        //vmDllPath.KillLockingProcesses();
                        Thread.Sleep(1000);
                    }

                    // Create an instance of the dynamically generated type
                    //var dynamicInstance = (IViewModel<IBase, IModelExtendedProperties>)Activator.CreateInstance(_modelVMType)!;
                    _modelInstance = (IBase)Activator.CreateInstance(_modelType)!;
                    _modelVMInstance = (IViewModel<IBase, IModelExtendedProperties>)Activator.CreateInstance(_modelVMType)!;
                    _iModelExtendedPropertiesType = typeof(IModelExtendedProperties);
                    _iViewModelType = typeof(IViewModel<,>).MakeGenericType(typeof(IBase), _iModelExtendedPropertiesType);
                    bool isAssignable = _iViewModelType.IsAssignableFrom(_modelVMType);
                    // Create an EventCallback for ItemsChanged
                    _itemsChangedCallback = EventCallback.Factory.Create<IEnumerable<IViewModel<IBase, IModelExtendedProperties>>>(this, _sampleData.OnItemsChanged);
                }

            }

            //_tableGridType = typeof(TableGrid<,>).MakeGenericType(_modelType, _iModelExtendedPropertiesType);
            //// Create an instance of the dynamically created component type
            //var componentInstance = Activator.CreateInstance(_tableGridType);
            //
            //// Use reflection to set the parameters
            //var parameters = new Dictionary<string, object>
            //{
            //    { "Title", _tableName },
            //    { "TableID", _tableID },
            //    { "ColumnDefinitions", _sampleData.ColumnDefinitions },
            //    { "Model", _modelInstance },
            //    { "IModel", iModel },
            //    { "ModelVM", _modelVMInstance },
            //    { "Items", _sampleData.Items },
            //    { "DataSources", _sampleData.DataSources },
            //    { "ItemsChanged", _itemsChangedCallback },
            //    { "AllowCellRangeSelection", true }
            //};

            //foreach (var param in parameters)
            //{
            //    var property = _tableGridType.GetProperty(param.Key);
            //    if (property != null && property.CanWrite)
            //    {
            //        property.SetValue(componentInstance, param.Value);
            //    }
            //}

            _tableGridComponent = _tableGridContext?.Render<TableGrid<IBase, IModelExtendedProperties>>(parameters => parameters
                    .Add(p => p.Title, _tableName) 
                    .Add(p => p.TableID, _tableID )
                    .Add(p => p.ColumnDefinitions, _sampleData.ColumnDefinitions )
                    .Add(p => p.Model, _modelInstance)
                    .Add(p => p.ModelVM, _modelVMInstance)
                    .Add(p => p.Items, _sampleData.Items)
                    .Add(p => p.DataSources, _sampleData.DataSources)
                    .Add(p => p.ItemsChanged, _sampleData.OnItemsChanged)
                    .Add(p => p.AllowCellRangeSelection, true)
                );

            // These codes below will be useful for testing the specific methods mentioned therein:
            //_tableGridMock.Verify(m => m.InitializeVariablesAsync(), Times.Once);
            //_tableGridMock.Verify(m => m.RenderMainContentAsync(It.IsAny<RenderTreeBuilder>()), Times.Once);
        }

        [TestMethod]
        public void DataTableGrid_Test()
        {

            // Arrange
            var dataTableGrid = _dataTableGridContext.Render<DataTableGrid>(parameters => parameters
                .Add(p => p.Title, "Test Title")
                .Add(p => p.DataSources, _sampleData.DataSources)
                .Add(p => p.SelectedTable, _sampleData.EmployeeDataTable)
                .Add(p => p.ModelsAssemblyName, _sampleData.ModelsAssemblyName)
                .Add(p => p.ViewModelsAssemblyName, _sampleData.ViewModelsAssemblyName)
                .Add(p => p.AllowCellRangeSelection, true)
                .Add(p => p.TableList, _sampleData.TableList)
                .Add(p => p.HiddenColumnNames, _sampleData.HiddenEmployeeColumns)
                .Add(p => p.HeaderNames, _sampleData.EmployeeHeaderNames)
                .Add(p => p.ItemsChanged, _sampleData.OnItemsChanged)
            );

            // Act
            var renderedMarkup = dataTableGrid.Markup;

            // Assert
            //dataTableGrid.Find("table").MarkupMatches("<table class=\"data-table-grid\">"); // this will only work if you complete the table markup
            //Assert.IsTrue(renderedMarkup.Contains("Test Title"));
            // Act
            var tableElement = dataTableGrid.Find("table.data-table-grid");

            // Assert
            Assert.IsNotNull(tableElement);
            Assert.AreEqual("data-table-grid", tableElement.ClassName);

        }

        [TestMethod]
        public void CreateEmployeeClass_Test()
        {
            string code = @"
    using System;
    namespace Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models
    {
        public class Employee
        {
            public int ID { get; set; }
            public string FirstName { get; set; } = default!;
            public string MiddleName { get; set; } = default!;
            public string LastName { get; set; } = default!;
            public DateOnly DateOfBirth { get; set; } = default!;
            public int CountryID { get; set; }
        }
    }";

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            string assemblyName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models";

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DateOnly).Assembly.Location)
            };

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, moduleName: assemblyName);
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references,
                compilationOptions);

            string outputPath = Path.Combine(Path.GetTempPath(), $"{assemblyName}.dll");

            using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                EmitResult result = compilation.Emit(fs);

                if (!result.Success)
                {
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        AppLogger.WriteInfo(diagnostic.ToString());
                    }
                }
            }

            // Load the saved assembly
            Assembly assembly = Assembly.LoadFile(outputPath);
            Module module = assembly.Modules.First();
            AppLogger.WriteInfo($"Module Name: {module.Name}");
            Type employeeType = assembly.GetType("Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models.Employee")!;
            AppLogger.WriteInfo($"Type: {employeeType.FullName}");
        }

        [TestMethod]
        public void TestDynamicAssemblyCreation()
        {
            // Define the source code for ClassA
            var classASource = @"
public class ClassA
{
    public virtual string GetMessage() => ""Hello from ClassA"";
}";

            // Define the source code for ClassB
            var classBSource = @"
public class ClassB : ClassA
{
    public override string GetMessage() => ""Hello from ClassB"";
}";

            // Compile both ClassA and ClassB into a single in-memory assembly
            var combinedAssemblyBytes = EmitAssemblyToMemory("CombinedAssembly", classASource, classBSource);

            // Load the combined assembly from memory
            var combinedAssembly = Assembly.Load(combinedAssemblyBytes);

            // Verify and list all types in the combined assembly (for diagnostic purposes)
            var typesInAssembly = combinedAssembly.GetTypes();
            foreach (var type in typesInAssembly)
            {
                AppLogger.WriteInfo($"Found type: {type.FullName}");
            }

            // Get types (be sure to use full names if necessary)
            var classAType = combinedAssembly.GetType("ClassA")!;
            var classBType = combinedAssembly.GetType("ClassB");

            if (classBType == null)
            {
                throw new InvalidOperationException("ClassB type could not be found in the assembly.");
            }

            bool isAssignable = classAType.IsAssignableFrom(classBType);
            Assert.IsTrue(isAssignable, "ClassA is not assignable from ClassB");
            // Create an instance of ClassB and assign it to a variable of type ClassA
            var classBInstance = Activator.CreateInstance(classBType);
            var classAInstance = (dynamic)classBInstance!;

            // Test the method calls
            var classAMessage = classAType?.GetMethod("GetMessage")?.Invoke(classAInstance, null);
            var classBMessage = classBType?.GetMethod("GetMessage")?.Invoke(classBInstance, null);

            // Assert the results
            Assert.AreEqual("Hello from ClassB", classAMessage);
            Assert.AreEqual("Hello from ClassB", classBMessage);
        }

        private static byte[] EmitAssemblyToMemory(string assemblyName, params string[] sourceCodes)
        {
            var syntaxTrees = sourceCodes.Select(code => CSharpSyntaxTree.ParseText(code)).ToList();
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references)
                .AddSyntaxTrees(syntaxTrees);

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
                foreach (var diagnostic in failures)
                {
                    AppLogger.WriteInfo($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                }
                throw new InvalidOperationException("Compilation failed");
            }

            return ms.ToArray();
        }

    }
}
