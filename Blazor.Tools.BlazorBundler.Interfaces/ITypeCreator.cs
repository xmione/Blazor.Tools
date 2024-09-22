using System.Reflection;
using System.Reflection.Emit;

namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface ITypeCreator
    {
        AssemblyName DefineAssemblyName(string contextAssemblyName = null!, string version = null!);
        PersistedAssemblyBuilder DefineAssemblyBuilder(AssemblyName? assemblyName = null, AssemblyBuilderAccess? assemblyBuilderAccess = null);
        ModuleBuilder DefineModule(PersistedAssemblyBuilder assemblyBuilder = null!, string moduleName = null!);
        Type DefineInterfaceType(ModuleBuilder mb, string fullyQualifiedName);
    }
}
