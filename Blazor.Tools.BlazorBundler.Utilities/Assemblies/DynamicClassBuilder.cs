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
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class DynamicClassBuilder : IDisposable, IDynamicClassBuilder
    {
        private AssemblyLoadContext? _assemblyLoadContext;
        private Assembly? _assembly;
        private AssemblyName? _assemblyName;
        private PersistedAssemblyBuilder? _assemblyBuilder;
        private ModuleBuilder? _moduleBuilder;
        private TypeBuilder? _typeBuilder;
        private bool _hasInterfaces;
        private Type[]? _interfaces;
        private Type? _dynamicType = default!;
        private List<(ConstructorBuilder ConstructorBuilder, Type[] ParameterTypes)>? _constructors = default!;
        private List<PropertyBuilder>? _addedProperties = default!;
        private List<string>? _addedMethods = default!;
        private string? _assemblyFilePath;
        private bool _disposed;
        private string? _baseClassName = null;
        private string? _fullyQualifiedClassName;
        private List<FieldBuilder>? _addedFields = default!;
        private string? _contextAssemblyName;
        private List<(string ColumnName, Type ColumnType)>? _propertyInfo;

        public Assembly? Assembly
        {
            get { return _assembly; }
        }

        public Type? DynamicType
        {
            get { return _dynamicType; }
            set { _dynamicType = value; }
        }

        public TypeBuilder? TypeBuilder
        {
            get { return _typeBuilder; }
            set { _typeBuilder = value; }
        }

        public List<PropertyBuilder>? AddedProperties 
        {
            get { return _addedProperties; }
            set { _addedProperties = value; }    
        }

        public List<FieldBuilder>? AddedFields
        {
            get { return _addedFields;  }
            set { _addedFields = value; }   
        }

        public string? AssemblyFilePath
        {
            get { return _assemblyFilePath; }
            set { _assemblyFilePath = value; }
        }

        public AssemblyLoadContext? AssemblyLoadContext
        {
            get { return _assemblyLoadContext; }
            set { _assemblyLoadContext = value; }
        }

        public DynamicClassBuilder(string? contextAssemblyName = null, string? assemblyName = null, bool useContextAsParent = false)
        {
            string tmpContextName = Path.GetFileNameWithoutExtension(Path.GetFileName(Path.GetTempFileName()));
            string tmpAssemblyName = Path.GetFileNameWithoutExtension(Path.GetFileName(Path.GetTempFileName()));
            _contextAssemblyName = contextAssemblyName ?? tmpContextName;
            assemblyName = useContextAsParent ? _contextAssemblyName + "." + (assemblyName ?? tmpAssemblyName) : assemblyName ?? tmpAssemblyName;
            _assemblyFilePath = Path.Combine(Path.GetTempPath(), useContextAsParent ? _contextAssemblyName : assemblyName) + ".dll"; // use the parent assembly name that envelopes class and interface namespaces

            _assemblyName = new AssemblyName(_contextAssemblyName); // use the parent assembly name
            _assemblyName.Version = new Version("1.0.0.0");
            _assemblyBuilder = new PersistedAssemblyBuilder(_assemblyName, typeof(object).Assembly);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(assemblyName);

            _constructors = new List<(ConstructorBuilder, Type[])>();
            _addedFields = new List<FieldBuilder>();
            _addedProperties = new List<PropertyBuilder>();
            _addedMethods = new List<string>();

            _propertyInfo = default!;
        }

        /// <summary>
        /// Creates a dynamic class builder object to manage the dynamic creation of .dll files.
        /// </summary>
        /// <param name="assemblyName">The assembly name. This is the default namespace if useContextAsParent is not supplied.</param>
        /// <param name="className">The dynamic class that will be created in the assembly.</param>
        /// <param name="baseType">The user supplied base type is used only if supplied. Otherwise, it does not create a base type.</param>
        /// <param name="interfaces">This is used to create the base type.</param>
        /// <param name="useContextAsParent">This is used to create a different namespace as parent namespace for all the namespaces.
        /// It will also serve as the assembly name.</param>
        public DynamicClassBuilder(string? contextAssemblyName = null, string? assemblyName = null, string? className = null, Type? baseType = null, Type[]? interfaces = null, bool useContextAsParent = false)
        {
            try
            {
                var tmpAssemblyName = string.Join(".", Path.GetFileNameWithoutExtension(Path.GetFileName(Path.GetTempFileName())), 
                                        Path.GetFileNameWithoutExtension(Path.GetFileName(Path.GetTempFileName())));

                assemblyName ??= tmpAssemblyName;
                if (useContextAsParent)
                {
                    var lastIndex = assemblyName.LastIndexOf('.');
                    contextAssemblyName ??= assemblyName[..lastIndex];
                }
                else 
                {
                    contextAssemblyName = assemblyName;
                }

                _contextAssemblyName = contextAssemblyName;
                _assemblyFilePath = Path.Combine(Path.GetTempPath(), useContextAsParent? _contextAssemblyName ?? default!: assemblyName) + ".dll"; // use the parent assembly name that envelopes class and interface namespaces
                _baseClassName = className;
                _fullyQualifiedClassName = $"{assemblyName}.{className}";
                
                _assemblyName = new AssemblyName(_contextAssemblyName ?? default!); // use the parent assembly name
                _assemblyName.Version = new Version("1.0.0.0");
                _assemblyBuilder = new PersistedAssemblyBuilder(_assemblyName, typeof(object).Assembly);
                _moduleBuilder = _assemblyBuilder.DefineDynamicModule(assemblyName);

                _hasInterfaces = interfaces != null && interfaces.Length > 0;
                _interfaces = interfaces;
                _constructors = new List<(ConstructorBuilder, Type[])>();
                _addedFields = new List<FieldBuilder>();
                _addedProperties = new List<PropertyBuilder>();
                _addedMethods = new List<string>();
                var attr = TypeAttributes.Public | TypeAttributes.Class;
                var baseTypeName = baseType?.FullName ?? default!;
                var baseTypeProperties = baseType?.GetProperties();
                var baseTypeInterfaces = baseType?.GetInterfaces();

                baseType = baseType != null ? DefineBaseClassType(_moduleBuilder, baseTypeName, baseTypeProperties, baseTypeInterfaces) : baseType;
                _typeBuilder = DefineTypes(
                    _moduleBuilder,
                    _fullyQualifiedClassName,
                    attr,
                    baseType,
                    interfaces
                ) ?? default!;

                _propertyInfo = null;
            }
            catch (Exception ex) 
            {
                ApplicationExceptionLogger.HandleException(ex);
            }
            
        }

        public void CreatePropertyInfoFromDataTable(DataTable dataTable)
        {
            _propertyInfo = new List<(string ColumnName, Type ColumnType)>();

            foreach (DataColumn dc in dataTable.Columns)
            {
                _propertyInfo.Add((dc.ColumnName, dc.DataType));
            }
        }

        public void CreateBaseClassType(string? baseClassName = null, Type[]? interfaces = null)
        {
            var tmpBaseClassName = Path.GetFileNameWithoutExtension(Path.GetFileName(Path.GetTempFileName()));
            _baseClassName = baseClassName ?? tmpBaseClassName;
            _hasInterfaces = interfaces != null && interfaces.Length > 0;
            _interfaces = interfaces;

            if (_moduleBuilder != null && _fullyQualifiedClassName != null)
            {
                var attr = TypeAttributes.Public | TypeAttributes.Class;

                var baseType = DefineBaseClassType(_moduleBuilder, _baseClassName, interfaces);

                _typeBuilder = DefineTypes(
                    _moduleBuilder,
                    _fullyQualifiedClassName,
                    attr,
                    baseType,
                    interfaces
                ) ?? default!;
            }
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
                    var nameSpaceAndClassName = interfaceType.GetNamespaceAndClassName();
                    var interfaceName = interfaceType.FullName ?? interfaceType.Name;
                    //var interfaceName = $"{nameSpaceAndClassName.Namespace}.{nameSpaceAndClassName.ClassName}";
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

        public FieldBuilder DefineField(string fieldName, Type fieldType, FieldAttributes fieldAttributes)
        {
            var fieldBuilder = AddField(_typeBuilder, fieldName, fieldType, fieldAttributes) ?? default!;

            return fieldBuilder;
        }

        public void AddProperty(string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = AddField(_typeBuilder, $"{propertyName.ToLower()}", propertyType, FieldAttributes.Private) ?? default!;

            PropertyBuilder propertyBuilder = _typeBuilder?.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null) ?? default!;

            MethodBuilder getMethodBuilder = _typeBuilder?.DefineMethod($"get_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                propertyType, Type.EmptyTypes) ?? default!;

            ILGenerator getIL = getMethodBuilder.GetILGenerator();
            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);

            MethodBuilder setMethodBuilder = _typeBuilder?.DefineMethod($"set_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null, new Type[] { propertyType }) ?? default!;

            ILGenerator setIL = setMethodBuilder.GetILGenerator();
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);

            _addedProperties?.Add(propertyBuilder);
        }

        public void DefineConstructor(Type[] parameterTypes, Action<ILGenerator> generateConstructorBody)
        {
            var constructorBuilder = _typeBuilder?.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                parameterTypes
            ) ?? default!;

            _constructors?.Add((constructorBuilder, parameterTypes));

            var ilg = constructorBuilder.GetILGenerator();
            generateConstructorBody(ilg);
            ilg.Emit(OpCodes.Ret);
        }

        public IReadOnlyList<(ConstructorBuilder ConstructorBuilder, Type[] ParameterTypes)> GetDefinedConstructors()
        {
            return _constructors?.AsReadOnly() ?? default!;
        }

        public Type? CreateType()
        {
            Type? type = null;

            try
            {
                type = _typeBuilder?.CreateType() ?? throw new InvalidOperationException("Failed to create type.");
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
                            var containsPropertyName = _addedProperties?.Any(t => t.Name == property.Name) ?? false;

                            if (!containsPropertyName) // Check if property already added
                            {
                                AddProperty(property.Name, property.PropertyType);
                            }

                            // Check if the property is a getter
                            if (property.CanRead)
                            {
                                string getMethodName = $"get_{property.Name}";
                                var containsMethodName = _addedMethods?.Contains(getMethodName) ?? false;
                                if (!containsMethodName) // Check if method already added
                                {
                                    FieldBuilder fieldBuilder = AddField(_typeBuilder, $"{property.Name.ToLower()}", property.PropertyType, FieldAttributes.Private) ?? default!;
                                    // Define a default getter method
                                    DefineMethod(getMethodName, property.PropertyType, Type.EmptyTypes, Array.Empty<string>(), (ilg, _) =>
                                    {
                                        ilg.Emit(OpCodes.Ldarg_0);
                                        ilg.Emit(OpCodes.Ldfld, fieldBuilder);
                                        ilg.Emit(OpCodes.Ret);
                                    });

                                    string setMethodName = $"set_{property.Name}";
                                    // Define setter method (optional, if needed)
                                    DefineMethod(setMethodName, null, new[] { property.PropertyType }, new[] { "value" }, (ilg, _) =>
                                    {
                                        ilg.Emit(OpCodes.Ldarg_0);
                                        ilg.Emit(OpCodes.Ldarg_1);
                                        ilg.Emit(OpCodes.Stfld, fieldBuilder);
                                        ilg.Emit(OpCodes.Ret);
                                    });

                                    _addedMethods?.Add(getMethodName);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DefineMethod(string methodName, Type? returnType, Type[] parameterTypes, string[]? parameterNames, Action<ILGenerator, LocalBuilder?> generateMethodBody)
        {
            MethodBuilder methodBuilder = _typeBuilder?.DefineMethod(
                methodName,
                MethodAttributes.Public | MethodAttributes.Virtual,
                returnType,
                parameterTypes
            ) ?? default!;

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
                Type? interfaceType = _typeBuilder?.GetInterfaces().FirstOrDefault();
                if (interfaceType != null)
                {
                    MethodInfo? interfaceMethod = interfaceType.GetMethod(methodName);
                    if (interfaceMethod != null)
                    {
                        _typeBuilder?.DefineMethodOverride(methodBuilder, interfaceMethod);
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

        public void SaveAssembly(string? assemblyFilePath = null, bool saveToStream = false)
        {
            try
            {
                if (assemblyFilePath != null)
                {
                    _assemblyFilePath = assemblyFilePath;
                }

                // Create the type before saving it to disk
                _dynamicType = _typeBuilder?.CreateType();

                DeleteAssembly();

                if (saveToStream)
                {
                    using var ms = new MemoryStream();
                    // Save the assembly to stream
                    _assemblyBuilder?.Save(ms);

                    ms.Seek(0, SeekOrigin.Begin);
                    _assembly = AssemblyLoadContext.Default.LoadFromStream(ms);

                }
                else
                {
                    // Save the assembly to disk
                    _assemblyBuilder?.Save(_assemblyFilePath ?? default!);

                    // Try to load the assembly to make sure that you can get the static version so that you can do more things like GetProperties()
                    _assembly = _assemblyFilePath?.LoadAssemblyFromDLLFile();
                    //_assembly = Assembly.LoadFile(_assemblyFilePath ?? default!);

                    //_assemblyLoadContext = new AssemblyLoadContext(null, true);  // Set isCollectible to true to enable unloading
                    //_assembly = _assemblyLoadContext.LoadFromAssemblyPath(_assemblyFilePath ?? default!);

                    //_assembly = null;
                    //_assemblyLoadContext.Unload();
                    //_assemblyLoadContext = null;
                    //GC.Collect();
                    //GC.WaitForPendingFinalizers();
                    //GC.Collect();

                    _assemblyFilePath?.IsFileInUse();
                    //_assembly = Assembly.LoadFrom(_assemblyFilePath ?? default!);

                }

                if (_assembly != null && _fullyQualifiedClassName != null)
                {
                    _dynamicType = _assembly.GetType(_fullyQualifiedClassName);
                }
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

        private FieldBuilder? AddField(TypeBuilder? typeBuilder, string fieldName, Type fieldType, FieldAttributes fieldAttributes)
        {
            var fieldBuilder = _addedFields?.FirstOrDefault(f => f.Name.Contains(fieldName));
            bool fieldExists = fieldBuilder != null;
            if (!fieldExists)
            {
                // Define a private field for the property
                fieldBuilder = typeBuilder?.DefineField($"_{fieldName.ToLower()}", fieldType, fieldAttributes) ?? default!;
                _addedFields?.Add(fieldBuilder);
            }

            return fieldBuilder;
        }

        private Type? DefineBaseClassType(ModuleBuilder mb, string fullyQualifiedName, Type[]? types = null)
        {
            Type? type = null;
            TypeBuilder? tb = null;

            tb = mb.DefineType(fullyQualifiedName, TypeAttributes.Public | TypeAttributes.Class, typeof(object), types);

            if (_propertyInfo != null)
            {
                foreach (var columnInfo in _propertyInfo)
                {
                    // Define the field
                    var fieldBuilder = tb.DefineField(columnInfo.Item1, columnInfo.Item2, FieldAttributes.Private);

                    // Define the property
                    var propertyBuilder = tb.DefineProperty(columnInfo.Item1, PropertyAttributes.HasDefault, columnInfo.Item2, null);

                    // Define the getter method
                    var getterBuilder = tb.DefineMethod(
                        $"get_{columnInfo.ColumnName}",
                        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.SpecialName,
                        columnInfo.ColumnType,
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
                        $"set_{columnInfo.ColumnName}",
                        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.SpecialName,
                        null,
                        new[] { columnInfo.ColumnType }
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
                            var interfaceGetterMethod = baseType.GetMethod($"get_{columnInfo.ColumnName}") ?? default!;
                            var interfaceSetterMethod = baseType.GetMethod($"set_{columnInfo.ColumnName}") ?? default!;
                            tb.DefineMethodOverride(getterBuilder, interfaceGetterMethod);
                            tb.DefineMethodOverride(setterBuilder, interfaceSetterMethod);
                        }
                    }
                    
                }

            }

            type = tb.CreateType();
            return type;
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
            // Extract the base type name
            string baseTypeName = GetBaseTypeName(fullyQualifiedName);

            tb = mb.DefineType(baseTypeName, TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

            // Extract and define generic parameters if needed
            DefineGenericParameters(tb, fullyQualifiedName);

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

        private static string GetBaseTypeName(string fullyQualifiedName)
        {
            int backtickIndex = fullyQualifiedName.IndexOf('`');
            if (backtickIndex != -1)
            {
                return fullyQualifiedName.Substring(0, backtickIndex);
            }
            return fullyQualifiedName;
        }

        private static void DefineGenericParameters(TypeBuilder tb, string fullyQualifiedName)
        {
            int backtickIndex = fullyQualifiedName.IndexOf('`');
            if (backtickIndex != -1)
            {
                string genericParams = fullyQualifiedName.Substring(backtickIndex + 1);
                string[] genericParamNames = genericParams.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

                // Define generic parameters for the type
                GenericTypeParameterBuilder[] genericParameters = tb.DefineGenericParameters(genericParamNames);
                for (int i = 0; i < genericParameters.Length; i++)
                {
                    genericParameters[i].SetGenericParameterAttributes(GenericParameterAttributes.None);
                    // Additional configuration of generic parameters can be done here
                }
            }
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
                    _contextAssemblyName = null;
                    _assemblyName = null;
                    _baseClassName = null;
                    _assemblyBuilder = null;
                    _moduleBuilder = null;
                    _typeBuilder = null;
                    _propertyInfo = null;
                    _hasInterfaces = false;
                    _interfaces = null;
                    _dynamicType = null;
                    _constructors = null;
                    _addedFields = null;
                    _addedProperties = null;
                    _addedMethods = null;
                    _assemblyFilePath = null;
                    _fullyQualifiedClassName = null;
                    _assemblyLoadContext?.Unload();
                    _assemblyLoadContext = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
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


