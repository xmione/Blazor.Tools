using System.Reflection;
using System;
using System.Runtime.Loader;
using DocumentFormat.OpenXml;

namespace Blazor.Tools.BlazorBundler.Utilities.Assemblies
{
    public class DisposableAssembly : IDisposable
    {
        private Assembly _assembly;
        private bool _disposed = false;
        private AssemblyLoadContext _context;

        // Constructor to initialize with an Assembly and LoadContext
        public DisposableAssembly(Assembly assembly, AssemblyLoadContext context)
        {
            _assembly = assembly;
            _context = context;
        }

        // Public property to access the underlying Assembly
        public Assembly Assembly => _assembly;

        // Static method to load an assembly from a file and return a DisposableAssembly
        public static DisposableAssembly LoadFile(string path)
        {
            // Read the DLL file into a byte array
            byte[] assemblyBytes = File.ReadAllBytes(path);

            // Create a new AssemblyLoadContext to manage the assembly
            var context = new AssemblyLoadContext(null, true);

            // Load the assembly from dll file
            // Assembly assembly = Assembly.LoadFile(path);
            
            // Load the assembly from the byte array
            Assembly assembly = context.LoadFromStream(new MemoryStream(assemblyBytes));

            return new DisposableAssembly(assembly, context);
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

                    // Unload the assembly
                    _context.Unload();
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
