using Blazor.Tools.ConsoleApp.Extensions;
using Microsoft.ML;
using System.Diagnostics;
using HtmlAgilityPack;
using Blazor.Tools.BlazorBundler.Extensions;
using Mono.Cecil;
using Blazor.Tools.BlazorBundler.Entities.SampleObjects;
using Blazor.Tools.BlazorBundler.Interfaces;
using Mono.Cecil.Rocks;
using System.ComponentModel.DataAnnotations;
using Blazor.Tools.BlazorBundler.Entities;
using System.Reflection;

namespace Blazor.Tools.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var answerConfig = new AnswerConfig();
            var mlFolder = @"C:\repo\Blazor.Tools\Blazor.Tools\Data\ML";
            var fileName = string.Empty;
            var trainingFilePath = string.Empty;
            var outputFilePath = string.Empty;
            var dateToday = DateTime.Now.ToString("yyyy-dd-MM_HH-mm-ss");
            var connectionString = "Server=(local);Database=AIDatabase;User Id=sa;Password=P@ssw0rd123;TrustServerCertificate=True;";
            var originalDataAccess = new OriginalQuestionAnsweringDataAccess(connectionString);
            var qaDataAccess = new QuestionAnsweringDataAccess(connectionString);
            IEnumerable<QuestionAnsweringData> questionAnswerList = default!;
            IEnumerable<OriginalQuestionAnsweringData> originalQuestionAnswerList = default!;
            var jsonParser = new JsonlParser();

            while (true)
            {
                Console.WriteLine("Please choose an option:");
                Console.WriteLine("[1] - Convert Tab Delimited Yelp file to CSV");
                Console.WriteLine("[2] - Log Available Properties v1.0-simplified_nq-dev-all.jsonl");
                Console.WriteLine("[3] - Log Available Properties v1.0-simplified_simplified-nq-train.jsonl");
                Console.WriteLine("[4] - Decompress and parse v1.0-simplified_nq-dev-all.jsonl.gz");
                Console.WriteLine("[5] - Decompress and parse v1.0-simplified_simplified-nq-train.jsonl.gz");
                Console.WriteLine("[6] - Parse and save v1.0-simplified_nq-dev-all.jsonl");
                Console.WriteLine("[7] - Parse and save v1.0-simplified_simplified-nq-train.jsonl");
                Console.WriteLine("[8] - Parse Json File and Save to database");
                Console.WriteLine("[9] - Get Language training data from Database then train model");
                Console.WriteLine("[10] - Train sample Language Data");
                Console.WriteLine("[11] - Decompile IL code");
                Console.WriteLine("[12] - Decompile EmployeeVM.dll");
                Console.WriteLine("[13] - Decompile EmployeeVM SetEditMode");
                Console.WriteLine("[14] - Save Dynamically Created Assembly to .dll file");
                Console.WriteLine("[15] - Run SetEditMethod of EmployeeVM from Blazor.Tools.BlazorBundler.dll ");
                Console.WriteLine("[16] - Decompile .dll to class (.cs) file to Temp folder");
                Console.WriteLine("[17] - Create Blazor.Tools.BlazorBundler.dll to Temp folder");
                Console.WriteLine("[18] - Run .dll file method from Temp folder");
                Console.WriteLine("[19] - Exit");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ConvertTabDelimitedFileToCsv(mlFolder);
                        break;
                    case "2":
                        fileName = Path.Combine(mlFolder,"v1.0-simplified_nq-dev-all.jsonl");
                        LogAvailableProperties(fileName);
                        break;
                    case "3":
                        fileName = Path.Combine(mlFolder, "v1.0-simplified_simplified-nq-train.jsonl");
                        LogAvailableProperties(fileName);
                        break;
                    case "4":
                        answerConfig = new AnswerConfig
                        {
                            StartProperty = "start_byte",
                            EndProperty = "end_byte",
                            IsBytePosition = true
                        };

                        fileName = "v1.0-simplified_nq-dev-all.jsonl";
                        DecompressAndParseJsonlFile(mlFolder, fileName, answerConfig);
                        break;
                    case "5":
                        answerConfig = new AnswerConfig
                        {
                            StartProperty = "start_token",
                            EndProperty = "end_token",
                            IsBytePosition = false
                        };

                        fileName = "v1.0-simplified_simplified-nq-train.jsonl";
                        DecompressAndParseJsonlFile(mlFolder, fileName, answerConfig);
                        break;
                    case "6":
                        answerConfig = new AnswerConfig
                        {
                            StartProperty = "start_byte",
                            EndProperty = "end_byte",
                            IsBytePosition = true
                        };

                        fileName = "v1.0-simplified_nq-dev-all.jsonl";
                        outputFilePath = Path.Combine(mlFolder, fileName);
                        trainingFilePath = Path.Combine(mlFolder, fileName + $"-{dateToday}.txt");
                        NqEntryExtensions.ParseJsonlFile(outputFilePath, trainingFilePath);
                        break;
                    case "7":
                        answerConfig = new AnswerConfig
                        {
                            StartProperty = "start_token",
                            EndProperty = "end_token",
                            IsBytePosition = false
                        };

                        fileName = "v1.0-simplified_simplified-nq-train.jsonl";
                        outputFilePath = Path.Combine(mlFolder, fileName);
                        trainingFilePath = Path.Combine(mlFolder, fileName + $"-{dateToday}.txt");
                        NqEntryExtensions.ParseJsonlFile(outputFilePath, trainingFilePath);
                        break;
                    case "8":
                       
                        fileName = "v1.0-simplified_nq-dev-all.jsonl";
                        outputFilePath = Path.Combine(mlFolder, fileName);
                        trainingFilePath = Path.Combine(mlFolder, fileName + $"-{dateToday}.txt");
                        
                        originalQuestionAnswerList = jsonParser.ParseJsonlFileToOriginal(outputFilePath);
                                                
                        foreach (var item in originalQuestionAnswerList)
                        { 
                            originalDataAccess.Create(item);
                        }
                                                
                        break;
                    case "9":
                        var progress = new Progress<double>(percentage =>
                        {
                            Console.WriteLine($"Progress: {percentage:F2}%");
                        });
                        
                        ConvertToQAData(originalDataAccess, originalQuestionAnswerList, questionAnswerList, jsonParser, trainingFilePath, progress);
                        
                        break;
                    case "10":
                        fileName = "languageData.txt";
                        TrainQuestionAnswers(mlFolder, fileName);
                        break; 
                    case "11":

                        DecompileILCode();
                        break;
                    case "12":

                        DecompileEmployeeVM();
                        break;
                    case "13":

                        DecompileEmployeeVMSetEditModeMethod();
                        break;
                    case "14":

                        SaveDynamicallyCreatedAssembly();
                        break;
                    case "15":

                        RunSetEditMethodOfEmployeeVMFromBundlerDLLFileAsync().Wait();
                        break;
                    case "16":

                        DecompileDLLFile();
                        break;
                    case "17":

                        CreateBundlerDLL();
                        break;
                    case "18":

                        RunDLLFileAsync().Wait(); 
                        break;
                    case "19":
                        return; 
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        public static void LogAvailableProperties(string jsonlFilePath)
        {
            var totalStopwatch = Stopwatch.StartNew();
            var jsonlReadStopwatch = Stopwatch.StartNew();
            var dateToday = DateTime.Now.ToString("yyyy-dd-MM_HH-mm-ss");
            var logFilePath = Path.Combine(Path.GetDirectoryName(jsonlFilePath), $"parsing_log-{dateToday}.txt");

            using (var logWriter = new StreamWriter(logFilePath, append: false))
            {
                Console.WriteLine($"Reading jsonl file {jsonlFilePath}...");
                logWriter.WriteLine($"Reading jsonl file {jsonlFilePath}...");
                var lines = File.ReadLines(jsonlFilePath).ToList();
                jsonlReadStopwatch.Stop();
                var jsonlReadDuration = jsonlReadStopwatch.Elapsed;

                Console.WriteLine("Duration for reading Jsonl file: {0:hh\\:mm\\:ss}", jsonlReadDuration);
                logWriter.WriteLine("Duration for reading Jsonl file: {0:hh\\:mm\\:ss}", jsonlReadDuration);

                NqEntryExtensions.LogAvailableProperties(jsonlFilePath, lines, logWriter);

                totalStopwatch.Stop();
                var totalDuration = totalStopwatch.Elapsed;
                Console.WriteLine("Total duration for reading file: {0:hh\\:mm\\:ss}", totalDuration);
                logWriter.WriteLine("Total duration for reading file: {0:hh\\:mm\\:ss}", totalDuration);
            }
        }

        private static void ConvertTabDelimitedFileToCsv(string folder)
        {
            string tabDelimitedFilePath = Path.Combine(folder, "yelp_labelled.txt");
            string csvFilePath = Path.Combine(folder, "yelp_labelled.csv");  

            tabDelimitedFilePath.ConvertTabDelimitedFileToCsv(csvFilePath);
        }

        public static void DecompressAndParseJsonlFile(string folder, string fileName, AnswerConfig config)
        {
            string gzipFilePath = Path.Combine(folder, fileName + ".gz");
            string outputFilePath = Path.Combine(folder, fileName);

            var entry = new NqEntry();
            entry.DecompressAndParseJsonlFile(gzipFilePath, outputFilePath, config);
        }

        public static void ConvertToQAData(OriginalQuestionAnsweringDataAccess originalDataAccess,
                                   IEnumerable<OriginalQuestionAnsweringData> originalQuestionAnswerList,
                                   IEnumerable<QuestionAnsweringData> questionAnswerList,
                                   JsonlParser jsonParser, string trainingFilePath,
                                   IProgress<double> progress = null)
        {
            // Fetch total record count
            int totalRecords = originalDataAccess.GetTotalRecordCount();

            // Define batch size
            int batchSize = 100; // Adjust as per your application's needs

            // Loop through batches
            for (int pageNumber = 1; pageNumber <= (totalRecords + batchSize - 1) / batchSize; pageNumber++)
            {
                // Fetch data for current page
                originalQuestionAnswerList = originalDataAccess.ListAll(batchSize, pageNumber);

                // Convert to QuestionAnsweringData using your JsonParser
                questionAnswerList = jsonParser.ConvertToQAData(originalQuestionAnswerList);

                // Train model with current batch
                NqEntryExtensions.TrainModel(questionAnswerList, trainingFilePath);

                // Report progress
                if (progress != null)
                {
                    double progressPercentage = (double)pageNumber / ((totalRecords + batchSize - 1) / batchSize) * 100;
                    progress.Report(progressPercentage);
                }
            }
        }

        public static void TrainQuestionAnswers(string folder, string fileName)
        {
            var mlContext = new MLContext();
            var dataFile = Path.Combine(folder, fileName);

            // Load the data
            var data = mlContext.Data.LoadFromTextFile<QuestionAnswer>(dataFile, hasHeader: true, separatorChar: '\t');

            // Preprocess HTML
            var preprocessedData = mlContext.Data.CreateEnumerable<QuestionAnswer>(data, reuseRowObject: false)
                .Select(row =>
                {
                    row.Context = PreprocessHtml(row.Context);
                    return row;
                });

            var preprocessedDataView = mlContext.Data.LoadFromEnumerable(preprocessedData);

            // Define the training pipeline
            var pipeline = mlContext.Transforms.Text.FeaturizeText("ContextFeaturized", nameof(QuestionAnswer.Context))
                .Append(mlContext.Transforms.Text.FeaturizeText("QuestionFeaturized", nameof(QuestionAnswer.Question)))
                .Append(mlContext.Transforms.Concatenate("Features", "ContextFeaturized", "QuestionFeaturized"))
                .Append(mlContext.Transforms.CopyColumns("Label", nameof(QuestionAnswer.AnswerIndex)))
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", maximumNumberOfIterations: 100));

            // Train the model
            var model = pipeline.Fit(preprocessedDataView);

            Console.WriteLine("Model training complete.");
        }

        public static string PreprocessHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode.InnerText;
        }

        private static void DecompileILCode()
        {
            var convertIL = new ILToSourceCodeConverter();
        }

        private static void DecompileEmployeeVM()
        {
            string blazorBundlerPath = @"C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\bin\Debug\net8.0\";
            string blazorBundlerFile = "Blazor.Tools.BlazorBundler.dll";
            string defaultTempFolder = Path.GetTempPath();

            string tempFolder = string.Empty;
            string dllFileName = string.Empty;
            string? assemblyPath = null;

            bool continueLoop = true;
            while (continueLoop)
            {
                Console.WriteLine("DLL Folder Path (default){0}: ", defaultTempFolder);
                Console.WriteLine("Please choose an option:");
                Console.WriteLine("[0] - Default Folder [{0}]", defaultTempFolder);
                Console.WriteLine("[1] - Blazor Bundler DLL Folder [{0}]", blazorBundlerPath);
                var folderChoice = Console.ReadLine();
                
                switch (folderChoice)
                {
                    case "":
                    case "0":
                        tempFolder = defaultTempFolder; // sets the default if you did not specify a dll file name.
                        dllFileName = "EmployeeVM.dll";
                        assemblyPath = Path.Combine(tempFolder, dllFileName);
                        
                        break;
                    case "1":
                        tempFolder = blazorBundlerPath; // sets the default if you did not specify a dll file name.
                        dllFileName = blazorBundlerFile;
                        assemblyPath = Path.Combine(tempFolder, dllFileName);
                         
                        break;
                    default:
                        Console.Write("DLL File Name (default)EmployeeVM.dll: ");
                        assemblyPath = Console.ReadLine();
                         
                        break;
                }

                if (File.Exists(assemblyPath))
                {
                    continueLoop = false;
                }
                else
                {
                    Console.WriteLine("Assembly File does not exist: {0}", assemblyPath);
                }
            }
            
            string typeName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.EmployeeVM";
            if (assemblyPath != null)
            {
                Console.WriteLine("Decompiling {0} [{1}]", assemblyPath, typeName);
                string decompiledCode = assemblyPath.DecompileType(typeName);

                Console.WriteLine(decompiledCode);
            }
        }

        private static void DecompileEmployeeVMSetEditModeMethod()
        {
            string assemblyPath = @"C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\bin\Debug\net8.0\Blazor.Tools.BlazorBundler.dll";
            string typeName = "Blazor.Tools.BlazorBundler.Entities.SampleObjects.EmployeeVM";  // Change to your desired type
            string methodName = "SetEditMode";

            // Using extension method
            string decompiledCodeFromExtension = assemblyPath.DecompileMethod(typeName, methodName);
            Console.WriteLine(decompiledCodeFromExtension);
        }

        public static void SaveDynamicallyCreatedAssembly()
        {
            // Define the paths in the Temp folder
            var tempFolderPath = Path.GetTempPath(); // Gets the system Temp directory
            string assemblyName = "Blazor.Tools.BlazorBundler";
            string nameSpace = "Blazor.Tools.BlazorBundler.Entities.SampleObjects";
            string className = "EmployeeVM";
            string dllPath = Path.Combine(tempFolderPath, $"{assemblyName}.dll");

            // Read the code for all relevant classes and interfaces
            string employeeVMClassFilePath = @"C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\Entities\SampleObjects\EmployeeVM.cs";
             
            string employeeVMClassCode = employeeVMClassFilePath.ReadFileContents();
            
            Save(employeeVMClassCode, nameSpace, className, dllPath, typeof(Employee));
        }

        private static void Save(string classCode, string nameSpace, string className, string dllPath, Type baseClassType)
        {
            var employeeVMClassGenerator = new ClassGenerator();

            var programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var systemFilePath = @"dotnet\shared\Microsoft.NETCore.App\8.0.8\System.dll";
            var systemRuntimeFilePath = @"dotnet\shared\Microsoft.NETCore.App\8.0.8\System.Runtime.dll";
            var systemCollectionsFilePath = @"dotnet\shared\Microsoft.NETCore.App\8.0.8\System.Collections.dll";
            var systemConsoleFilePath = @"dotnet\shared\Microsoft.NETCore.App\8.0.8\System.Console.dll";
            var systemLinqFilePath = @"dotnet\shared\Microsoft.NETCore.App\8.0.8\System.Linq.dll";
            var systemThreadingTasksFilePath = @"dotnet\shared\Microsoft.NETCore.App\8.0.8\System.Threading.Tasks.dll";
            var systemPrivateCoreLibFilePath = @"dotnet\shared\Microsoft.NETCore.App\8.0.8\System.Private.CoreLib.dll";

            var systemLocation = Path.Combine(programFilesPath, systemFilePath);
            var systemRuntimeLocation = Path.Combine(programFilesPath, systemRuntimeFilePath);
            var systemCollectionsLocation = Path.Combine(programFilesPath, systemCollectionsFilePath);
            var systemConsoleLocation = Path.Combine(programFilesPath, systemConsoleFilePath);
            var systemLinqLocation = Path.Combine(programFilesPath, systemLinqFilePath);
            var systemThreadingTasksLocation = Path.Combine(programFilesPath, systemThreadingTasksFilePath);
            var systemPrivateCoreLibLocation = Path.Combine(programFilesPath, systemPrivateCoreLibFilePath);

            // Add references to existing assemblies that contain types used in the dynamic class
            employeeVMClassGenerator.AddReference(systemLocation);  // System.dll
            employeeVMClassGenerator.AddReference(systemPrivateCoreLibLocation);  // Object types
            employeeVMClassGenerator.AddReference(systemRuntimeLocation);  // System.Runtime.dll
            employeeVMClassGenerator.AddReference(systemCollectionsLocation);  // System.Collections.dll
            employeeVMClassGenerator.AddReference(systemConsoleLocation);  // System.Console.dll
            employeeVMClassGenerator.AddReference(systemLinqLocation);  // System.Collections.dll
            employeeVMClassGenerator.AddReference(systemThreadingTasksLocation);  // System.Threading.Tasks.dll

            // Add references to assemblies containing other required types
            employeeVMClassGenerator.AddReference(baseClassType.Assembly.Location);
            employeeVMClassGenerator.AddReference(typeof(IValidatableObject).Assembly.Location);
            employeeVMClassGenerator.AddReference(typeof(ICloneable<>).Assembly.Location);
            employeeVMClassGenerator.AddReference(typeof(IViewModel<,>).Assembly.Location);

            Console.WriteLine("//Class Code: \r\n{0}", classCode);
             
            employeeVMClassGenerator.CreateType(classCode, nameSpace, className);
             
            // Save the compiled assembly to the Temp folder
            employeeVMClassGenerator.SaveAssemblyToTempFolder(dllPath);
             
        }

        public static void CreateAndSaveDynamicAssembly(string assemblyName, string nameSpace, string className, string dllPath)
        {
            // Create assembly and module
            var assemblyDefinition = assemblyName.CreateAssemblyDefinition();
            var moduleDefinition = assemblyDefinition.MainModule;

            // Import necessary references
            var baseTypeRef = moduleDefinition.ImportReference(typeof(Employee));
            var typeDefinition = nameSpace.CreateTypeDefinition(className, baseTypeRef);

            // Implement interfaces
            var iViewModelRef = moduleDefinition.ImportReference(typeof(IViewModel<,>));
            var iValidatableObjectRef = moduleDefinition.ImportReference(typeof(IValidatableObject));
            var iCloneableRef = moduleDefinition.ImportReference(typeof(ICloneable<>));

            var iViewModelGeneric = new GenericInstanceType(iViewModelRef);
            iViewModelGeneric.GenericArguments.Add(baseTypeRef);
            iViewModelGeneric.GenericArguments.Add(moduleDefinition.ImportReference(typeof(IModelExtendedProperties)));

            typeDefinition.DeriveFromInterfaces(
                iValidatableObjectRef,
                iCloneableRef.MakeGenericInstanceType(typeDefinition),
                iViewModelGeneric);

            // Add fields

            CreateType(type: typeof(List<EmployeeVM>), moduleDefinition: moduleDefinition, typeDefinition: typeDefinition, fieldName: "_employees");
            CreateType(iType: typeof(IContextProvider), type: typeof(List<EmployeeVM>), moduleDefinition: moduleDefinition, typeDefinition: typeDefinition, fieldName: "_contextProvider", isReadOnly: true);

            // Add properties
            typeDefinition.AddProperty(moduleDefinition, "RowID", moduleDefinition.TypeSystem.Int32);
            typeDefinition.AddProperty(moduleDefinition, "IsEditMode", moduleDefinition.TypeSystem.Boolean);
            typeDefinition.AddProperty(moduleDefinition, "IsVisible", moduleDefinition.TypeSystem.Boolean);
            typeDefinition.AddProperty(moduleDefinition, "StartCell", moduleDefinition.TypeSystem.Int32);
            typeDefinition.AddProperty(moduleDefinition, "EndCell", moduleDefinition.TypeSystem.Int32);
            typeDefinition.AddProperty(moduleDefinition, "IsFirstCellClicked", moduleDefinition.TypeSystem.Boolean);

            // Add the constructor with field initialization
            typeDefinition.AddConstructor(moduleDefinition);

            // Add type definition to module
            moduleDefinition.Types.Add(typeDefinition);

            // Save the assembly to disk
            assemblyDefinition.Write(dllPath);

            Console.WriteLine("Assembly created and saved as {0}", dllPath);
        }

        private static void CreateType(
                                Type? iType = null,
                                Type? type = null,
                                Mono.Cecil.ModuleDefinition? moduleDefinition = null,
                                Mono.Cecil.TypeDefinition? typeDefinition = null,
                                string? fieldName = null,
                                bool isReadOnly = false)
        {
            if (moduleDefinition == null || typeDefinition == null || fieldName == null || type == null)
            {
                return; // Exit early if essential parameters are missing
            }

            Mono.Cecil.TypeReference typeRef;
            if (type.IsGenericType)
            {
                // Handle generic types like List<Employee>
                var genericTypeDef = type.GetGenericTypeDefinition(); // e.g., List`1
                var genericTypeRef = moduleDefinition.ImportReference(genericTypeDef); // Import the generic type definition

                var genericInstanceType = new GenericInstanceType(genericTypeRef); // Create an instance of the generic type

                foreach (var arg in type.GetGenericArguments())
                {
                    genericInstanceType.GenericArguments.Add(moduleDefinition.ImportReference(arg)); // Import and add the type argument (e.g., Employee)
                }

                typeRef = genericInstanceType; // Assign the generic instance type as the reference
            }
            else
            {
                typeRef = moduleDefinition.ImportReference(type); // Handle non-generic types
            }

            if (iType != null)
            {
                var iTypeRef = moduleDefinition.ImportReference(iType);
                typeDefinition.AddFieldWithInitializer(fieldName, type, iTypeRef, moduleDefinition, isReadOnly);
            }
            else
            {
                typeDefinition.AddFieldWithInitializer(fieldName, type, typeRef, moduleDefinition, isReadOnly);
            }

            // Print available types
            foreach (var t in moduleDefinition.Types)
            {
                Console.WriteLine($"Available Type: {t.Name}");
            }

            // Get the assembly and types from it
            var assemblyReference = (AssemblyNameReference)typeRef.Scope;
            var externalAssembly = moduleDefinition.AssemblyResolver.Resolve(assemblyReference);

            // YOU CAN COMMENT IT OUT BUT DO NOT REMOVE: THIS IS FOR TESTING
            //var list = externalAssembly?.MainModule.Types
            //                .Select(type => type.Name)
            //                .OrderBy(name => name)
            //                .ToList();

            // After getting the externalAssembly
            if (externalAssembly == null)
            {
                Console.WriteLine("External assembly not found.");
                return;
            }

            // Handling generic type names
            if (type.IsGenericType)
            {
                // Get the generic type definition name (e.g., List`1)
                var genericTypeName = type.GetGenericTypeDefinition().Name;

                // Search for the generic type definition in the external assembly
                var genericTypeDefinition = externalAssembly.MainModule.Types
                    .FirstOrDefault(t => t.Name == genericTypeName);

                if (genericTypeDefinition != null)
                {
                    // Resolve the actual types of the generic arguments in the context of the module
                    var resolvedGenericArguments = type.GetGenericArguments()
                        .Select(arg => moduleDefinition.ImportReference(arg).Resolve())
                        .ToList();

                    // Compare the resolved types with the generic parameters of the found type definition
                    bool matchFound = true;
                    for (int i = 0; i < resolvedGenericArguments.Count; i++)
                    {
                        var genericArgument = resolvedGenericArguments[i];
                        var expectedType = moduleDefinition.ImportReference(type.GetGenericArguments()[i]).Resolve();

                        // Compare the resolved types instead of just names
                        if (genericArgument.FullName != expectedType.FullName)
                        {
                            matchFound = false;
                            break;
                        }
                    }

                    Console.WriteLine(matchFound
                        ? $"Found the {type.Name} type (generic) in the external assembly."
                        : $"{type.Name} type (generic) was not found with matching generic arguments.");
                }
                else
                {
                    Console.WriteLine($"{genericTypeName} type definition was not found in the external assembly.");
                }
            }
            else
            {
                // Handling non-generic types
                var concreteFieldName = fieldName.Replace("_", "").ToPascalCase();
                var contextProviderType = externalAssembly.MainModule.Types
                    .FirstOrDefault(t => t.Name == concreteFieldName);

                Console.WriteLine(contextProviderType != null
                    ? $"Found the {concreteFieldName} type in the external assembly."
                    : $"{concreteFieldName} type was not found in the external assembly.");
            }


        }


        public static async Task RunSetEditMethodOfEmployeeVMFromBundlerDLLFileAsync()
        {
            // Define the paths in the Temp folder
            string bundlerDLLPath = @"C:\repo\Blazor.Tools\Blazor.Tools.BlazorBundler\bin\Debug\net8.0\";
            string assemblyName = "Blazor.Tools.BlazorBundler";
            string nameSpace = "Blazor.Tools.BlazorBundler.Entities.SampleObjects";
            string dllPath = Path.Combine(bundlerDLLPath, $"{assemblyName}.dll");

            // Load the assembly
            var assembly = dllPath.LoadAssemblyFromDLLFile();

            // Define method and type names
            string methodName = "SetEditMode";
            string typeName = $"{nameSpace}.EmployeeVM";

            // Create an instance of the type
            Type type = assembly.GetType(typeName);
            object instance = Activator.CreateInstance(type)
                              ?? throw new InvalidOperationException($"Cannot create an instance of type '{typeName}'.");

            // Invoke the method asynchronously
            Console.WriteLine("Trying to set IsEditMode to false");
            await assembly.InvokeMethodAsync(typeName, methodName, instance, false); // Pass parameters as needed

            object isEditModeValue = instance.GetProperty("IsEditMode");
            Console.WriteLine($"IsEditMode is set to: {isEditModeValue}");

            Console.WriteLine("Trying to set IsEditMode to true");
            await assembly.InvokeMethodAsync(typeName, methodName, instance, true); // Pass parameters as needed

            isEditModeValue = instance.GetProperty("IsEditMode");
            Console.WriteLine($"IsEditMode is set to: {isEditModeValue}");

        }

        public static void DecompileDLLFile()
        {
            // Define the paths in the Temp folder
            var tempFolderPath = Path.GetTempPath(); // Gets the system Temp directory
            string assemblyName = "Blazor.Tools.BlazorBundler";
            string dllPath = Path.Combine(tempFolderPath, $"{assemblyName}.dll");
            var outputPath = Path.Combine(tempFolderPath, "DecompiledCode.cs");

            dllPath.DecompileWholeModuleToClass(outputPath);

        }

        public static void CreateBundlerDLL()
        {
            // Define the paths in the Temp folder
            var tempFolderPath = Path.GetTempPath(); // Gets the system Temp directory
            string assemblyName = "Blazor.Tools.BlazorBundler";
            string nameSpace = "Blazor.Tools.BlazorBundler.Entities.SampleObjects";
            string dllPath = Path.Combine(tempFolderPath, $"{assemblyName}.dll");
            var outputPath = Path.Combine(tempFolderPath, "DecompiledCode.cs");

            var sampleData = new SampleData();

            var selectedTable = sampleData.EmployeeDataTable;
            var tableName = selectedTable.TableName;
            var className = $"{tableName}VM";
            var viewModel = new ViewModelClassGenerator(nameSpace);
            viewModel.CreateFromDataTable(selectedTable);
            var classCode = viewModel.ToString();

            Save(classCode, nameSpace, className, dllPath, typeof(Employee));

        }

        private static async Task RunDLLFileAsync()
        {
            // Define the paths in the Temp folder
            var tempFolderPath = Path.GetTempPath(); // Gets the system Temp directory
            string assemblyName = "Blazor.Tools.BlazorBundler";
            string nameSpace = "Blazor.Tools.BlazorBundler.Entities.SampleObjects";
            string dllPath = Path.Combine(tempFolderPath, $"{assemblyName}.dll");

            // Load the assembly
            var assembly = dllPath.LoadAssemblyFromDLLFile();

            // Define method and type names
            string methodName = "SetEditMode";
            string typeName = $"{nameSpace}.EmployeeVM";

            // Create an instance of the type
            Type type = assembly.GetType(typeName);
            object instance = Activator.CreateInstance(type)
                              ?? throw new InvalidOperationException($"Cannot create an instance of type '{typeName}'.");

            // Invoke the method asynchronously
            Console.WriteLine("Trying to set IsEditMode to false");
            await assembly.InvokeMethodAsync(typeName, methodName, instance, false); // Pass parameters as needed

            object isEditModeValue = instance.GetProperty("IsEditMode");
            Console.WriteLine($"IsEditMode is set to: {isEditModeValue}");

            Console.WriteLine("Trying to set IsEditMode to true");
            await assembly.InvokeMethodAsync(typeName, methodName, instance, true); // Pass parameters as needed

            isEditModeValue = instance.GetProperty("IsEditMode");
            Console.WriteLine($"IsEditMode is set to: {isEditModeValue}");
        }

    }
}
