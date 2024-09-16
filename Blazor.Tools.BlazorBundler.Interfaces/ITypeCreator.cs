using System.Reflection.Emit;

namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface ITypeCreator
    {
        Type DefineInterfaceType(ModuleBuilder mb, string fullyQualifiedName);
    }
}
