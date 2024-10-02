using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels;
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Assemblies;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using BlazorBootstrap;
using Humanizer;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using Moq;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using DataTableExtensions = Blazor.Tools.BlazorBundler.Extensions.DataTableExtensions;

namespace Blazor.Tools.BlazorBundler.Components.Grid
{
    public partial class DataTableGrid : ComponentBase, IDataTableGrid
    {
        [Parameter] public string Title { get; set; } = default!;
        [Parameter] public Dictionary<string, object> DataSources { get; set; } = default!;
        [Parameter] public DataTable SelectedTable { get; set; } = default!;
        [Parameter] public string ModelsAssemblyName { get; set; } = default!;
        [Parameter] public string ViewModelsAssemblyName { get; set; } = default!;
        [Parameter] public bool AllowCellRangeSelection { get; set; } = false;
        [Parameter] public List<AssemblyTable>? TableList { get; set; } = default!;
        [Parameter] public List<string> HiddenColumnNames { get; set; } = default!;
        [Parameter] public Dictionary<string, string> HeaderNames { get; set; } = default!;
        [Parameter] public EventCallback<IEnumerable<IModelExtendedProperties>> ItemsChanged { get; set; }

        [Inject] public IJSRuntime JSRuntime { get; set; } = default!;
        //[Inject] public ISessionTableService _sessionTableService { get; set; } = default!;

        private DataTable _selectedTableVM = default!;
        private DataRow[]? _selectedData = default!;
        private string _selectedFieldValue = string.Empty;
        //private SessionManager _sessionManager = SessionManager.Instance;
        //private bool _isRetrieved = false;
        private Type? _tableGridType = default!;

        private List<TargetTable>? _targetTables;
        private string _tableName = string.Empty;
        private List<TableColumnDefinition> _columnDefinitions = default!;
        private object? _modelVMInstance;
        private Type _modelType = default!;
        private Type _modelVMType = default!;
        private Type _iModelExtendedPropertiesType = default!;
        private object? _modelInstance;
        private bool _isFirstCellClicked = true;
        private string _startCell = string.Empty;
        private string _endCell = string.Empty;
        private int _startRow;
        private int _endRow;
        private int _startCol;
        private int _endCol;
        private IEnumerable<object> _items = Enumerable.Empty<object>();
        //private object? _tableGridInstance;
        private object? _tableGridComponentReference;
        private string _tableID = string.Empty;
        private Type _iViewModelType;

        //private IList<SessionItem>? _sessionItems;

        protected override async Task OnParametersSetAsync()
        {
            await InitializeVariables();
            //await RetrieveDataFromSessionTableAsync();
            await base.OnParametersSetAsync();
        }

        public async Task InitializeVariables()
        {
            try 
            {
                _selectedTableVM = SelectedTable?.Copy() ?? _selectedTableVM; // use this variable and do not change the parameter variable SelectedTable
                _tableName = SelectedTable?.TableName ?? _tableName; //Employee
                _tableID = _tableName.ToLower();

                //await CreateDynamicBundlerDLL();
                await CreateBundlerDLL();
                // Get the TableGrid component type with the correct generic types
                _tableGridType = typeof(TableGrid<,>).MakeGenericType(_modelType, _iModelExtendedPropertiesType);

                //_sessionItems = new List<SessionItem>
                //{
                //    new SessionItem()
                //    {
                //        Key = $"{Title}_selectedData", Value = _selectedData, Type = typeof(DataRow[]), Serialize = true
                //    },
                //    new SessionItem()
                //    {
                //        Key = $"{Title}_targetTables", Value = _targetTables, Type = typeof(List<TargetTable>), Serialize = true
                //    },
                //    new SessionItem()
                //    {
                //        Key = $"{Title}_tableSourceName", Value = _tableSourceName, Type = typeof(string), Serialize = true
                //    },
                //    new SessionItem()
                //    {
                //        Key = $"{Title}_addSetTargetTableModal", Value = _addSetTargetTableModal, Type = typeof(bool), Serialize = true
                //    }
                //};
            }
            catch (Exception ex)
            {
                AppLogger.HandleException(ex);
            }   
            

            await Task.CompletedTask;
        }

        public async Task CreateDynamicBundlerDLL()
        {
            try 
            {
                // Define the paths in the Temp folder
                //var tempFolderPath = Path.GetTempPath(); // Gets the system Temp directory
                //var modelTempDllPath = Path.Combine(tempFolderPath, $"{ModelsAssemblyName}.dll");
                //var modelVMTempDllPath = Path.Combine(tempFolderPath, $"{ViewModelsAssemblyName}.dll");
                string modelTempDllPath = string.Empty;
                string modelVMTempDllPath = string.Empty;

                // Create an instance of DynamicClassBuilder for TModel
                using (var modelClassBuilder = new DynamicClassBuilder(null, ModelsAssemblyName, _tableName, null, null))
                //using (var modelClassBuilder = new DynamicClassBuilder(null, null, _tableName, null, null))
                {
                    modelTempDllPath = modelClassBuilder.AssemblyFilePath ?? default!;
                    // Create the TModel class from the DataTable
                    if (SelectedTable != null)
                    {
                        modelClassBuilder.CreateClassFromDataTable(SelectedTable);
                    }

                    // Save the model assembly to the Temp folder
                    modelClassBuilder.SaveAssembly();

                    // Retrieve type from the model instance
                    _modelType = modelClassBuilder?.DynamicType ?? default!;
                    modelTempDllPath = modelClassBuilder?.AssemblyFilePath ?? default!;

                    if (_modelType == null)
                    {
                        throw new InvalidOperationException("Failed to retrieve the type from the model instance.");
                    }
                }

                // Define and create an instance of DynamicClassBuilder for TModelVM
                var tiModelType = typeof(IModelExtendedProperties);
                Type iViewModelGenericType = typeof(IViewModel<,>);
                var iViewModelTypes = new Type[] { iViewModelGenericType.MakeGenericType(_modelType, tiModelType), tiModelType };

                Assembly? vmAssembly;
                using (var modelVMClassBuilder = new DynamicClassBuilder(
                    null,
                    ViewModelsAssemblyName,
                    _tableName + "VM", // EmployeeVM
                    _modelType, // base class e.g.: Employee
                    iViewModelTypes,
                    true
                ))
                //using (var modelVMClassBuilder = new DynamicClassBuilder(
                //    null,
                //    null,
                //    _tableName + "VM", // EmployeeVM
                //    _modelType, // base class e.g.: Employee
                //    iViewModelTypes,
                //    useContextAsParent: true
                //))
                {
                    // Create the TModelVM class from the DataTable
                    modelVMClassBuilder.CreateClassFromDataTable(SelectedTable);

                    await DefineConstructors(modelVMClassBuilder, modelVMClassBuilder.AssemblyFilePath ?? default!);
                    await DefineMethods(modelVMClassBuilder, _modelType, tiModelType);

                    // Save the model view assembly to the Temp folder
                    modelVMClassBuilder.SaveAssembly();

                    vmAssembly = modelVMClassBuilder.Assembly;
                    // Retrieve type from the model view instance
                    _modelVMType = modelVMClassBuilder?.DynamicType ?? default!;
                    modelVMTempDllPath = modelVMClassBuilder?.AssemblyFilePath ?? default!;

                    if (_modelVMType == null)
                    {
                        throw new InvalidOperationException("Failed to retrieve the type from the model view instance.");
                    }

                    var tIModelTypeFullName = tiModelType.FullName ?? default!;
                    _iModelExtendedPropertiesType = _modelVMType.GetInterface(tIModelTypeFullName) ?? default!;
                }

                await DefineTableColumns(modelVMTempDllPath, ModelsAssemblyName, ViewModelsAssemblyName);

                _modelInstance = Activator.CreateInstance(_modelType);
                _modelVMInstance = Activator.CreateInstance(_modelVMType);

                var iViewModelType = vmAssembly?.GetTypes()
                                                .FirstOrDefault(t => (t.FullName?.Contains("IViewModel") ?? false)) ?? default!;

                Type specificIViewModelType = iViewModelGenericType?.MakeGenericType(_modelType, tiModelType) ?? default!;

                var isInterfaceEqual = iViewModelType == specificIViewModelType;
                var interfaces = _modelVMInstance?.GetType().GetInterfaces().Select(i => i.Name);


                if (_modelVMInstance == null)
                {
                    Console.WriteLine("Failed to create an instance of the ViewModel.");
                    return;
                }

                // Check if the ViewModel implements the specific IViewModel<T, U>
                _modelVMType = _modelVMInstance.GetType();
                bool implementsViewModelInterface = iViewModelType.IsAssignableFrom(_modelVMType);
                Console.WriteLine($"Implements IViewModel<{_tableName}, IModelExtendedProperties>: {implementsViewModelInterface}");

                if (implementsViewModelInterface)
                {
                    // Cast the instance to the interface type
                    var viewModelInterfaceInstance = _modelVMInstance as dynamic;

                    if (viewModelInterfaceInstance != null)
                    {
                        Console.WriteLine("Successfully casted to the interface type.");
                        // You can now work with the viewModelInterfaceInstance
                    }
                    else
                    {
                        Console.WriteLine("Failed to cast to the interface type.");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to cast to the interface type.");
                }
            }
            catch(Exception ex) 
            {
                AppLogger.HandleException(ex);
            }
            

        }

        public async Task CreateBundlerDLL()
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

            var selectedTable = _selectedTableVM;
            var tableName = selectedTable.TableName;
            string baseDLLPath = Path.Combine(tempFolderPath, $"{baseClassAssemblyName}.dll") ?? default!;

            string vmDllPath = Path.Combine(tempFolderPath, $"{vmClassAssemblyName}.dll") ?? default!;

            var vmClassName = $"{tableName}VM";
            Type vmClassType = default!;
            //Assembly vmClassAssembly = default!;

            var usingStatements = new List<string>
            {
                "System"
            };

            Type baseClassType = default!;
            using (var baseClassGenerator = new EntityClassDynamicBuilder(baseClassAssemblyName, selectedTable, usingStatements))
            {
                baseClassCode = baseClassGenerator.ToString();
                baseClassGenerator.EmitAssemblyToMemorySave(baseClassAssemblyName, version, baseDLLPath, baseClassCode);
                //baseClassGenerator.LoadAssembly();
                baseClassType = baseClassGenerator?.ClassType!;

                //baseClassCode = baseClassCode.RemoveLines("using System");

                using (var vmClassGenerator = new ViewModelClassGenerator(vmClassAssemblyName, baseClassType))
                {
                    vmClassGenerator.CreateFromDataTable(selectedTable);

                    vmClassCode = vmClassGenerator.ToString();
                    vmClassGenerator.EmitAssemblyToMemorySave(vmClassAssemblyName, version, vmDllPath, baseClassCode, vmClassCode);
                    //vmClassGenerator.LoadAssembly();
                    vmClassType = vmClassGenerator?.ClassType!;

                     
                }

            }


            //baseClassType = null;
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
            //var dynamicInstance = (IViewModel<IBase, IModelExtendedProperties>)Activator.CreateInstance(vmClassType)!;
            var dynamicInstance =  Activator.CreateInstance(vmClassType)!;

            
            _modelType = baseClassType;
            _modelVMType = vmClassType;

            _modelInstance = Activator.CreateInstance(_modelType);
            _modelVMInstance = dynamicInstance;
            _iModelExtendedPropertiesType = typeof(IModelExtendedProperties);

            _iViewModelType = typeof(IViewModel<,>).MakeGenericType(_modelType,_iModelExtendedPropertiesType);
            var iViewModelType = vmClassType.Assembly.GetType(_iViewModelType.FullName!);

            _iViewModelType.DisplayTypeDifferences(iViewModelType!);
            typeof(Employee).DisplayTypeDifferences(_modelType);
            typeof(EmployeeVM).DisplayTypeDifferences(_modelVMType);

            bool isAssignable = _modelVMType.IsAssignableFrom(iViewModelType);
            await DefineTableColumns();
            await Task.CompletedTask;
        }

        private async Task DefineTableColumns()
        {
            
            var modelExtendedProperties = new ModelExtendedProperties();
            var excludedColumns = modelExtendedProperties.GetProperties();
            // TableColumnDefinition should be based on model type, e.g.: Employee class and not EmployeeVM
            var props = _modelType.GetProperties();
            if (props != null)
            {
                //_selectedTableVM.AddPropertiesFromPropertyInfoList(props);
                _columnDefinitions = new List<TableColumnDefinition>();
                foreach (PropertyInfo property in props)
                {
                    var tableColumnDefinition = new TableColumnDefinition
                    {
                        ColumnName = property.Name,
                        HeaderText = property.Name,
                        ColumnType = typeof(string),
                        CellClicked = async (columnName, vm, rowIndex) => await HandleCellClickAsync(columnName, vm, rowIndex),
                        ValueChanged = new Action<object, object>((newValue, modelInstance) => OnValueChanged(property.Name, newValue, modelInstance))
                    };

                    _columnDefinitions.Add(tableColumnDefinition);
                }
            }

            _selectedTableVM.AddPropertiesFromPropertyInfoList(excludedColumns!);

            // Use reflection to call the ConvertDataTableToObjects method
            var itemsMethod = typeof(DataTableExtensions).GetMethod(
                "ConvertDataTableToObjects",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] { typeof(DataTable) }, // Specify the parameters here
                null);

            if (itemsMethod != null)
            {
                var genericMethod = itemsMethod.MakeGenericMethod(_modelVMType);
                var items = genericMethod.Invoke(null, new object[] { _selectedTableVM });

                // Cast items to IEnumerable<object>
                if (items != null)
                {
                    _items = (IEnumerable<object>)items;
                }
            }
            else
            {
                throw new InvalidOperationException("The ConvertDataTableToObjects method was not found.");
            }

            await Task.CompletedTask;
        }

        public async Task DefineConstructors(IDynamicClassBuilder vmClassBuilder, string modelVMTempDllPath)
        {
            // Define the field for the contextProvider only once
            FieldBuilder contextProviderField = vmClassBuilder.DefineField("contextProvider", typeof(IContextProvider), FieldAttributes.Private);
            // Define constructors
            // Parameterless constructor
            vmClassBuilder.DefineConstructor(Type.EmptyTypes, ilg =>
            {
                ConstructorInfo baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Call, baseConstructor);

                // Initialize contextProvider with a new instance of ContextProvider
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Newobj, typeof(ContextProvider).GetConstructor(Type.EmptyTypes)!);
                ilg.Emit(OpCodes.Stfld, contextProviderField);
                ilg.Emit(OpCodes.Ret);

                Console.WriteLine("Constructor with no parameters defined.");
            });

            // Constructor with IContextProvider parameter
            vmClassBuilder.DefineConstructor(new[] { typeof(IContextProvider) }, ilg =>
            {
                ConstructorInfo baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Call, baseConstructor);

                // Set contextProvider field
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ldarg_1);
                ilg.Emit(OpCodes.Stfld, contextProviderField);
                ilg.Emit(OpCodes.Ret);

                Console.WriteLine("Constructor with IContextProvider parameter defined.");
            });

            // Constructor with IContextProvider and Employee parameter
            vmClassBuilder.DefineConstructor(new[] { typeof(IContextProvider), _modelType }, ilg =>
            {
                ConstructorInfo baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Call, baseConstructor);

                // Set contextProvider field
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ldarg_1);
                ilg.Emit(OpCodes.Stfld, contextProviderField);

                // Set properties from Employee model
                ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                ilg.Emit(OpCodes.Ldarg_2); // Load Employee model
                foreach (var prop in _modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.CanWrite)
                    {
                        ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                        ilg.Emit(OpCodes.Ldarg_2); // Load Employee model
                        ilg.Emit(OpCodes.Callvirt, prop.GetGetMethod()!); // Get property value
                        ilg.Emit(OpCodes.Callvirt, prop.GetSetMethod()!); // Set property value
                    }
                }
                ilg.Emit(OpCodes.Ret);

                Console.WriteLine("Constructor with IContextProvider and Employee parameter defined.");
            });

            // Update _modelVMType. 
            _modelVMType = vmClassBuilder.TypeBuilder ?? default!;

            // Constructor with IContextProvider and EmployeeVM parameter
            vmClassBuilder.DefineConstructor(new[] { typeof(IContextProvider), _modelVMType }, ilg =>
            {
                ConstructorInfo baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Call, baseConstructor);

                // Set contextProvider field
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ldarg_1);
                ilg.Emit(OpCodes.Stfld, contextProviderField);

                // Set properties from EmployeeVM modelVM
                ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                ilg.Emit(OpCodes.Ldarg_2); // Load EmployeeVM modelVM

                var addedProperties = vmClassBuilder.AddedProperties ?? default!;
                foreach (PropertyBuilder prop in addedProperties)
                {
                    if (prop.CanWrite)
                    {
                        ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                        ilg.Emit(OpCodes.Ldarg_2); // Load EmployeeVM modelVM
                        ilg.Emit(OpCodes.Callvirt, prop.GetGetMethod()!); // Get property value
                        ilg.Emit(OpCodes.Callvirt, prop.GetSetMethod()!); // Set property value
                    }
                }

                ilg.Emit(OpCodes.Ret);

                Console.WriteLine("Constructor with IContextProvider and EmployeeVM parameter defined.");
            });

            //// Verify all constructors
            //var vmType = modelVMClassBuilder.CreateType(); // Create the final VM class

            //// Log all constructors to ensure they are defined
            //var constructors = vmType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //foreach (var ctor in constructors)
            //{
            //    var parameters = ctor.GetParameters();
            //    Console.WriteLine($"Constructor: {ctor.Name}, Parameters: {string.Join(", ", parameters.Select(p => p.ParameterType.Name))}");
            //}

            await Task.CompletedTask;
        }

        public async Task DefineMethods(IDynamicClassBuilder vmClassBuilder, Type tModelType, Type tiModelType)
        {

            // Define methods dynamically
            vmClassBuilder.DefineMethod("ToNewModel", tModelType, Type.EmptyTypes, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Check for parameterless constructor
                ConstructorInfo constructor = tModelType.GetConstructor(Type.EmptyTypes)
                    ?? throw new InvalidOperationException($"No parameterless constructor found for type {tModelType.Name}");

                // Emit IL code to call the constructor
                ilg.Emit(OpCodes.Newobj, constructor);
                ilg.Emit(OpCodes.Ret);
            });

            vmClassBuilder.DefineMethod("ToNewIModel", tiModelType, Type.EmptyTypes, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Get the constructor of tModelType
                ConstructorInfo? constructor = tModelType.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                {
                    throw new InvalidOperationException($"No parameterless constructor found for type {tModelType.Name}");
                }

                // Emit IL to create a new instance of tModelType
                ilg.Emit(OpCodes.Newobj, constructor); // Create new instance of tModelType
                LocalBuilder localBuilderInstance = ilg.DeclareLocal(tModelType); // Declare a local variable for the instance
                ilg.Emit(OpCodes.Stloc, localBuilderInstance); // Store the instance in the local variable

                // Get properties of tModelType
                var properties = tModelType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite) // Only properties with setters
                    .ToArray();

                // Set properties on the new instance
                foreach (var property in properties)
                {
                    // Load the instance and the value to set
                    ilg.Emit(OpCodes.Ldloc, localBuilderInstance); // Load instance
                    ilg.Emit(OpCodes.Ldarg_0); // Load 'this'

                    // Load the value of the property from 'this'
                    var prop = tModelType.GetProperty(property.Name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    if (prop != null)
                    {
                        ilg.Emit(OpCodes.Callvirt, prop?.GetGetMethod() ?? default!); // Call property getter
                        ilg.Emit(OpCodes.Callvirt, property?.GetSetMethod() ?? default!); // Call property setter
                    }
                }

                // Return the new instance
                ilg.Emit(OpCodes.Ldloc, localBuilderInstance);
                ilg.Emit(OpCodes.Ret);
            });

            // Define the method in the dynamic class builder
            vmClassBuilder.DefineMethod("FromModel", typeof(Task<>).MakeGenericType(typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType)), new[] { tModelType }, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Load 'this' onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_0);

                // Load the model argument onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_1);

                // Get the 'FromModel' method from the IViewModel<TModel, TIModel> interface
                var iViewModelType = typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType);
                var fromModelMethod = iViewModelType.GetMethod("FromModel", new[] { tModelType });

                if (fromModelMethod == null)
                {
                    throw new InvalidOperationException($"No method found with name 'FromModel' in type {iViewModelType.Name}");
                }

                // Call the 'FromModel' method
                ilg.Emit(OpCodes.Callvirt, fromModelMethod);

                // Convert the result to Task<IViewModel<TModel, TIModel>>
                ilg.Emit(OpCodes.Castclass, typeof(Task<>).MakeGenericType(typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType)));

                // Return the result
                ilg.Emit(OpCodes.Ret);
            });

            vmClassBuilder.DefineMethod("SetEditMode", typeof(Task<>).MakeGenericType(typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType)), new[] { typeof(bool) }, new[] { "isEditMode" }, (ilg, localBuilder) =>
            {
                // Load 'this' onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_0);

                // Load the isEditMode argument onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_1);

                // Set the IsEditMode property
                var isEditModeProperty = tiModelType.GetProperty("IsEditMode", BindingFlags.Public | BindingFlags.Instance);
                if (isEditModeProperty == null)
                {
                    throw new InvalidOperationException("IsEditMode property not found.");
                }

                ilg.Emit(OpCodes.Callvirt, isEditModeProperty?.GetSetMethod() ?? default!);

                // Emit async completion
                var completedTaskProperty = typeof(Task).GetProperty(nameof(Task.CompletedTask));
                if (completedTaskProperty == null)
                {
                    throw new InvalidOperationException("CompletedTask property not found.");
                }

                ilg.Emit(OpCodes.Call, completedTaskProperty?.GetGetMethod() ?? default!);
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ret);
            });

            vmClassBuilder.DefineMethod("SaveModelVM", typeof(Task<>).MakeGenericType(typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType)), Type.EmptyTypes, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Load 'this' onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_0);

                // Set the IsEditMode property to false
                var isEditModeProperty = tiModelType.GetProperty("IsEditMode", BindingFlags.Public | BindingFlags.Instance);
                if (isEditModeProperty == null)
                {
                    throw new InvalidOperationException("IsEditMode property not found.");
                }

                ilg.Emit(OpCodes.Ldc_I4_0);
                ilg.Emit(OpCodes.Callvirt, isEditModeProperty?.GetSetMethod() ?? default!);

                // Emit async completion
                var completedTaskProperty = typeof(Task).GetProperty(nameof(Task.CompletedTask));
                if (completedTaskProperty == null)
                {
                    throw new InvalidOperationException("CompletedTask property not found.");
                }

                ilg.Emit(OpCodes.Call, completedTaskProperty?.GetGetMethod() ?? default!);
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ret);
            });

            vmClassBuilder.DefineMethod("SaveModelVMToNewModelVM", typeof(Task<>).MakeGenericType(typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType)), Type.EmptyTypes, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Load 'this' onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_0);

                // Get all defined constructors
                var constructors = vmClassBuilder.GetDefinedConstructors();

                foreach (var (ctor, parameters) in constructors)
                {
                    Console.WriteLine($"Constructor: {ctor.Name}, Parameters: {string.Join(", ", parameters.Select(p => p.Name))}");
                }

                // Identify the constructor with IContextProvider
                var constructor = constructors.FirstOrDefault(c =>
                    c.ParameterTypes.Length == 1 && c.ParameterTypes[0] == typeof(IContextProvider));

                if (constructor == default)
                {
                    throw new InvalidOperationException("Constructor with IContextProvider not found.");
                }

                // Create a new instance of tModelType using the identified constructor
                ilg.Emit(OpCodes.Ldarg_1); // Load the IContextProvider argument
                ilg.Emit(OpCodes.Newobj, constructor.ConstructorBuilder); // Call the constructor to create a new instance

                // Store the new instance in a local variable
                var newInstance = ilg.DeclareLocal(tModelType);
                ilg.Emit(OpCodes.Stloc, newInstance);

                // Load properties from 'this' and set them on the new instance
                var properties = tModelType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite)
                    .ToArray();

                foreach (var property in properties)
                {
                    // Load the new instance
                    ilg.Emit(OpCodes.Ldloc, newInstance);

                    // Load property value from 'this'
                    ilg.Emit(OpCodes.Ldarg_0);
                    ilg.Emit(OpCodes.Callvirt, property?.GetGetMethod() ?? default! );

                    // Set the property on the new instance
                    ilg.Emit(OpCodes.Callvirt, property?.GetSetMethod() ?? default!);
                }

                // Emit async completion
                var completedTaskProperty = typeof(Task).GetProperty(nameof(Task.CompletedTask));
                if (completedTaskProperty == null)
                {
                    throw new InvalidOperationException("CompletedTask property not found.");
                }

                ilg.Emit(OpCodes.Call, completedTaskProperty?.GetGetMethod() ?? default!);

                // Load the new instance and return it as Task<IViewModel<TModel, TIModel>>
                ilg.Emit(OpCodes.Ldloc, newInstance);
                ilg.Emit(OpCodes.Ret);
            });

            vmClassBuilder.DefineMethod("AddItemToList",
             typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType))),
             new[] { typeof(IEnumerable<>).MakeGenericType(typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType)) },
             new[] { "modelVMList" },
             (ilg, localBuilder) =>
             {
                 // Load the modelVMList argument
                 ilg.Emit(OpCodes.Ldarg_1);

                 // Create a list from the argument
                 var toListMethod = typeof(Enumerable).GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)
                     ?.MakeGenericMethod(typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType)) ?? default!;
                 ilg.Emit(OpCodes.Call, toListMethod);

                 // Add 'this' to the list
                 ilg.Emit(OpCodes.Ldloc_0);
                 ilg.Emit(OpCodes.Ldarg_0);
                 var addMethod = typeof(List<>).MakeGenericType(typeof(IViewModel<,>)
                     .MakeGenericType(tModelType, tiModelType))
                 .GetMethod("Add") ?? default!;
                 ilg.Emit(OpCodes.Callvirt, addMethod);

                 // Emit async completion
                 var completedTaskProperty = typeof(Task).GetProperty(nameof(Task.CompletedTask));
                 if (completedTaskProperty == null)
                 {
                     throw new InvalidOperationException("CompletedTask property not found.");
                 }

                 ilg.Emit(OpCodes.Call, completedTaskProperty?.GetGetMethod() ?? default!);
                 ilg.Emit(OpCodes.Ldloc_0);
                 ilg.Emit(OpCodes.Ret);
             });

            vmClassBuilder.DefineMethod(
             "UpdateList",
             typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType))),
             new[] { typeof(IEnumerable<>).MakeGenericType(typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType)), typeof(bool) },
             new[] { "modelVMList", "isAdding" },
             (ilg, localBuilder) =>
             {
                 // Convert modelVMList to List
                 ilg.Emit(OpCodes.Ldarg_1);
                 var toListMethod = typeof(Enumerable).GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)
                     ?.MakeGenericMethod(typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType)) ?? default!;
                 ilg.Emit(OpCodes.Call, toListMethod);

                 // Store the list in a local variable
                 var listLocal = ilg.DeclareLocal(typeof(List<>).MakeGenericType(typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType)));
                 ilg.Emit(OpCodes.Stloc, listLocal);

                 // If isAdding, remove 'this' from the list
                 var addLabel = ilg.DefineLabel();
                 ilg.Emit(OpCodes.Ldarg_2);  // Load isAdding argument
                 ilg.Emit(OpCodes.Brtrue_S, addLabel); // If true, jump to adding logic

                 // Remove 'this' from the list
                 ilg.Emit(OpCodes.Ldloc, listLocal);
                 ilg.Emit(OpCodes.Ldarg_0); // Load 'this'
                 var removeMethod = typeof(List<>).MakeGenericType(typeof(IViewModel<,>)
                     .MakeGenericType(tModelType, tiModelType)).GetMethod("Remove") ?? default!;
                 ilg.Emit(OpCodes.Callvirt, removeMethod);

                 // Assign list back to modelVMList (mimics modelVMList = list)
                 ilg.MarkLabel(addLabel); // Add logic starts here
                 ilg.Emit(OpCodes.Ldloc, listLocal); // Load the modified list
                 ilg.Emit(OpCodes.Starg_S, 1); // Store it back to the modelVMList argument

                 // Emit async completion
                 var completedTaskProperty = typeof(Task).GetProperty(nameof(Task.CompletedTask));
                 if (completedTaskProperty == null)
                 {
                     throw new InvalidOperationException("CompletedTask property not found.");
                 }

                 ilg.Emit(OpCodes.Call, completedTaskProperty?.GetGetMethod() ?? default!);
                 ilg.Emit(OpCodes.Ret);
             });

            vmClassBuilder.DefineMethod(
            "DeleteItemFromList",
            typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType))),
            new[] { typeof(IEnumerable<>).MakeGenericType(typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType)) },
            new[] { "modelVMList" },
            (ilg, localBuilder) =>
            {
                // Load the modelVMList argument
                ilg.Emit(OpCodes.Ldarg_1);

                // Convert to list
                var toListMethod = typeof(Enumerable).GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)
                    ?.MakeGenericMethod(typeof(IViewModel<,>).MakeGenericType(tModelType, tiModelType)) ?? default!;
                ilg.Emit(OpCodes.Call, toListMethod);

                // Remove 'this' from the list
                ilg.Emit(OpCodes.Ldloc_0);
                ilg.Emit(OpCodes.Ldarg_0);
                var listType = typeof(List<>);
                var iViewModelType = typeof(IViewModel<,>);
                var removeMethod = listType.MakeGenericType(iViewModelType.MakeGenericType(tModelType, tiModelType))?.GetMethod("Remove") ?? default!;
                ilg.Emit(OpCodes.Callvirt, removeMethod);

                // Emit async completion
                var completedTaskProperty = typeof(Task).GetProperty(nameof(Task.CompletedTask));
                if (completedTaskProperty == null)
                {
                    throw new InvalidOperationException("CompletedTask property not found.");
                }

                ilg.Emit(OpCodes.Call, completedTaskProperty?.GetGetMethod() ?? default!);
                ilg.Emit(OpCodes.Ldloc_0);
                ilg.Emit(OpCodes.Ret);
            });

            await Task.CompletedTask;
        }

        public async Task DefineTableColumns(string dllPath, string modelTypeName, string modelVMTypeName)
        {
            var modelExtendedProperties = new ModelExtendedProperties();
            var excludedColumns = modelExtendedProperties.GetProperties() ?? default!;
            // TableColumnDefinition should be based on model type, e.g.: Employee class and not EmployeeVM
            var props = _modelType.GetProperties();
            if (props != null)
            {
                //_selectedTableVM.AddPropertiesFromPropertyInfoList(props);
                _columnDefinitions = new List<TableColumnDefinition>();
                foreach (PropertyInfo property in props)
                {
                    var tableColumnDefinition = new TableColumnDefinition
                    {
                        ColumnName = property.Name,
                        HeaderText = property.Name,
                        ColumnType = typeof(string),
                        CellClicked = async (columnName, vm, rowIndex) => await HandleCellClickAsync(columnName, vm, rowIndex),
                        ValueChanged = new Action<object, object>((newValue, modelInstance) => OnValueChanged(property.Name, newValue, modelInstance))
                    };

                    _columnDefinitions.Add(tableColumnDefinition);
                }
            }

            _selectedTableVM.AddPropertiesFromPropertyInfoList(excludedColumns);
            
            // Use reflection to call the ConvertDataTableToObjects method
            var itemsMethod = typeof(DataTableExtensions).GetMethod(
                "ConvertDataTableToObjects",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] { typeof(DataTable) }, // Specify the parameters here
                null);

            if (itemsMethod != null)
            {
                var genericMethod = itemsMethod.MakeGenericMethod(_modelVMType);
                var items = genericMethod.Invoke(null, new object[] { _selectedTableVM });

                // Cast items to IEnumerable<object>
                if (items != null)
                {
                    _items = (IEnumerable<object>)items;
                }
            }
            else
            {
                throw new InvalidOperationException("The ConvertDataTableToObjects method was not found.");
            }

            await Task.CompletedTask;
        }

        public void OnValueChanged(string propertyName, object newValue, object modelInstance)
        {
            if (modelInstance == null)
                return;

            var modelType = modelInstance.GetType();
            var property = modelType.GetProperty(propertyName);

            if (property != null)
            {
                var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                var safeValue = Convert.ChangeType(newValue, propertyType);
                property.SetValue(modelInstance, safeValue);

                // Update the items list
                foreach (var item in _items)
                {
                    var itemType = item.GetType();
                    var idProperty = itemType.GetProperty("RowID"); // Adjust based on your model
                    if (idProperty != null && idProperty.GetValue(item)?.Equals(modelInstance.GetType().GetProperty("RowID")?.GetValue(modelInstance)) == true)
                    {
                        var existingProperty = itemType.GetProperty(propertyName);
                        if (existingProperty != null)
                        {
                            existingProperty.SetValue(item, safeValue);
                        }
                        break; // Assuming RowID is unique, exit after updating
                    }
                }
            }

            // Notify that state has changed (if applicable)
            // StateHasChanged(); this should be triggered on the calling program after calling this method
        }

        public async Task HandleCellClickAsync(string id, object modelInstance, int column)
        {
            var modelVM = modelInstance as IModelExtendedProperties; // Adjust if necessary for your generic model
            if (modelVM == null || modelVM.IsEditMode)
            {
                return;
            }

            string cellIdentifier = $"R{modelVM.RowID}C{column}";
            var startCellID = $"{_tableID}-start-cell";
            var endCellID = $"{_tableID}-end-cell";
            _startCell = await JSRuntime.InvokeAsync<string>("getValue", startCellID);
            _endCell = await JSRuntime.InvokeAsync<string>("getValue", endCellID);

            bool areBothFilled = !string.IsNullOrEmpty(_startCell) && !string.IsNullOrEmpty(_endCell);
            if (string.IsNullOrEmpty(_startCell) || _isFirstCellClicked)
            {
                _startRow = modelVM.RowID;
                _startCol = column;
                _startCell = cellIdentifier;
                _isFirstCellClicked = areBothFilled ? _isFirstCellClicked : false;

                await JSRuntime.InvokeVoidAsync("setValue", $"{_tableID}-start-row", $"{_startRow}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{_tableID}-start-col", $"{_startCol}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{_tableID}-start-cell", cellIdentifier);
            }
            else
            {
                _endRow = modelVM.RowID;
                _endCol = column;
                _endCell = cellIdentifier;
                _isFirstCellClicked = areBothFilled ? _isFirstCellClicked : true;

                await JSRuntime.InvokeVoidAsync("setValue", $"{_tableID}-end-row", $"{_endRow}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{_tableID}-end-col", $"{_endCol}");
                await JSRuntime.InvokeVoidAsync("setValue", $"{_tableID}-end-cell", cellIdentifier);
            }

            areBothFilled = !string.IsNullOrEmpty(_startCell) && !string.IsNullOrEmpty(_endCell);

            if (areBothFilled)
            {
                var totalRows = _items.Count();
                var totalCols = _columnDefinitions?.Count;
                await JSRuntime.InvokeVoidAsync("toggleCellBorders", _startRow, _endRow, _startCol, _endCol, totalRows, totalCols, _tableID, true);
            }

            await Task.CompletedTask;
        }

        private async Task RetrieveDataFromSessionTableAsync()
        {
            //try
            //{
            //    if (!_isRetrieved && _sessionItems != null)
            //    {
            //        _sessionItems = await _sessionManager.RetrieveSessionListAsync(_sessionItems);
            //        _selectedData = (DataRow[]?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_selectedData"))?.Value;
            //        _targetTables = (List<TargetTable>?)_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_targetTables"))?.Value;
            //        _addSetTargetTableModal = (bool)(_sessionItems?.FirstOrDefault(s => s.Key.Equals($"{Title}_addSetTargetTableModal"))?.Value ?? false);

            //        _isRetrieved = true;
            //        StateHasChanged();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Error: {0}", ex.Message);
            //}

            await Task.CompletedTask;
        }

        private async Task CloseSetTargetTableModalAsync()
        {
            var modalId = $"{Title.ToLower()}-set-target-table-modal";
            await JSRuntime.InvokeVoidAsync("removeClassFromElement", modalId, "show");

            //_sessionManager.SaveToSessionTableAsync($"{Title}_addSetTargetTableModal", _addSetTargetTableModal, serialize: true).Wait();
        }

        private async Task SaveToTargetTableAsync(List<TargetTable>? targetTables)
        {
            await CloseSetTargetTableModalAsync();

            _targetTables = targetTables;
            //await _sessionManager.SaveToSessionTableAsync($"{Title}_targetTables", _targetTables, serialize: true);

            StateHasChanged();

            await Task.CompletedTask;
        }

        private async Task HandleSelectedDataComb(DataRow[] selectedData)
        {
            _selectedData = selectedData;

            //await _sessionManager.SaveToSessionTableAsync($"{Title}_selectedData", _selectedData, serialize: true);
            //await _tableGrid.HandleSelectedDataComb(selectedData);

            StateHasChanged();

            await Task.CompletedTask;
        }

        private async Task HandleSetTargetTableColumnList(List<TargetTableColumn> targetTableColumnList)
        {
            await Task.CompletedTask;
        }

        private async Task HandleFieldValueChangedAsync(string newValue)
        {
            _selectedFieldValue = newValue;
            await Task.CompletedTask;
        }

        private async Task ShowSetTargetTableModalAsync()
        {
            var startCellID = $"{_tableID}-start-cell";
            var endCellID = $"{_tableID}-end-cell";
            _startCell = await JSRuntime.InvokeAsync<string>("getValue", startCellID);
            _endCell = await JSRuntime.InvokeAsync<string>("getValue", endCellID);

            // Get the method info for ShowSetTargetTableModalAsync
            var showTargetTableModalAsyncMethod = _tableGridType?.GetMethod("ShowSetTargetTableModalAsync");
            var reloadTableGridInternalsComponent = _tableGridType?.GetMethod("ReloadTableGridInternalsComponent");

            if (_tableGridComponentReference != null)
            {
                // Cast the reference to the appropriate type (generic table grid)
                var tableGridInstance = _tableGridComponentReference as dynamic;

                // Invoke the method
                if (tableGridInstance != null)
                {

                    var resultTask = tableGridInstance.ShowSetTargetTableModalAsync(_startCell, _endCell);

                    // Since it's a Task, you can await it or use other async handling
                    _selectedData = await resultTask;

                    // Now you can use the 'rows' variable which is of type DataRow[]?
                    if (_selectedData != null)
                    {
                        foreach (var row in _selectedData)
                        {
                            // LoadAssembly each row here
                            Console.WriteLine(row);
                        }
                    }
                }
            }

            //_selectedData = await showTargetTableModalAsyncMethod.Invoke();
            if (_selectedData != null)
            {
                //_showSetTargetTableModal = false;
                var modalId = $"{Title.ToLower()}-set-target-table-modal";
                await JSRuntime.InvokeVoidAsync("addClassToElement", modalId, "show");
            }

            await Task.CompletedTask;
        }

        private async Task UploadData()
        {
            if (_targetTables != null)
            {
                //await _sessionTableService.UploadTableListAsync(_targetTables);

                //TODO: sol: Optionally, show a success message or handle post-upload actions
            }

            await Task.CompletedTask;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (_tableGridType != null)
            {
                var seq = 0;

                // Open the component using the resolved type
                builder.OpenComponent(seq++, _tableGridType);
                builder.AddAttribute(seq++, "Title", Title);
                builder.AddAttribute(seq++, "TableID", _tableName.ToLower());
                builder.AddAttribute(seq++, "Model", _modelInstance);
                builder.AddAttribute(seq++, "ModelVM", _modelVMInstance);
                builder.AddAttribute(seq++, "IModel", default(IModelExtendedProperties)); // Use default or provide an actual instance if needed

                // Add the items as an attribute to the TableGrid component
                builder.AddAttribute(seq++, "Items", _items);

                builder.AddAttribute(seq++, "DataSources", DataSources);
                // Handle ItemsChanged EventCallback
                var callbackMethod = typeof(EventCallbackFactory).GetMethod("Create", new[] { typeof(object), typeof(Action<>) });
                if (callbackMethod != null)
                {
                    var genericCallbackMethod = callbackMethod.MakeGenericMethod(typeof(IEnumerable<>).MakeGenericType(_modelVMType));
                    var callback = genericCallbackMethod.Invoke(null, new object[] { this, ItemsChanged });
                    builder.AddAttribute(seq++, "ItemsChanged", callback);
                }
                builder.AddAttribute(seq++, "ColumnDefinitions", _columnDefinitions);
                builder.AddAttribute(seq++, "AllowCellRangeSelection", AllowCellRangeSelection);
                
                // Capture the component reference
                builder.AddComponentReferenceCapture(seq++, inst =>
                {
                    _tableGridComponentReference = inst;
                });

                builder.CloseComponent(); // Closing TableGrid

                // Icon for Set Target Table Modal
                builder.OpenComponent<Icon>(seq++);
                builder.AddAttribute(seq++, "Name", IconName.Table);
                builder.AddAttribute(seq++, "Class", "text-success icon-button mb-2 cursor-pointer");
                builder.AddAttribute(seq++, "title", "Step 1. Set Target Table");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, ShowSetTargetTableModalAsync));
                builder.CloseComponent();

                // Target tables grid rendering
                //if (_targetTables != null)
                //{
                //    foreach (var targetTable in _targetTables)
                //    {
                //        if (targetTable != null && !string.IsNullOrEmpty(targetTable.DT))
                //        {
                //            var dt = targetTable.DT.DeserializeAsync<DataTable>().Result;
                //            builder.OpenComponent<TableGrid>(seq++);
                //            builder.AddAttribute(seq++, "DataTable", dt);
                //            builder.CloseComponent();
                //        }
                //    }
                //}

                // Icon for Upload Data
                builder.OpenComponent<Icon>(seq++);
                builder.AddAttribute(seq++, "Name", IconName.CloudUpload);
                builder.AddAttribute(seq++, "Class", "cursor-pointer");
                builder.AddAttribute(seq++, "title", "Upload to existing AccSol tables");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, UploadData));
                builder.CloseComponent();

                builder.OpenComponent<SetTargetTableModal>(seq++);
                builder.AddAttribute(seq++, "Title", Title);
                builder.AddAttribute(seq++, "OnClose", EventCallback.Factory.Create(this, CloseSetTargetTableModalAsync));
                builder.AddAttribute(seq++, "OnSave", EventCallback.Factory.Create<List<TargetTable>>(this, SaveToTargetTableAsync));
                builder.AddAttribute(seq++, "SelectedData", _selectedData);
                builder.AddAttribute(seq++, "OnSelectedDataComb", EventCallback.Factory.Create<DataRow[]>(this, HandleSelectedDataComb));
                builder.AddAttribute(seq++, "TableList", TableList);
                builder.CloseComponent();

            }

        }
         
    }
}
