/*====================================================================================================
    Class Name  : DynamicClassCreator
    Created By  : Solomio S. Sisante
    Created On  : September 6, 2024
    Purpose     : To provide POC for System.Reflection.Emit implementations.
  ====================================================================================================*/
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class DynamicAssemblyCreator
    {
        public DynamicAssemblyCreator()
        {
        }

        public async Task CreateAssembly()
        {
            string nameSpace = "DynamicAssembly";
            string moduleName = nameSpace;
            string modelName = "Employee";
            string vmName = "EmployeeVM";

            // Define a new assembly
            var assemblyName = new AssemblyNameDefinition(nameSpace, new Version(1, 0));
            var assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Dll);

            // Define IModelExtendedProperties interface
            var iModelExtendedProps = new TypeDefinition(nameSpace, "IModelExtendedProperties",
                TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);
            assemblyDefinition.MainModule.Types.Add(iModelExtendedProps);

            // Define IViewModel interface
            var iViewModel = new TypeDefinition(nameSpace, "IViewModel`2",
                TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);
            iViewModel.Interfaces.Add(new InterfaceImplementation(iModelExtendedProps));

            // Define Task<T> TypeReference
            var taskType = new TypeReference("System.Threading.Tasks", "Task`1",
                assemblyDefinition.MainModule, null);
            var genericParameter = new GenericParameter("T", taskType);
            taskType.GenericParameters.Add(genericParameter);

            // Define SomeAsyncMethod with Task<Type> return type
            var asyncMethod = new MethodDefinition("SomeAsyncMethod",
                MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.Virtual, taskType);
            asyncMethod.Parameters.Add(new ParameterDefinition("parameter",
                ParameterAttributes.None, assemblyDefinition.MainModule.TypeSystem.Object));
            iViewModel.Methods.Add(asyncMethod);
            assemblyDefinition.MainModule.Types.Add(iViewModel);

            // Define Employee class
            var employeeType = new TypeDefinition(nameSpace, modelName,
                TypeAttributes.Public | TypeAttributes.Class);
            var nameField = new FieldDefinition("Name", FieldAttributes.Public,
                assemblyDefinition.MainModule.TypeSystem.String);
            employeeType.Fields.Add(nameField);
            assemblyDefinition.MainModule.Types.Add(employeeType);

            // Define EmployeeVM class
            var employeeVMType = new TypeDefinition(nameSpace, vmName,
                TypeAttributes.Public | TypeAttributes.Class);

            // Create TypeReference for the base type
            var employeeTypeRef = new TypeReference(nameSpace, modelName, assemblyDefinition.MainModule, null);

            employeeVMType.BaseType = employeeTypeRef;

            employeeVMType.Interfaces.Add(new InterfaceImplementation(iViewModel));

            // Implement SomeAsyncMethod
            var methodImpl = new MethodDefinition("SomeAsyncMethod",
                MethodAttributes.Public | MethodAttributes.Virtual, taskType);
            var ilProcessor = methodImpl.Body.GetILProcessor();
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldtoken, employeeType));
            ilProcessor.Append(ilProcessor.Create(OpCodes.Call,
                assemblyDefinition.MainModule.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle"))));
            ilProcessor.Append(ilProcessor.Create(OpCodes.Call,
                assemblyDefinition.MainModule.ImportReference(typeof(Task).GetMethod("FromResult").MakeGenericMethod(typeof(Type)))));
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
            employeeVMType.Methods.Add(methodImpl);
            assemblyDefinition.MainModule.Types.Add(employeeVMType);

            // Save the assembly
            var fileName = $"{nameSpace}.dll";
            assemblyDefinition.Write(fileName);

            Console.WriteLine($"Assembly {fileName} created successfully.");

            // Use the dynamically created assembly
            try
            {
                var loadedAssembly = System.Reflection.Assembly.LoadFrom(fileName);
                var types = loadedAssembly.GetTypes();

                foreach (var type in types)
                {
                    Console.WriteLine($"Loaded type: {type.FullName}");
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine("Exception occurred while loading types:");

                foreach (var loaderException in ex.LoaderExceptions)
                {
                    Console.WriteLine(loaderException.Message);
                }

                foreach (var type in ex.Types)
                {
                    if (type != null)
                    {
                        Console.WriteLine($"Loaded type: {type.FullName}");
                    }
                }
            }
        }
    }
}
