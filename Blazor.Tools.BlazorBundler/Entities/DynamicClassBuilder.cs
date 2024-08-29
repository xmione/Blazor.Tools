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
using Blazor.Tools.BlazorBundler.Extensions;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class DynamicClassBuilder
    {
        private readonly AssemblyName _assemblyName;
        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly ModuleBuilder _moduleBuilder;
        private readonly TypeBuilder _typeBuilder;
        private readonly bool _hasInterfaces;
        private Type[]? _interfaces;
        private Type _dynamicType = default!;
        private readonly List<(ConstructorBuilder ConstructorBuilder, Type[] ParameterTypes)> _constructors = default!;
        private List<string> _addedProperties = default!;
        private List<string> _addedMethods = default!;
        private string? _assemblyFilePath;

        public Type DynamicType
        {
            get { return _dynamicType; }
            set { _dynamicType = value; }
        }

        public DynamicClassBuilder(string className, Type? baseType = null, Type[]? interfaces = null)
        {
            _assemblyName = new AssemblyName("DynamicAssembly");
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.Run);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule("MainModule");

            _hasInterfaces = interfaces != null && interfaces.Length > 0;
            _interfaces = interfaces;
            _constructors = new List<(ConstructorBuilder, Type[])>();
            _addedProperties = new List<string>();
            _addedMethods = new List<string>();
            _typeBuilder = _moduleBuilder.DefineType(
                className,
                TypeAttributes.Public | TypeAttributes.Class,
                baseType,
                interfaces
            );
        }

        public FieldBuilder DefineField(string fieldName, Type fieldType, FieldAttributes attributes)
        {
            // Define a private field for the property
            return _typeBuilder.DefineField($"_{fieldName.ToLower()}", fieldType, attributes);
        }

        public void AddProperty(string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = _typeBuilder.DefineField($"_{propertyName.ToLower()}", propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = _typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            MethodBuilder getMethodBuilder = _typeBuilder.DefineMethod($"get_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                propertyType, Type.EmptyTypes);

            ILGenerator getIL = getMethodBuilder.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);

            MethodBuilder setMethodBuilder = _typeBuilder.DefineMethod($"set_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null, new Type[] { propertyType });

            ILGenerator setIL = setMethodBuilder.GetILGenerator();
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);
        }

        public void DefineConstructor(Type[] parameterTypes, Action<ILGenerator> generateConstructorBody)
        {
            var constructorBuilder = _typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                parameterTypes
            );

            _constructors.Add((constructorBuilder, parameterTypes));

            var ilg = constructorBuilder.GetILGenerator();
            generateConstructorBody(ilg);
            ilg.Emit(OpCodes.Ret);
        }

        public IReadOnlyList<(ConstructorBuilder ConstructorBuilder, Type[] ParameterTypes)> GetDefinedConstructors()
        {
            return _constructors.AsReadOnly();
        }

        public Type CreateType()
        {
            Type? type = null;

            try 
            {
                type = _typeBuilder.CreateType() ?? throw new InvalidOperationException("Failed to create type.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            return type;
        }

        public object? CreateInstance()
        {
            _dynamicType = CreateType();
            return Activator.CreateInstance(_dynamicType);
        }

        public void CreateClassFromDataTable(DataTable? table)
        {
            if (table != null)
            {
                foreach (DataColumn column in table.Columns)
                {
                    AddProperty(column.ColumnName, column.DataType);
                }

                // Automatically add properties from implemented interfaces
                if (_interfaces != null)
                {
                    foreach (Type interfaceType in _interfaces)
                    {
                        var properties = interfaceType.GetProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (!_addedProperties.Contains(property.Name)) // Check if property already added
                            {
                                AddProperty(property.Name, property.PropertyType);
                                _addedProperties.Add(property.Name);
                            }

                            // Check if the property is a getter
                            if (property.CanRead)
                            {
                                string getMethodName = $"get_{property.Name}";
                                if (!_addedMethods.Contains(getMethodName)) // Check if method already added
                                {
                                    // Define a default getter method
                                    DefineMethod(getMethodName, property.PropertyType, Type.EmptyTypes, Array.Empty<string>(), (ilg, _) =>
                                    {
                                        ilg.Emit(OpCodes.Ldarg_0);
                                        ilg.Emit(OpCodes.Ldfld, _typeBuilder.DefineField($"_{property.Name.ToLower()}", property.PropertyType, FieldAttributes.Private));
                                        ilg.Emit(OpCodes.Ret);
                                    });

                                    string setMethodName = $"set_{property.Name}";
                                    // Define setter method (optional, if needed)
                                    DefineMethod(setMethodName, null, new[] { property.PropertyType }, new[] { "value" }, (ilg, _) =>
                                    {
                                        ilg.Emit(OpCodes.Ldarg_0);
                                        ilg.Emit(OpCodes.Ldarg_1);
                                        ilg.Emit(OpCodes.Stfld, _typeBuilder.DefineField($"_{property.Name.ToLower()}", property.PropertyType, FieldAttributes.Private));
                                        ilg.Emit(OpCodes.Ret);
                                    });

                                    _addedMethods.Add(getMethodName);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DefineMethod(string methodName, Type returnType, Type[] parameterTypes, string[]? parameterNames, Action<ILGenerator, LocalBuilder?> generateMethodBody)
        {
            MethodBuilder methodBuilder = _typeBuilder.DefineMethod(
                methodName,
                MethodAttributes.Public | MethodAttributes.Virtual,
                returnType,
                parameterTypes
            );

            // Define parameter names if provided
            if (parameterNames != null)
            {
                for (int i = 0; i < parameterNames.Length; i++)
                {
                    methodBuilder.DefineParameter(i + 1, ParameterAttributes.None, parameterNames[i]);
                }
            }

            ILGenerator ilg = methodBuilder.GetILGenerator();

            generateMethodBody(ilg, null);

            if (_hasInterfaces)
            {
                Type? interfaceType = _typeBuilder.GetInterfaces().FirstOrDefault();
                if (interfaceType != null)
                {
                    MethodInfo? interfaceMethod = interfaceType.GetMethod(methodName);
                    if (interfaceMethod != null)
                    {
                        _typeBuilder.DefineMethodOverride(methodBuilder, interfaceMethod);
                    }
                }
            }
        }

        public ConstructorInfo[] GetConstructors()
        {
            Type dynamicType = CreateType();
            if (dynamicType == null)
            {
                throw new InvalidOperationException("Dynamic type creation failed.");
            }

            return dynamicType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void SaveAssembly(string assemblyFilePath)
        {
            _assemblyFilePath = assemblyFilePath;
            // Create an assembly in memory
            var assembly = _moduleBuilder.Assembly;

            // Save the assembly to disk
            using (var fileStream = new FileStream(assemblyFilePath, FileMode.Create, FileAccess.Write))
            {
                //var assemblyBytes = ; //get assemblyBytes here and do not create any additional methods for this
                //fileStream.Write(assemblyBytes, 0, assemblyBytes.Length);
            }
        }

        public void DeleteAssembly()
        {
            if (!string.IsNullOrEmpty(_assemblyFilePath) && File.Exists(_assemblyFilePath))
            {
                File.Delete(_assemblyFilePath);
            }
        }



    }
}


