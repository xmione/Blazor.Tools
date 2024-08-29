/*====================================================================================================
    Class Name  : AssemblyDecompiler
    Created By  : Solomio S. Sisante
    Created On  : August 28, 2024
    Purpose     : To manage decompiling of DLL classes.
  ====================================================================================================*/
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using System.Text;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class AssemblyDecompiler
    {
        private readonly string _assemblyPath;

        public AssemblyDecompiler(string assemblyPath)
        {
            _assemblyPath = assemblyPath;
        }

        public string DecompileWholeAssembly()
        {
            var decompiledCode = string.Empty;
            using (var stream = new FileStream(_assemblyPath, FileMode.Open, FileAccess.Read))
            {
                var module = new PEFile("Assembly", stream);

                // Use a no-op assembly resolver
                var assemblyResolver = new AssemblyResolver();
                var typeSystem = new DecompilerTypeSystem(module, assemblyResolver);

                var decompilerSettings = GetDecompilerSettings();

                var decompiler = new CSharpDecompiler(typeSystem, decompilerSettings);

                // Decompile the entire assembly to C#
                decompiledCode = decompiler.DecompileWholeModuleAsString();
            }

            return decompiledCode;
        }

        public string DecompileType(string typeName)
        {
            var decompiledCode = string.Empty;
            using (var stream = new FileStream(_assemblyPath, FileMode.Open, FileAccess.Read))
            {
                var module = new PEFile("Assembly", stream);

                // Use a no-op assembly resolver
                var assemblyResolver = new AssemblyResolver();
                var typeSystem = new DecompilerTypeSystem(module, assemblyResolver);

                var decompilerSettings = GetDecompilerSettings();

                var decompiler = new CSharpDecompiler(typeSystem, decompilerSettings);


                // Find the type to decompile
                var type = typeSystem.MainModule.TypeDefinitions
                    .FirstOrDefault(t => t.FullName == typeName);

                if (type == null)
                    throw new ArgumentException($"Type {typeName} not found in assembly.");

                decompiledCode = decompiler.DecompileTypeAsString(type.FullTypeName);
            }
            
            return decompiledCode;
        }

        public string DecompileMethod(string typeName, string methodName)
        {
            var decompiledMethod = string.Empty;

            // Decompile the type and clean up the code
            var decompiledCode = DecompileType(typeName);
            decompiledCode = CleanUpDecompiledCode(decompiledCode);

            // Extract the specific method from the decompiled code
            decompiledMethod = GetMethodCode(methodName, decompiledCode);

            if (string.IsNullOrEmpty(decompiledMethod))
                throw new ArgumentException($"Method {methodName} not found in type {typeName}.");

            return decompiledMethod;
        }

        // Helper method to extract the full method code
        private string GetMethodCode(string methodName, string contents)
        {
            // Split the content into lines for easier processing
            var lines = contents.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder methodCode = new StringBuilder();
            bool methodFound = false;
            int braceCount = 0;

            foreach (var line in lines)
            {
                // Check if the line contains the method name and hasn't started processing yet
                if (!methodFound && line.Contains(methodName) && IsMethodSignature(line, methodName))
                {
                    methodFound = true;
                }

                // Once the method is found, start adding lines to the methodCode
                if (methodFound)
                {
                    methodCode.AppendLine(line);

                    // Count the number of opening and closing braces
                    braceCount += line.Count(c => c == '{');
                    braceCount -= line.Count(c => c == '}');

                    // If we encounter an opening brace on a new line after the method name
                    if (braceCount == 0 && line.Contains("{"))
                    {
                        braceCount++;
                    }

                    // When braceCount reaches 0 after processing the method's body, we're done
                    if (braceCount == 0 && line.Contains("}"))
                    {
                        break;
                    }
                }
            }

            // If the method wasn't found, return an empty string
            if (!methodFound)
            {
                return string.Empty;
            }

            return methodCode.ToString();
        }

        // Helper method to determine if a line represents a method signature
        private bool IsMethodSignature(string line, string methodName)
        {
            // A basic check for a method signature: line should contain access modifiers and method name,
            // followed by parentheses indicating method parameters
            var methodSignaturePattern = $@"\b(public|private|protected|internal|static|async|virtual|override|sealed)?\s*[\w<>\[\]]+\s+{methodName}\s*\(.*\)";
            return System.Text.RegularExpressions.Regex.IsMatch(line.Trim(), methodSignaturePattern);
        }

        public static string CleanUpDecompiledCode(string decompiledCode)
        {
            // Simple removal of known unwanted attributes
            var cleanedCode = decompiledCode
                .Replace("[AsyncStateMachine(typeof(<SetEditMode>d__37))]", "")
                .Replace("[DebuggerStepThrough]", "")
                .Replace("System.Threading.Tasks.Task", "Task")
                .Replace("global::", "")
                .Replace("//IL_0007: Unknown result type (might be due to invalid IL or missing references)", "")
                .Replace("//IL_000c: Unknown result type (might be due to invalid IL or missing references)", "");

            // Additional cleanup if needed
            return cleanedCode;
        }

        public static DecompilerSettings GetDecompilerSettings()
        {
            var decompilerSettings = new DecompilerSettings
            {
                AggressiveInlining = false,
                AggressiveScalarReplacementOfAggregates = false,
                AlwaysCastTargetsOfExplicitInterfaceImplementationCalls = false,
                AlwaysQualifyMemberReferences = false,
                AlwaysShowEnumMemberValues = false,
                AlwaysUseBraces = true,
                AlwaysUseGlobal = false,
                AnonymousMethods = true,
                AnonymousTypes = true,
                ApplyWindowsRuntimeProjections = true,
                ArrayInitializers = true,
                AssumeArrayLengthFitsIntoInt32 = true,
                AsyncAwait = true,
                AsyncEnumerator = true,
                AsyncUsingAndForEachStatement = true,
                AutomaticEvents = true,
                AutomaticProperties = true,
                AwaitInCatchFinally = true,
                CheckedOperators = true,
                CovariantReturns = true,
                DecimalConstants = true,
                DecompileMemberBodies = true,
                Deconstruction = true,
                DictionaryInitializers = true,
                Discards = true,
                DoWhileStatement = true,
                Dynamic = true,
                ExpandMemberDefinitions = false,
                ExpandUsingDeclarations = false,
                ExpressionTrees = true,
                ExtensionMethods = true,
                ExtensionMethodsInCollectionInitializers = true,
                FileScopedNamespaces = true,
                FixedBuffers = true,
                FoldBraces = false,
                ForEachStatement = true,
                ForEachWithGetEnumeratorExtension = true,
                ForStatement = true,
                FunctionPointers = true,
                GetterOnlyAutomaticProperties = true,
                InitAccessors = true,
                IntroduceIncrementAndDecrement = true,
                IntroduceReadonlyAndInModifiers = true,
                IntroduceRefModifiersOnStructs = true,
                IntroduceUnmanagedConstraint = true,
                LifetimeAnnotations = true,
                LiftNullables = true,
                LoadInMemory = false,
                LocalFunctions = true,
                LockStatement = true,
                MakeAssignmentExpressions = true,
                NamedArguments = true,
                NativeIntegers = true,
                NonTrailingNamedArguments = true,
                NullPropagation = true,
                NullableReferenceTypes = true,
                NumericIntPtr = true,
                ObjectOrCollectionInitializers = true,
                OptionalArguments = true,
                OutVariables = true,
                PatternBasedFixedStatement = true,
                PatternCombinators = true,
                PatternMatching = true,
                QueryExpressions = true,
                Ranges = true,
                ReadOnlyMethods = true,
                RecordClasses = true,
                RecordStructs = true,
                RecursivePatternMatching = true,
                RefExtensionMethods = true,
                RelationalPatterns = true,
                RemoveDeadCode = false,
                RemoveDeadStores = false,
                RequiredMembers = true,
                ScopedRef = true,
                SeparateLocalVariableDeclarations = false,
                ShowDebugInfo = false,
                ShowXmlDocumentation = true,
                SparseIntegerSwitch = true,
                StackAllocInitializers = true,
                StaticLocalFunctions = true,
                StringConcat = true,
                StringInterpolation = true,
                SwitchExpressions = true,
                SwitchOnReadOnlySpanChar = true,
                SwitchStatementOnString = true,
                ThrowExpressions = true,
                ThrowOnAssemblyResolveErrors = true,
                TupleComparisons = true,
                TupleConversions = true,
                TupleTypes = true,
                UnsignedRightShift = true,
                UseDebugSymbols = true,
                UseEnhancedUsing = true,
                UseExpressionBodyForCalculatedGetterOnlyProperties = true,
                UseImplicitMethodGroupConversion = true,
                UseLambdaSyntax = true,
                UseNestedDirectoriesForNamespaces = false,
                UsePrimaryConstructorSyntax = true,
                UseRefLocalsForAccurateOrderOfEvaluation = true,
                UseSdkStyleProjectFormat = true,
                UsingDeclarations = true,
                UsingStatement = true,
                Utf8StringLiterals = true,
                WithExpressions = true,
                YieldReturn = true
            };


            return decompilerSettings;
        }
    }

    // Built-in implementation for AssemblyResolver
    public class AssemblyResolver : IAssemblyResolver
    {
        public PEFile? Resolve(IAssemblyReference reference)
        {
            // You can provide logic to resolve referenced assemblies here if needed
            return null; // Default behavior: no assembly resolution
        }

        public PEFile? ResolveModule(PEFile mainModule, string moduleName)
        {
            // You can provide logic to resolve modules here if needed
            return null; // Default behavior: no module resolution
        }

        public Task<PEFile?> ResolveAsync(IAssemblyReference reference)
        {
            return Task.FromResult(Resolve(reference));
        }

        public Task<PEFile?> ResolveModuleAsync(PEFile mainModule, string moduleName)
        {
            return Task.FromResult(ResolveModule(mainModule, moduleName));
        }
    }
}
