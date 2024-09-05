using System.Reflection;
using System;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class DisposableAssembly : IDisposable
    {
        private Assembly _assembly;
        private bool _disposed = false;

        // Constructor to initialize with an Assembly
        public DisposableAssembly(Assembly assembly)
        {
            _assembly = assembly;
        }

        // Public property to access the underlying Assembly
        public Assembly Assembly => _assembly;

        // Static method to load an assembly from a file and return a DisposableAssembly
        public static DisposableAssembly LoadFile(string path)
        {
            Assembly assembly = Assembly.LoadFile(path);
            return new DisposableAssembly(assembly);
        }

        // Instance method to get a Type from the loaded assembly
        public Type GetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentException("Parameter typeName is required.");
            }

            return _assembly.GetType(typeName);
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
                    _assembly = null;
                }
                _disposed = true;
            }
        }

        // Destructor to ensure resources are cleaned up
        ~DisposableAssembly()
        {
            Dispose(false);
        }
    }
}
