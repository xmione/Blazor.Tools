/*====================================================================================================
    Class Name  : EntityClassDynamicBuilder
    Created By  : Solomio S. Sisante
    Created On  : September 3, 2024
    Purpose     : To manage the construction of an entity model class.
  ====================================================================================================*/

using Blazor.Tools.BlazorBundler.Extensions;
using System.Data;
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

        public DisposableAssembly? DisposableAssembly
        {
            get { return _disposableAssembly; }
            set { _disposableAssembly = value; }
        }

        public EntityClassDynamicBuilder(string nameSpace, DataTable dataTable, List<string>? usingStatements = null)
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
            _sb?.AppendLine($"\tpublic class {_className}");
            _sb?.AppendLine("\t{");
            AddProperties();
            _sb?.AppendLine("\t}");
        }

        private void AddProperties()
        {
            if (_columns != null)
            {
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

        public void Save(string assemblyName, string version, string classCode, string nameSpace, string className, string dllPath, string? baseClassCode = null)
        {
            Console.WriteLine("//Class Code: \r\n{0}", classCode);
            var classGenerator = new ClassGenerator(assemblyName, version);
            var systemPrivateCoreLibLocation = typeof(object).Assembly.Location;
            var systemLocation = Path.Combine(Path.GetDirectoryName(systemPrivateCoreLibLocation)!, "System.dll");
            var systemRuntimeLocation = Path.Combine(Path.GetDirectoryName(systemPrivateCoreLibLocation)!, "System.Runtime.dll");
            var systemCollectionsLocation = Path.Combine(Path.GetDirectoryName(systemPrivateCoreLibLocation)!, "System.Collections.dll");

            // Add references to existing assemblies that contain types used in the dynamic class
            classGenerator.AddReference(systemPrivateCoreLibLocation);  // Object types
            classGenerator.AddReference(systemLocation);  // System.dll
            classGenerator.AddReference(systemRuntimeLocation);  // System.Runtime.dll

            // Add the base class code as a module if provided
            if (baseClassCode != null)
            {
                classGenerator.AddModule(baseClassCode, nameSpace);
            }

            // Add the class code as a module if provided
            classGenerator.AddModule(classCode, nameSpace);

            // Create the type from the provided class code
            _classType = classGenerator.CreateType(classCode, nameSpace, className, baseClassCode);

            // Save the compiled assembly to the Temp folder
            classGenerator.SaveAssemblyToTempFolder(dllPath);

            while (dllPath.IsFileInUse())
            {
                Console.WriteLine("EntityClassDynamicBuilder.Save()-Before LoadFile");
                //vmDllPath.KillLockingProcesses();
                Thread.Sleep(1000);
            }

            using (var disposableAssembly = DisposableAssembly.LoadFile(dllPath))
            {
                _classType = disposableAssembly.GetType($"{nameSpace}.{className}"); // type created from loading an assembly file has an assembly location.
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
