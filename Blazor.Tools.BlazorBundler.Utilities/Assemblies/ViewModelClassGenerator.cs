/*====================================================================================================
    Class Name  : ViewModelClassGenerator
    Created By  : Solomio S. Sisante
    Created On  : September 2, 2024
    Purpose     : To manage the construction of a view model class.
  ====================================================================================================*/
using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
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
        private Type? _baseClassType;
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
        private byte[] _assemblyBytes;

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
        private PropertyInfo[] _baseClassProperties;
        private List<string> _assemblyLocations;

        public DisposableAssembly? DisposableAssembly
        {
            get { return _disposableAssembly; }
            set { _disposableAssembly = value; }
        }

        public ViewModelClassGenerator(string nameSpace, Type? baseClassType)
        {
            _sb = new StringBuilder();
            _nameSpace = nameSpace;
            _baseClassName = default!;
            _vmClassName = default!;
            _iModelExtendedPropertiesProperties = null!;
            _baseClassType = baseClassType;
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

        public void AddReferences(List<string> assemblyLocations)
        { 
            _assemblyLocations = assemblyLocations;
        }

        public byte[] EmitAssemblyToMemorySave(string assemblyName, string version, string dllPath, params string[] sourceCodes)
        {

            foreach (var sourceCode in sourceCodes)
            {
                AppLogger.WriteInfo(sourceCode);
            }

            var classGenerator = new ClassGenerator(assemblyName, version);
            var systemPrivateCoreLibLocation = typeof(object).Assembly.Location;
            var systemLocation = Path.Combine(Path.GetDirectoryName(systemPrivateCoreLibLocation)!, "System.dll");
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
            //classGenerator.AddReference(baseClassTypeLocation);
            classGenerator.AddReference(typeof(IValidatableObject).Assembly.Location); // System.ComponentModel.DataAnnotations.dll
            classGenerator.AddReference(typeof(ICloneable<>).Assembly.Location); // Blazor.Tools.BlazorBundler.Interfaces, same with ICloneable, IViewModel and IContextProvider
            classGenerator.AddReference(typeof(ContextProvider).Assembly.Location); // Blazor.Tools.BlazorBundler.Entities
            classGenerator.AddReference(typeof(ReflectionExtensions).Assembly.Location); // Blazor.Tools.BlazorBundler.Extensions

            if (_assemblyLocations != null)
            {
                foreach (string assemblyLocation in _assemblyLocations)
                {
                    classGenerator.AddReference(assemblyLocation); 
                }
            }

            // Create the type from the provided class code
            _classType = classGenerator.CreateType(_nameSpace!, _vmClassName!, sourceCodes);
            _assemblyBytes = classGenerator.AssemblyBytes;
            return _assemblyBytes;
        }

        public void LoadAssembly()
        {
            using (var disposableAssembly = DisposableAssembly.LoadAssembly(_assemblyBytes!))
            {
                _disposableAssembly = disposableAssembly;
            }
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
using Blazor.Tools.BlazorBundler.Extensions;
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
            string[] interfaces = { "IValidatableObject", $"ICloneable<{_vmClassName}>", $"IViewModel<IBase, IModelExtendedProperties>" };

            // Combine baseClass and interfaces with a comma if both are present
            string inheritance = string.Join(", ", new[] { "IBase" }.Concat(interfaces).Where(s => !string.IsNullOrEmpty(s)));

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
            
            if (_baseClassType != null)
            {
                _baseClassProperties = _baseClassType?.GetProperties()!;
                foreach (var property in _baseClassProperties)
                {
                    string propertyName = property.Name;
                    string aliasTypeName = property.PropertyType.ToAliasType();
                    string fieldName = propertyName == "ID" ? "_id" : $"_{propertyName.ToCamelCase()}";
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

            _iModelExtendedPropertiesProperties = typeof(IModelExtendedProperties).GetProperties();
            foreach (var property in _iModelExtendedPropertiesProperties)
            {
                string propertyName = property.Name;
                string aliasTypeName = property.PropertyType.ToAliasType();
                string fieldName = propertyName == "ID" ? "_id" : $"_{propertyName.ToCamelCase()}";
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
                string fieldName = propertyName == "ID" ? "_id" : $"_{propertyName.ToCamelCase()}";
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
                public EmployeeVM(IContextProvider contextProvider, IBase model)
                {
                    _contextProvider = contextProvider;
                    _id = model.ID;
                    _firstName = model.GetPropertyValue<string>("FirstName");
                    _middleName = model.GetPropertyValue<string>("MiddleName");
                    _lastName = model.GetPropertyValue<string>("LastName");
                    _dateOfBirth = model.GetPropertyValue<DateOnly>("DateOfBirth");
                    _countryID = model.GetPropertyValue<int>("CountryID");
                }      
             */

            _sb?.AppendLine($"\t\tpublic {_vmClassName}(IContextProvider contextProvider, {_baseClassName} model)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine("\t\t\t_contextProvider = contextProvider;");
            _sb?.AppendLine();

            _sb?.AppendLine($"\t\t\t_id = model.ID;");

            foreach (var property in _baseClassProperties)
            {
                string propertyName = property.Name;
                string fieldName = propertyName == "ID" ? "_id" : $"_{propertyName.ToCamelCase()}";
                Type fieldType = property.PropertyType;

                if (propertyName != "ID")
                {
                    _sb?.AppendLine($"\t\t\t{fieldName} = model.GetPropertyValue<{fieldType.Name}>(\"{propertyName}\");");
                }
            }

            _sb?.AppendLine("\t\t}");
            _sb?.AppendLine();


            /*
                public EmployeeVM(IContextProvider contextProvider, EmployeeVM modelVM)
                {
                    _contextProvider = contextProvider;
                    _isEditMode = modelVM.IsEditMode;
                    _isVisible = modelVM.IsVisible;
                    _isFirstCellClicked = modelVM.IsFirstCellClicked;
                    _startCell = modelVM.StartCell;
                    _endCell = modelVM.EndCell;
                    _rowID = modelVM.RowID;
                    _id = modelVM.ID;
                    _firstName = modelVM.FirstName;
                    _middleName = modelVM.MiddleName;
                    _lastName = modelVM.LastName;
                    _dateOfBirth = modelVM.DateOfBirth;
                    _countryID = modelVM.CountryID;
                }         
             */

            _sb?.AppendLine($"\t\tpublic {_vmClassName}(IContextProvider contextProvider, {_vmClassName} modelVM)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine("\t\t\t_contextProvider = contextProvider;");
            _sb?.AppendLine();

            foreach (var property in _baseClassProperties)
            {
                string propertyName = property.Name;
                string fieldName = propertyName == "ID" ? "_id" : $"_{propertyName.ToCamelCase()}";

                _sb?.AppendLine($"\t\t\t{fieldName} = modelVM.{propertyName};");
            }

            foreach (var property in _iModelExtendedPropertiesProperties)
            {
                string propertyName = property.Name;
                string fieldName = propertyName == "ID" ? "_id" : $"_{propertyName.ToCamelCase()}";

                _sb?.AppendLine($"\t\t\t{fieldName} = modelVM.{propertyName};");
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
                string propertyName = property.Name;

                _sb?.AppendLine($"\t\t\t\t{propertyName} = this.{propertyName},");

            }

            foreach (var property in _baseClassProperties)
            {
                string propertyName = property.Name;

                _sb?.AppendLine($"\t\t\t\t{propertyName} = this.{propertyName},");

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
            _sb?.AppendLine($"\t\t\t\tvar foundItem = {listVarName}.FirstOrDefault(p => p.ID != currentItemId);");
            _sb?.AppendLine($"\t\t\t\talreadyExists = foundItem != null;");
            _sb?.AppendLine("\t\t\t}");
            _sb?.AppendLine();
            _sb?.AppendLine("\t\t\treturn alreadyExists;");
            _sb?.AppendLine("\t\t}");
            _sb?.AppendLine();
        }

        /*
            public async Task<IViewModel<IBase, IModelExtendedProperties>> FromModel(IBase model)
            {
                try
                {
                    if (model != null)
                    {
                        await Task.Run(() =>
                        {
                            _id = model.GetPropertyValue<int>("ID")!;
                            _firstName = model.GetPropertyValue<string>("FirstName");
                            _middleName = model.GetPropertyValue<string>("MiddleName");
                            _lastName = model.GetPropertyValue<string>("LastName");
                            _dateOfBirth = model.GetPropertyValue<DateOnly>("DateOfBirth")!;
                            _countryID = model.GetPropertyValue<int>("CountryID")!;
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
            _sb?.AppendLine($"\t\tpublic async Task<IViewModel<IBase, IModelExtendedProperties>> FromModel(IBase model)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine("\t\t\ttry");
            _sb?.AppendLine("\t\t\t{");
            _sb?.AppendLine("\t\t\t\tif (model != null)");
            _sb?.AppendLine("\t\t\t\t{");
            _sb?.AppendLine($"\t\t\t\t\tawait Task.Run(() =>");
            _sb?.AppendLine("\t\t\t\t\t{");

            _sb?.AppendLine($"\t\t\t\t\t\t_id = model.ID;");

            foreach (var property in _baseClassProperties)
            {
                string propertyName = property.Name;
                string fieldName = propertyName == "ID" ? "_id" : $"_{propertyName.ToCamelCase()}";
                Type fieldType = property.PropertyType;

                if (propertyName != "ID")
                {
                    _sb?.AppendLine($"\t\t\t\t\t\t{fieldName} = model.GetPropertyValue<{fieldType.Name}>(\"{propertyName}\");");
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
            public IBase ToNewModel()
            {
                var type = typeof(IBase);
                var typeInstance = (IBase)Activator.CreateInstance(type)!;

                typeInstance.SetValue("ID", this.ID);
                typeInstance.SetValue("FirstName", this.FirstName);
                typeInstance.SetValue("MiddleName", this.MiddleName);
                typeInstance.SetValue("LastName", this.LastName);
                typeInstance.SetValue("DateOfBirth", this.DateOfBirth);
                typeInstance.SetValue("CountryID", this.CountryID);

                return typeInstance;
            }
         */
        private void AddToNewModelMethod()
        {

            _sb?.AppendLine($"\t\tpublic IBase ToNewModel()");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine($"\t\t\tvar type = typeof(IBase);");
            _sb?.AppendLine($"\t\t\tvar typeInstance = (IBase)Activator.CreateInstance(type)!;");
            _sb?.AppendLine("\t\t\t");

            foreach (var property in _baseClassProperties)
            {
                string propertyName = property.Name;

                if (propertyName != "ID")
                {
                    _sb?.AppendLine($"\t\t\ttypeInstance.SetValue(\"{propertyName}\", this.{propertyName});");
                }
            }

            _sb?.AppendLine();
            _sb?.AppendLine("\t\t\t return typeInstance;");

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
                string propertyName = property.Name;
                Type fieldType = property.PropertyType;

                _sb?.AppendLine($"\t\t\t\t{propertyName}  = this.{propertyName},");

            }

            foreach (var property in _baseClassProperties)
            {
                string propertyName = property.Name;
                Type fieldType = property.PropertyType;

                _sb?.AppendLine($"\t\t\t\t{propertyName}  = this.{propertyName},");

            }

            _sb?.AppendLine("\t\t\t};");

            _sb?.AppendLine("\t\t}");

            _sb?.AppendLine();
        }

        /*
            public async Task<IViewModel<IBase, IModelExtendedProperties>> SetEditMode(bool isEditMode)
            {
                IsEditMode = isEditMode;
                await Task.CompletedTask;
                return this;
            }         
         */
        private void AddSetEditModeMethod()
        {
            string listVarName = $"_{_baseClassName?.Pluralize().ToLower()}";
            _sb?.AppendLine($"\t\tpublic async Task<IViewModel<IBase, IModelExtendedProperties>> SetEditMode(bool isEditMode)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine("\t\t\tIsEditMode = isEditMode;");
            _sb?.AppendLine("\t\t\tawait Task.CompletedTask;");
            _sb?.AppendLine("\t\t\treturn this;");
            _sb?.AppendLine("\t\t}");

            _sb?.AppendLine();
        }

        /*
            public async Task<IViewModel<IBase, IModelExtendedProperties>> SaveModelVM()
            {
                IsEditMode = false;
                await Task.CompletedTask;
                return this;
            }         
         */
        private void AddSaveModelVMMethod()
        {
            string listVarName = $"_{_baseClassName?.Pluralize().ToLower()}";
            _sb?.AppendLine($"\t\tpublic async Task<IViewModel<IBase, IModelExtendedProperties>> SaveModelVM()");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine("\t\t\tIsEditMode = false;");
            _sb?.AppendLine("\t\t\tawait Task.CompletedTask;");
            _sb?.AppendLine("\t\t\treturn this;");
            _sb?.AppendLine("\t\t}");

            _sb?.AppendLine();
        }

        /*
            public async Task<IViewModel<IBase, IModelExtendedProperties>> SaveModelVMToNewModelVM()
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
            _sb?.AppendLine($"\t\tpublic async Task<IViewModel<IBase, IModelExtendedProperties>> SaveModelVMToNewModelVM()");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine($"\t\t\tvar newModel = new {_vmClassName}(_contextProvider)");
            _sb?.AppendLine("\t\t\t{");

            foreach (var property in _iModelExtendedPropertiesProperties)
            {
                string propertyName = property.Name;

                _sb?.AppendLine($"\t\t\t\t{propertyName}  = this.{propertyName},");

            }
            foreach (var property in _baseClassProperties)
            {
                string propertyName = property.Name;

                _sb?.AppendLine($"\t\t\t\t{propertyName}  = this.{propertyName},");

            }

            _sb?.AppendLine("\t\t\t};");
            _sb?.AppendLine();
            _sb?.AppendLine("\t\t\tawait Task.CompletedTask;");
            _sb?.AppendLine("\t\t\treturn newModel;");
            _sb?.AppendLine("\t\t}");

            _sb?.AppendLine();
        }

        /*
            public async Task<IEnumerable<IViewModel<IBase, IModelExtendedProperties>>> AddItemToList(IEnumerable<IViewModel<IBase, IModelExtendedProperties>> modelVMList)
            {
                var list = modelVMList.ToList();

                int listCount = list.Count();
                RowID = listCount + 1;
                if (listCount > 0)
                {
                    var firstItem = list.First();
                    IsFirstCellClicked = firstItem.IsFirstCellClicked;
                    StartCell = firstItem.StartCell;
                    EndCell = firstItem.EndCell;
                }

                list.Add(this);

                await Task.CompletedTask;

                return list;
            }         
         */
        private void AddAddItemToListMethod()
        {
            string listVarName = $"_{_baseClassName?.Pluralize().ToLower()}";
            _sb?.AppendLine($"\t\tpublic async Task<IEnumerable<IViewModel<IBase, IModelExtendedProperties>>> AddItemToList(IEnumerable<IViewModel<IBase, IModelExtendedProperties>> modelVMList)");
            _sb?.AppendLine("\t\t{");
            _sb?.AppendLine($"\t\t\tvar list = modelVMList.ToList();");
            _sb?.AppendLine();
            _sb?.AppendLine("\t\t\tint listCount = list.Count();");
            _sb?.AppendLine("\t\t\tRowID = listCount + 1;");
            _sb?.AppendLine("\t\t\tif (listCount > 0)");
            _sb?.AppendLine("\t\t\t{");
            _sb?.AppendLine("\t\t\t\tvar firstItem = list.First();");
            _sb?.AppendLine("\t\t\t\tIsFirstCellClicked = firstItem.IsFirstCellClicked;");
            _sb?.AppendLine("\t\t\t\tStartCell = firstItem.StartCell;");
            _sb?.AppendLine("\t\t\t\tEndCell = firstItem.EndCell;");
            _sb?.AppendLine("\t\t\t}");
            _sb?.AppendLine();
            _sb?.AppendLine("\t\t\tlist.Add(this);");
            _sb?.AppendLine("\t\t\tawait Task.CompletedTask;");
            _sb?.AppendLine("\t\t\treturn list;");

            _sb?.AppendLine("\t\t}");

            _sb?.AppendLine();
        }

        /*
            public async Task<IEnumerable<IViewModel<IBase, IModelExtendedProperties>>> UpdateList(IEnumerable<IViewModel<IBase, IModelExtendedProperties>> modelVMList, bool isAdding)
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
                    var modelVM = foundModel == null ? default : (EmployeeVM)foundModel;

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
            _sb?.AppendLine($"\t\tpublic async Task<IEnumerable<IViewModel<IBase, IModelExtendedProperties>>> UpdateList(IEnumerable<IViewModel<IBase, IModelExtendedProperties>> modelVMList, bool isAdding)");
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
                string propertyName = property.Name;
                Type fieldType = property.PropertyType;

                _sb?.AppendLine($"\t\t\t\t\tmodelVM.{propertyName}  = {propertyName};");

            }

            foreach (var property in _baseClassProperties)
            {
                string propertyName = property.Name;
                Type fieldType = property.PropertyType;

                _sb?.AppendLine($"\t\t\t\t\tmodelVM.{propertyName}  = {propertyName};");

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
            public async Task<IEnumerable<IViewModel<IBase, IModelExtendedProperties>>> DeleteItemFromList(IEnumerable<IViewModel<IBase, IModelExtendedProperties>> modelVMList)
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
            _sb?.AppendLine($"\t\tpublic async Task<IEnumerable<IViewModel<IBase, IModelExtendedProperties>>> DeleteItemFromList(IEnumerable<IViewModel<IBase, IModelExtendedProperties>> modelVMList)");
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
