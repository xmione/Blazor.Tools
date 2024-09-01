/*====================================================================================================
    Class Name  : ClassGenerator
    Created By  : Solomio S. Sisante
    Created On  : August 31, 2024
    Purpose     : To create EmployeeVM Assembly class dynamically and save it to a .dll file.
  ====================================================================================================*/
using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;
using OutputKind = Microsoft.CodeAnalysis.OutputKind;
using Assembly = System.Reflection.Assembly;
using static Microsoft.ML.Transforms.OneHotEncodingEstimator;
namespace Blazor.Tools.BlazorBundler.Entities
{
    public class ClassGenerator
    {
        private CSharpCompilation _compilation;

        public CSharpCompilation Compilation
        {
            get { return _compilation; }
            set { _compilation = value; }
        }

        private OutputKind _outputKind;

        public OutputKind OutputKind
        {
            get { return _outputKind; }
            set { _outputKind = value; }
        }

        public ClassGenerator() 
        {
            _outputKind = OutputKind.DynamicallyLinkedLibrary;
            _compilation = CSharpCompilation.Create("DynamicAssembly",
                options: new CSharpCompilationOptions(_outputKind));

        }

        public void AddReference(string assemblyPath)
        {
            Console.WriteLine("Adding reference to {0}", assemblyPath);

            var reference = MetadataReference.CreateFromFile(assemblyPath);
            _compilation = _compilation?.AddReferences(reference) ?? CSharpCompilation.Create("DynamicAssembly").AddReferences(reference);
        }

        public void AddReference(Stream assemblyStream)
        {
            var reference = MetadataReference.CreateFromStream(assemblyStream);
            _compilation = _compilation.AddReferences(reference);
        }

        public void AddReference(byte[] assemblyBytes)
        {
            var reference = MetadataReference.CreateFromImage(assemblyBytes);
            _compilation = _compilation.AddReferences(reference);
        }

        public Type CreateType(string classCode, string nameSpace, string className)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(classCode);

            _compilation = _compilation.AddSyntaxTrees(syntaxTree);

            using var ms = new MemoryStream();
            var result = _compilation.Emit(ms);

            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
                foreach (var diagnostic in failures)
                {
                    Console.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                }
                throw new InvalidOperationException("Compilation failed.");
            }

            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());
            return assembly.GetType($"{nameSpace}.{className}");
        }

        public void AddBaseClass()
        { 
        }
        public void DeriveFromInterface()
        { 
        }

        public void SaveAssemblyToTempFolder(string fileName)
        {
            using var ms = new MemoryStream();
            var result = _compilation.Emit(ms);

            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
                foreach (var diagnostic in failures)
                {
                    Console.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                }
                throw new InvalidOperationException("Compilation failed.");
            }

            ms.Seek(0, SeekOrigin.Begin);
            File.WriteAllBytes(fileName, ms.ToArray());

            Console.WriteLine("Successfully compiled and saved to {0}", fileName);
        }

        public static void GenerateEmployeeVM(string assemblyPath, string nameSpace)
        {
            Console.WriteLine("Creating dynamic class in assembly file {0}", assemblyPath);

            // Define class
            var className = "EmployeeVM";
            var classDeclaration = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("Employee")),
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IValidatableObject")),
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("ICloneable<EmployeeVM>")),
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IViewModel<Employee, IModelExtendedProperties>"))
                );

            // Define tuples with explicit types
            var fields = new (string FieldName, string FieldType, SyntaxKind? Modifier)[]
            {
                ("_employees", "List<EmployeeVM>", null),
                ("_contextProvider", "IContextProvider", SyntaxKind.ReadOnlyKeyword),
                ("_rowID", "int", null),
                ("_isEditMode", "bool", null),
                ("_isVisible", "bool", null),
                ("_startCell", "int", null),
                ("_endCell", "int", null),
                ("_isFirstCellClicked", "bool", null)
            };

            foreach (var field in fields)
            {
                var modifiers = new List<SyntaxKind> { SyntaxKind.PrivateKeyword };
                if (field.Item3 != default) modifiers.Add((SyntaxKind)field.Item3);

                classDeclaration = classDeclaration.AddMembers(
                    SyntaxFactory.FieldDeclaration(
                        SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(field.Item2))
                            .AddVariables(SyntaxFactory.VariableDeclarator(field.Item1))
                    ).AddModifiers(modifiers.Select(SyntaxFactory.Token).ToArray())
                );
            }

            // Add constructor
            var constructor = SyntaxFactory.ConstructorDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block(
                    // Initialize fields
                    SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName("_employees"),
                        SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("List<EmployeeVM>"))
                            .WithArgumentList(SyntaxFactory.ArgumentList())
                    )),
                    SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName("_contextProvider"),
                        SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("ContextProvider"))
                            .WithArgumentList(SyntaxFactory.ArgumentList())
                    )),
                    // Default field initializations
                    SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName("_rowID"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0))
                    )),
                    SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName("_isEditMode"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)
                    )),
                    SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName("_isVisible"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)
                    )),
                    SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName("_startCell"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0))
                    )),
                    SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName("_endCell"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0))
                    )),
                    SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName("_isFirstCellClicked"),
                        SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)
                    ))
                ));

            classDeclaration = classDeclaration.AddMembers(constructor);

            // Add properties
            foreach (var field in fields.Where(f => !f.Item1.StartsWith("_contextProvider")))
            {
                classDeclaration = AddProperty(classDeclaration, field.Item1, field.Item2);
            }

            // Add FromModel method
            classDeclaration = AddFromModelMethod(classDeclaration);

            // Add ToNewModel method
            classDeclaration = AddToNewModelMethod(classDeclaration);

            // Add ToNewIModel method
            classDeclaration = AddToNewIModelMethod(classDeclaration);

            // Add SetEditMode async method
            classDeclaration = AddSetEditModeAsyncMethod(classDeclaration);

            // Add SaveModelVM async method
            classDeclaration = AddSaveModelVMMethod(classDeclaration);

            // Add SaveModelVM async method
            classDeclaration = AddSaveModelVMToNewModelVMMethod(classDeclaration);

            // Add SaveModelVM async method
            classDeclaration = AddAddItemToListMethod(classDeclaration);

            // Add UpdateList async method
            //classDeclaration = AddUpdateListMethod(classDeclaration);

            // Add Validate method
            classDeclaration = AddValidateMethod(classDeclaration);

            // Add Clone method
            classDeclaration = AddCloneMethod(classDeclaration);

            // Create namespace
            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(nameSpace))
                .AddMembers(classDeclaration);

            // Wrap the namespace declaration in a CompilationUnit
            var compilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Blazor.Tools.BlazorBundler.Interfaces")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.ComponentModel.DataAnnotations")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks"))
                )
                .AddMembers(namespaceDeclaration);

            // Generate syntax tree
            var syntaxTree = SyntaxFactory.SyntaxTree(compilationUnit);

            // Compile and save assembly
            CompileAndSaveAssembly(syntaxTree, assemblyPath);
        }

        private static ClassDeclarationSyntax AddProperty(ClassDeclarationSyntax classDeclaration, string fieldName, string fieldType)
        {
            var propertyName = fieldName.TrimStart('_');

            var propertyDeclaration = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(fieldType), propertyName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(fieldName)))),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(fieldName),
                                SyntaxFactory.IdentifierName("value")))))
                );

            return classDeclaration.AddMembers(propertyDeclaration);
        }

        private static ClassDeclarationSyntax AddFromModelMethod(ClassDeclarationSyntax classDeclaration)
        {
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                        .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.GenericName(SyntaxFactory.Identifier("IViewModel"))
                                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SeparatedList<TypeSyntax>(new[]
                                        {
                                    SyntaxFactory.ParseTypeName("Employee"),
                                    SyntaxFactory.ParseTypeName("IModelExtendedProperties")
                                        })))
                            ))),
                    SyntaxFactory.Identifier("FromModel"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("model"))
                        .WithType(SyntaxFactory.ParseTypeName("Employee"))
                )
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.TryStatement()
                        .WithBlock(SyntaxFactory.Block(
                            SyntaxFactory.IfStatement(
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.NotEqualsExpression,
                                    SyntaxFactory.IdentifierName("model"),
                                    SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                                ),
                                SyntaxFactory.Block(
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.AwaitExpression(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.IdentifierName("Task.Run")
                                            )
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.ParenthesizedLambdaExpression()
                                                                .WithBlock(SyntaxFactory.Block(
                                                                    SyntaxFactory.ExpressionStatement(
                                                                        SyntaxFactory.AssignmentExpression(
                                                                            SyntaxKind.SimpleAssignmentExpression,
                                                                            SyntaxFactory.IdentifierName("ID"),
                                                                            SyntaxFactory.IdentifierName("model.ID")
                                                                        )
                                                                    ),
                                                                    SyntaxFactory.ExpressionStatement(
                                                                        SyntaxFactory.AssignmentExpression(
                                                                            SyntaxKind.SimpleAssignmentExpression,
                                                                            SyntaxFactory.IdentifierName("FirstName"),
                                                                            SyntaxFactory.IdentifierName("model.FirstName")
                                                                        )
                                                                    ),
                                                                    SyntaxFactory.ExpressionStatement(
                                                                        SyntaxFactory.AssignmentExpression(
                                                                            SyntaxKind.SimpleAssignmentExpression,
                                                                            SyntaxFactory.IdentifierName("MiddleName"),
                                                                            SyntaxFactory.IdentifierName("model.MiddleName")
                                                                        )
                                                                    ),
                                                                    SyntaxFactory.ExpressionStatement(
                                                                        SyntaxFactory.AssignmentExpression(
                                                                            SyntaxKind.SimpleAssignmentExpression,
                                                                            SyntaxFactory.IdentifierName("LastName"),
                                                                            SyntaxFactory.IdentifierName("model.LastName")
                                                                        )
                                                                    ),
                                                                    SyntaxFactory.ExpressionStatement(
                                                                        SyntaxFactory.AssignmentExpression(
                                                                            SyntaxKind.SimpleAssignmentExpression,
                                                                            SyntaxFactory.IdentifierName("DateOfBirth"),
                                                                            SyntaxFactory.IdentifierName("model.DateOfBirth")
                                                                        )
                                                                    ),
                                                                    SyntaxFactory.ExpressionStatement(
                                                                        SyntaxFactory.AssignmentExpression(
                                                                            SyntaxKind.SimpleAssignmentExpression,
                                                                            SyntaxFactory.IdentifierName("CountryID"),
                                                                            SyntaxFactory.IdentifierName("model.CountryID")
                                                                        )
                                                                    )
                                                                ))
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        ))
                        .WithCatches(SyntaxFactory.SingletonList(
                            SyntaxFactory.CatchClause()
                                .WithDeclaration(
                                    SyntaxFactory.CatchDeclaration(SyntaxFactory.IdentifierName("Exception"))
                                        .WithIdentifier(SyntaxFactory.Identifier("ex"))
                                )
                                .WithBlock(SyntaxFactory.Block(
                                    SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.IdentifierName("Console.WriteLine")
                                        )
                                        .WithArgumentList(SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                                new SyntaxNodeOrToken[]{
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                    SyntaxFactory.Literal("FromModel(Employee model): {0}\r\n{1}")
                                                )
                                            ),
                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.IdentifierName("ex.Message")
                                            ),
                                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.IdentifierName("ex.StackTrace")
                                            )
                                                }
                                            )
                                        ))
                                    )
                                ))
                        ))
                )
                .AddStatements(
                    SyntaxFactory.ReturnStatement(SyntaxFactory.ThisExpression())
                ));

            return classDeclaration.AddMembers(method);
        }

        private static ClassDeclarationSyntax AddToNewModelMethod(ClassDeclarationSyntax classDeclaration)
        {
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.ParseTypeName("Employee"),
                    SyntaxFactory.Identifier("ToNewModel"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("Employee"))
                            .WithInitializer(
                                SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                                    SyntaxFactory.SeparatedList<ExpressionSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                    SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                        SyntaxFactory.IdentifierName("ID"),
                                        SyntaxFactory.IdentifierName("this.ID")),
                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                    SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                        SyntaxFactory.IdentifierName("FirstName"),
                                        SyntaxFactory.IdentifierName("this.FirstName")),
                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                    SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                        SyntaxFactory.IdentifierName("MiddleName"),
                                        SyntaxFactory.IdentifierName("this.MiddleName")),
                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                    SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                        SyntaxFactory.IdentifierName("LastName"),
                                        SyntaxFactory.IdentifierName("this.LastName")),
                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                    SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                        SyntaxFactory.IdentifierName("DateOfBirth"),
                                        SyntaxFactory.IdentifierName("this.DateOfBirth")),
                                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                                    SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                        SyntaxFactory.IdentifierName("CountryID"),
                                        SyntaxFactory.IdentifierName("this.CountryID"))
                                        }
                                    )
                                )
                            )
                    )
                ));

            return classDeclaration.AddMembers(method);
        }

        private static ClassDeclarationSyntax AddToNewIModelMethod(ClassDeclarationSyntax classDeclaration)
        {
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.ParseTypeName("IModelExtendedProperties"),
                    SyntaxFactory.Identifier("ToNewIModel"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("EmployeeVM"))
                            .WithArgumentList(
                                SyntaxFactory.ArgumentList().AddArguments(
                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("_contextProvider"))
                                )
                            )
                            .WithInitializer(
                                SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression)
                                    .AddExpressions(
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName("IsEditMode"),
                                            SyntaxFactory.IdentifierName("this.IsEditMode")
                                        ),
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName("IsVisible"),
                                            SyntaxFactory.IdentifierName("this.IsVisible")
                                        ),
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName("IsFirstCellClicked"),
                                            SyntaxFactory.IdentifierName("this.IsFirstCellClicked")
                                        ),
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName("StartCell"),
                                            SyntaxFactory.IdentifierName("this.StartCell")
                                        ),
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName("EndCell"),
                                            SyntaxFactory.IdentifierName("this.EndCell")
                                        ),
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName("RowID"),
                                            SyntaxFactory.IdentifierName("this.RowID")
                                        ),
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName("ID"),
                                            SyntaxFactory.IdentifierName("this.ID")
                                        ),
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName("FirstName"),
                                            SyntaxFactory.IdentifierName("this.FirstName")
                                        ),
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName("MiddleName"),
                                            SyntaxFactory.IdentifierName("this.MiddleName")
                                        ),
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName("LastName"),
                                            SyntaxFactory.IdentifierName("this.LastName")
                                        ),
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName("DateOfBirth"),
                                            SyntaxFactory.IdentifierName("this.DateOfBirth")
                                        ),
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName("CountryID"),
                                            SyntaxFactory.IdentifierName("this.CountryID")
                                        )
                                    )
                            )
                    )
                ));

            return classDeclaration.AddMembers(method);
        }

        private static ClassDeclarationSyntax AddSetEditModeAsyncMethod(ClassDeclarationSyntax classDeclaration)
        {
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                        .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.GenericName(SyntaxFactory.Identifier("IViewModel"))
                                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SeparatedList<TypeSyntax>(new[]
                                        {
                                    SyntaxFactory.ParseTypeName("Employee"),
                                    SyntaxFactory.ParseTypeName("IModelExtendedProperties")
                                        })))
                            ))),
                    SyntaxFactory.Identifier("SetEditMode"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("isEditMode"))
                        .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)))
                )
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName("_isEditMode"),
                        SyntaxFactory.IdentifierName("isEditMode")
                    )),
                    SyntaxFactory.ExpressionStatement(SyntaxFactory.AwaitExpression(SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("Task"),
                            SyntaxFactory.IdentifierName("CompletedTask")
                        )
                    ))),
                    SyntaxFactory.ReturnStatement(SyntaxFactory.ThisExpression())
                ));

            return classDeclaration.AddMembers(method);
        }

        private static ClassDeclarationSyntax AddSaveModelVMMethod(ClassDeclarationSyntax classDeclaration)
        {
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                        .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.GenericName(SyntaxFactory.Identifier("IViewModel"))
                                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SeparatedList<TypeSyntax>(new[]
                                        {
                                    SyntaxFactory.ParseTypeName("Employee"),
                                    SyntaxFactory.ParseTypeName("IModelExtendedProperties")
                                        })))
                            ))),
                    SyntaxFactory.Identifier("SaveModelVM"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName("IsEditMode"),
                            SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)
                        )
                    ),
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AwaitExpression(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("Task"),
                                    SyntaxFactory.IdentifierName("CompletedTask")
                                )
                            )
                        )
                    ),
                    SyntaxFactory.ReturnStatement(SyntaxFactory.ThisExpression())
                ));

            return classDeclaration.AddMembers(method);
        }

        private static ClassDeclarationSyntax AddSaveModelVMToNewModelVMMethod(ClassDeclarationSyntax classDeclaration)
        {
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                        .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.GenericName(SyntaxFactory.Identifier("IViewModel"))
                                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SeparatedList<TypeSyntax>(new[]
                                        {
                                    SyntaxFactory.ParseTypeName("Employee"),
                                    SyntaxFactory.ParseTypeName("IModelExtendedProperties")
                                        })))
                            ))),
                    SyntaxFactory.Identifier("SaveModelVMToNewModelVM"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.IdentifierName("EmployeeVM"),
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("newEmployee"))
                                    .WithInitializer(
                                        SyntaxFactory.EqualsValueClause(
                                            SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("EmployeeVM"))
                                                .WithArgumentList(
                                                    SyntaxFactory.ArgumentList(
                                                        SyntaxFactory.SingletonSeparatedList(
                                                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("_contextProvider"))
                                                        )
                                                    )
                                                )
                                                .WithInitializer(
                                                    SyntaxFactory.InitializerExpression(
                                                        SyntaxKind.ObjectInitializerExpression,
                                                        SyntaxFactory.SeparatedList<ExpressionSyntax>(new[]
                                                        {
                                                    SyntaxFactory.AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName("IsEditMode"),
                                                        SyntaxFactory.IdentifierName("IsEditMode")
                                                    ),
                                                    SyntaxFactory.AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName("IsVisible"),
                                                        SyntaxFactory.IdentifierName("IsVisible")
                                                    ),
                                                    SyntaxFactory.AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName("IsFirstCellClicked"),
                                                        SyntaxFactory.IdentifierName("IsFirstCellClicked")
                                                    ),
                                                    SyntaxFactory.AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName("StartCell"),
                                                        SyntaxFactory.IdentifierName("StartCell")
                                                    ),
                                                    SyntaxFactory.AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName("EndCell"),
                                                        SyntaxFactory.IdentifierName("EndCell")
                                                    ),
                                                    SyntaxFactory.AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName("RowID"),
                                                        SyntaxFactory.IdentifierName("RowID")
                                                    ),
                                                    SyntaxFactory.AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName("ID"),
                                                        SyntaxFactory.IdentifierName("ID")
                                                    ),
                                                    SyntaxFactory.AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName("FirstName"),
                                                        SyntaxFactory.IdentifierName("FirstName")
                                                    ),
                                                    SyntaxFactory.AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName("MiddleName"),
                                                        SyntaxFactory.IdentifierName("MiddleName")
                                                    ),
                                                    SyntaxFactory.AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName("LastName"),
                                                        SyntaxFactory.IdentifierName("LastName")
                                                    ),
                                                    SyntaxFactory.AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName("DateOfBirth"),
                                                        SyntaxFactory.IdentifierName("DateOfBirth")
                                                    ),
                                                    SyntaxFactory.AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName("CountryID"),
                                                        SyntaxFactory.IdentifierName("CountryID")
                                                    )
                                                        })
                                                    )
                                                )
                                        )
                                    )
                            )
                        )
                    ),
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AwaitExpression(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("Task"),
                                    SyntaxFactory.IdentifierName("CompletedTask")
                                )
                            )
                        )
                    ),
                    SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("newEmployee"))
                ));

            return classDeclaration.AddMembers(method);
        }

        private static ClassDeclarationSyntax AddAddItemToListMethod(ClassDeclarationSyntax classDeclaration)
        {
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                        .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.GenericName(SyntaxFactory.Identifier("IEnumerable"))
                                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                            SyntaxFactory.GenericName(SyntaxFactory.Identifier("IViewModel"))
                                                .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                                    SyntaxFactory.SeparatedList<TypeSyntax>(new[]
                                                    {
                                                SyntaxFactory.ParseTypeName("Employee"),
                                                SyntaxFactory.ParseTypeName("IModelExtendedProperties")
                                                    })))
                                        )
                                    ))
                            ))),
                    SyntaxFactory.Identifier("AddItemToList"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("modelVMList"))
                        .WithType(SyntaxFactory.GenericName(SyntaxFactory.Identifier("IEnumerable"))
                            .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("IViewModel"))
                                        .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                            SyntaxFactory.SeparatedList<TypeSyntax>(new[]
                                            {
                                        SyntaxFactory.ParseTypeName("Employee"),
                                        SyntaxFactory.ParseTypeName("IModelExtendedProperties")
                                            }))
                                        )
                                ))))
                )
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("List"))
                                .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                        SyntaxFactory.GenericName(SyntaxFactory.Identifier("IViewModel"))
                                            .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SeparatedList<TypeSyntax>(new[]
                                                {
                                            SyntaxFactory.ParseTypeName("Employee"),
                                            SyntaxFactory.ParseTypeName("IModelExtendedProperties")
                                                }))
                                            )
                                    ))))
                            .WithVariables(SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("list"))
                                    .WithInitializer(
                                        SyntaxFactory.EqualsValueClause(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.GenericName(SyntaxFactory.Identifier("ToList"))
                                                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                            SyntaxFactory.GenericName(SyntaxFactory.Identifier("IViewModel"))
                                                                .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                                                    SyntaxFactory.SeparatedList<TypeSyntax>(new[]
                                                                    {
                                                                SyntaxFactory.ParseTypeName("Employee"),
                                                                SyntaxFactory.ParseTypeName("IModelExtendedProperties")
                                                                    }))
                                                                )
                                                        ))))
                                            .WithArgumentList(
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.IdentifierName("modelVMList")
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                            )
                        )
                    ),
                    SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)))
                            .WithVariables(SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("listCount"))
                                    .WithInitializer(
                                        SyntaxFactory.EqualsValueClause(
                                            SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("list.Count"))
                                        )
                                    )
                            ))
                    ),
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName("RowID"),
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.AddExpression,
                                SyntaxFactory.IdentifierName("listCount"),
                                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1))
                            )
                        )
                    ),
                    SyntaxFactory.IfStatement(
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.GreaterThanExpression,
                            SyntaxFactory.IdentifierName("listCount"),
                            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0))
                        ),
                        SyntaxFactory.Block(
                            SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("var"))
                                        .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                                SyntaxFactory.GenericName(SyntaxFactory.Identifier("IViewModel"))
                                                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                                        SyntaxFactory.SeparatedList<TypeSyntax>(new[]
                                                        {
                                                    SyntaxFactory.ParseTypeName("Employee"),
                                                    SyntaxFactory.ParseTypeName("IModelExtendedProperties")
                                                        }))
                                                    )
                                            ))))
                                    .WithVariables(SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("firstItem"))
                                            .WithInitializer(
                                                SyntaxFactory.EqualsValueClause(
                                                    SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.IdentifierName("list"),
                                                            SyntaxFactory.IdentifierName("First")
                                                        )
                                                    )
                                                )
                                            )
                                    )
                                )
                            )
                        )
                    ),
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("list"),
                                SyntaxFactory.IdentifierName("Add")
                            )
                        )
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Argument(SyntaxFactory.ThisExpression())
                                )
                            )
                        )
                    ),
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AwaitExpression(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("Task"),
                                    SyntaxFactory.IdentifierName("CompletedTask")
                                )
                            )
                        )
                    ),
                    SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("list"))
                ));

            return classDeclaration.AddMembers(method);
        }

        //private static ClassDeclarationSyntax AddUpdateListMethod(ClassDeclarationSyntax classDeclaration)
        //{
        //    var method = SyntaxFactory.MethodDeclaration(
        //            SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
        //                .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
        //                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
        //                        SyntaxFactory.GenericName(SyntaxFactory.Identifier("IEnumerable"))
        //                            .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
        //                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
        //                                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("IViewModel"))
        //                                        .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
        //                                            SyntaxFactory.SeparatedList<TypeSyntax>(new[]
        //                                            {
        //                                        SyntaxFactory.ParseTypeName("Employee"),
        //                                        SyntaxFactory.ParseTypeName("IModelExtendedProperties")
        //                                            })))
        //                                )
        //                            ))
        //                    ))),
        //            SyntaxFactory.Identifier("UpdateList"))
        //        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
        //        .AddParameterListParameters(
        //            SyntaxFactory.Parameter(SyntaxFactory.Identifier("modelVMList"))
        //                .WithType(SyntaxFactory.GenericName(SyntaxFactory.Identifier("IEnumerable"))
        //                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
        //                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
        //                            SyntaxFactory.GenericName(SyntaxFactory.Identifier("IViewModel"))
        //                                .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
        //                                    SyntaxFactory.SeparatedList<TypeSyntax>(new[]
        //                                    {
        //                                        SyntaxFactory.ParseTypeName("Employee"),
        //                                        SyntaxFactory.ParseTypeName("IModelExtendedProperties")
        //                                    })
        //                                )
        //                            )
        //                        )
        //                    )
        //                )
        //            ),
        //            SyntaxFactory.Parameter(SyntaxFactory.Identifier("isAdding"))
        //                .WithType(SyntaxFactory.GenericName(SyntaxFactory.Identifier("bool")))
        //        )
        //        .WithBody(SyntaxFactory.Block(
        //            SyntaxFactory.LocalDeclarationStatement(
        //                SyntaxFactory.VariableDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("List"))
        //                        .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
        //                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
        //                                SyntaxFactory.GenericName(SyntaxFactory.Identifier("IViewModel"))
        //                                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
        //                                        SyntaxFactory.SeparatedList<TypeSyntax>(new[]
        //                                        {
        //                                    SyntaxFactory.ParseTypeName("Employee"),
        //                                    SyntaxFactory.ParseTypeName("IModelExtendedProperties")
        //                                        }))
        //                                    )
        //                            ))))
        //                    .WithVariables(SyntaxFactory.SingletonSeparatedList(
        //                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("list"))
        //                            .WithInitializer(
        //                                SyntaxFactory.EqualsValueClause(
        //                                    SyntaxFactory.InvocationExpression(
        //                                        SyntaxFactory.GenericName(SyntaxFactory.Identifier("ToList"))
        //                                            .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
        //                                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
        //                                                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("IViewModel"))
        //                                                        .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
        //                                                            SyntaxFactory.SeparatedList<TypeSyntax>(new[]
        //                                                            {
        //                                                        SyntaxFactory.ParseTypeName("Employee"),
        //                                                        SyntaxFactory.ParseTypeName("IModelExtendedProperties")
        //                                                            }))
        //                                                        )
        //                                                ))))
        //                                    .WithArgumentList(
        //                                        SyntaxFactory.ArgumentList(
        //                                            SyntaxFactory.SingletonSeparatedList(
        //                                                SyntaxFactory.Argument(
        //                                                    SyntaxFactory.IdentifierName("modelVMList")
        //                                                )
        //                                            )
        //                                        )
        //                                    )
        //                                )
        //                            )
        //                    )
        //                )
        //            ),
        //            SyntaxFactory.LocalDeclarationStatement(
        //                SyntaxFactory.VariableDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)))
        //                    .WithVariables(SyntaxFactory.SingletonSeparatedList(
        //                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("listCount"))
        //                            .WithInitializer(
        //                                SyntaxFactory.EqualsValueClause(
        //                                    SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("list.Count"))
        //                                )
        //                            )
        //                    ))
        //            ),
        //            SyntaxFactory.ExpressionStatement(
        //                SyntaxFactory.AssignmentExpression(
        //                    SyntaxKind.SimpleAssignmentExpression,
        //                    SyntaxFactory.IdentifierName("RowID"),
        //                    SyntaxFactory.BinaryExpression(
        //                        SyntaxKind.AddExpression,
        //                        SyntaxFactory.IdentifierName("listCount"),
        //                        SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1))
        //                    )
        //                )
        //            ),
        //            SyntaxFactory.IfStatement(
        //                SyntaxFactory.BinaryExpression(
        //                    SyntaxKind.GreaterThanExpression,
        //                    SyntaxFactory.IdentifierName("listCount"),
        //                    SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0))
        //                ),
        //                SyntaxFactory.Block(
        //                    SyntaxFactory.LocalDeclarationStatement(
        //                        SyntaxFactory.VariableDeclaration(
        //                            SyntaxFactory.GenericName(SyntaxFactory.Identifier("var"))
        //                                .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
        //                                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
        //                                        SyntaxFactory.GenericName(SyntaxFactory.Identifier("IViewModel"))
        //                                            .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
        //                                                SyntaxFactory.SeparatedList<TypeSyntax>(new[]
        //                                                {
        //                                            SyntaxFactory.ParseTypeName("Employee"),
        //                                            SyntaxFactory.ParseTypeName("IModelExtendedProperties")
        //                                                }))
        //                                            )
        //                                    ))))
        //                            .WithVariables(SyntaxFactory.SingletonSeparatedList(
        //                                SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("firstItem"))
        //                                    .WithInitializer(
        //                                        SyntaxFactory.EqualsValueClause(
        //                                            SyntaxFactory.InvocationExpression(
        //                                                SyntaxFactory.MemberAccessExpression(
        //                                                    SyntaxKind.SimpleMemberAccessExpression,
        //                                                    SyntaxFactory.IdentifierName("list"),
        //                                                    SyntaxFactory.IdentifierName("First")
        //                                                )
        //                                            )
        //                                        )
        //                                    )
        //                            )
        //                        )
        //                    )
        //                )
        //            ),
        //            SyntaxFactory.ExpressionStatement(
        //                SyntaxFactory.InvocationExpression(
        //                    SyntaxFactory.MemberAccessExpression(
        //                        SyntaxKind.SimpleMemberAccessExpression,
        //                        SyntaxFactory.IdentifierName("list"),
        //                        SyntaxFactory.IdentifierName("Add")
        //                    )
        //                )
        //                .WithArgumentList(
        //                    SyntaxFactory.ArgumentList(
        //                        SyntaxFactory.SingletonSeparatedList(
        //                            SyntaxFactory.Argument(SyntaxFactory.ThisExpression())
        //                        )
        //                    )
        //                )
        //            ),
        //            SyntaxFactory.ExpressionStatement(
        //                SyntaxFactory.AwaitExpression(
        //                    SyntaxFactory.InvocationExpression(
        //                        SyntaxFactory.MemberAccessExpression(
        //                            SyntaxKind.SimpleMemberAccessExpression,
        //                            SyntaxFactory.IdentifierName("Task"),
        //                            SyntaxFactory.IdentifierName("CompletedTask")
        //                        )
        //                    )
        //                )
        //            ),
        //            SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("list"))
        //        ));

        //    return classDeclaration.AddMembers(method);
        //}

        private static ClassDeclarationSyntax AddValidateMethod(ClassDeclarationSyntax classDeclaration)
        {
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.ParseTypeName("IEnumerable<ValidationResult>"),
                    SyntaxFactory.Identifier("Validate"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("validationContext"))
                        .WithType(SyntaxFactory.ParseTypeName("ValidationContext"))
                )
                .WithBody(SyntaxFactory.Block(
                    // Return an empty list for now
                    SyntaxFactory.ReturnStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("List<ValidationResult>"))
                        .WithArgumentList(SyntaxFactory.ArgumentList())
                    )
                ));

            return classDeclaration.AddMembers(method);
        }

        private static ClassDeclarationSyntax AddCloneMethod(ClassDeclarationSyntax classDeclaration)
        {
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.ParseTypeName("EmployeeVM"),
                    SyntaxFactory.Identifier("Clone"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ReturnStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("EmployeeVM"))
                        .WithArgumentList(SyntaxFactory.ArgumentList()
                        .AddArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("_employees"))))
                    )
                ));

            return classDeclaration.AddMembers(method);
        }

        private static void CompileAndSaveAssembly(SyntaxTree syntaxTree, string assemblyPath)
        {
            // Get the location of System.Runtime.dll
            string systemRuntimeLocation = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                @"dotnet\shared\Microsoft.NETCore.App\8.0.8\System.Runtime.dll"
            );


            //string systemRuntimeLocation = typeof(System.Runtime.InteropServices.Marshal).GetAssemblyLocation();
            Console.WriteLine("System.Runtime Location: " + systemRuntimeLocation);
            string dataAnnotationsLocation = typeof(AssociatedMetadataTypeTypeDescriptionProvider).GetAssemblyLocation();
            string interfacesLocation = typeof(IContextProvider).GetAssemblyLocation();

            // Create metadata references
            var metadataReferences = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // System.Object
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location), // System.Linq
                MetadataReference.CreateFromFile(systemRuntimeLocation), // System.Runtime
                MetadataReference.CreateFromFile(dataAnnotationsLocation), // System.ComponentModel.DataAnnotations
                MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location), // System.Collections
                MetadataReference.CreateFromFile(typeof(Task).Assembly.Location), // System.Threading.Tasks
                MetadataReference.CreateFromFile(interfacesLocation), // Blazor.Tools.BlazorBundler.Interfaces
            };

            // Create a C# compilation
            var compilation = CSharpCompilation.Create(
                "EmployeeVMAssembly",
                new[] { syntaxTree },
                metadataReferences,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            // Emit the compilation to a file
            using (var stream = new FileStream(assemblyPath, FileMode.Create))
            {
                var result = compilation.Emit(stream);
                if (!result.Success)
                {
                    var failures = result.Diagnostics
                        .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                        .ToList();
                    Console.WriteLine("Compilation failed:");
                    foreach (var diagnostic in failures)
                    {
                        Console.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                    }
                }
                else
                {
                    Console.WriteLine("Assembly generated successfully at {0}", assemblyPath);
                }
            }
        }
    }
}