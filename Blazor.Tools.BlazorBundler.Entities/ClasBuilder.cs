/*====================================================================================================
    Class Name  : ClassBuilder
    Created By  : Solomio S. Sisante
    Created On  : August 31, 2024
    Purpose     : To create Assembly classes dynamically and save them to a .dll file.
  ====================================================================================================*/
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class ClassBuilder
    {
        private readonly ModuleDefinition _module;
        private readonly TypeDefinition _typeDefinition;
        private readonly List<MethodDefinition> _constructors = new List<MethodDefinition>();

        public ClassBuilder(string className, string @namespace, ModuleDefinition module, TypeAttributes attributes = TypeAttributes.Public | TypeAttributes.Class)
        {
            _module = module;
            _typeDefinition = new TypeDefinition(@namespace, className, attributes, module.ImportReference(typeof(object)));
        }

        public ClassBuilder SetBaseClass(Type baseClass)
        {
            _typeDefinition.BaseType = _module.ImportReference(baseClass);
            return this;
        }

        public ClassBuilder AddInterface(Type interfaceType)
        {
            _typeDefinition.Interfaces.Add(new InterfaceImplementation(_module.ImportReference(interfaceType)));
            return this;
        }

        public ClassBuilder AddField(string fieldName, Type fieldType, FieldAttributes attributes = FieldAttributes.Private)
        {
            var field = new FieldDefinition(fieldName, attributes, _module.ImportReference(fieldType));
            _typeDefinition.Fields.Add(field);
            return this;
        }

        public ClassBuilder AddProperty(string propertyName, FieldDefinition backingField, MethodAttributes accessorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig)
        {
            var property = new PropertyDefinition(propertyName, PropertyAttributes.None, backingField.FieldType);

            var getMethod = new MethodDefinition($"get_{propertyName}", accessorAttributes, backingField.FieldType);
            var ilProcessor = getMethod.Body.GetILProcessor();
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldfld, backingField);
            ilProcessor.Emit(OpCodes.Ret);
            property.GetMethod = getMethod;
            _typeDefinition.Methods.Add(getMethod);

            var setMethod = new MethodDefinition($"set_{propertyName}", accessorAttributes, _module.ImportReference(typeof(void)));
            setMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, backingField.FieldType));
            ilProcessor = setMethod.Body.GetILProcessor();
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.Emit(OpCodes.Stfld, backingField);
            ilProcessor.Emit(OpCodes.Ret);
            property.SetMethod = setMethod;
            _typeDefinition.Methods.Add(setMethod);

            _typeDefinition.Properties.Add(property);
            return this;
        }

        public ClassBuilder AddConstructor(Action<ILProcessor> bodyAction)
        {
            var ctor = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, _module.ImportReference(typeof(void)));
            var ilProcessor = ctor.Body.GetILProcessor();
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Call, _module.ImportReference(typeof(object).GetConstructor(Type.EmptyTypes)));
            bodyAction(ilProcessor);
            ilProcessor.Emit(OpCodes.Ret);
            _typeDefinition.Methods.Add(ctor);
            return this;
        }

        public ClassBuilder AddMethod(string methodName, Type returnType, MethodAttributes attributes, Action<ILProcessor> bodyAction, params Type[] parameterTypes)
        {
            var method = new MethodDefinition(methodName, attributes, _module.ImportReference(returnType));
            foreach (var paramType in parameterTypes)
            {
                method.Parameters.Add(new ParameterDefinition(_module.ImportReference(paramType)));
            }
            var ilProcessor = method.Body.GetILProcessor();
            bodyAction(ilProcessor);
            ilProcessor.Emit(OpCodes.Ret);
            _typeDefinition.Methods.Add(method);
            return this;
        }

        public ClassBuilder AddEvent(string eventName, Type eventHandlerType, FieldAttributes fieldAttributes = FieldAttributes.Private)
        {
            var eventField = new FieldDefinition(eventName, fieldAttributes, _module.ImportReference(eventHandlerType));
            _typeDefinition.Fields.Add(eventField);

            var eventDef = new EventDefinition(eventName, EventAttributes.None, _module.ImportReference(eventHandlerType))
            {
                AddMethod = CreateEventMethod($"add_{eventName}", eventField),
                RemoveMethod = CreateEventMethod($"remove_{eventName}", eventField)
            };
            _typeDefinition.Events.Add(eventDef);

            return this;
        }

        private MethodDefinition CreateEventMethod(string methodName, FieldDefinition eventField)
        {
            var method = new MethodDefinition(methodName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, _module.ImportReference(typeof(void)));
            var ilProcessor = method.Body.GetILProcessor();
            ilProcessor.Emit(OpCodes.Ret);
            return method;
        }

        public TypeDefinition Build()
        {
            _module.Types.Add(_typeDefinition);
            return _typeDefinition;
        }

        //public static void GenerateEmployeeVM(string assemblyPath)
        //{
        //    var assemblyDefinition = AssemblyDefinition.CreateAssembly(
        //        new AssemblyNameDefinition("Blazor.Tools.BlazorBundler.Entities.SampleObjects", new Version(1, 0)),
        //        "Blazor.Tools.BlazorBundler.Entities.SampleObjects",
        //        ModuleKind.Dll);

        //    var module = assemblyDefinition.MainModule;

        //    // Define EmployeeVM class
        //    var employeeVMBuilder = new ClassBuilder("EmployeeVM", "Blazor.Tools.BlazorBundler.Entities.SampleObjects", module)
        //        .SetBaseClass(typeof(Employee))
        //        .AddInterface(typeof(IValidatableObject))
        //        .AddInterface(typeof(ICloneable<EmployeeVM>))
        //        .AddInterface(typeof(IViewModel<Employee, IModelExtendedProperties>))
        //        .AddField("_employees", typeof(List<EmployeeVM>))
        //        .AddField("_contextProvider", typeof(IContextProvider), FieldAttributes.Private | FieldAttributes.InitOnly)
        //        .AddField("_rowID", typeof(int), FieldAttributes.Public)
        //        .AddField("_isEditMode", typeof(bool), FieldAttributes.Public)
        //        .AddField("_isVisible", typeof(bool), FieldAttributes.Public)
        //        .AddField("_startCell", typeof(int), FieldAttributes.Public)
        //        .AddField("_endCell", typeof(int), FieldAttributes.Public)
        //        .AddField("_isFirstCellClicked", typeof(bool), FieldAttributes.Public);

        //    var rowIDField = employeeVMBuilder.AddField("_rowID", typeof(int)).Build().Fields.First(f => f.Name == "_rowID");

        //    employeeVMBuilder
        //        .AddProperty("RowID", rowIDField)
        //        .AddConstructor(il =>
        //        {
        //            il.Emit(OpCodes.Ldarg_0);
        //            il.Emit(OpCodes.Newobj, module.ImportReference(typeof(List<EmployeeVM>).GetConstructor(Type.EmptyTypes)));
        //            il.Emit(OpCodes.Stfld, employeeVMBuilder.Build().Fields.First(f => f.Name == "_employees"));

        //            il.Emit(OpCodes.Ldarg_0);
        //            il.Emit(OpCodes.Newobj, module.ImportReference(typeof(ContextProvider).GetConstructor(Type.EmptyTypes)));
        //            il.Emit(OpCodes.Stfld, employeeVMBuilder.Build().Fields.First(f => f.Name == "_contextProvider"));

        //            il.Emit(OpCodes.Ldarg_0);
        //            il.Emit(OpCodes.Ldc_I4_0);
        //            il.Emit(OpCodes.Stfld, rowIDField);
        //        });

        //    // Add the class to the module and save
        //    employeeVMBuilder.Build();
        //    assemblyDefinition.Write(assemblyPath);
        //}

    }

}
