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
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class DynamicClassBuilder : IDisposable
    {
        private Assembly? _assembly;
        private AssemblyName? _assemblyName;
        private PersistedAssemblyBuilder? _assemblyBuilder;
        private ModuleBuilder? _moduleBuilder;
        private TypeBuilder? _typeBuilder;
        private bool _hasInterfaces;
        private Type[]? _interfaces;
        private Type? _dynamicType = default!;
        private List<(ConstructorBuilder ConstructorBuilder, Type[] ParameterTypes)>? _constructors = default!;
        private List<string>? _addedProperties = default!;
        private List<string>? _addedMethods = default!;
        private string? _assemblyFilePath;
        private bool _disposed;
        private string? _className = null;

        public Assembly? Assembly
        {
            get { return _assembly; }
        }

        public Type DynamicType
        {
            get { return _dynamicType; }
            set { _dynamicType = value; }
        }

        public DynamicClassBuilder()
        {
        }

        public DynamicClassBuilder(string assemblyName, string className, Type? baseType = null, Type[]? interfaces = null)
        {
            _className = className;
            var fullyQualifiedClassName = $"{assemblyName}.{className}";
            _assemblyName = new AssemblyName(assemblyName);
            _assemblyName.Version = new Version("1.0.0.0");
            _assemblyBuilder = new PersistedAssemblyBuilder(_assemblyName, typeof(object).Assembly);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(assemblyName);
            
            _hasInterfaces = interfaces != null && interfaces.Length > 0;
            _interfaces = interfaces;
            _constructors = new List<(ConstructorBuilder, Type[])>();
            _addedProperties = new List<string>();
            _addedMethods = new List<string>();
            var attr = TypeAttributes.Public | TypeAttributes.Class;
            var baseTypeName = baseType?.FullName ?? default!;
            var baseTypeProperties = baseType?.GetProperties();
            var baseTypeInterfaces = baseType?.GetInterfaces();
            
            baseType = baseType != null? DefineBaseClassType(_moduleBuilder, baseTypeName, baseTypeProperties, baseTypeInterfaces) : baseType;
            _typeBuilder = DefineTypes(
                _moduleBuilder,
                fullyQualifiedClassName,
                attr,
                baseType,
                interfaces
            ) ?? default!;
        }

        public TypeBuilder? DefineTypes(ModuleBuilder mb, string fullyQualifiedClassName, TypeAttributes attr, Type? parent, Type[]? types = null)
        {
            TypeBuilder? tb = null;
            Type[]? dynamicTypes = null;
            if (types != null)
            {
                // Initialize the dynamicTypes array with the same length as types
                dynamicTypes = new Type[types.Length];

                // Loop through the provided types and define dynamic types using DefineInterfaceType
                for (int i = 0; i < types.Length; i++)
                {
                    Type interfaceType = types[i];

                    // Use the type's name or fully qualified name to define each interface
                    var interfaceName = interfaceType.FullName ?? interfaceType.Name;
                    var propInfo = interfaceType.GetProperties();
                    Type iType = DefineInterfaceType(mb, interfaceName, propInfo) ?? default!;

                    // Store the dynamically created type in the dynamicTypes array
                    dynamicTypes[i] = iType;
                }

            }

            // Define the type that implements the interfaces in dynamicTypes
            tb = mb.DefineType(fullyQualifiedClassName, attr, parent, dynamicTypes);
            return tb;
        }

        private Type? DefineBaseClassType(ModuleBuilder mb, string fullyQualifiedName, PropertyInfo[]? columnsInfo, Type[]? types = null)
        {
            Type? type = null;
            TypeBuilder? tb = null;

            tb = mb.DefineType(fullyQualifiedName, TypeAttributes.Public | TypeAttributes.Class, typeof(object), types);

            if (columnsInfo != null)
            {
                foreach (var columnInfo in columnsInfo)
                {
                    // Define the field
                    var fieldBuilder = tb.DefineField(columnInfo.Name, columnInfo.PropertyType, FieldAttributes.Private);

                    // Define the property
                    var propertyBuilder = tb.DefineProperty(columnInfo.Name, PropertyAttributes.HasDefault, columnInfo.PropertyType, null);

                    // Define the getter method
                    var getterBuilder = tb.DefineMethod(
                        $"get_{columnInfo.Name}",
                        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.SpecialName,
                        columnInfo.PropertyType,
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
                    var setterBuilder = tb.DefineMethod(
                        $"set_{columnInfo.Name}",
                        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.SpecialName,
                        null,
                        new[] { columnInfo.PropertyType }
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
                    if (types != null)
                    {
                        foreach (Type baseType in types)
                        {
                            var interfaceGetterMethod = baseType.GetMethod($"get_{columnInfo.Name}") ?? default!;
                            var interfaceSetterMethod = baseType.GetMethod($"set_{columnInfo.Name}") ?? default!;
                            tb.DefineMethodOverride(getterBuilder, interfaceGetterMethod);
                            tb.DefineMethodOverride(setterBuilder, interfaceSetterMethod);
                        }
                    }
                    
                }

            }

            type = tb.CreateType();
            return type;
        }
        
        private Type? DefineInterfaceType(ModuleBuilder mb, string fullyQualifiedName, PropertyInfo[] columnsInfo)
        {
            Type? type = null;
            TypeBuilder? tb = null;

            tb = mb.DefineType(fullyQualifiedName, TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

            foreach (var columnInfo in columnsInfo)
            {
                // Define the getter method
                tb.DefineMethod(
                    $"get_{columnInfo.Name}",
                    MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                    columnInfo.PropertyType,
                    Type.EmptyTypes
                );

                // Define the setter method
                tb.DefineMethod(
                    $"set_{columnInfo.Name}",
                    MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                    null,
                    new[] { columnInfo.PropertyType }
                );
            }

            type = tb.CreateType();
            return type;
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

        
        public Type? CreateType()
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

        public void DefineMethod(string methodName, Type? returnType, Type[] parameterTypes, string[]? parameterNames, Action<ILGenerator, LocalBuilder?> generateMethodBody)
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
            Type dynamicType = CreateType() ?? default!;
            if (dynamicType == null)
            {
                throw new InvalidOperationException("Dynamic type creation failed.");
            }

            return dynamicType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void SaveAssembly(string assemblyFilePath)
        {
            try 
            {
                _assemblyFilePath = assemblyFilePath;

                // Create the type before saving it to disk
                _dynamicType = _typeBuilder?.CreateType();

                // Save the assembly to disk
                _assemblyBuilder?.Save(assemblyFilePath);

                // Try to load the assembly to make sure that you can get the static version so that you can do more things like GetProperties()
                var assembly = assemblyFilePath.LoadAssemblyFromDLLFile();
                if (assembly != null && _assemblyName != null)
                {
                    var fullyQualifiedName = $"{_assemblyName.Name}.{_className}";
                    _dynamicType = assembly.GetType(fullyQualifiedName);
                }

                _assembly = assembly;
            }
            catch (Exception ex)
            {
                ApplicationExceptionLogger.HandleException(ex);
            }
            
        }

        public void DeleteAssembly()
        {
            if (!string.IsNullOrEmpty(_assemblyFilePath) && File.Exists(_assemblyFilePath))
            {
                File.Delete(_assemblyFilePath);
            }
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
                    _assembly = null;
                    _assemblyName = null;
                    _className = null;
                    _assemblyBuilder = null;
                    _moduleBuilder = null;
                    _typeBuilder = null;
                    _hasInterfaces = false;
                    _interfaces = null;
                    _dynamicType = null;
                    _constructors = null;
                    _addedProperties = null;
                    _addedMethods = null;
                    _assemblyFilePath = null;
    }

                _disposed = true;
            }
        }

        // Destructor to ensure resources are cleaned up
        ~DynamicClassBuilder()
        {
            Dispose(false);
        }
    }
}


