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
using System.Runtime.Loader;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class CreateDLLFromDataTable
    {
        private static string _iEmployeeFullyQualifiedName = string.Empty;
        private static string _employeeFullyQualifiedName = string.Empty;
        private static string _dllPath = string.Empty;

        public static void BuildAndSaveAssembly(DataTable dataTable)
        {
            try 
            {
                var contextAssemblyName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects";
                var employeeNameSpace = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models";
                var employeeTypeName = "Employee";
                var employeeFullyQualifiedName = $"{employeeNameSpace}.{employeeTypeName}";
                _dllPath = Path.Combine(Path.GetTempPath(), contextAssemblyName + ".dll"); // use the parent assembly name that envelopes class and interface namespaces

                var iEmployeeNameSpace = "Blazor.Tools.BlazorBundler.Interfaces";
                var iEmployeeTypeName = "IEmployee";
                var iEmployeeFullyQualifiedName = $"{iEmployeeNameSpace}.{iEmployeeTypeName}";

                //var assemblyName = new AssemblyName("DynamicAssembly");
                var assemblyName = new AssemblyName(contextAssemblyName); // use the parent assembly name
                assemblyName.Version = new Version("1.0.0.0");
                var persistedAssemblyBuilder = new PersistedAssemblyBuilder(assemblyName, typeof(object).Assembly);
                var moduleBuilder = persistedAssemblyBuilder.DefineDynamicModule(employeeNameSpace);
                //var moduleBuilder = persistedAssemblyBuilder.DefineDynamicModule("MainModule");

                _iEmployeeFullyQualifiedName = iEmployeeFullyQualifiedName;
                _employeeFullyQualifiedName = employeeFullyQualifiedName;
                // Define the IEmployee interface

                var interfaceBuilder = moduleBuilder.DefineType(iEmployeeFullyQualifiedName, TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

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
                var classBuilder = moduleBuilder.DefineType(employeeFullyQualifiedName, TypeAttributes.Public | TypeAttributes.Class, typeof(object), new[] { iEmployee });

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
                //using (var fileStream = new FileStream("DynamicAssembly.dll", FileMode.Create, FileAccess.Write))
                //{
                //    persistedAssemblyBuilder.Save("DynamicAssembly.dll");
                //}

                Console.WriteLine("DisposableAssembly saved successfully!");
            }
            catch (Exception ex) 
            { 
                ApplicationExceptionLogger.HandleException(ex);
            }
            
        }

        public static void CreateAndUseInstance()
        {
            // Load the saved assembly from the file
            var assemblyBytes = File.ReadAllBytes(_dllPath);
            var assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(assemblyBytes));

            // Get the types
            var iEmployeeType = assembly.GetType(_iEmployeeFullyQualifiedName);
            var employeeType = assembly.GetType(_employeeFullyQualifiedName);

            if (iEmployeeType == null || employeeType == null)
            {
                Console.WriteLine("Failed to load types.");
                return;
            }

            // Create an instance of the dynamically generated Employee class
            var newEmployee = Activator.CreateInstance(employeeType) ?? default!;

            // Check if the newEmployee instance is of the IEmployee type
            if (iEmployeeType.IsAssignableFrom(newEmployee.GetType()))
            {
                Console.WriteLine("Successfully created an instance of Employee that implements IEmployee.");
            }

            // Set properties dynamically using reflection
            foreach (var column in employeeType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var propertyValue = column.FieldType == typeof(int) ? (object)1 : "John Doe";
                column.SetValue(newEmployee, propertyValue);
            }

            // Output the values
            foreach (var column in employeeType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Console.WriteLine($"{column.Name}: {column.GetValue(newEmployee)}");
            }
        }

        public static void Run()
        {
            // Create a sample DataTable
            var dataTable = new DataTable();
            dataTable.Columns.Add("Id", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Age", typeof(int));

            // Generate and save the dynamic assembly
            CreateDLLFromDataTable.BuildAndSaveAssembly(dataTable);

            // Create and use an instance of the dynamically generated type
            CreateDLLFromDataTable.CreateAndUseInstance();
        }
    }
}
