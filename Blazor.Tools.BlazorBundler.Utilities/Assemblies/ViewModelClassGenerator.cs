/*====================================================================================================
    Class Name  : ViewModelClassGenerator
    Created By  : Solomio S. Sisante
    Created On  : September 2, 2024
    Purpose     : To manage the construction of a view model class.
  ====================================================================================================*/
using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using Humanizer;
using Microsoft.CodeAnalysis.CSharp;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Text;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class ViewModelClassGenerator : IDisposable
    {
        private DataTable? _sourceTable;
        private string? _baseClassName;
        private string? _vmClassName;
        private DataColumnCollection? _columns;
        private PropertyInfo[] _iModelExtendedPropertiesProperties;
        private bool _disposed;

        private StringBuilder? _sb;

        public StringBuilder? SB
        {
            get { return _sb; }
            set { _sb = value; }
        }

        private CSharpCompilation? _compilation;

        public CSharpCompilation? Compilation
        {
            get { return _compilation; }
        }

        private Type? _classType;

        public Type? ClassType
        {
            get { return _classType; }
        }

        private string _nameSpace;

        public string NameSpace
        {
            get { return _nameSpace; }
            set { _nameSpace = value; }
        }

        private DisposableAssembly? _disposableAssembly;

        public DisposableAssembly? DisposableAssembly
        {
            get { return _disposableAssembly; }
            set { _disposableAssembly = value; }
        }

        public ViewModelClassGenerator(string nameSpace)
        {
            _sb = new StringBuilder();
            _nameSpace = nameSpace;
            _baseClassName = default!;
            _vmClassName = default!;
            _iModelExtendedPropertiesProperties = null!;
        }

        public void CreateFromDataTable(DataTable sourceTable)
        {
            _sourceTable = sourceTable;
            _baseClassName = sourceTable.TableName;
            _vmClassName = _baseClassName + "VM";
            _columns = _sourceTable?.Columns;

            AddUsings();
            _sb?.AppendLine();
            AddNameSpace();

        }

        public override string ToString()
        {
            string stringValue = string.Empty;

            if (_sb != null)
            {
                stringValue = _sb.ToString();
            }

            return stringValue;
        }

        public void Save(string assemblyName, string version, string classCode, string nameSpace, string className, string dllPath, Type baseClassType, string baseClassCode, string baseClassTypeLocation)
        {
            Console.WriteLine("//Class Code: \r\n{0}", classCode);
            var classGenerator = new ClassGenerator(assemblyName, version);

            var systemPrivateCoreLibLocation = typeof(object).Assembly.Location;
            var systemLocation = Path.Combine(Path.GetDirectoryName(systemPrivateCoreLibLocation)!, "System.dll") ;
            var systemRuntimeLocation = Path.Combine(Path.GetDirectoryName(systemPrivateCoreLibLocation)!, "System.Runtime.dll");
            var systemCollectionsLocation = Path.Combine(Path.GetDirectoryName(systemPrivateCoreLibLocation)!, "System.Collections.dll");
            var systemConsoleLocation = typeof(System.Console).Assembly.Location;
            var systemLinqLocation = typeof(System.Linq.Enumerable).Assembly.Location;
            var systemThreadingTasksLocation = Path.Combine(Path.GetDirectoryName(systemPrivateCoreLibLocation)!, "System.Threading.Tasks.dll");

            // Add references to existing assemblies that contain types used in the dynamic class
            classGenerator.AddReference(systemPrivateCoreLibLocation);  // Object types
            classGenerator.AddReference(systemLocation);  // System.dll
            classGenerator.AddReference(systemRuntimeLocation);  // System.Runtime.dll
            classGenerator.AddReference(systemCollectionsLocation);  // System.Collections.dll
            classGenerator.AddReference(systemConsoleLocation);  // System.Console.dll
            classGenerator.AddReference(systemLinqLocation);  // System.Collections.dll
            classGenerator.AddReference(systemThreadingTasksLocation);  // System.Threading.Tasks.dll

            // Add references to assemblies containing other required types
            classGenerator.AddReference(baseClassTypeLocation);
            classGenerator.AddReference(typeof(IValidatableObject).Assembly.Location); // System.ComponentModel.DataAnnotations.dll
            classGenerator.AddReference(typeof(ICloneable<>).Assembly.Location); // Blazor.Tools.BlazorBundler.Interfaces, same with ICloneable, IViewModel and IContextProvider
            classGenerator.AddReference(typeof(ContextProvider).Assembly.Location); // Blazor.Tools.BlazorBundler.Entities

            // Add the class code as a module if provided
            classGenerator.AddModule(classCode, nameSpace); 
            //classGenerator.AddModule(baseClassCode, nameSpace);

            _compilation = classGenerator.Compilation;
            _classType = classGenerator.CreateType(classCode, nameSpace, className, baseClassCode);

            // Save the compiled assembly to the Temp folder
            classGenerator.SaveAssemblyToTempFolder(dllPath);

            using (var assembly = DisposableAssembly.LoadFile(dllPath)) // type created from memory stream does not have assembly location.
            {
                _disposableAssembly = assembly;
                _classType = assembly.GetType($"{nameSpace}.{className}"); // type created from loading an assembly file has an assembly location.
            }

            while (dllPath.IsFileInUse())
            {
                //vmDllPath.KillLockingProcesses();
                Thread.Sleep(1000);
            }
        }

        private void AddUsings()
        {
            string usingStatements = @"using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models;
using Blazor.Tools.BlazorBundler.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;";

            _sb?.AppendLine(usingStatements);
        }

        private void AddNameSpace()
        {
            _sb?.AppendLine($"namespace {_nameSpace}");
            _sb?.AppendLine("{");
            AddClass();
            _sb?.AppendLine("}");
        }

        private void AddClass()
        {
            string[] interfaces = { "IValidatableObject", $"ICloneable<{_vmClassName}>", $"IViewModel<{_baseClassName}, IModelExtendedProperties>" };

            // Combine baseClass and interfaces with a comma if both are present
            string inheritance = string.Join(", ", new[] { _baseClassName }.Concat(interfaces).Where(s => !string.IsNullOrEmpty(s)));

            string classDeclaration = $"\tpublic class {_vmClassName}{(string.IsNullOrEmpty(inheritance) ? "" : $": {inheritance}")}";

            _sb?.AppendLine(classDeclaration);
            _sb?.AppendLine("\t{");
            AddFieldsAndProperties();
            _sb?.AppendLine();

            AddConstructors();
            AddCloneMethod();
            AddSetListMethod();
            AddValidateMethod();
            AddAlreadyExistsMethod();
            AddFromModelMethod();
            AddToNewModelMethod();
            AddToNewIModelMethod();
            AddSetEditModeMethod();
            AddSaveModelVMMethod();
            AddSaveModelVMToNewModelVM();
            AddAddItemToListMethod();
            AddUpdateListMethod();
            AddDeleteItemFromListMethod();

            _sb?.AppendLine("\t}");
        }

        private void AddFieldsAndProperties()
        {
            string listVarName = $"_{_baseClassName?.Pluralize().ToLower()}";
            string items = $"private List<{_vmClassName}> {listVarName};";
            string contextProvider = "private readonly IContextProvider _contextProvider;";

            _sb?.AppendLine($"\t\t{items}");
            _sb?.AppendLine($"\t\t{contextProvider}");
            _sb?.AppendLine();

            _iModelExtendedPropertiesProperties = typeof(IModelExtendedProperties).GetProperties();
            foreach (var property in _iModelExtendedPropertiesProperties)
            {
                string propertyName = property.Name;
                string aliasTypeName = property.PropertyType.ToAliasType();
                string fieldName = $"_{propertyName.ToCamelCase()}";
                string fieldDeclaration = $"\t\tprivate {aliasTypeName} {fieldName};";
                _sb?.AppendLine(fieldDeclaration);
                _sb?.AppendLine();

                _sb?.AppendLine($"\t\tpublic {aliasTypeName} {propertyName}");
                _sb?.AppendLine("\t\t{");
                _sb?.AppendLine("\t\t\tget");
                _sb?.AppendLine("\t\t\t{");
                _sb?.AppendLine($"\t\t\t\treturn {fieldName};");
                _sb?.AppendLine("\t\t\t}");
                _sb?.AppendLine("\t\t\tset");
                _sb?.AppendLine("\t\t\t{");
                _sb?.AppendLine($"\t\t\t\t{fieldName} = value;");
                _sb?.AppendLine("\t\t\t}");
                _sb?.AppendLine("\t\t}");
                _sb?.AppendLine();
            }
        }

        private void AddConstructors()
        {
            /*
                public EmployeeVM()
                {
                    _employees = new List<EmployeeVM>();
                    _contextProvider = new ContextProvider();
                    _rowID = 0;
                    _isEditMode = false;
                    _isVisible = false;
                    _startCell = 0;
                    _endCell = 0;
                    _isFirstCellClicked = false;
                }
             */
            _sb?.AppendLine($"\t\tpublic {_vmClassName}()");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine($"\t\t\t_{_baseClassName?.ToLower().Pluralize()} = new List<{_vmClassName}>();");
            _sb?.AppendLine($"\t\t\t_contextProvider = new ContextProvider();");
            foreach (var property in _iModelExtendedPropertiesProperties)
            {
                string propertyName = property.Name;
                string aliasTypeName = property.PropertyType.ToAliasType();
                string fieldName = $"_{propertyName.ToCamelCase()}";
                Type fieldType = property.PropertyType;

                // Use GenerateDefaultValueAsString() to handle default value as a string
                string fieldValueAsString = fieldType.GenerateDefaultValueAsString();

                // Handle special cases for non-string types (e.g., include quotation marks for strings)
                if (fieldType == typeof(string))
                {
                    _sb?.AppendLine($"\t\t\t{fieldName} = \"{fieldValueAsString}\";");
                }
                else
                {
                    _sb?.AppendLine($"\t\t\t{fieldName} = {fieldValueAsString};");
                }
            }

            _sb?.AppendLine("\t\t}");
            _sb?.AppendLine();

            /*
                public EmployeeVM(IContextProvider contextProvider)
                {
                    _contextProvider = contextProvider;
                }             
             */
            _sb?.AppendLine($"\t\tpublic {_vmClassName}(IContextProvider contextProvider)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine("\t\t\t_contextProvider = contextProvider;");
            _sb?.AppendLine("\t\t}");
            _sb?.AppendLine();

            /*
                public EmployeeVM(IContextProvider contextProvider, Employee model)
                {
                    _contextProvider = contextProvider;
                    ID = model.ID;
                    FirstName = model.FirstName;
                    MiddleName = model.MiddleName;
                    LastName = model.LastName;
                    DateOfBirth = model.DateOfBirth;
                    CountryID = model.CountryID;
                }             
             */
            _sb?.AppendLine($"\t\tpublic {_vmClassName}(IContextProvider contextProvider, {_baseClassName} model)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine("\t\t\t_contextProvider = contextProvider;");
            _sb?.AppendLine();
            if (_columns != null)
            {
                foreach (DataColumn column in _columns)
                {
                    string fieldName = column.ColumnName;
                    string aliasTypeName = column.DataType.ToAliasType();
                    Type fieldType = column.DataType;
                    object fieldValue = fieldType.GenerateDefaultValue();

                    _sb?.AppendLine($"\t\t\t{fieldName} = model.{fieldName};");

                }
            }

            _sb?.AppendLine("\t\t}");
            _sb?.AppendLine();

            /*
                public EmployeeVM(IContextProvider contextProvider, EmployeeVM modelVM)
                {
                    _contextProvider = contextProvider;
                    IsEditMode = modelVM.IsEditMode;
                    IsVisible = modelVM.IsVisible;
                    IsFirstCellClicked = modelVM.IsFirstCellClicked;
                    StartCell = modelVM.StartCell;
                    EndCell = modelVM.EndCell;
                    RowID = modelVM.RowID;
                    ID = modelVM.ID;
                    FirstName = modelVM.FirstName;
                    MiddleName = modelVM.MiddleName;
                    LastName = modelVM.LastName;
                    DateOfBirth = modelVM.DateOfBirth;
                    CountryID = modelVM.CountryID;
                }             
             */
            _sb?.AppendLine($"\t\tpublic {_vmClassName}(IContextProvider contextProvider, {_vmClassName} modelVM)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine("\t\t\t_contextProvider = contextProvider;");
            _sb?.AppendLine();

            foreach (var property in _iModelExtendedPropertiesProperties)
            {
                string fieldName = property.Name;
                Type fieldType = property.PropertyType;
                object fieldValue = fieldType.GenerateDefaultValue();

                _sb?.AppendLine($"\t\t\t{fieldName} = modelVM.{fieldName};");

            }

            if (_columns != null)
            {
                foreach (DataColumn column in _columns)
                {
                    string fieldName = column.ColumnName;
                    string aliasTypeName = column.DataType.ToAliasType();
                    Type fieldType = column.DataType;
                    object fieldValue = fieldType.GenerateDefaultValue();

                    _sb?.AppendLine($"\t\t\t{fieldName} = modelVM.{fieldName};");

                }
            }

            _sb?.AppendLine("\t\t}");
            _sb?.AppendLine();

        }

        /*
            public EmployeeVM Clone()
            {
                return new EmployeeVM(_contextProvider)
                {
                    IsEditMode = this.IsEditMode,
                    IsVisible = this.IsVisible,
                    IsFirstCellClicked = this.IsFirstCellClicked,
                    StartCell = this.StartCell,
                    EndCell = this.EndCell,
                    RowID = this.RowID,
                    ID = this.ID,
                    FirstName = this.FirstName,
                    MiddleName = this.MiddleName,
                    LastName = this.LastName,
                    DateOfBirth = this.DateOfBirth,
                    CountryID = this.CountryID
                };
            }         
         */
        private void AddCloneMethod()
        {

            _sb?.AppendLine($"\t\tpublic {_vmClassName} Clone()");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine($"\t\t\treturn new {_vmClassName}(_contextProvider)");
            _sb?.AppendLine("\t\t\t{");
            foreach (var property in _iModelExtendedPropertiesProperties)
            {
                string fieldName = property.Name;
                Type fieldType = property.PropertyType;
                object fieldValue = fieldType.GenerateDefaultValue();

                _sb?.AppendLine($"\t\t\t\t{fieldName} = {fieldName},");

            }

            if (_columns != null)
            {
                foreach (DataColumn column in _columns)
                {
                    string fieldName = column.ColumnName;
                    string aliasTypeName = column.DataType.ToAliasType();
                    Type fieldType = column.DataType;
                    object fieldValue = fieldType.GenerateDefaultValue();

                    _sb?.AppendLine($"\t\t\t\t{fieldName} = {fieldName},");

                }

            }

            _sb?.AppendLine("\t\t\t};");

            _sb?.AppendLine("\t\t}");

            _sb?.AppendLine();
        }

        /*
            public void SetList(List<EmployeeVM> items)
            {
                _employees = items;
            }         
         */
        private void AddSetListMethod()
        {
            string listVarName = $"_{_baseClassName?.Pluralize().ToLower()}";
            _sb?.AppendLine($"\t\tpublic void SetList(List<{_vmClassName}> items)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine($"\t\t\t{listVarName} = items;");
            _sb?.AppendLine("\t\t}");
            _sb?.AppendLine();
        }

        /*
            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                // Ensure _clientVMEntryList is set before calling Validate
                if (_employees == null)
                {
                    // Log or handle the situation where _clientVMEntryList is not set
                    yield break; // Exit the validation early
                }

                // Implement your custom validation logic here
                //if (!IsEditing && AlreadyExists(Name, ID)) // Check existence only in editing mode
                //{
                //    yield return new ValidationResult("Name already exists.", new[] { nameof(Name) });
                //}
            }
         */
        private void AddValidateMethod()
        {
            string listVarName = $"_{_baseClassName?.Pluralize().ToLower()}";
            _sb?.AppendLine($"\t\tpublic IEnumerable<ValidationResult> Validate(ValidationContext validationContext)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine($"\t\t\tif ({listVarName} == null)");
            _sb?.AppendLine("\t\t\t{");
            _sb?.AppendLine("\t\t\t\tyield break;");
            _sb?.AppendLine("\t\t\t}");
            _sb?.AppendLine("\t\t}");
            _sb?.AppendLine();
        }

        /*private bool AlreadyExists(string name, int currentItemId)
        {
            bool alreadyExists = false;

            if (name != null)
            {
                // Exclude the current item from the search
                var foundItem = _employees.FirstOrDefault(p => p.FirstName == name && p.ID != currentItemId);
                alreadyExists = foundItem != null;
            }

            return alreadyExists;
        }*/

        private void AddAlreadyExistsMethod()
        {
            string listVarName = $"_{_baseClassName?.Pluralize().ToLower()}";
            _sb?.AppendLine("\t\tpublic bool AlreadyExists(string name, int currentItemId)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine($"\t\t\tbool alreadyExists = false;");
            _sb?.AppendLine();
            _sb?.AppendLine("\t\t\tif (name != null)");
            _sb?.AppendLine("\t\t\t{");
            _sb?.AppendLine($"\t\t\t\tvar foundItem = {listVarName}.FirstOrDefault(p => p.FirstName == name && p.ID != currentItemId);");
            _sb?.AppendLine($"\t\t\t\talreadyExists = foundItem != null;");
            _sb?.AppendLine("\t\t\t}");
            _sb?.AppendLine();
            _sb?.AppendLine("\t\t\treturn alreadyExists;");
            _sb?.AppendLine("\t\t}");
            _sb?.AppendLine();
        }

        /*
        public async Task<IViewModel<Employee, IModelExtendedProperties>> FromModel(Employee model)
        {
            try
            {
                if (model != null)
                {
                    await Task.Run(() =>
                    {
                        ID = model.ID;
                        FirstName = model.FirstName;
                        MiddleName = model.MiddleName;
                        LastName = model.LastName;
                        DateOfBirth = model.DateOfBirth;
                        CountryID = model.CountryID;
                    });

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FromModel(Employee model, Dictionary<string, object> serviceList): {0}\r\n{1}", ex.Message, ex.StackTrace);
            }

            return this;
        }
         */

        private void AddFromModelMethod()
        {
            string listVarName = $"_{_baseClassName?.Pluralize().ToLower()}";
            _sb?.AppendLine($"\t\tpublic async Task<IViewModel<{_baseClassName}, IModelExtendedProperties>> FromModel({_baseClassName} model)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine("\t\t\ttry");
            _sb?.AppendLine("\t\t\t{");
            _sb?.AppendLine("\t\t\t\tif (model != null)");
            _sb?.AppendLine("\t\t\t\t{");
            _sb?.AppendLine($"\t\t\t\t\tawait Task.Run(() =>");
            _sb?.AppendLine("\t\t\t\t\t{");

            if (_columns != null)
            {
                foreach (DataColumn column in _columns)
                {
                    string fieldName = column.ColumnName;
                    string aliasTypeName = column.DataType.ToAliasType();
                    Type fieldType = column.DataType;
                    object fieldValue = fieldType.GenerateDefaultValue();

                    _sb?.AppendLine($"\t\t\t\t\t\t{fieldName} = model.{fieldName};");

                }
            }

            _sb?.AppendLine("\t\t\t\t\t});"); // await
            _sb?.AppendLine("\t\t\t\t}"); // if
            _sb?.AppendLine("\t\t\t}"); // try
            _sb?.AppendLine("\t\t\tcatch(Exception ex)");
            _sb?.AppendLine("\t\t\t{");
            _sb?.AppendLine($"\t\t\t\t Console.WriteLine(\"FromModel({_baseClassName} model, Dictionary<string, object> serviceList): {0}\\r\\n{1}\", ex.Message, ex.StackTrace);");
            _sb?.AppendLine("\t\t\t}");
            _sb?.AppendLine();
            _sb?.AppendLine("\t\t\treturn this;");
            _sb?.AppendLine("\t\t}");

            _sb?.AppendLine();
        }

        /*
        public Employee ToNewModel()
        {
            return new Employee
            {
                ID = this.ID,
                FirstName = this.FirstName,
                MiddleName = this.MiddleName,
                LastName = this.LastName,
                DateOfBirth = this.DateOfBirth,
                CountryID = this.CountryID
            };
        }
         */
        private void AddToNewModelMethod()
        {

            _sb?.AppendLine($"\t\tpublic {_baseClassName} ToNewModel()");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine($"\t\t\treturn new {_baseClassName}");
            _sb?.AppendLine("\t\t\t{");

            if (_columns != null)
            {

                foreach (DataColumn column in _columns)
                {
                    string fieldName = column.ColumnName;
                    string aliasTypeName = column.DataType.ToAliasType();
                    Type fieldType = column.DataType;
                    object fieldValue = fieldType.GenerateDefaultValue();

                    _sb?.AppendLine($"\t\t\t\t{fieldName} = this.{fieldName},");

                }
            }

            _sb?.AppendLine("\t\t\t};");

            _sb?.AppendLine("\t\t}");

            _sb?.AppendLine();
        }

        /*
            public IModelExtendedProperties ToNewIModel()
            {
                return new EmployeeVM(_contextProvider)
                {
                    IsEditMode = this.IsEditMode,
                    IsVisible = this.IsVisible,
                    IsFirstCellClicked = this.IsFirstCellClicked,
                    StartCell = this.StartCell,
                    EndCell = this.EndCell,
                    RowID = this.RowID,
                    ID = this.ID,
                    FirstName = this.FirstName,
                    MiddleName = this.MiddleName,
                    LastName = this.LastName,
                    DateOfBirth = this.DateOfBirth,
                    CountryID = this.CountryID
                };
            }         
         */
        private void AddToNewIModelMethod()
        {

            _sb?.AppendLine($"\t\tpublic IModelExtendedProperties ToNewIModel()");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine($"\t\t\treturn new {_vmClassName}(_contextProvider)");
            _sb?.AppendLine("\t\t\t{");

            foreach (var property in _iModelExtendedPropertiesProperties)
            {
                string fieldName = property.Name;
                Type fieldType = property.PropertyType;
                object fieldValue = fieldType.GenerateDefaultValue();

                _sb?.AppendLine($"\t\t\t\t{fieldName}  = this.{fieldName},");

            }

            if (_columns != null)
            {
                foreach (DataColumn column in _columns)
                {
                    string fieldName = column.ColumnName;
                    string aliasTypeName = column.DataType.ToAliasType();
                    Type fieldType = column.DataType;
                    object fieldValue = fieldType.GenerateDefaultValue();

                    _sb?.AppendLine($"\t\t\t\t{fieldName} = this.{fieldName},");

                }
            }

            _sb?.AppendLine("\t\t\t};");

            _sb?.AppendLine("\t\t}");

            _sb?.AppendLine();
        }

        /*
            public async Task<IViewModel<Employee, IModelExtendedProperties>> SetEditMode(bool isEditMode)
            {
                IsEditMode = isEditMode;
                await Task.CompletedTask;
                return this;
            }         
         */
        private void AddSetEditModeMethod()
        {
            string listVarName = $"_{_baseClassName?.Pluralize().ToLower()}";
            _sb?.AppendLine($"\t\tpublic async Task<IViewModel<{_baseClassName}, IModelExtendedProperties>> SetEditMode(bool isEditMode)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine("\t\t\tIsEditMode = isEditMode;");
            _sb?.AppendLine("\t\t\tawait Task.CompletedTask;");
            _sb?.AppendLine("\t\t\treturn this;");
            _sb?.AppendLine("\t\t}");

            _sb?.AppendLine();
        }

        /*
            public async Task<IViewModel<Employee, IModelExtendedProperties>> SaveModelVM()
            {
                IsEditMode = false;
                await Task.CompletedTask;
                return this;
            }         
         */
        private void AddSaveModelVMMethod()
        {
            string listVarName = $"_{_baseClassName?.Pluralize().ToLower()}";
            _sb?.AppendLine($"\t\tpublic async Task<IViewModel<{_baseClassName}, IModelExtendedProperties>> SaveModelVM()");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine("\t\t\tIsEditMode = false;");
            _sb?.AppendLine("\t\t\tawait Task.CompletedTask;");
            _sb?.AppendLine("\t\t\treturn this;");
            _sb?.AppendLine("\t\t}");

            _sb?.AppendLine();
        }

        /*
            public async Task<IViewModel<Employee, IModelExtendedProperties>> SaveModelVMToNewModelVM()
            {
                var newModel = new EmployeeVM(_contextProvider)
                {
                    IsEditMode = IsEditMode,
                    IsVisible = IsVisible,
                    IsFirstCellClicked = IsFirstCellClicked,
                    StartCell = StartCell,
                    EndCell = EndCell,
                    RowID = RowID,
                    ID = ID,
                    FirstName = FirstName,
                    MiddleName = MiddleName,
                    LastName = LastName,           
                    DateOfBirth = DateOfBirth,
                    CountryID = CountryID
                };

                await Task.CompletedTask;
                return newModel;
            }         
         */
        private void AddSaveModelVMToNewModelVM()
        {
            string listVarName = $"_{_baseClassName?.Pluralize().ToLower()}";
            _sb?.AppendLine($"\t\tpublic async Task<IViewModel<{_baseClassName}, IModelExtendedProperties>> SaveModelVMToNewModelVM()");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine($"\t\t\tvar newModel = new {_vmClassName}(_contextProvider)");
            _sb?.AppendLine("\t\t\t{");

            foreach (var property in _iModelExtendedPropertiesProperties)
            {
                string fieldName = property.Name;
                Type fieldType = property.PropertyType;
                object fieldValue = fieldType.GenerateDefaultValue();

                _sb?.AppendLine($"\t\t\t\t{fieldName}  = this.{fieldName},");

            }
            if (_columns != null)
            {
                foreach (DataColumn column in _columns)
                {
                    string fieldName = column.ColumnName;
                    string aliasTypeName = column.DataType.ToAliasType();
                    Type fieldType = column.DataType;
                    object fieldValue = fieldType.GenerateDefaultValue();

                    _sb?.AppendLine($"\t\t\t\t{fieldName} = this.{fieldName},");
                }
            }

            _sb?.AppendLine("\t\t\t};");
            _sb?.AppendLine();
            _sb?.AppendLine("\t\t\tawait Task.CompletedTask;");
            _sb?.AppendLine("\t\t\treturn newModel;");
            _sb?.AppendLine("\t\t}");

            _sb?.AppendLine();
        }

        /*
            public async Task<IEnumerable<IViewModel<Employee, IModelExtendedProperties>>> AddItemToList(IEnumerable<IViewModel<Employee, IModelExtendedProperties>> modelVMList)
            {
                var list = modelVMList.ToList();

                int listCount = list.Count();
                RowID = listCount + 1;
                if (listCount > 0)
                {
                    var firstItem = list.First();
                }

                list.Add(this);

                await Task.CompletedTask;

                return list;
            }         
         */
        private void AddAddItemToListMethod()
        {
            string listVarName = $"_{_baseClassName?.Pluralize().ToLower()}";
            _sb?.AppendLine($"\t\tpublic async Task<IEnumerable<IViewModel<{_baseClassName}, IModelExtendedProperties>>> AddItemToList(IEnumerable<IViewModel<{_baseClassName}, IModelExtendedProperties>> modelVMList)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine($"\t\t\tvar list = modelVMList.ToList();");
            _sb?.AppendLine();
            _sb?.AppendLine("\t\t\tint listCount = list.Count();");
            _sb?.AppendLine("\t\t\tRowID = listCount + 1;");
            _sb?.AppendLine("\t\t\tif (listCount > 0)");
            _sb?.AppendLine("\t\t\t{");
            _sb?.AppendLine("\t\t\t\tvar firstItem = list.First();");
            _sb?.AppendLine("\t\t\t}");
            _sb?.AppendLine();
            _sb?.AppendLine("\t\t\tlist.Add(this);");
            _sb?.AppendLine("\t\t\tawait Task.CompletedTask;");
            _sb?.AppendLine("\t\t\treturn list;");

            _sb?.AppendLine("\t\t}");

            _sb?.AppendLine();
        }

        /*
            public async Task<IEnumerable<IViewModel<Employee, IModelExtendedProperties>>> UpdateList(IEnumerable<IViewModel<Employee, IModelExtendedProperties>> modelVMList, bool isAdding)
            {

                if (isAdding)
                {
                    var list = modelVMList.ToList();
                    list.Remove(this);

                    modelVMList = list;
                }
                else
                {

                    var foundModel = modelVMList.FirstOrDefault(e => e.RowID == RowID);
                    var modelVM = foundModel == null? default : (EmployeeVM)foundModel;

                    if (modelVM != null)
                    {
                        modelVM.IsEditMode = IsEditMode;
                        modelVM.IsVisible = IsVisible;
                        modelVM.IsFirstCellClicked = IsFirstCellClicked;
                        modelVM.StartCell = StartCell;
                        modelVM.EndCell = EndCell;
                        modelVM.ID = ID;
                        modelVM.FirstName = FirstName;
                        modelVM.MiddleName = MiddleName;
                        modelVM.LastName = LastName;
                        modelVM.DateOfBirth = DateOfBirth;
                        modelVM.CountryID = CountryID;
                    }
                }


                await Task.CompletedTask;

                return modelVMList;
            }         
         */

        private void AddUpdateListMethod()
        {
            string listVarName = $"_{_baseClassName?.Pluralize().ToLower()}";
            _sb?.AppendLine($"\t\tpublic async Task<IEnumerable<IViewModel<{_baseClassName}, IModelExtendedProperties>>> UpdateList(IEnumerable<IViewModel<{_baseClassName}, IModelExtendedProperties>> modelVMList, bool isAdding)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine($"\t\t\tif (isAdding)");
            _sb?.AppendLine("\t\t\t{");
            _sb?.AppendLine("\t\t\t\tvar list = modelVMList.ToList();");
            _sb?.AppendLine("\t\t\t\tlist.Remove(this);");
            _sb?.AppendLine("\t\t\t\tmodelVMList = list;");
            _sb?.AppendLine("\t\t\t}");
            _sb?.AppendLine("\t\t\telse");
            _sb?.AppendLine("\t\t\t{");
            _sb?.AppendLine("\t\t\t\tvar foundModel = modelVMList.FirstOrDefault(e => e.RowID == RowID);");
            _sb?.AppendLine($"\t\t\t\tvar modelVM = foundModel == null? default : ({_vmClassName})foundModel;");
            _sb?.AppendLine();
            _sb?.AppendLine("\t\t\t\tif (modelVM != null)");
            _sb?.AppendLine("\t\t\t\t{");

            foreach (var property in _iModelExtendedPropertiesProperties)
            {
                string fieldName = property.Name;
                Type fieldType = property.PropertyType;
                object fieldValue = fieldType.GenerateDefaultValue();

                _sb?.AppendLine($"\t\t\t\t\tmodelVM.{fieldName}  = {fieldName};");

            }

            if (_columns != null)
            {
                foreach (DataColumn column in _columns)
                {
                    string fieldName = column.ColumnName;
                    string aliasTypeName = column.DataType.ToAliasType();
                    Type fieldType = column.DataType;
                    object fieldValue = fieldType.GenerateDefaultValue();

                    _sb?.AppendLine($"\t\t\t\t\tmodelVM.{fieldName} = {fieldName};");

                }
            }

            _sb?.AppendLine("\t\t\t\t}");
            _sb?.AppendLine("\t\t\t}");
            _sb?.AppendLine();

            _sb?.AppendLine("\t\t\t await Task.CompletedTask;");
            _sb?.AppendLine();
            _sb?.AppendLine("\t\t\t return modelVMList;");

            _sb?.AppendLine("\t\t}");

            _sb?.AppendLine();
        }

        /*
            public async Task<IEnumerable<IViewModel<Employee, IModelExtendedProperties>>> DeleteItemFromList(IEnumerable<IViewModel<Employee, IModelExtendedProperties>> modelVMList)
            {
                var list = modelVMList.ToList();

                var isDeleted = list.Remove(this);

                //TODO: sol: Add logic here for deleted and not deleted conditions
                if (isDeleted) { }
                else { }

                await Task.CompletedTask;

                return list;
            }         
         */
        private void AddDeleteItemFromListMethod()
        {
            string listVarName = $"_{_baseClassName?.Pluralize().ToLower()}";
            _sb?.AppendLine($"\t\tpublic async Task<IEnumerable<IViewModel<{_baseClassName}, IModelExtendedProperties>>> DeleteItemFromList(IEnumerable<IViewModel<{_baseClassName}, IModelExtendedProperties>> modelVMList)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine($"\t\t\tvar list = modelVMList.ToList();");
            _sb?.AppendLine();
            _sb?.AppendLine("\t\t\tvar isDeleted = list.Remove(this);");
            _sb?.AppendLine("\t\t\tawait Task.CompletedTask;");
            _sb?.AppendLine("\t\t\treturn list;");

            _sb?.AppendLine("\t\t}");

            _sb?.AppendLine();
        }

        // Implement IDisposable to clean up resources
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected method for cleanup logic
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Perform any necessary cleanup here
                    _sourceTable = null;
                    _baseClassName = null;
                    _vmClassName = null;
                    _columns = null;
                    _sb = null;
                }

                _disposed = true;
            }
        }

        // Destructor to ensure resources are cleaned up
        ~ViewModelClassGenerator()
        {
            Dispose(false);
        }
    }
}
