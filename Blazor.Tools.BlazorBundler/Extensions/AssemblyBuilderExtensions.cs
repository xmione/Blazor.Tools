using Blazor.Tools.BlazorBundler.Entities.SampleObjects;
using Blazor.Tools.BlazorBundler.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.ComponentModel.DataAnnotations;

namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class AssemblyBuilderExtensions
    {
        /// <summary>
        /// Extension method to create a dynamic assembly with a specific class and save it to a file.
        /// </summary>
        /// <param name="assemblyName">Name of the dynamic assembly.</param>
        /// <param name="nameSpace">Namespace name.</param>
        /// <param name="className">Class name.</param>
        /// <param name="dllPath">Path where the DLL will be saved.</param>
        public static void CreateAndSaveDynamicAssembly(this string assemblyName, string nameSpace, string className, string dllPath)
        {
            // Create a new assembly definition
            var assemblyNameDefinition = new AssemblyNameDefinition(assemblyName, new Version("1.0.0"));
            var assemblyDefinition = AssemblyDefinition.CreateAssembly(
                assemblyNameDefinition,
                "MainModule",
                ModuleKind.Dll);

            var moduleDefinition = assemblyDefinition.MainModule;

            // Import necessary references
            var taskTypeRef = moduleDefinition.ImportReference(typeof(Task));
            var iViewModelRef = moduleDefinition.ImportReference(typeof(IViewModel<,>));
            var iValidatableObjectRef = moduleDefinition.ImportReference(typeof(IValidatableObject));
            var iCloneableRef = moduleDefinition.ImportReference(typeof(ICloneable<>));
            var employeeTypeRef = moduleDefinition.ImportReference(typeof(Employee));

            // Create the generic instance of IViewModel<Employee, IModelExtendedProperties>
            var iViewModelGeneric = new GenericInstanceType(iViewModelRef);
            iViewModelGeneric.GenericArguments.Add(employeeTypeRef);
            iViewModelGeneric.GenericArguments.Add(moduleDefinition.ImportReference(typeof(IModelExtendedProperties)));

            // Create the generic instance of Task<IViewModel<Employee, IModelExtendedProperties>>
            var taskOfIViewModelType = new GenericInstanceType(taskTypeRef);
            taskOfIViewModelType.GenericArguments.Add(iViewModelGeneric);

            // Define the EmployeeVM class
            var typeDefinition = new TypeDefinition(
                nameSpace,
                className,
                TypeAttributes.Public | TypeAttributes.Class,
                employeeTypeRef);

            // Implement the interfaces: IValidatableObject, ICloneable<EmployeeVM>, IViewModel<Employee, IModelExtendedProperties>
            typeDefinition.Interfaces.Add(new InterfaceImplementation(iValidatableObjectRef));
            typeDefinition.Interfaces.Add(new InterfaceImplementation(iCloneableRef.MakeGenericInstanceType(typeDefinition)));
            typeDefinition.Interfaces.Add(new InterfaceImplementation(iViewModelGeneric));

            moduleDefinition.Types.Add(typeDefinition);

            // Define a field for IsEditMode
            var isEditModeField = new FieldDefinition(
                "IsEditMode",
                FieldAttributes.Private,
                moduleDefinition.TypeSystem.Boolean);
            typeDefinition.Fields.Add(isEditModeField);

            // Define the SetEditMode method
            var setEditModeMethod = new MethodDefinition(
                "SetEditMode",
                MethodAttributes.Public,
                taskOfIViewModelType);

            var isEditModeParameter = new ParameterDefinition("isEditMode", ParameterAttributes.None, moduleDefinition.TypeSystem.Boolean);
            setEditModeMethod.Parameters.Add(isEditModeParameter);

            var ilProcessor = setEditModeMethod.Body.GetILProcessor();

            // Set the IsEditMode field
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // this
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_1)); // isEditMode
            ilProcessor.Append(ilProcessor.Create(OpCodes.Stfld, isEditModeField)); // this.IsEditMode = isEditMode;

            // Return Task.CompletedTask
            var completedTaskGetMethod = moduleDefinition.ImportReference(
                typeof(Task)
                .GetProperty("CompletedTask", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                ?.GetGetMethod());

            if (completedTaskGetMethod == null)
            {
                throw new Exception("Failed to import the CompletedTask getter method.");
            }

            ilProcessor.Append(ilProcessor.Create(OpCodes.Call, completedTaskGetMethod)); // return Task.CompletedTask
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));

            typeDefinition.Methods.Add(setEditModeMethod);

            // Save the assembly to disk
            assemblyDefinition.Write(dllPath);

            Console.WriteLine("Assembly created and saved as {0}", dllPath);
        }

        public static AssemblyDefinition CreateAssemblyDefinition(this string assemblyName)
        {
            var assemblyNameDefinition = new AssemblyNameDefinition(assemblyName, new Version("1.0.0"));
            return AssemblyDefinition.CreateAssembly(
                assemblyNameDefinition,
                "MainModule",
                ModuleKind.Dll);
        }

        public static TypeDefinition CreateTypeDefinition(this string nameSpace, string className, TypeReference baseTypeRef)
        {
            return new TypeDefinition(
                nameSpace,
                className,
                TypeAttributes.Public | TypeAttributes.Class,
                baseTypeRef);
        }

        public static void AddBaseClass(this TypeDefinition typeDefinition, TypeReference baseTypeRef)
        {
            typeDefinition.BaseType = baseTypeRef;
        }

        public static void DeriveFromInterfaces(this TypeDefinition typeDefinition, params TypeReference[] interfaceRefs)
        {
            foreach (var interfaceRef in interfaceRefs)
            {
                typeDefinition.Interfaces.Add(new InterfaceImplementation(interfaceRef));
            }
        }

        public static void AddField(this TypeDefinition typeDefinition, string fieldName, TypeReference fieldType, bool isReadOnly = false)
        {
            // Define the attributes for the field
            var attributes = FieldAttributes.Private;
            if (isReadOnly)
            {
                attributes |= FieldAttributes.InitOnly;
            }

            // Create the field definition with the specified attributes
            var fieldDefinition = new FieldDefinition(
                fieldName,
                attributes,
                fieldType);

            // Add the field to the type definition
            typeDefinition.Fields.Add(fieldDefinition);
        }

        public static void AddFieldWithInitializer(this TypeDefinition typeDefinition, string fieldName, Type type, TypeReference fieldType, ModuleDefinition moduleDefinition, bool isReadOnly = false)
        {
            // Define the attributes for the field
            var attributes = FieldAttributes.Private;
            if (isReadOnly)
            {
                attributes |= FieldAttributes.InitOnly;
            }

            var fieldDefinition = new FieldDefinition(fieldName, attributes, fieldType);
            typeDefinition.Fields.Add(fieldDefinition);

            var constructor = typeDefinition.Methods.FirstOrDefault(m => m.IsConstructor && !m.IsStatic);
            if (constructor == null)
            {
                constructor = new MethodDefinition(
                    ".ctor",
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    moduleDefinition.TypeSystem.Void);

                typeDefinition.Methods.Add(constructor);
            }

            var ilProcessor = constructor.Body.GetILProcessor();
            var firstInstruction = constructor.Body.Instructions.FirstOrDefault();

            if (firstInstruction == null)
            {
                ilProcessor.Append(ilProcessor.Create(OpCodes.Nop));
                firstInstruction = constructor.Body.Instructions.First();
            }

            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldarg_0));
            // Handle concrete types with constructor
            if (type.IsClass)
            {
                var constructorInfo = type.GetConstructor(Type.EmptyTypes);
                if (constructorInfo == null)
                {
                    throw new InvalidOperationException($"The type {type.FullName} does not have a parameterless constructor.");
                }

                ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Newobj, moduleDefinition.ImportReference(constructorInfo)));
            }
            else
            {
                // Interface types don't have constructors
                ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldnull));
            }

            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Stfld, fieldDefinition));

            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ret));
        }

        public static void AddProperty(this TypeDefinition typeDefinition, ModuleDefinition moduleDefinition, string propertyName, TypeReference propertyType, FieldAttributes fieldAttributes = FieldAttributes.Public)
        {
            // Ensure the module definition is not null
            if (moduleDefinition == null)
                throw new ArgumentNullException(nameof(moduleDefinition));

            // Create the field if it doesn't exist
            var fieldName = $"_{char.ToLower(propertyName[0])}{propertyName.Substring(1)}"; // Private field convention
            var field = typeDefinition.Fields.FirstOrDefault(f => f.Name == fieldName);
            if (field == null)
            {
                field = new FieldDefinition(fieldName, fieldAttributes, propertyType);
                typeDefinition.Fields.Add(field);
            }

            // Create the property definition
            var propertyDefinition = new PropertyDefinition(
                propertyName,
                PropertyAttributes.None,
                propertyType);

            // Create getter method
            var getMethod = new MethodDefinition(
                "get_" + propertyName,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                propertyType);

            // Create setter method
            var setMethod = new MethodDefinition(
                "set_" + propertyName,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                moduleDefinition.TypeSystem.Void);

            // Define the parameter for the setter method
            var valueParameter = new ParameterDefinition("value", ParameterAttributes.None, propertyType);
            setMethod.Parameters.Add(valueParameter);


            // Define the body of the getter
            var getIlProcessor = getMethod.Body.GetILProcessor();
            getIlProcessor.Append(getIlProcessor.Create(OpCodes.Ldarg_0)); // Load 'this'
            getIlProcessor.Append(getIlProcessor.Create(OpCodes.Ldfld, field)); // Load field value
            getIlProcessor.Append(getIlProcessor.Create(OpCodes.Ret)); // Return field value

            // Define the body of the setter
            var setIlProcessor = setMethod.Body.GetILProcessor();
            setIlProcessor.Append(setIlProcessor.Create(OpCodes.Ldarg_0)); // Load 'this'
            setIlProcessor.Append(setIlProcessor.Create(OpCodes.Ldarg_1)); // Load 'value' (argument 1)
            setIlProcessor.Append(setIlProcessor.Create(OpCodes.Stfld, field)); // Store value in field
            setIlProcessor.Append(setIlProcessor.Create(OpCodes.Ret)); // Return

            // Add methods to the property
            propertyDefinition.GetMethod = getMethod;
            propertyDefinition.SetMethod = setMethod;

            // Add methods to the type definition
            typeDefinition.Methods.Add(getMethod);
            typeDefinition.Methods.Add(setMethod);

            // Add property to the type definition
            typeDefinition.Properties.Add(propertyDefinition);
        }

        public static void AddMethod(this TypeDefinition typeDefinition, string methodName, MethodAttributes methodAttributes, TypeReference returnType, Action<MethodDefinition> configureMethod)
        {
            var methodDefinition = new MethodDefinition(methodName, methodAttributes, returnType);
            configureMethod(methodDefinition);
            typeDefinition.Methods.Add(methodDefinition);
        }

        public static void AddConstructor(this TypeDefinition typeDefinition, ModuleDefinition moduleDefinition)
        {
            // Remove all existing constructors
            foreach (var method in typeDefinition.Methods.ToList())
            {
                if (method.IsConstructor)
                {
                    typeDefinition.Methods.Remove(method);
                }
            }

            // Create a parameterless constructor
            var parameterlessConstructor = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                moduleDefinition.TypeSystem.Void);

            var ilProcessor = parameterlessConstructor.Body.GetILProcessor();
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load 'this'

            // Call base class constructor (object)
            ilProcessor.Append(ilProcessor.Create(OpCodes.Call, moduleDefinition.ImportReference(typeof(object).GetConstructor(Type.EmptyTypes))));

            // Initialize fields
            foreach (var field in typeDefinition.Fields)
            {
                var fieldType = field.FieldType;
                var fieldTypeDefinition = fieldType.Resolve(); // Resolve to TypeDefinition

                if (fieldTypeDefinition != null)
                {
                    if (fieldTypeDefinition.IsClass && !fieldTypeDefinition.IsAbstract)
                    {
                        // Check if the field type has a parameterless constructor
                        var parameterlessConstructorDefinition = fieldTypeDefinition.GetConstructors()
                            .FirstOrDefault(c => c.Parameters.Count == 0);

                        if (fieldType.IsValueType)
                        {
                            // Initialize value types to their default values
                            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load 'this'

                            if (fieldType == moduleDefinition.TypeSystem.Boolean)
                            {
                                // Default value for bool (false)
                                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4_0)); // Push 0 (false)
                            }
                            else if (fieldType == moduleDefinition.TypeSystem.Int32)
                            {
                                // Default value for int (0)
                                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4_0)); // Push 0
                            }
                            // Add cases for other value types if needed...

                            ilProcessor.Append(ilProcessor.Create(OpCodes.Stfld, field)); // Store value in field
                        }
                        else if (parameterlessConstructorDefinition != null)
                        {
                            // Handle reference types with parameterless constructors
                            //ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load 'this'
                            //ilProcessor.Append(ilProcessor.Create(OpCodes.Newobj, moduleDefinition.ImportReference(parameterlessConstructorDefinition)));
                            //ilProcessor.Append(ilProcessor.Create(OpCodes.Stfld, field)); // Store in field

                            //if (type.IsGenericType)
                            //{
                            //    // Get the generic type definition name (e.g., List`1)
                            //    var genericTypeName = type.GetGenericTypeDefinition().Name;

                            //    // Search for the generic type definition in the external assembly
                            //    var genericTypeDefinition = externalAssembly.MainModule.Types
                            //        .FirstOrDefault(t => t.Name == genericTypeName);

                            //    if (genericTypeDefinition != null)
                            //    {
                            //        // Resolve the actual types of the generic arguments in the context of the module
                            //        var resolvedGenericArguments = type.GetGenericArguments()
                            //            .Select(arg => moduleDefinition.ImportReference(arg).Resolve())
                            //            .ToList();

                            //        // Compare the resolved types with the generic parameters of the found type definition
                            //        bool matchFound = true;
                            //        for (int i = 0; i < resolvedGenericArguments.Count; i++)
                            //        {
                            //            var genericArgument = resolvedGenericArguments[i];
                            //            var expectedType = moduleDefinition.ImportReference(type.GetGenericArguments()[i]).Resolve();

                            //            // Compare the resolved types instead of just names
                            //            if (genericArgument.FullName != expectedType.FullName)
                            //            {
                            //                matchFound = false;
                            //                break;
                            //            }
                            //        }

                            //        Console.WriteLine(matchFound
                            //            ? $"Found the {type.Name} type (generic) in the external assembly."
                            //            : $"{type.Name} type (generic) was not found with matching generic arguments.");
                            //    }
                            //    else
                            //    {
                            //        Console.WriteLine($"{genericTypeName} type definition was not found in the external assembly.");
                            //    }
                            //}
                            if (fieldType.IsGenericInstance)
                            {
                                // Resolve the generic instance type (e.g., List<EmployeeVM>)
                                var genericInstanceType = (GenericInstanceType)fieldType;

                                // Resolve the generic type definition (e.g., List<T>)
                                var genericTypeDef = genericInstanceType.ElementType.Resolve();

                                // Import the generic type with the specific argument (e.g., List<EmployeeVM>)
                                var listTypeRef = moduleDefinition.ImportReference(genericInstanceType);

                                // Get the constructor of List<T> (the parameterless constructor)
                                var listCtor = genericTypeDef.Methods.FirstOrDefault(m => m.IsConstructor && !m.HasParameters);

                                if (listCtor != null)
                                {
                                    // Import the constructor reference to the module
                                    var listCtorRef = moduleDefinition.ImportReference(listCtor);

                                    // Create a new instance of List<EmployeeVM>
                                    var newListInstance = new GenericInstanceType(genericTypeDef);
                                    newListInstance.GenericArguments.Add(genericInstanceType.GenericArguments[0]);

                                    var newListCtor = moduleDefinition.ImportReference(newListInstance.Resolve().GetConstructors().First(c => c.Parameters.Count == 0));

                                    ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load 'this'
                                    ilProcessor.Append(ilProcessor.Create(OpCodes.Newobj, newListCtor)); // Call the constructor for List<EmployeeVM>

                                    // Store the newly created instance in the field
                                    ilProcessor.Append(ilProcessor.Create(OpCodes.Stfld, field)); // Store in field
                                }
                                else
                                {
                                    // Handle cases where no constructor is found (e.g., abstract or interface types)
                                    ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load 'this'
                                    ilProcessor.Append(ilProcessor.Create(OpCodes.Ldnull)); // Load null
                                    ilProcessor.Append(ilProcessor.Create(OpCodes.Stfld, field)); // Store null in field
                                }
                            }
                            else
                            {
                                // Handle non-generic types (similar to the existing logic)
                                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load 'this'
                                ilProcessor.Append(ilProcessor.Create(OpCodes.Newobj, moduleDefinition.ImportReference(parameterlessConstructorDefinition))); // Create new instance
                                ilProcessor.Append(ilProcessor.Create(OpCodes.Stfld, field)); // Store in field
                            }

                            //var fieldName = field.Name.Replace("_", "");
                            //fieldName = fieldName.ToPascalCase();

                            //AssemblyDefinition? externalAssembly = null;

                            //// Get the assembly from which the type was imported
                            //externalAssembly = moduleDefinition.AssemblyResolver.Resolve((AssemblyNameReference)fieldType.Scope);
                            //var typeNameList = externalAssembly?.MainModule.Types
                            //                                    .Select(type => type.Name)
                            //                                    .OrderBy(name => name) // Sort the type names alphabetically
                            //                                    .ToList();
                            //// Check the module types
                            //var fieldTypeName = ((TypeSpecification)fieldType).Name;

                            //var employeeListType = externalAssembly?.MainModule.Types.FirstOrDefault(t => t.Name == fieldTypeName);

                            //// Create a new List<Employee>
                            //var listCtor = moduleDefinition.ImportReference(employeeListType?.Resolve().GetConstructors().First(c => c.Parameters.Count == 0));

                            //ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load 'this'
                            //ilProcessor.Append(ilProcessor.Create(OpCodes.Newobj, moduleDefinition.ImportReference(parameterlessConstructorDefinition)));
                            ////ilProcessor.Append(ilProcessor.Create(OpCodes.Stfld, field)); // Store in field

                            ////ilProcessor.Append(ilProcessor.Create(OpCodes.Newobj, listCtor));

                            //// Store the new List<Employee> into the _employees field
                            //ilProcessor.Append(ilProcessor.Create(OpCodes.Stfld, field));

                            // Finish the constructor
                            //ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));


                        }
                        else
                        {
                            // Initialize reference types without a parameterless constructor to null
                            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load 'this'
                            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldnull)); // Load null
                            ilProcessor.Append(ilProcessor.Create(OpCodes.Stfld, field)); // Store in field
                        }


                    }
                    else
                    {
                        // Handle non-class types (e.g., primitive types)
                        if (fieldType.IsValueType)
                        {
                            // Initialize with default value (0 for int, false for bool, etc.)
                            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load 'this'
                            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4_0)); // Default value for integer types
                            ilProcessor.Append(ilProcessor.Create(OpCodes.Stfld, field)); // Store in field
                        }
                        else
                        {
                            var fieldName = field.Name.Replace("_", "");
                            fieldName = fieldName.ToPascalCase();

                            AssemblyDefinition? externalAssembly = null;

                            // Get the assembly from which the type was imported
                            externalAssembly = moduleDefinition.AssemblyResolver.Resolve((AssemblyNameReference)fieldType.Scope);

                            // Check the module types
                            var contextProviderType = externalAssembly?.MainModule.Types.FirstOrDefault(t => t.Name == fieldName);

                            if (contextProviderType != null)
                            {
                                // For interface types or other non-instantiable types, initialize with null
                                var constructors = contextProviderType.Methods.Where(m => m.IsConstructor).ToList();

                                if (constructors.Any())
                                {
                                    foreach (var constructor in constructors)
                                    {
                                        Console.WriteLine($"Constructor found: {constructor}");
                                        // Use the first constructor found
                                        var contextProviderConstructor = moduleDefinition.ImportReference(constructors.First());

                                        // Proceed with the IL generation
                                        var processor = constructor.Body.GetILProcessor();
                                        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0));  // Load 'this'
                                        ilProcessor.Append(ilProcessor.Create(OpCodes.Newobj, contextProviderConstructor));  // New instance
                                        ilProcessor.Append(ilProcessor.Create(OpCodes.Stfld, typeDefinition.Fields.First(f => f.Name == field.Name)));  // Store instance
                                    }


                                }
                                else
                                {
                                    Console.WriteLine("No constructors found for this type.");
                                }
                            }

                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Unable to resolve type {fieldType.FullName}");
                }
            }

            ilProcessor.Append(ilProcessor.Create(OpCodes.Ret)); // Return from constructor

            typeDefinition.Methods.Add(parameterlessConstructor);
        }

        public static string  GetAssemblyLocation(this Type type)
        {
            string location = string.Empty;

            if (type != null)
            {
                //var assembly = type.Assembly;
                var assembly = System.Reflection.Assembly.GetAssembly(type);
                location = assembly.Location;
            }


            return location;
        }
    }
}
