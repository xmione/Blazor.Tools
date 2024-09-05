﻿/*====================================================================================================
    Class Name  : EntityClassDynamicBuilder
    Created By  : Solomio S. Sisante
    Created On  : September 3, 2024
    Purpose     : To manage the construction of an entity model class.
  ====================================================================================================*/

using Blazor.Tools.BlazorBundler.Extensions;
using System.Data;
using System.Reflection;
using System.Text;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class EntityClassDynamicBuilder
    {
        private string _nameSpace;
        private DataTable _dataTable;
        private List<string>? _usingStatements;
        private string _className;
        private DataColumnCollection _columns;
        private StringBuilder _sb;
        private Type? _classType;
        public Type? ClassType
        {
            get { return _classType; }
        }

        public EntityClassDynamicBuilder(string nameSpace, DataTable dataTable, List<string>? usingStatements)
        {
            _nameSpace = nameSpace;
            _dataTable = dataTable;
            _usingStatements = usingStatements;

            if (nameSpace == null)
            {
                throw new ArgumentNullException("nameSpace parameter is required.");
            }

            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable parameter is required.");
            }

            _className = dataTable.TableName;
            _columns = dataTable.Columns;
            _sb = new StringBuilder();

            AddUsings();
            AddNameSpace();
        }

        private void AddUsings()
        {
            if (_usingStatements != null)
            {
                foreach (string nameSpace in _usingStatements)
                {
                    _sb.AppendLine($"using {nameSpace};");
                }
                _sb.AppendLine();
            }
        }

        private void AddNameSpace()
        {
            _sb.AppendLine($"namespace {_nameSpace}");
            _sb.AppendLine("{");
            AddClass();
            _sb.AppendLine("}");

        }

        private void AddClass()
        {
            _sb.AppendLine($"\tpublic class {_className}");
            _sb.AppendLine("\t{");
            AddProperties();
            _sb.AppendLine("\t}");
        }

        private void AddProperties()
        {
            foreach (DataColumn dc in _columns)
            {
                string fieldName = dc.ColumnName;
                string fieldType = dc.DataType.ToAliasType();
                string items = $"\t\tpublic {fieldType} {fieldName} ";
                _sb.Append(items);
                _sb.AppendLine("{get; set;}");
            }

            _sb.AppendLine();
        }

        public override string ToString()
        {
            return _sb.ToString();
        }

        public void Save(string assemblyName, string classCode, string nameSpace, string className, string dllPath)
        {

            Console.WriteLine("//Class Code: \r\n{0}", classCode);
            var classGenerator = new ClassGenerator(assemblyName);


            var programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var systemPrivateCoreLibFilePath = @"dotnet\shared\Microsoft.NETCore.App\8.0.8\System.Private.CoreLib.dll";
            var systemFilePath = @"dotnet\shared\Microsoft.NETCore.App\8.0.8\System.dll";
            var systemRuntimeFilePath = @"dotnet\shared\Microsoft.NETCore.App\8.0.8\System.Runtime.dll";

            var systemPrivateCoreLibLocation = Path.Combine(programFilesPath, systemPrivateCoreLibFilePath);
            var systemLocation = Path.Combine(programFilesPath, systemFilePath);
            var systemRuntimeLocation = Path.Combine(programFilesPath, systemRuntimeFilePath);

            // Add references to existing assemblies that contain types used in the dynamic class
            classGenerator.AddReference(systemPrivateCoreLibLocation);  // Object types
            classGenerator.AddReference(systemLocation);  // System.dll
            classGenerator.AddReference(systemRuntimeLocation);  // System.Runtime.dll

            _classType = classGenerator.CreateType(classCode, nameSpace, className); // type created from memory stream does not have assembly location.

            // Save the compiled assembly to the Temp folder
            classGenerator.SaveAssemblyToTempFolder(dllPath);

            using (var assembly = DisposableAssembly.LoadFile(dllPath))
            {
                _classType = assembly.GetType($"{nameSpace}.{className}"); // type created from loading an assembly file has an assembly location.
            }
        }

    }
}
