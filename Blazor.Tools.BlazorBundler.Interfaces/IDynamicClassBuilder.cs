using System.Data;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.Loader;

namespace Blazor.Tools.BlazorBundler.Interfaces
{
    public interface IDynamicClassBuilder: IDisposable
    {
        public Assembly? Assembly { get; }
        public Type? DynamicType { get; set; }
        public TypeBuilder? TypeBuilder { get; set; }
        public List<PropertyBuilder>? AddedProperties { get; set; }
        public List<FieldBuilder>? AddedFields { get; set; }
        public string? AssemblyFilePath { get; set; }
        public AssemblyLoadContext? AssemblyLoadContext { get; set; }

        void CreatePropertyInfoFromDataTable(DataTable dataTable);
        void CreateBaseClassType(string? baseClassName = null, Type[]? interfaces = null);
        TypeBuilder? DefineTypes(ModuleBuilder mb, string fullyQualifiedClassName, TypeAttributes attr, Type? parent, Type[]? types = null);
        FieldBuilder DefineField(string fieldName, Type fieldType, FieldAttributes fieldAttributes);
        void AddProperty(string propertyName, Type propertyType);
        void DefineConstructor(Type[] parameterTypes, Action<ILGenerator> generateConstructorBody);
        IReadOnlyList<(ConstructorBuilder ConstructorBuilder, Type[] ParameterTypes)> GetDefinedConstructors();
        Type? CreateType();
        void CreateClassFromDataTable(DataTable? table);
        void DefineMethod(string methodName, Type? returnType, Type[] parameterTypes, string[]? parameterNames, Action<ILGenerator, LocalBuilder?> generateMethodBody);
        public ConstructorInfo[] GetConstructors();
        public void SaveAssembly(string? assemblyFilePath = null, bool saveToStream = false);
        public void DeleteAssembly();
    }
}
