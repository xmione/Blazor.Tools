/*====================================================================================================
    Class Name  : CreateDLLFromDataTable
    Created By  : Solomio S. Sisante
    Created On  : September 10, 2024
    Purpose     : To provide a sample POC prototype class for creating dynamic classes dll file using
                  System.Reflection.Emit PersistedAssemblyBuilder.
  ====================================================================================================*/
using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models;
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using static Blazor.Tools.BlazorBundler.Utilities.Assemblies.AssemblyGenerator;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class CreateDLLFromDataTable : ICreateDLLFromDataTable
    {
        // Existing fields and _modelProperties
        private string _dllPath = string.Empty;
        private string _employeeNameSpace;
        private string _employeeTypeName;
        private string _iEmployeeNameSpace;
        private string _iEmployeeTypeName;
        private string _contextAssemblyName;
        private string _employeeFullyQualifiedTypeName;
        private string _iEmployeeFullyQualifiedTypeName;
        private string _iModelExtendedPropertiesFullyQualifiedTypeName;
        private string _employeeVMNameSpace;
        private string _employeeVMTypeName;
        private string _employeeVMFullyQualifiedTypeName;
        private List<(ConstructorBuilder ConstructorBuilder, Type[] ParameterTypes)>? _constructors = default!;
        private Type _iEmployeeType = default!;
        private Type _employeeType = default!;
        private Type _iModelExtendedPropertiesType = default!;
        private Type _iViewModelType = default!;
        private Type _vmType = default!;
        private List<PropertyDefinition>? _modelProperties;
        private List<PropertyDefinition>? _iModelExtendedProperties;
        private List<(string TypeName, PropertyBuilder PropertyBuilder)>? _addedProperties;

        public string ContextAssemblyName
        {
            get { return _contextAssemblyName; }
        }

        public string DLLPath
        {
            get { return _dllPath; }
        }

        public CreateDLLFromDataTable(string? employeeNameSpace = null, string? employeeTypeName = null, string? iEmployeeNameSpace = null, string? iEmployeeTypeName = null)
        {
            _employeeNameSpace = employeeNameSpace ?? "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models";
            _employeeTypeName = employeeTypeName ?? "Employee";
            _iEmployeeNameSpace = iEmployeeNameSpace ?? "Blazor.Tools.BlazorBundler.Interfaces";
            _iEmployeeTypeName = iEmployeeTypeName ?? "IEmployee";
            
            _employeeFullyQualifiedTypeName = $"{_employeeNameSpace}.{_employeeTypeName}";
            _iEmployeeFullyQualifiedTypeName = $"{_iEmployeeNameSpace}.{_iEmployeeTypeName}";
            _iModelExtendedPropertiesFullyQualifiedTypeName = $"{_iEmployeeNameSpace}.IModelExtendedProperties";

            var lastIndex = _employeeNameSpace.LastIndexOf('.');
            _contextAssemblyName = _employeeNameSpace[..lastIndex];
            _dllPath = Path.Combine(Path.GetTempPath(), _contextAssemblyName + ".dll"); // use the parent assembly name that envelops class and interface namespaces

            _employeeVMNameSpace = _employeeNameSpace.Replace("Models", "ViewModels");
            _employeeVMTypeName = _employeeTypeName + "VM";
            _employeeVMFullyQualifiedTypeName = $"{_employeeVMNameSpace}.{_employeeVMTypeName}";
            _addedProperties = new List<(string TypeName, PropertyBuilder PropertyBuilder)>();
            _constructors = new List<(ConstructorBuilder ConstructorBuilder, Type[] ParameterTypes)>();
        }

        public void BuildAndSaveAssembly(DataTable dataTable)
        {
            try
            {
                var assemblyName = new AssemblyName(_contextAssemblyName);
                assemblyName.Version = new Version("1.0.0.0");
                var persistedAssemblyBuilder = new PersistedAssemblyBuilder(assemblyName, typeof(object).Assembly);
                var moduleBuilder = persistedAssemblyBuilder.DefineDynamicModule(_employeeNameSpace);
                var propertyInfoList = dataTable.GetProperties(); // Get the PropertyInfo collection

                _modelProperties = propertyInfoList?.Select(p => new PropertyDefinition
                {
                    Name = p.Name,
                    Type = p.PropertyType
                }).ToList() ?? default!;
                
                _iModelExtendedProperties = typeof(IModelExtendedProperties).GetProperties()?.Select(p => new PropertyDefinition
                {
                    Name = p.Name,
                    Type = p.PropertyType
                }).ToList() ?? default!;

                _iEmployeeType = CreateType(
                    ref moduleBuilder,
                    typeName: _iEmployeeFullyQualifiedTypeName,
                    typeAttributes: TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract,
                    properties: _modelProperties
                    );

                _iEmployeeType.GetProperties();
                _employeeType = CreateType(
                    ref moduleBuilder,
                    typeName: _employeeFullyQualifiedTypeName,
                    typeAttributes: TypeAttributes.Public | TypeAttributes.Class,
                    properties: _modelProperties
                    );

                _iModelExtendedPropertiesType = CreateType(
                    ref moduleBuilder,
                    typeName: $"{_iEmployeeNameSpace}.IModelExtendedProperties",
                    typeAttributes: TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract,
                    properties: _iModelExtendedProperties
                    );

                
                _iViewModelType = CreateType(
                    ref moduleBuilder,
                    typeName: $"{_iEmployeeNameSpace}.IViewModel",
                    typeAttributes: TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract,
                    interfaces: new Type[] { _iModelExtendedPropertiesType },
                    genericParameterNames: new[] { "TModel", "TIModel" },  // Define <TModel, TIModel>
                    methodDefinitions: null!, //iViewModelMethods,
                    defineMethodsAction: tb => DefineViewModelMethods(tb)
                    );

                _iViewModelType.DisplayTypeDifferences(typeof(IViewModel<Employee, IModelExtendedProperties>));
                // Known generic type for testing
                var knownType = typeof(List<string>);

                // Find Remove method
                var methodInfo = knownType.GetMethod("Remove", BindingFlags.Instance | BindingFlags.Public, Type.DefaultBinder, new[] { typeof(string) }, null);

                if (methodInfo != null)
                {
                    Console.WriteLine("Remove method found.");
                }
                else
                {
                    Console.WriteLine("Remove method not found.");
                }

                var genericType = typeof(List<>).MakeGenericType(_iViewModelType);
                var removeMethod = genericType.GetMethod("Remove", BindingFlags.Instance | BindingFlags.Public, Type.DefaultBinder, new[] { _iViewModelType }, null) ?? default!;

                _vmType = CreateType(
                    ref moduleBuilder,
                    typeName: _employeeVMFullyQualifiedTypeName,
                    typeAttributes: TypeAttributes.Public | TypeAttributes.Class,
                    baseType: _employeeType,
                    interfaces: new Type[] { _iViewModelType, _iModelExtendedPropertiesType },
                    properties: _modelProperties,
                    defineConstructorsAction: tb => DefineConstructors(tb),
                    defineMethodsAction: tb => DefineMethods(tb, _employeeType, _iModelExtendedPropertiesType)
                    );
                 
                // Save the assembly to a file
                persistedAssemblyBuilder.Save(_dllPath);

                Console.WriteLine("DisposableAssembly saved to {0} successfully!", _dllPath);
            }
            catch (Exception ex)
            {
                AppLogger.HandleException(ex);
            }
        }

        // Method to define the iViewModelMethods of IViewModel<TModel, TIModel> interface
        private TypeBuilder DefineViewModelMethods(TypeBuilder typeBuilder)
        {
            // Define the return type and method signature for each method

            // Method: TModel ToNewModel()
            typeBuilder = DefineMethod(typeBuilder, "ToNewModel", typeof(object), Type.EmptyTypes);

            // Method: TIModel ToNewIModel()
            typeBuilder = DefineMethod(typeBuilder, "ToNewIModel", typeof(object), Type.EmptyTypes);

            // Method: Task<IViewModel<TModel, TIModel>> FromModel(TModel model)
            typeBuilder = DefineMethod(typeBuilder, "FromModel", typeof(Task<>).MakeGenericType(typeBuilder), new[] { typeof(object) });

            // Method: Task<IViewModel<TModel, TIModel>> SetEditMode(bool isEditMode)
            typeBuilder = DefineMethod(typeBuilder, "SetEditMode", typeof(Task<>).MakeGenericType(typeBuilder), new[] { typeof(bool) });

            // Method: Task<IViewModel<TModel, TIModel>> SaveModelVM()
            typeBuilder = DefineMethod(typeBuilder, "SaveModelVM", typeof(Task<>).MakeGenericType(typeBuilder), Type.EmptyTypes);

            // Method: Task<IViewModel<TModel, TIModel>> SaveModelVMToNewModelVM()
            typeBuilder = DefineMethod(typeBuilder, "SaveModelVMToNewModelVM", typeof(Task<>).MakeGenericType(typeBuilder), Type.EmptyTypes);

            // Method: Task<IEnumerable<IViewModel<TModel, TIModel>>> AddItemToList(IEnumerable<IViewModel<TModel, TIModel>> modelVMList)
            typeBuilder = DefineMethod(typeBuilder, "AddItemToList", typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(typeBuilder)), new[] { typeof(IEnumerable<>).MakeGenericType(typeBuilder) });

            // Method: Task<IEnumerable<IViewModel<TModel, TIModel>>> UpdateList(IEnumerable<IViewModel<TModel, TIModel>> modelVMList, bool isAdding)
            typeBuilder = DefineMethod(typeBuilder, "UpdateList", typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(typeBuilder)), new[] { typeof(IEnumerable<>).MakeGenericType(typeBuilder), typeof(bool) });

            // Method: Task<IEnumerable<IViewModel<TModel, TIModel>>> DeleteItemFromList(IEnumerable<IViewModel<TModel, TIModel>> modelVMList)
            typeBuilder = DefineMethod(typeBuilder, "DeleteItemFromList", typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(typeBuilder)), new[] { typeof(IEnumerable<>).MakeGenericType(typeBuilder) });

            return typeBuilder;
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

        private Type CreateType(
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
                                    Func<TypeBuilder, TypeBuilder> defineMethodsAction = default!
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

                type = typeBuilder.CreateType();
            }
            catch (Exception ex)
            {
                AppLogger.HandleException(ex);
                throw; // propagate
            }

            return type;
        }


        public TypeBuilder DefineConstructors(TypeBuilder typeBuilder)
        {
            // Define the field for the contextProvider only once
            FieldBuilder contextProviderField = typeBuilder.DefineField("contextProvider", typeof(IContextProvider), FieldAttributes.Private);
            // Define constructors
            // Parameterless constructor
            typeBuilder = DefineConstructor(typeBuilder, Type.EmptyTypes, ilg =>
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
            typeBuilder = DefineConstructor(typeBuilder, new[] { typeof(IContextProvider) }, ilg =>
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
            typeBuilder = DefineConstructor(typeBuilder, new[] { typeof(IContextProvider), _employeeType }, ilg =>
            {
                ConstructorInfo baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Call, baseConstructor);

                // Set contextProvider field
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ldarg_1);
                ilg.Emit(OpCodes.Stfld, contextProviderField);

                // Set _modelProperties from Employee model
                ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                ilg.Emit(OpCodes.Ldarg_2); // Load Employee model

                if (_addedProperties != null)
                {
                    foreach ((string typeName, PropertyBuilder prop) in _addedProperties)
                    {
                        if(_employeeFullyQualifiedTypeName == typeName)
                        {
                            if (prop.CanWrite)
                            {
                                ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                                ilg.Emit(OpCodes.Ldarg_2); // Load Employee model
                                ilg.Emit(OpCodes.Callvirt, prop.GetGetMethod()!); // Get property value
                                ilg.Emit(OpCodes.Callvirt, prop.GetSetMethod()!); // Set property value
                            }

                        }
                        
                    }
                }
                
                ilg.Emit(OpCodes.Ret);

                Console.WriteLine("Constructor with IContextProvider and Employee parameter defined.");
            });

            // Constructor with IContextProvider and EmployeeVM parameter
            typeBuilder = DefineConstructor(typeBuilder, new[] { typeof(IContextProvider), typeBuilder }, ilg =>
            {
                ConstructorInfo baseConstructor = typeof(object).GetConstructor(Type.EmptyTypes)!;
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Call, baseConstructor);

                // Set contextProvider field
                ilg.Emit(OpCodes.Ldarg_0);
                ilg.Emit(OpCodes.Ldarg_1);
                ilg.Emit(OpCodes.Stfld, contextProviderField);

                // Set _modelProperties from EmployeeVM modelVM
                ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                ilg.Emit(OpCodes.Ldarg_2); // Load EmployeeVM modelVM

                if (_addedProperties != null)
                {
                    foreach ((string typeName, PropertyBuilder prop) in _addedProperties)
                    {
                        if (typeName == _employeeFullyQualifiedTypeName && prop.CanWrite)
                        {
                            ilg.Emit(OpCodes.Ldarg_0); // Load "this"
                            ilg.Emit(OpCodes.Ldarg_2); // Load EmployeeVM modelVM
                            ilg.Emit(OpCodes.Callvirt, prop.GetGetMethod()!); // Get property value
                            ilg.Emit(OpCodes.Callvirt, prop.GetSetMethod()!); // Set property value
                        }
                    }
                }

                ilg.Emit(OpCodes.Ret);

                Console.WriteLine("Constructor with IContextProvider and EmployeeVM parameter defined.");
            });

            //// Verify all constructors
            //var _vmType = modelVMClassBuilder.CreateType(); // Create the final VM class

            //// Log all constructors to ensure they are defined
            //var constructors = _vmType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //foreach (var ctor in constructors)
            //{
            //    var parameters = ctor.GetParameters();
            //    Console.WriteLine($"Constructor: {ctor.Name}, Parameters: {string.Join(", ", parameters.Select(p => p.ParameterType.Name))}");
            //}

            return typeBuilder;
        }

        public IReadOnlyList<(ConstructorBuilder ConstructorBuilder, Type[] ParameterTypes)> GetDefinedConstructors()
        {
            return _constructors?.AsReadOnly() ?? default!;
        }

        public TypeBuilder DefineMethods(TypeBuilder typeBuilder, Type tModelType, Type tiModelType)
        {
            // Define methodDefinitions dynamically
            typeBuilder = DefineMethod(typeBuilder, "ToNewModel", tModelType, Type.EmptyTypes, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Check for parameterless constructor
                ConstructorInfo constructor = tModelType.GetConstructor(Type.EmptyTypes)
                    ?? throw new InvalidOperationException($"No parameterless constructor found for type {tModelType.Name}");

                // Emit IL code to call the constructor
                ilg.Emit(OpCodes.Newobj, constructor);
                ilg.Emit(OpCodes.Ret);
            });

            typeBuilder = DefineMethod(typeBuilder, "ToNewIModel", tiModelType, Type.EmptyTypes, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Get the constructor of tModelType
                ConstructorInfo? constructor = _employeeType.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                {
                    throw new InvalidOperationException($"No parameterless constructor found for type {_employeeType.Name}");
                }

                // Emit IL to create a new instance of tModelType
                ilg.Emit(OpCodes.Newobj, constructor); // Create new instance of tModelType
                LocalBuilder localBuilderInstance = ilg.DeclareLocal(_employeeType); // Declare a local variable for the instance
                ilg.Emit(OpCodes.Stloc, localBuilderInstance); // Store the instance in the local variable

                // Get _modelProperties of tModelType
                if (_addedProperties != null)
                {
                    foreach ((string typeName, PropertyBuilder prop) in _addedProperties)
                    {
                        if (typeName == _employeeFullyQualifiedTypeName && prop.CanWrite)
                        {
                            // Load the instance and the value to set
                            ilg.Emit(OpCodes.Ldloc, localBuilderInstance); // Load instance
                            ilg.Emit(OpCodes.Ldarg_0); // Load 'this'

                            // Load the value of the property from 'this'
                            if (prop != null)
                            {
                                ilg.Emit(OpCodes.Callvirt, prop?.GetGetMethod() ?? default!); // Call property getter
                                ilg.Emit(OpCodes.Callvirt, prop?.GetSetMethod() ?? default!); // Call property setter
                            }
                        }
                    }

                    // Return the new instance
                    ilg.Emit(OpCodes.Ldloc, localBuilderInstance);
                    ilg.Emit(OpCodes.Ret);
                }
            });

            // Define the method in the dynamic class builder
            typeBuilder = DefineMethod(typeBuilder, "FromModel", typeof(Task<>).MakeGenericType(_iViewModelType), new[] { tModelType }, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Load 'this' onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_0);

                // Load the model argument onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_1);

                // Get the 'FromModel' method from the IViewModel<TModel, TIModel> interface
                var method = _iViewModelType.GetMethod("FromModel"); 

                if (method == null)
                {
                    throw new InvalidOperationException($"No method found with name 'FromModel' in type {typeBuilder.Name}");
                }

                // Call the 'FromModel' method
                ilg.Emit(OpCodes.Callvirt, method);

                // Convert the result to Task<IViewModel<TModel, TIModel>>
                ilg.Emit(OpCodes.Castclass, typeof(Task<>).MakeGenericType(_iViewModelType));

                // Return the result
                ilg.Emit(OpCodes.Ret);
            });

            typeBuilder = DefineMethod(typeBuilder, "SetEditMode", typeof(Task<>).MakeGenericType(_iViewModelType), new[] { typeof(bool) }, new[] { "isEditMode" }, (ilg, localBuilder) =>
            {
                // Load 'this' onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_0);

                // Load the isEditMode argument onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_1);

                // Set the IsEditMode property
                var isEditModeItem = _addedProperties?.FirstOrDefault(p => p.PropertyBuilder.Name.Contains("IsEditMode"));
                var isEditModeProperty = isEditModeItem?.PropertyBuilder;
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

            typeBuilder = DefineMethod(typeBuilder, "SaveModelVM", typeof(Task<>).MakeGenericType(_iViewModelType), Type.EmptyTypes, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Load 'this' onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_0);

                // Set the IsEditMode property to false
                var isEditModeItem = _addedProperties?.FirstOrDefault(p => p.PropertyBuilder.Name.Contains("IsEditMode"));
                var isEditModeProperty = isEditModeItem?.PropertyBuilder;
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

            typeBuilder = DefineMethod(typeBuilder, "SaveModelVMToNewModelVM", typeof(Task<>).MakeGenericType(_iViewModelType), Type.EmptyTypes, Array.Empty<string>(), (ilg, localBuilder) =>
            {
                // Load 'this' onto the evaluation stack
                ilg.Emit(OpCodes.Ldarg_0);

                // Get all defined constructors
                var constructors = GetDefinedConstructors();

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

                // Load _modelProperties from 'this' and set them on the new instance
                if (_addedProperties != null)
                {
                    foreach ((string typeName, PropertyBuilder prop) in _addedProperties)
                    {
                        if (new[]{ _employeeFullyQualifiedTypeName, _iModelExtendedPropertiesFullyQualifiedTypeName}.Contains(typeName) && prop.CanWrite)
                        {
                            // Load the new instance
                            ilg.Emit(OpCodes.Ldloc, newInstance);

                            // Load property value from 'this'
                            ilg.Emit(OpCodes.Ldarg_0);
                            ilg.Emit(OpCodes.Callvirt, prop?.GetGetMethod() ?? default!);

                            // Set the property on the new instance
                            ilg.Emit(OpCodes.Callvirt, prop?.GetSetMethod() ?? default!);
                        }
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
                }
            });

            typeBuilder = DefineMethod(typeBuilder, "AddItemToList", typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(_iViewModelType)), new[] { typeof(IEnumerable<>).MakeGenericType(_iViewModelType) }, new[] { "modelVMList" }, (ilg, localBuilder) =>
            {
                 // Load the modelVMList argument
                 ilg.Emit(OpCodes.Ldarg_1);

                // Create a list from the argument
                var toListMethod = typeof(Enumerable).GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(_iViewModelType) ?? default!;
                 ilg.Emit(OpCodes.Call, toListMethod);

                 // Add 'this' to the list
                 ilg.Emit(OpCodes.Ldloc_0);
                 ilg.Emit(OpCodes.Ldarg_0);
                var method = _iViewModelType.GetMethod("FromModel");

                if (method == null)
                {
                    throw new InvalidOperationException($"No method found with name 'FromModel' in type {typeBuilder.Name}");
                }
                ilg.Emit(OpCodes.Callvirt, method);

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

            typeBuilder = DefineMethod(typeBuilder, "UpdateList", typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(_iViewModelType)), new[] { typeof(IEnumerable<>).MakeGenericType(_iViewModelType), typeof(bool) }, new[] { "modelVMList", "isAdding" }, (ilg, localBuilder) =>
            {
                 // Convert modelVMList to List
                 ilg.Emit(OpCodes.Ldarg_1);
                 var toListMethod = typeof(Enumerable).GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(_iViewModelType) ?? default!;
                 ilg.Emit(OpCodes.Call, toListMethod);

                 // Store the list in a local variable
                 var listLocal = ilg.DeclareLocal(typeof(List<>).MakeGenericType(_iViewModelType));
                 ilg.Emit(OpCodes.Stloc, listLocal);

                 // If isAdding, remove 'this' from the list
                 var addLabel = ilg.DefineLabel();
                 ilg.Emit(OpCodes.Ldarg_2);  // Load isAdding argument
                 ilg.Emit(OpCodes.Brtrue_S, addLabel); // If true, jump to adding logic

                 // Remove 'this' from the list
                 ilg.Emit(OpCodes.Ldloc, listLocal);
                 ilg.Emit(OpCodes.Ldarg_0); // Load 'this'
                var genericType = typeof(List<>).MakeGenericType(_iViewModelType);
                 var removeMethod = genericType.GetMethod("Remove", new[] {_iViewModelType}) ?? default!;
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

            typeBuilder = DefineMethod(typeBuilder, "DeleteItemFromList", typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(_iViewModelType)), new[] { typeof(IEnumerable<>).MakeGenericType(_iViewModelType) }, new[] { "modelVMList" }, (ilg, localBuilder) =>
            {
                // Load the modelVMList argument
                ilg.Emit(OpCodes.Ldarg_1);

                // Convert to list
                var toListMethod = typeof(Enumerable).GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(_iViewModelType) ?? default!;
                ilg.Emit(OpCodes.Call, toListMethod);

                // Remove 'this' from the list
                ilg.Emit(OpCodes.Ldloc_0);
                ilg.Emit(OpCodes.Ldarg_0);
                var removeMethod = typeof(List<>).MakeGenericType(_iViewModelType).GetMethod("Remove") ?? default!;
                ilg.Emit(OpCodes.Callvirt, removeMethod);

                // Emit async completion
                var completedTaskProperty = typeof(Task).GetProperty(nameof(Task.CompletedTask));
                if (completedTaskProperty == null)
                {
                    throw new InvalidOperationException("CompletedTask property not found.");
                }

                ilg.Emit(OpCodes.Call, completedTaskProperty.GetGetMethod() ?? default!);
                ilg.Emit(OpCodes.Ldloc_0);
                ilg.Emit(OpCodes.Ret);
            });

            return typeBuilder;
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

    }

}
