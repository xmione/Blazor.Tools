/*====================================================================================================
    Class Name  : DynamicClassBuilder
    Created By  : Solomio S. Sisante
    Created On  : August 19, 2024
    Purpose     : To manage dynamic creation of classes.

    To test:
        // Sample DataTable
        DataTable table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Age", typeof(int));

        // Create an instance of DynamicClassBuilder for a class named "Person"
        DynamicClassBuilder classBuilder = new DynamicClassBuilder("Person");

        // Create the class from the DataTable
        classBuilder.CreateClassFromDataTable(table);

        // Create an instance of the dynamic class
        object dynamicInstance = classBuilder.CreateInstance();

        // Set property values
        Type dynamicType = dynamicInstance.GetType();
        dynamicType.GetProperty("Id").SetValue(dynamicInstance, 1);
        dynamicType.GetProperty("Name").SetValue(dynamicInstance, "Alice");
        dynamicType.GetProperty("Age").SetValue(dynamicInstance, 30);

        // Get property values
        int idValue = (int)dynamicType.GetProperty("Id").GetValue(dynamicInstance);
        string nameValue = (string)dynamicType.GetProperty("Name").GetValue(dynamicInstance);
        int ageValue = (int)dynamicType.GetProperty("Age").GetValue(dynamicInstance);

        // Output the values
        Console.WriteLine($"Id: {idValue}, Name: {nameValue}, Age: {ageValue}");

  ====================================================================================================*/
namespace Blazor.Tools.BlazorBundler.Entities
{
    using System;
    using System.Data;
    using System.Reflection;
    using System.Reflection.Emit;

    public class DynamicClassBuilder
    {
        private readonly AssemblyName _assemblyName;
        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly ModuleBuilder _moduleBuilder;
        private readonly TypeBuilder _typeBuilder;

        public DynamicClassBuilder(string className)
        {
            // Define the assembly name
            _assemblyName = new AssemblyName("DynamicAssembly");

            // Create the assembly and module
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.Run);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule("MainModule");

            // Define the type (class) with the given name
            _typeBuilder = _moduleBuilder.DefineType(className, TypeAttributes.Public);
        }

        // Method to add a property to the dynamic class
        public void AddProperty(string propertyName, Type propertyType)
        {
            // Define the private field to store the property value
            FieldBuilder fieldBuilder = _typeBuilder.DefineField($"_{propertyName.ToLower()}", propertyType, FieldAttributes.Private);

            // Define the public property
            PropertyBuilder propertyBuilder = _typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            // Define the 'get' accessor method
            MethodBuilder getMethodBuilder = _typeBuilder.DefineMethod($"get_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                propertyType, Type.EmptyTypes);

            ILGenerator getIL = getMethodBuilder.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);

            // Define the 'set' accessor method
            MethodBuilder setMethodBuilder = _typeBuilder.DefineMethod($"set_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null, new Type[] { propertyType });

            ILGenerator setIL = setMethodBuilder.GetILGenerator();
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ret);

            // Map the get and set methods to the property
            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);
        }



        // Method to create the dynamic class type
        public Type CreateType()
        {
            return _typeBuilder.CreateType();
        }

        // Method to create an instance of the dynamic class
        public object? CreateInstance()
        {
            Type dynamicType = CreateType();
            var instance = Activator.CreateInstance(dynamicType);

            return instance;
        }

        // Method to create a class from a DataTable
        public void CreateClassFromDataTable(DataTable table)
        {
            foreach (DataColumn column in table.Columns)
            {
                AddProperty(column.ColumnName, column.DataType);
            }
        }
    }

}
