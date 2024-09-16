/*====================================================================================================
    Class Name  : AssemblyGenerator
    Created By  : Solomio S. Sisante
    Created On  : September 7, 2024
    Purpose     : To manage the dynamic creation of dll files with Program DataBase (PDB).
                  It uses PersistedAssembly for assembly creation.
  ====================================================================================================*/

using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class AssemblyGenerator
    {
        private List<Assembly>? _referencedAssemblies;

        public AssemblyGenerator()
        {
            _referencedAssemblies = new List<Assembly>();
        }

        public void DefineReferences(params Assembly[] assemblies)
        {
            _referencedAssemblies?.AddRange(assemblies);
        }

        public void Create(string outputFolder, string assemblyName, List<ModuleDefinition> moduleDefinitions)
        {
            // Define the assembly
            AssemblyName asmName = new AssemblyName(assemblyName);
            PersistedAssemblyBuilder assemblyBuilder = new PersistedAssemblyBuilder(asmName, typeof(object).Assembly);

            // Define each module
            foreach (var moduleDef in moduleDefinitions)
            {
                ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleDef.ModuleName);
                DefineTypesInModule(moduleBuilder, moduleDef.TypeDefinitions);
            }

            // Define the metadata builder
            var metadataBuilder = new MetadataBuilder();
            AddReferencesToMetadata(metadataBuilder);

            // Generate the metadata and write the assembly
            var ilStream = new BlobBuilder();
            var fieldData = new BlobBuilder();
            var pdbBuilder = new BlobBuilder();

            var metadataRootBuilder = new MetadataRootBuilder(metadataBuilder);
            var peHeaderBuilder = new PEHeaderBuilder(imageCharacteristics: Characteristics.Dll);
            var managedPEBuilder = new ManagedPEBuilder(
                peHeaderBuilder,
                metadataRootBuilder,
                ilStream,
                fieldData
            );

            var peBlob = new BlobBuilder();
            managedPEBuilder.Serialize(peBlob);

            var dllPath = Path.Combine(outputFolder, $"{assemblyName}.dll");
            using (var fileStream = new FileStream(dllPath, FileMode.Create, FileAccess.Write))
            {
                peBlob.WriteContentTo(fileStream);
            }

            Console.WriteLine($"Successfully created the assembly file at {dllPath}");
        }


        private void DefineTypesInModule(ModuleBuilder moduleBuilder, List<TypeDefinition> typeDefinitions)
        {
            foreach (var typeDef in typeDefinitions)
            {
                TypeBuilder typeBuilder = moduleBuilder.DefineType(typeDef.TypeName, typeDef.TypeAttributes);

                if (typeDef.BaseType != null)
                {
                    typeBuilder.SetParent(typeDef.BaseType);
                }

                // Define fields
                foreach (var field in typeDef.Fields)
                {
                    typeBuilder.DefineField(field.Name, field.Type, field.Attributes);
                }

                // Define properties
                foreach (var prop in typeDef.Properties)
                {
                    PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(prop.Name, prop.Attributes, prop.Type, null);

                    if (!string.IsNullOrEmpty(prop.Getter))
                    {
                        var getMethodBuilder = DefineMethod(typeBuilder, new MethodDefinition
                        {
                            Name = $"get_{prop.Name}",
                            Attributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                            ReturnType = prop.Type,
                            ParameterTypes = Type.EmptyTypes,
                            Body = prop.Getter
                        });
                        propertyBuilder.SetGetMethod(getMethodBuilder);
                    }

                    if (!string.IsNullOrEmpty(prop.Setter))
                    {
                        var setMethodBuilder = DefineMethod(typeBuilder, new MethodDefinition
                        {
                            Name = $"set_{prop.Name}",
                            Attributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                            ReturnType = typeof(void),
                            ParameterTypes = new Type[] { prop.Type },
                            Body = prop.Setter
                        });
                        propertyBuilder.SetSetMethod(setMethodBuilder);
                    }
                }

                // Define events
                foreach (var ev in typeDef.Events)
                {
                    EventBuilder eventBuilder = typeBuilder.DefineEvent(ev.Name, ev.Attributes, ev.Type);

                    MethodBuilder addMethodBuilder = typeBuilder.DefineMethod($"add_{ev.Name}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, typeof(void), new Type[] { ev.Type });
                    ILGenerator addIl = addMethodBuilder.GetILGenerator();
                    addIl.Emit(OpCodes.Nop);
                    addIl.Emit(OpCodes.Ret);
                    eventBuilder.SetAddOnMethod(addMethodBuilder);
                }

                // Define methods
                foreach (var method in typeDef.Methods)
                {
                    var methodBuilder = DefineMethod(typeBuilder, method);
                    if (!string.IsNullOrEmpty(method.Body)) // Check if Body is a text
                    {
                        var ilGenerator = methodBuilder.GetILGenerator();
                        var ilInstructions = ParseBodyToIL(method.Body, typeBuilder);

                        foreach (var instruction in ilInstructions)
                        {
                            switch (instruction.Operand)
                            {
                                case null:
                                    ilGenerator.Emit(instruction.OpCode);
                                    break;
                                case byte operandByte:
                                    ilGenerator.Emit(instruction.OpCode, operandByte);
                                    break;
                                case sbyte operandSByte:
                                    ilGenerator.Emit(instruction.OpCode, operandSByte);
                                    break;
                                case short operandShort:
                                    ilGenerator.Emit(instruction.OpCode, operandShort);
                                    break;
                                case int operandInt:
                                    ilGenerator.Emit(instruction.OpCode, operandInt);
                                    break;
                                case long operandLong:
                                    ilGenerator.Emit(instruction.OpCode, operandLong);
                                    break;
                                case float operandFloat:
                                    ilGenerator.Emit(instruction.OpCode, operandFloat);
                                    break;
                                case double operandDouble:
                                    ilGenerator.Emit(instruction.OpCode, operandDouble);
                                    break;
                                case string operandString:
                                    ilGenerator.Emit(instruction.OpCode, operandString);
                                    break;
                                case LocalBuilder local:
                                    ilGenerator.Emit(instruction.OpCode, local);
                                    break;
                                case Label label:
                                    ilGenerator.Emit(instruction.OpCode, label);
                                    break;
                                case FieldInfo field:
                                    ilGenerator.Emit(instruction.OpCode, field);
                                    break;
                                case MethodInfo methodInfo:
                                    ilGenerator.Emit(instruction.OpCode, methodInfo);
                                    break;
                                case ConstructorInfo constructor:
                                    ilGenerator.Emit(instruction.OpCode, constructor);
                                    break;
                                case Type type:
                                    ilGenerator.Emit(instruction.OpCode, type);
                                    break;
                                case Label[] labels:
                                    ilGenerator.Emit(instruction.OpCode, labels);
                                    break;
                                default:
                                    throw new InvalidOperationException($"Unsupported operand type: {instruction.Operand.GetType()}");
                            }

                        }
                    }
                }

                // Define constructors
                foreach (var ctor in typeDef.Constructors)
                {
                    var ctorBuilder = typeBuilder.DefineConstructor(ctor.Attributes, ctor.CallingConvention, ctor.ParameterTypes);
                    var ilGenerator = ctorBuilder.GetILGenerator();
                    ilGenerator.Emit(OpCodes.Ret);
                }

                // Create the type
                typeBuilder.CreateType();
            }
        }

        private MethodBuilder DefineMethod(TypeBuilder typeBuilder, MethodDefinition methodDef)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodDef.Name, methodDef.Attributes, methodDef.ReturnType, methodDef.ParameterTypes);
            var ilGenerator = methodBuilder.GetILGenerator();

            if (!string.IsNullOrEmpty(methodDef.Body))
            {
                var ilInstructions = ParseBodyToIL(methodDef.Body, typeBuilder);
                foreach (var instruction in ilInstructions)
                {
                    if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
                    {
                        var methodInfo = instruction.Operand as MethodInfo;
                        if (methodInfo != null)
                        {
                            ilGenerator.Emit(instruction.OpCode, methodInfo);
                        }
                        else
                        {
                            throw new InvalidOperationException("Operand for Call or Callvirt is not a MethodInfo.");
                        }
                    }
                    else if (instruction.OpCode == OpCodes.Newobj)
                    {
                        var constructorInfo = instruction.Operand as ConstructorInfo;
                        if (constructorInfo != null)
                        {
                            ilGenerator.Emit(instruction.OpCode, constructorInfo);
                        }
                        else
                        {
                            throw new InvalidOperationException("Operand for Newobj is not a ConstructorInfo.");
                        }
                    }
                    else if (instruction.OpCode == OpCodes.Ldstr)
                    {
                        var stringValue = instruction.Operand as string;
                        if (stringValue != null)
                        {
                            ilGenerator.Emit(instruction.OpCode, stringValue);
                        }
                        else
                        {
                            throw new InvalidOperationException("Operand for Ldstr is not a string.");
                        }
                    }
                    else
                    {
                        // Handle other opcodes or throw an exception if the opcode is not supported
                        ilGenerator.Emit(instruction.OpCode);
                    }

                }
            }

            ilGenerator.Emit(OpCodes.Ret); // Ensure method ends with a return instruction
            return methodBuilder;
        }

        private void AddReferencesToMetadata(MetadataBuilder metadataBuilder)
        {
            if (_referencedAssemblies != null)
            {
                foreach (var assembly in _referencedAssemblies)
                {
                    // Get assembly name
                    var assemblyName = assembly.GetName().Name;

                    // Add assembly reference
                    var assemblyRef = metadataBuilder.AddAssemblyReference(
                        metadataBuilder.GetOrAddString(assemblyName), // DisposableAssembly name
                        new Version(1, 0, 0, 0), // Version
                        metadataBuilder.GetOrAddString("neutral"), // Culture
                        metadataBuilder.GetOrAddBlob(new byte[0]), // Public key
                        AssemblyFlags.PublicKey, // DisposableAssembly flags (example)
                        metadataBuilder.GetOrAddBlob(new byte[0]) // Additional blob (optional)
                    );
                }
            }
            
        }

        private List<ILInstruction> ParseBodyToIL(string bodyText, TypeBuilder typeBuilder)
        {
            var instructions = new List<ILInstruction>();
            var lines = bodyText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Create the type before accessing its fields
            var createdType = typeBuilder.CreateType();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Handling assignment operations (e.g., _employees = items;)
                if (trimmedLine.Contains("="))
                {
                    var parts = trimmedLine.Split('=');
                    var leftSide = parts[0].Trim();
                    var rightSide = parts[1].Trim().TrimEnd(';');

                    if (leftSide.StartsWith("_"))
                    {
                        // Handle field assignment
                        var field = createdType.GetField(leftSide, BindingFlags.NonPublic | BindingFlags.Instance);
                        if (field != null)
                        {
                            instructions.Add(new ILInstruction { OpCode = OpCodes.Ldarg_0 }); // Load 'this'
                            instructions.Add(new ILInstruction { OpCode = OpCodes.Ldarg_1 }); // Load parameter (could be extended)
                            instructions.Add(new ILInstruction { OpCode = OpCodes.Stfld, Operand = field });
                        }
                    }
                }
                // Handling return statements
                else if (trimmedLine.StartsWith("return"))
                {
                    var returnValue = trimmedLine.Replace("return", "").Trim().TrimEnd(';');
                    if (!string.IsNullOrEmpty(returnValue))
                    {
                        instructions.Add(new ILInstruction { OpCode = OpCodes.Ldarg_0 }); // Load the return value (adjust as needed)
                    }
                    instructions.Add(new ILInstruction { OpCode = OpCodes.Ret });
                }
                // Handle object creation
                else if (trimmedLine.Contains("new"))
                {
                    var objectCreation = trimmedLine.Replace("new", "").Trim().TrimEnd(';');
                    var typeName = objectCreation.Split('(')[0].Trim();

                    var newType = Type.GetType(typeName); // Resolve type by name
                    if (newType != null)
                    {
                        instructions.Add(new ILInstruction { OpCode = OpCodes.Newobj, Operand = newType.GetConstructor(Type.EmptyTypes) });
                    }
                }
                // Handle conditionals and loops (expand this logic based on your needs)
                else if (trimmedLine.StartsWith("if"))
                {
                    // Handle if conditions
                    instructions.Add(new ILInstruction { OpCode = OpCodes.Brtrue }); // Conditional branch (this is a placeholder)
                }
                else if (trimmedLine.StartsWith("foreach") || trimmedLine.StartsWith("for"))
                {
                    // Handle loops
                    instructions.Add(new ILInstruction { OpCode = OpCodes.Br }); // Loop branch (this is a placeholder)
                }
            }

            return instructions;
        }


        public class ModuleDefinition
        {
            public string ModuleName { get; set; }
            public string ModuleFileName { get; set; }
            public List<Assembly> AssemblyReferences { get; set; }
            public List<TypeDefinition> TypeDefinitions { get; set; }
        }

        public class TypeDefinition
        {
            public string TypeName { get; set; }
            public Type BaseType { get; set; }
            public TypeAttributes TypeAttributes { get; set; }
            public List<FieldDefinition> Fields { get; set; } = new List<FieldDefinition>();
            public List<PropertyDefinition> Properties { get; set; } = new List<PropertyDefinition>();
            public List<EventDefinition> Events { get; set; } = new List<EventDefinition>();
            public List<MethodDefinition> Methods { get; set; } = new List<MethodDefinition>();
            public List<ConstructorDefinition> Constructors { get; set; } = new List<ConstructorDefinition>();
        }

        public class FieldDefinition
        {
            public string Name { get; set; }
            public Type Type { get; set; }
            public FieldAttributes Attributes { get; set; }
        }

        public class PropertyDefinition
        {
            public string Name { get; set; }
            public Type Type { get; set; }
            public object DefaultValue { get; set; }
            public PropertyAttributes Attributes { get; set; }
            public string Getter { get; set; }
            public string Setter { get; set; }
        }

        public class EventDefinition
        {
            public string Name { get; set; }
            public Type Type { get; set; }
            public EventAttributes Attributes { get; set; }
        }

        public class MethodDefinition
        {
            public TypeBuilder TB { get; set; }    
            public string Name { get; set; }
            public MethodAttributes Attributes { get; set; }
            public Type ReturnType { get; set; }
            public Type[] ParameterTypes { get; set; }
            public string Body { get; set; } 
        }

        public class ConstructorDefinition
        {
            public MethodAttributes Attributes { get; set; }
            public CallingConventions CallingConvention { get; set; }
            public Type[] ParameterTypes { get; set; }
            public string Body { get; set; }  // Add this property
            public List<ParameterDefinition> Parameters { get; set; }  // Add this property
        }

        public class ParameterDefinition
        {
            public string Name { get; set; }
            public Type Type { get; set; }
        }

        public class ILInstruction
        {
            public OpCode OpCode { get; set; }
            public object Operand { get; set; }
        }
    }
}

