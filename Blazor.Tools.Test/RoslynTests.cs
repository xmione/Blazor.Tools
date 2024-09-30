using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models;
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Assemblies;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;

namespace Blazor.Tools.Test
{
    [TestClass]
    public sealed class RoslynTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            // Initialize resources for this test class.
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Cleanup resources after this test class is done.
        }

        [TestInitialize]
        public void TestInit()
        {
            // Set up before each test method.
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Tear down after each test method.
        }

        [TestMethod]
        public void CreateBundlerDLL_Test()
        {
            // Define the paths in the Temp folder
            var tempFolderPath = Path.GetTempPath(); // Gets the system Temp directory
            string baseClassAssemblyName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models";
            string vmClassAssemblyName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels";
            string interfaceAssemblyName = "Blazor.Tools.BlazorBundler.Interfaces";
            string version = "3.1.20.0";
            string iModelExtendedPropertiesFullyQualifiedName = $"{interfaceAssemblyName}.IModelExtendedProperties";
            string iViewModelFullyQualifiedName = $"{interfaceAssemblyName}.IViewModel";
            string baseClassCode = string.Empty;
            string vmClassCode = string.Empty;
            string iCloneableCode = "C:\\repo\\Blazor.Tools\\Blazor.Tools.BlazorBundler.Interfaces\\ICloneable.cs".ReadFileContents();
            string iViewModelCode = "C:\\repo\\Blazor.Tools\\Blazor.Tools.BlazorBundler.Interfaces\\IViewModel.cs".ReadFileContents();
            string iModelExtendedPropertiesCode = "C:\\repo\\Blazor.Tools\\Blazor.Tools.BlazorBundler.Interfaces\\IModelExtendedProperties.cs".ReadFileContents();

            var sampleData = new SampleData();

            var selectedTable = sampleData.EmployeeDataTable;
            var tableName = selectedTable.TableName;
            string baseDLLPath = Path.Combine(tempFolderPath, $"{baseClassAssemblyName}.dll") ?? default!;

            var usingStatements = new List<string>
            {
                "System"
            };

            string baseClassNameSpace = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models";
            Type baseClassType = default!;
            using (var baseClassGenerator = new EntityClassDynamicBuilder(baseClassNameSpace, selectedTable, usingStatements))
            {
                baseClassCode = baseClassGenerator.ToString();
                baseClassGenerator.Save(baseClassAssemblyName, version, baseClassCode, baseClassNameSpace, tableName, baseDLLPath);
            }

            //baseClassCode = baseClassCode.RemoveLines("using System");
            string vmClassNameSpace = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels";
            string vmDllPath = Path.Combine(tempFolderPath, $"{vmClassAssemblyName}.dll") ?? default!;

            var vmClassName = $"{tableName}VM";
            Type vmClassType = default!;
            Assembly vmClassAssembly = default!;
            using (var viewModelClassGenerator = new ViewModelClassGenerator(vmClassNameSpace))
            {
                viewModelClassGenerator.CreateFromDataTable(selectedTable);
                vmClassCode = viewModelClassGenerator.ToString() + iCloneableCode + iModelExtendedPropertiesCode + iViewModelCode;
                //vmClassCode = viewModelClassGenerator.ToString() + baseClassCode + iCloneableCode + iModelExtendedPropertiesCode + iViewModelCode;
                //vmClassCode = viewModelClassGenerator.ToString() + baseClassCode + iCloneableCode + iViewModelCode;
                viewModelClassGenerator.Save(vmClassAssemblyName, version, vmClassCode, vmClassNameSpace, vmClassName, vmDllPath, baseClassType, baseClassCode, baseDLLPath);
                vmClassType = viewModelClassGenerator.ClassType ?? default!;
                vmClassAssembly = viewModelClassGenerator?.DisposableAssembly?.Assembly ?? default!;

                var references = viewModelClassGenerator?.Compilation?.References.ToList();

                if (references != null)
                {
                    AppLogger.WriteInfo("Start displaying references...");
                    foreach (var reference in references)
                    {
                        AppLogger.WriteInfo(reference.Display!);
                    }
                    AppLogger.WriteInfo("Displaying references has ended.");
                }
            }

            //baseClassType = null;
            while (baseDLLPath.IsFileInUse())
            {
                //baseDLLPath.KillLockingProcesses();

                Thread.Sleep(1000);
            }

            while (vmDllPath.IsFileInUse())
            {
                //vmDllPath.KillLockingProcesses();
                Thread.Sleep(1000);
            }

            // Get the types from the assemblies
            //Type baseClassType = baseClassAssembly.GetTypes()
            //.FirstOrDefault(t => t.Name == "Employee"); // Adjust the name as needed

            //Type vmClassType = vmClassAssembly.GetTypes()
            //    .FirstOrDefault(t => t.Name == "EmployeeVM"); // Adjust the name as needed

            //Type iModelExtendedPropertiesType = typeof(IModelExtendedProperties);
            //Type iViewModelGenericType = typeof(IViewModel<,>);

            //// Create an instance of the ViewModel dynamically
            //object viewModelInstance = Activator.CreateInstance(vmClassType) ?? default!;
            using (var assembly = DisposableAssembly.LoadFile(vmDllPath))
            {
                using (var baseAssembly = DisposableAssembly.LoadFile(baseDLLPath))
                {
                    // Load types dynamically from the assembly
                    baseClassType = baseAssembly.GetType($"{baseClassAssemblyName}.{selectedTable.TableName}");
                    var iModelExtendedPropertiesType = assembly.GetType(iModelExtendedPropertiesFullyQualifiedName);
                    var viewModelType = assembly.GetType($"{vmClassAssemblyName}.EmployeeVM");
                    var iViewModelGenericType = assembly.GetType("Blazor.Tools.BlazorBundler.Interfaces.IViewModel`2");

                    if (baseClassType == null || viewModelType == null || iModelExtendedPropertiesType == null)
                    {
                        AppLogger.WriteInfo("Base class, ViewModel type, or IModelExtendedProperties type not found.");
                        return;
                    }

                    // Create the specific IViewModel<Employee, IModelExtendedProperties>
                    Type IViewModelType = iViewModelGenericType.MakeGenericType(baseClassType, iModelExtendedPropertiesType);

                    // Create an instance of ViewModel (e.g., EmployeeVM)
                    var viewModelInstance = Activator.CreateInstance(viewModelType); // Assuming you create an instance dynamically
                    if (viewModelInstance == null)
                    {
                        AppLogger.WriteInfo("Failed to create an instance of the ViewModel.");
                        return;
                    }

                    // Check if the ViewModel implements IViewModel<Employee, IModelExtendedProperties>
                    Type EmployeeVMType = viewModelInstance.GetType();
                    bool implementsViewModelInterface = IViewModelType.IsAssignableFrom(EmployeeVMType);

                    AppLogger.WriteInfo($"baseClassType == typeof(Employee): {(baseClassType == typeof(Employee)).ToString()}");
                    AppLogger.WriteInfo($"EmployeeVMType == viewModelType: {(EmployeeVMType == viewModelType).ToString()}");
                    AppLogger.WriteInfo($"EmployeeVMType == typeof(IViewModel<Employee, IModelExtendedProperties>): {(EmployeeVMType == typeof(IViewModel<Employee, IModelExtendedProperties>)).ToString()}");
                    AppLogger.WriteInfo($"viewModelType == typeof(IViewModel<Employee, IModelExtendedProperties>): {(viewModelType == typeof(IViewModel<Employee, IModelExtendedProperties>)).ToString()}");
                    AppLogger.WriteInfo($"Implements IViewModel<{baseClassType.Name}, IModelExtendedProperties>: {implementsViewModelInterface}");
                    baseClassType.DisplayTypeDifferences(typeof(Employee));
                    EmployeeVMType.DisplayTypeDifferences(typeof(IViewModel<Employee, IModelExtendedProperties>));

                    if (implementsViewModelInterface)
                    {
                        // Now cast the instance to the interface type (IViewModel<Employee, IModelExtendedProperties>)
                        var viewModelInterfaceInstance = (IViewModel<Employee, IModelExtendedProperties>)viewModelInstance; // Casting to the interface (with generics resolved)
                                                                                                                            // Note: object is used because you have runtime types (replace with the actual types if possible)

                        if (viewModelInterfaceInstance != null)
                        {
                            AppLogger.WriteInfo("Successfully casted to the interface type.");
                        }
                        else
                        {
                            AppLogger.WriteInfo("Casting to the interface type failed.");
                        }
                    }
                    else
                    {
                        AppLogger.WriteInfo("The ViewModel instance does not implement the expected interface.");
                    }

                    if (implementsViewModelInterface)
                    {
                        // Cast the instance to the interface type (IViewModel<Employee, IModelExtendedProperties>)
                        //var viewModelInterfaceInstance = Convert.ChangeType(viewModelInstance, IViewModelType); // for primitive types only like int, string
                        var viewModelInterfaceInstance = viewModelInstance as dynamic; // use dynamic casting for custom classes

                        viewModelInterfaceInstance = viewModelInterfaceInstance as IViewModel<Employee, IModelExtendedProperties>;
                        if (viewModelInterfaceInstance != null)
                        {
                            AppLogger.WriteInfo("Successfully casted to the interface type.");
                        }
                        else
                        {
                            AppLogger.WriteInfo("Casting to the interface type failed.");
                        }
                    }
                    else
                    {
                        AppLogger.WriteInfo("The ViewModel instance does not implement the expected interface.");
                    }

                    if (IViewModelType.IsAssignableFrom(viewModelType))
                    {
                        AppLogger.WriteInfo("The ViewModel implements the expected interface.");
                    }
                    else
                    {
                        AppLogger.WriteInfo("The ViewModel does not implement the expected interface.");
                    }

                    AppLogger.WriteInfo(viewModelInstance.GetType().AssemblyQualifiedName!);
                    AppLogger.WriteInfo(IViewModelType.AssemblyQualifiedName!);

                }

            }

        }

        [TestMethod]
        public void CreateEmployeeClass_Test()
        {
            string code = @"
    using System;
    namespace Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models
    {
        public class Employee
        {
            public int ID { get; set; }
            public string FirstName { get; set; } = default!;
            public string MiddleName { get; set; } = default!;
            public string LastName { get; set; } = default!;
            public DateOnly DateOfBirth { get; set; } = default!;
            public int CountryID { get; set; }
        }
    }";

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            string assemblyName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models";

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DateOnly).Assembly.Location)
            };

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, moduleName: assemblyName);
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references,
                compilationOptions);

            string outputPath = Path.Combine(Path.GetTempPath(), $"{assemblyName}.dll");

            using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                EmitResult result = compilation.Emit(fs);

                if (!result.Success)
                {
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        AppLogger.WriteInfo(diagnostic.ToString());
                    }
                }
            }

            // Load the saved assembly
            Assembly assembly = Assembly.LoadFile(outputPath);
            Module module = assembly.Modules.First();
            AppLogger.WriteInfo($"Module Name: {module.Name}");
            Type employeeType = assembly.GetType("Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models.Employee")!;
            AppLogger.WriteInfo($"Type: {employeeType.FullName}");
        }

        [TestMethod]
        public void TestDynamicAssemblyCreation()
        {
            // Define the source code for ClassA
            var classASource = @"
public class ClassA
{
    public virtual string GetMessage() => ""Hello from ClassA"";
}";

            // Define the source code for ClassB
            var classBSource = @"
public class ClassB : ClassA
{
    public override string GetMessage() => ""Hello from ClassB"";
}";

            // Compile both ClassA and ClassB into a single in-memory assembly
            var combinedAssemblyBytes = EmitAssemblyToMemory("CombinedAssembly", classASource, classBSource);

            // Load the combined assembly from memory
            var combinedAssembly = Assembly.Load(combinedAssemblyBytes);

            // Verify and list all types in the combined assembly (for diagnostic purposes)
            var typesInAssembly = combinedAssembly.GetTypes();
            foreach (var type in typesInAssembly)
            {
                AppLogger.WriteInfo($"Found type: {type.FullName}");
            }

            // Get types (be sure to use full names if necessary)
            var classAType = combinedAssembly.GetType("ClassA")!;
            var classBType = combinedAssembly.GetType("ClassB");

            if (classBType == null)
            {
                throw new InvalidOperationException("ClassB type could not be found in the assembly.");
            }

            // Create an instance of ClassB and assign it to a variable of type ClassA
            var classBInstance = Activator.CreateInstance(classBType);
            var classAInstance = (dynamic)classBInstance!;

            // Test the method calls
            var classAMessage = classAType?.GetMethod("GetMessage")?.Invoke(classAInstance, null);
            var classBMessage = classBType?.GetMethod("GetMessage")?.Invoke(classBInstance, null);

            // Assert the results
            Assert.AreEqual("Hello from ClassB", classAMessage);
            Assert.AreEqual("Hello from ClassB", classBMessage);
        }

        private static byte[] EmitAssemblyToMemory(string assemblyName, params string[] sourceCodes)
        {
            var syntaxTrees = sourceCodes.Select(code => CSharpSyntaxTree.ParseText(code)).ToList();
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references)
                .AddSyntaxTrees(syntaxTrees);

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
                foreach (var diagnostic in failures)
                {
                    AppLogger.WriteInfo($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                }
                throw new InvalidOperationException("Compilation failed");
            }

            return ms.ToArray();
        }

    }
}
