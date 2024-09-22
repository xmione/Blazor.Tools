/*====================================================================================================
    Class Name  : TypeCreator
    Created By  : Solomio S. Sisante
    Created On  : September 15, 2024
    Purpose     : To provide a POC prototype for testing type creations.
  ====================================================================================================*/
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using static Blazor.Tools.BlazorBundler.Utilities.Assemblies.AssemblyGenerator;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class TypeCreator : ITypeCreator
    {
        private string _contextAssemblyName;
        private string _modelTypeAssemblyName;
        private string _modelTypeName;
        private string _iModelTypeAssemblyName;
        private string _iModelTypeName;
        private string _version;
        private AssemblyName _assemblyName;
        private AssemblyBuilderAccess _assemblyBuilderAccess;
        private PersistedAssemblyBuilder _assemblyBuilder;
        private string _moduleName;
        private ModuleBuilder _moduleBuilder;
        private List<(string TableName, string ColumnName, Type ColumnType)>? _columnDefinitions;
        private List<(string TypeName, PropertyBuilder PropertyBuilder)>? _addedProperties;
        private List<(ConstructorBuilder ConstructorBuilder, Type[] ParameterTypes)>? _constructors = default!;

        public List<(string TypeName, PropertyBuilder PropertyBuilder)>? AddedProperties 
        {
            get { return _addedProperties; }
        }

        public TypeCreator()
        {
            _addedProperties = new List<(string TypeName, PropertyBuilder PropertyBuilder)>();
            _constructors = new List<(ConstructorBuilder ConstructorBuilder, Type[] ParameterTypes)>();
        }

        public TypeCreator(string contextAssemblyName, string modelTypeAssemblyName, string modelTypeName, string iModelTypeAssemblyName, string iModelTypeName, string version, AssemblyBuilderAccess assemblyBuilderAccess) 
        {
            _contextAssemblyName = contextAssemblyName;
            _modelTypeAssemblyName = modelTypeAssemblyName;
            _modelTypeName = modelTypeName;
            _iModelTypeAssemblyName = iModelTypeAssemblyName;
            _iModelTypeName = iModelTypeName;
            _version = version;
            _assemblyName = default!;
            _assemblyBuilderAccess = assemblyBuilderAccess;
            _assemblyBuilder = default!;
            _moduleName = default!;
            _moduleBuilder = default!;
        }

        public List<(string TableName, string ColumnName, Type ColumnType)> GetDataTableColumnDefinitions(DataTable dataTable)
        {

            var columnDefinitions = dataTable.GetColumnDefinitions(); // Get the column definitions List<(string ColumnName, Type ColumnType)>
            if (_columnDefinitions == null)
            {
                _columnDefinitions = new List<(string TableName, string ColumnName, Type ColumnType)>();
            }

            if (columnDefinitions != null)
            {
                //Remove first existing property info for items with the specified table name 
                _columnDefinitions =_columnDefinitions.Where(d => d.TableName != dataTable.TableName).ToList();
                _columnDefinitions.AddRange(columnDefinitions);
            }

            return _columnDefinitions;
        }

        public AssemblyName DefineAssemblyName(string contextAssemblyName = null!, string version = null!)
        {
            _contextAssemblyName = contextAssemblyName ?? _contextAssemblyName;
            _version = version ?? _version;

            AssemblyName assemblyName = new AssemblyName(_contextAssemblyName);
            assemblyName.Version = new Version(_version);
            _assemblyName = assemblyName;

            return assemblyName;
        }
        
        public PersistedAssemblyBuilder DefineAssemblyBuilder(AssemblyName? assemblyName = null, AssemblyBuilderAccess? assemblyBuilderAccess = null)
        {
            _assemblyName = assemblyName ?? _assemblyName;
            _assemblyBuilderAccess = assemblyBuilderAccess ?? _assemblyBuilderAccess;

            _assemblyBuilder = new PersistedAssemblyBuilder(_assemblyName, typeof(object).Assembly);

            return _assemblyBuilder;
        }

        public ModuleBuilder DefineModule(PersistedAssemblyBuilder assemblyBuilder = null!, string moduleName = null!)
        {
            _assemblyBuilder = assemblyBuilder ?? _assemblyBuilder;
            _moduleName = moduleName ?? _moduleName;
            ModuleBuilder moduleBuilder = _assemblyBuilder.DefineDynamicModule(_moduleName);
            _moduleBuilder = moduleBuilder;

            return moduleBuilder;
        }

        public Type CreateType(
                                ref ModuleBuilder moduleBuilder,
                                string typeName,
                                TypeAttributes typeAttributes,
                                Type baseType = default!,
                                Type[] interfaces = default!,
                                string[] genericParameterNames = default!,
                                IEnumerable<PropertyDefinition> properties = default!,
                                IEnumerable<MethodDefinition> methodDefinitions = default!,
                                IEnumerable<EventDefinition> events = default!,
                                Func<TypeBuilder, TypeBuilder> defineConstructorsAction = default!,
                                Func<TypeBuilder, TypeBuilder> defineMethodsAction = default!,
                                List<(Type InterfaceType, bool isTypeUsed)> iImplementations = default! 
            )
        {
            Type type = default!;
            try
            {
                // Define the type builder
                var typeBuilder = moduleBuilder.DefineType(
                    typeName,
                    typeAttributes,
                    baseType,
                    interfaces
                );

                // Declare generic parameters if needed
                if (genericParameterNames != null && genericParameterNames.Length > 0)
                {
                    var genericParams = typeBuilder.DefineGenericParameters(genericParameterNames);
                }

                // Add _modelProperties
                if (properties != null)
                {
                    foreach (var prop in properties)
                    {
                        typeBuilder = DefineProperty(typeName, typeBuilder, prop.Name, prop.Type);
                    }
                }

                // Add constructors using the provided action
                if (defineConstructorsAction != null)
                {
                    typeBuilder = defineConstructorsAction(typeBuilder); // Call the delegate to define constructors
                }

                // Add iViewModelMethods
                if (methodDefinitions != null)
                {
                    foreach (var method in methodDefinitions)
                    {
                        typeBuilder = DefineMethod(typeBuilder, method.Name, method.ReturnType, method.ParameterTypes);
                    }
                }

                // Add methods using the provided action
                if (defineMethodsAction != null)
                {
                    typeBuilder = defineMethodsAction(typeBuilder); // Call the delegate to define methods
                }

                // Add events
                if (events != null)
                {
                    foreach (var eventDefinition in events)
                    {
                        DefineEvent(typeBuilder, eventDefinition.Name, eventDefinition.Type);
                    }
                }

                if (iImplementations != null)
                {
                    foreach (var imp in iImplementations)
                    {
                        if (imp.isTypeUsed)
                        {
                            typeBuilder.AddInterfaceImplementation(imp.InterfaceType.MakeGenericType(typeBuilder));
                        }
                        else
                        {
                            typeBuilder.AddInterfaceImplementation(imp.InterfaceType);
                        }
                        
                    }
                    
                }

                type = typeBuilder.CreateType();
            }
            catch (Exception ex)
            {
                AppLogger.HandleException(ex);
                throw; // propagate
            }

            return type;
        }

        public TypeBuilder DefineConstructor(TypeBuilder typeBuilder, Type[] parameterTypes, Action<ILGenerator> generateConstructorBody)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                parameterTypes
            ) ?? default!;

            var ilg = constructorBuilder.GetILGenerator();
            generateConstructorBody(ilg);
            ilg.Emit(OpCodes.Ret);

            _constructors?.Add((constructorBuilder, parameterTypes));

            return typeBuilder;

        }

        public IReadOnlyList<(ConstructorBuilder ConstructorBuilder, Type[] ParameterTypes)> GetDefinedConstructors()
        {
            return _constructors?.AsReadOnly() ?? default!;
        }

        public TypeBuilder DefineMethod(TypeBuilder typeBuilder, string methodName, Type? returnType = null, Type[]? parameterTypes = null, string[]? parameterNames = null, Action<ILGenerator, LocalBuilder?>? generateMethodBody = null)
        {
            try
            {
                MethodAttributes methodAttributes = typeBuilder.IsInterface ? MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.Virtual : MethodAttributes.Public | MethodAttributes.Virtual;

                MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                    methodName,
                    methodAttributes,
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

                // Only generate method bodies for non-interface types
                if (!typeBuilder.IsInterface && generateMethodBody != null)
                {
                    ILGenerator ilg = methodBuilder.GetILGenerator();
                    generateMethodBody(ilg, null);
                }

                var interfaces = typeBuilder.GetInterfaces();
                if (interfaces.Count() > 0)
                {
                    Type? interfaceType = interfaces.FirstOrDefault();
                    if (interfaceType != null)
                    {
                        MethodInfo? interfaceMethod = interfaceType.GetMethod(methodName);
                        if (interfaceMethod != null)
                        {
                            typeBuilder.DefineMethodOverride(methodBuilder, interfaceMethod);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.HandleException(ex);
                throw;
            }

            return typeBuilder;
        }

        public Type DefineInterfaceType(ModuleBuilder moduleBuilder = null!, string fullyQualifiedName = null!)
        {
            _moduleBuilder = moduleBuilder ?? _moduleBuilder;

            // Extract base type name and generic parameters
            string baseTypeName = GetBaseTypeName(fullyQualifiedName);
            string[] genericParams = GetGenericParameters(fullyQualifiedName);

            // Define the type with generic parameters
            TypeBuilder tb = _moduleBuilder.DefineType(baseTypeName, TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

            // Define the actual generic parameters with their type constraints
            DefineGenericParameters(tb, genericParams);

            // Create and return the type
            return tb.CreateType();
        }

        public void Save(string dllPath)
        {
            try
            {
                if (File.Exists(dllPath))
                {
                    File.Delete(dllPath);
                }

                _assemblyBuilder.Save(dllPath);

                if (File.Exists(dllPath))
                {
                    AppLogger.WriteInfo($"Assembly file was successfully saved to {dllPath}.");
                }
                else 
                {
                    throw new Exception($"Unable to save file {dllPath} successfully.");
                }

                var assembly = Assembly.LoadFile(dllPath);
                var types = assembly.GetTypes();
            }
            catch (Exception ex)
            {
                AppLogger.HandleException(ex);
            }
        }

        private TypeBuilder DefineProperty(string typeName, TypeBuilder typeBuilder, string propertyName, Type propertyType, TypeBuilder baseTypeBuilder = null!)
        {
            try
            {
                // Define the private field
                var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);

                // Define the property
                var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

                // Define the 'get' accessor
                var getterBuilder = typeBuilder.DefineMethod(
                    $"get_{propertyName}",
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                    propertyType,
                    Type.EmptyTypes
                );
                var getterIL = getterBuilder.GetILGenerator();
                getterIL.Emit(OpCodes.Ldarg_0);
                getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
                getterIL.Emit(OpCodes.Ret);
                propertyBuilder.SetGetMethod(getterBuilder);

                // Define the 'set' accessor
                var setterBuilder = typeBuilder.DefineMethod(
                    $"set_{propertyName}",
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                    null,
                    new[] { propertyType }
                );
                var setterIL = setterBuilder.GetILGenerator();
                setterIL.Emit(OpCodes.Ldarg_0);
                setterIL.Emit(OpCodes.Ldarg_1);
                setterIL.Emit(OpCodes.Stfld, fieldBuilder);
                setterIL.Emit(OpCodes.Ret);
                propertyBuilder.SetSetMethod(setterBuilder);

                _addedProperties?.Add((typeName, propertyBuilder));

                // Override iViewModelMethods if baseTypeBuilder is provided
                if (baseTypeBuilder != null)
                {
                    var baseProperty = baseTypeBuilder.GetProperty(propertyName);
                    if (baseProperty != null)
                    {
                        typeBuilder.DefineMethodOverride(getterBuilder, baseProperty?.GetGetMethod() ?? default!);
                        typeBuilder.DefineMethodOverride(setterBuilder, baseProperty?.GetSetMethod() ?? default!);
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.HandleException(ex);
                throw;
            }

            return typeBuilder;
        }

        private TypeBuilder DefineEvent(TypeBuilder typeBuilder, string eventName, Type eventHandlerType)
        {
            try
            {
                var eventBuilder = typeBuilder.DefineEvent(
                eventName,
                EventAttributes.None,
                eventHandlerType
            );

                // Define add and remove iViewModelMethods for the event
                var addMethodBuilder = typeBuilder.DefineMethod(
                    $"add_{eventName}",
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                    null,
                    new[] { eventHandlerType }
                );

                var removeMethodBuilder = typeBuilder.DefineMethod(
                    $"remove_{eventName}",
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                    null,
                    new[] { eventHandlerType }
                );

                eventBuilder.SetAddOnMethod(addMethodBuilder);
                eventBuilder.SetRemoveOnMethod(removeMethodBuilder);
            }
            catch (Exception ex)
            {
                AppLogger.HandleException(ex);
                throw;
            }

            return typeBuilder;
        }

        private string GetBaseTypeName(string fullyQualifiedName)
        {
            int backtickIndex = fullyQualifiedName.IndexOf('`');
            return backtickIndex != -1 ? fullyQualifiedName.Substring(0, backtickIndex) : fullyQualifiedName;
        }

        private string[] GetGenericParameters(string fullyQualifiedName)
        {
            int backtickIndex = fullyQualifiedName.IndexOf('`');
            if (backtickIndex != -1)
            {
                string genericParamsSection = fullyQualifiedName.Substring(backtickIndex + 1).Trim();
                if (genericParamsSection.StartsWith("[[") && genericParamsSection.EndsWith("]]"))
                {
                    string paramsContent = genericParamsSection.Substring(2, genericParamsSection.Length - 4);
                    return paramsContent.Split(new[] { "],[" }, StringSplitOptions.None);
                }
            }
            return Array.Empty<string>();
        }

        private void DefineGenericParameters(TypeBuilder tb, string[] genericParams)
        {
            if (genericParams.Length > 0)
            {
                // Define the generic parameters
                string[] genericParamNames = new string[genericParams.Length];
                for (int i = 0; i < genericParams.Length; i++)
                {
                    genericParamNames[i] = $"T{i + 1}";
                }

                // Define the generic parameters on the type builder
                GenericTypeParameterBuilder[] genericTypeParameters = tb.DefineGenericParameters(genericParamNames);

                // Define constraints or additional attributes if needed
            }
        }

        public static void Create()
        {
            string contextAssemblyName = "Blazor.Tools.BlazorBundler.Interfaces";
            string modelTypeAssemblyName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models";
            string modelTypeName = "Employee";
            string iModelTypeAssemblyName = "Blazor.Tools.BlazorBundler.Interfaces";
            string iModelTypeName = "IModelExtendedProperties";
            string version = "3.1.2.0";

            var typeCreator = new TypeCreator(contextAssemblyName, modelTypeAssemblyName, modelTypeName, iModelTypeAssemblyName, iModelTypeName, version, AssemblyBuilderAccess.Run);
            typeCreator.DefineAssemblyName();
            typeCreator.DefineAssemblyBuilder();
            typeCreator.DefineModule(moduleName: contextAssemblyName);

            // Fully qualified name for the type
            string fullyQualifiedName = $"{contextAssemblyName}.IViewModel`2[[{modelTypeAssemblyName}.{modelTypeName}, {modelTypeAssemblyName}, Version={version}, Culture=neutral, PublicKeyToken=null],[{iModelTypeAssemblyName}.{iModelTypeName}, {iModelTypeAssemblyName}, Version={version}, Culture=neutral, PublicKeyToken=null]]";

            Type createdType = typeCreator.DefineInterfaceType(fullyQualifiedName: fullyQualifiedName);

            Console.WriteLine($"Expected FullName: {fullyQualifiedName}");
            Console.WriteLine($"Created Type FullName: {createdType.FullName}");
        }
    }
}
