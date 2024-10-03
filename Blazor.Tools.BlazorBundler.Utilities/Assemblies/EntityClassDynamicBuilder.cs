/*====================================================================================================
    Class Name  : EntityClassDynamicBuilder
    Created By  : Solomio S. Sisante
    Created On  : September 3, 2024
    Purpose     : To manage the construction of an entity model class.
  ====================================================================================================*/

using Blazor.Tools.BlazorBundler.Extensions;
using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualBasic.FileIO;
using System.Data;
using System.Reflection;
using System.Text;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class EntityClassDynamicBuilder : IDisposable
    {
        private string? _nameSpace;
        private DataTable? _dataTable;
        private List<string>? _usingStatements;
        private string? _className;
        private DataColumnCollection? _columns;
        private StringBuilder? _sb;
        private bool _disposed;

        private Type? _classType;

        public Type? ClassType
        {
            get { return _classType; }
        }

        private DisposableAssembly? _disposableAssembly;
        private byte[]? _assemblyBytes;
        private ClassGenerator _classGenerator;
        private List<string>? _assemblyLocations;

        public DisposableAssembly? DisposableAssembly
        {
            get { return _disposableAssembly; }
            set { _disposableAssembly = value; }
        }

        public EntityClassDynamicBuilder(string nameSpace, DataTable dataTable, List<string>? assemblyLocations = null, List<string>? usingStatements = null)
        {
            _nameSpace = nameSpace;
            _dataTable = dataTable;
            _assemblyLocations = assemblyLocations;
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

        public void AddReferences(List<string> assemblyLocations)
        {
            _assemblyLocations = assemblyLocations;
        }

        private void AddUsings()
        {
            if (_usingStatements != null)
            {
                foreach (string nameSpace in _usingStatements)
                {
                    _sb?.AppendLine($"using {nameSpace};");
                }

                _sb?.AppendLine();
            }
        }

        private void AddNameSpace()
        {
            _sb?.AppendLine($"namespace {_nameSpace}");
            _sb?.AppendLine("{");
            AddClass();
            _sb?.AppendLine("}");

        }

        private void AddClass()
        {
            _sb?.AppendLine($"\tpublic class {_className}: IBase");
            _sb?.AppendLine("\t{");
            AddProperties();
            _sb?.AppendLine("\t}");
        }

        private void AddProperties()
        {
            if (_columns != null)
            {
                bool isIDNotFound = !_columns.Contains("ID");
                if (isIDNotFound)
                {
                    _sb?.Append($"\t\tpublic int ID ");
                    _sb?.AppendLine("{get; set;}");
                }

                foreach (DataColumn dc in _columns)
                {
                    string fieldName = dc.ColumnName;
                    string fieldType = dc.DataType.ToAliasType();
                    string items = $"\t\tpublic {fieldType} {fieldName} ";
                    _sb?.Append(items);
                    _sb?.AppendLine("{get; set;}");
                }
            }

            _sb?.AppendLine();
        }

        public override string ToString()
        {
            string stringValue = string.Empty;

            if (_sb != null)
            {
                stringValue = _sb.ToString();
            }

            return stringValue;
        }

        public byte[] EmitAssemblyToMemorySave(string assemblyName, string version, string dllPath, params string[] sourceCodes)
        {

            foreach (var sourceCode in sourceCodes)
            {
                AppLogger.WriteInfo(sourceCode);
            }

            _classGenerator = new ClassGenerator(assemblyName, version);
            var systemPrivateCoreLibLocation = typeof(object).Assembly.Location;
            var systemLocation = Path.Combine(Path.GetDirectoryName(systemPrivateCoreLibLocation)!, "System.dll");
            var systemRuntimeLocation = Path.Combine(Path.GetDirectoryName(systemPrivateCoreLibLocation)!, "System.Runtime.dll");
            var systemCollectionsLocation = Path.Combine(Path.GetDirectoryName(systemPrivateCoreLibLocation)!, "System.Collections.dll");

            // Add references to existing assemblies that contain types used in the dynamic class
            _classGenerator.AddReference(systemPrivateCoreLibLocation);  // Object types
            _classGenerator.AddReference(systemLocation);  // System.dll
            _classGenerator.AddReference(systemRuntimeLocation);  // System.Runtime.dll

            // Add references from the assembly location list
            foreach (var assemblyLocation in _assemblyLocations!)
            {
                _classGenerator.AddReference(assemblyLocation);
            }
            
            // Create the type from the provided class code
            _classType = _classGenerator.CreateType(_nameSpace!, _className!, sourceCodes);
            _assemblyBytes = _classGenerator.AssemblyBytes;
            return _assemblyBytes;
        }

        public void LoadAssembly()
        {
            using (var disposableAssembly = DisposableAssembly.LoadAssembly(_assemblyBytes!))
            {
                _disposableAssembly = disposableAssembly;
            }
        }

        public void Save(string dllPath)
        {

            // Save the compiled assembly to the Temp folder
            _classGenerator.SaveAssemblyToTempFolder(dllPath);

            while (dllPath.IsFileInUse())
            {
                Console.WriteLine("EntityClassDynamicBuilder.Save()-Before LoadFile");
                //vmDllPath.KillLockingProcesses();
                Thread.Sleep(1000);
            }

            using (var disposableAssembly = DisposableAssembly.LoadFile(dllPath))
            {
                _classType = disposableAssembly.GetType($"{_nameSpace}.{_className}"); // type created from loading an assembly file has an assembly location.
                _disposableAssembly = disposableAssembly;
            }

            while (dllPath.IsFileInUse())
            {
                Console.WriteLine("EntityClassDynamicBuilder.Save()-After LoadFile");
                //vmDllPath.KillLockingProcesses();
                Thread.Sleep(1000);
            }
        }
        
        // Implement IDisposable to clean up resources
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected method for cleanup logic
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Perform any necessary cleanup here
                    _nameSpace = null;
                    _dataTable = null;
                    _usingStatements = null;
                    _className = null;
                    _columns = null;
                    _sb = null;
                    _classType = null;
                    _assemblyBytes = null;
                }

                _disposed = true;
            }
        }

        // Destructor to ensure resources are cleaned up
        ~EntityClassDynamicBuilder()
        {
            Dispose(false);
        }
    }
}
