/*====================================================================================================
    Class Name  : CreateDLLFromDataTable
    Created By  : Solomio S. Sisante
    Created On  : September 10, 2024
    Purpose     : To provide a sample POC prototype class for creating dynamic classes dll file using
                  System.Reflection.Emit PersistedAssemblyBuilder.
  ====================================================================================================*/
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class CreateDLLFromDataTable : ICreateDLLFromDataTable
    {
        
        private string _dllPath = string.Empty;
        private string _employeeNameSpace;
        private string _employeeTypeName;
        private string _iEmployeeNameSpace;
        private string _iEmployeeTypeName;
        private string _contextAssemblyName;
        private string _employeeFullyQualifiedName;
        private string _iEmployeeFullyQualifiedName;

        public string ContextAssemblyName 
        {
            get{ return _contextAssemblyName; }
        }
        
        public string DLLPath 
        {
            get{ return _dllPath; }
        }

        public CreateDLLFromDataTable(string? employeeNameSpace = null, string? employeeTypeName = null, string? iEmployeeNameSpace = null, string? iEmployeeTypeName = null)
        {
            _employeeNameSpace = employeeNameSpace ?? "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models";    
            _employeeTypeName = employeeTypeName ?? "Employee";    
            _iEmployeeNameSpace = iEmployeeNameSpace ?? "Blazor.Tools.BlazorBundler.Interfaces";    
            _iEmployeeTypeName = iEmployeeTypeName ?? "IEmployee";
            _employeeFullyQualifiedName = $"{_employeeNameSpace}.{_employeeTypeName}";
            _iEmployeeFullyQualifiedName = $"{_iEmployeeNameSpace}.{_iEmployeeTypeName}";

            var lastIndex = _employeeNameSpace.LastIndexOf('.');
            _contextAssemblyName = _employeeNameSpace[..lastIndex];
            _dllPath = Path.Combine(Path.GetTempPath(), _contextAssemblyName + ".dll"); // use the parent assembly name that envelopes class and interface namespaces
        }

        public void BuildAndSaveAssembly(DataTable dataTable)
        {
            try 
            {
                var assemblyName = new AssemblyName(_contextAssemblyName); // use the parent assembly name
                assemblyName.Version = new Version("1.0.0.0");
                var persistedAssemblyBuilder = new PersistedAssemblyBuilder(assemblyName, typeof(object).Assembly);
                var moduleBuilder = persistedAssemblyBuilder.DefineDynamicModule(_employeeNameSpace);
                
                // Define the IEmployee interface
                var interfaceBuilder = moduleBuilder.DefineType(_iEmployeeFullyQualifiedName, TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

                foreach (DataColumn column in dataTable.Columns)
                {
                    // Define the getter method
                    interfaceBuilder.DefineMethod(
                        $"get_{column.ColumnName}",
                        MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                        column.DataType,
                        Type.EmptyTypes
                    );

                    // Define the setter method
                    interfaceBuilder.DefineMethod(
                        $"set_{column.ColumnName}",
                        MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                        null,
                        new[] { column.DataType }
                    );
                }

                var iEmployee = interfaceBuilder.CreateType();

                // Define the Employee class that implements IEmployee
                var classBuilder = moduleBuilder.DefineType(_employeeFullyQualifiedName, TypeAttributes.Public | TypeAttributes.Class, typeof(object), new[] { iEmployee });

                foreach (DataColumn column in dataTable.Columns)
                {
                    // Define the field
                    var fieldBuilder = classBuilder.DefineField(column.ColumnName, column.DataType, FieldAttributes.Private);

                    // Define the property
                    var propertyBuilder = classBuilder.DefineProperty(column.ColumnName, PropertyAttributes.HasDefault, column.DataType, null);

                    // Define the getter method
                    var getterBuilder = classBuilder.DefineMethod(
                        $"get_{column.ColumnName}",
                        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.SpecialName,
                        column.DataType,
                        Type.EmptyTypes
                    );

                    // Generate IL for the getter method
                    var getterIL = getterBuilder.GetILGenerator();
                    getterIL.Emit(OpCodes.Ldarg_0); // Load "this" onto the stack
                    getterIL.Emit(OpCodes.Ldfld, fieldBuilder); // Load the field value onto the stack
                    getterIL.Emit(OpCodes.Ret); // Return the value

                    // Set the getter method for the property
                    propertyBuilder.SetGetMethod(getterBuilder);

                    // Define the setter method
                    var setterBuilder = classBuilder.DefineMethod(
                        $"set_{column.ColumnName}",
                        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.SpecialName,
                        null,
                        new[] { column.DataType }
                    );

                    // Generate IL for the setter method
                    var setterIL = setterBuilder.GetILGenerator();
                    setterIL.Emit(OpCodes.Ldarg_0); // Load "this" onto the stack
                    setterIL.Emit(OpCodes.Ldarg_1); // Load the new value onto the stack
                    setterIL.Emit(OpCodes.Stfld, fieldBuilder); // Store the value into the field
                    setterIL.Emit(OpCodes.Ret); // Return

                    // Set the setter method for the property
                    propertyBuilder.SetSetMethod(setterBuilder);

                    // Implement the interface methods
                    var interfaceGetterMethod = iEmployee.GetMethod($"get_{column.ColumnName}") ?? default!;
                    var interfaceSetterMethod = iEmployee.GetMethod($"set_{column.ColumnName}") ?? default!;
                    classBuilder.DefineMethodOverride(getterBuilder, interfaceGetterMethod);
                    classBuilder.DefineMethodOverride(setterBuilder, interfaceSetterMethod);
                }
                var employeeType = classBuilder.CreateType();

                // Save the assembly to a file
                persistedAssemblyBuilder.Save(_dllPath);

                Console.WriteLine("DisposableAssembly saved to {0} successfully!", _dllPath);
            }
            catch (Exception ex) 
            { 
                ApplicationExceptionLogger.HandleException(ex);
            }
            
        }
         
        public void Run()
        {
            // Create a sample DataTable
            var dataTable = new DataTable();
            dataTable.Columns.Add("Id", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Age", typeof(int));

            var cdft = new CreateDLLFromDataTable();
            // Generate and save the dynamic assembly
            cdft.BuildAndSaveAssembly(dataTable);
        }
    }
}
