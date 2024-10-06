using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Assemblies;
using Blazor.Tools.BlazorBundler.Utilities.Exceptions;
using Blazor.Tools.Components.Pages;
using Bunit;
using DocumentFormat.OpenXml.Spreadsheet;
using Moq;
using System.Reflection.Metadata;
using System.Text;

namespace Blazor.Tools.Test
{
    /// <summary>
    /// Test Generic Component's use of Dynamically created assembly classes.
    /// Steps:
    /// 
    /// 1. Compose the source code for class to mock. This class will be used to pass an object to a component.
    /// 2. Compile the mock class and mock it.
    /// 3. Compose all the source codes for base and derived types.
    /// 4. Compile the source codes and mock them with a specified value.
    /// 5. Test component by passing the mock object with the specified value.
    /// 6. Check if expected markup is equal to actual markup.
    /// </summary>
    [TestClass]
    public class TestGenericComponentTests : BunitContext
    {
        [TestMethod]
        public void TestGenericComponent_ShouldRenderCorrectly()
        {
            var combinedSourceCode = @"
using System;
using System.Threading.Tasks;
using Blazor.Tools.BlazorBundler.Interfaces;

namespace Models
{
    public class TestM : IBase
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
    }

    public class TestVM : ITestVM<IBase, ITestMEP>, IBase, ITestMEP
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public virtual bool IsEditMode { get; set; }

        private string _message;

        public virtual void SetMessage(string message)
        {
            _message = message;
        }

        public virtual string GetMessage()
        {
            return _message;
        }

        public async Task<ITestVM<IBase, ITestMEP>> SetEditMode(bool isEditMode)
        {
            IsEditMode = isEditMode;
            await Task.CompletedTask;
            return this;
        }
    }
}
";
            // Compile the class dynamically
            var assemblyEmitter = new AssemblyEmitter();
            assemblyEmitter.EmitAssemblyToMemory("CombinedAssembly", combinedSourceCode);
            //assemblyEmitter.EmitAssemblyToMemory("CombinedAssembly", interfaceSourceCode, combinedSourceCode);
            assemblyEmitter.LoadAssembly();

            // Get the dynamically generated type TestVM
            var testVMType = assemblyEmitter.CombinedAssembly.GetType("Models.TestVM")!;
            var testMType = assemblyEmitter.CombinedAssembly.GetType("Models.TestM")!;
            //var iBaseType = assemblyEmitter.CombinedAssembly.GetType("Blazor.Tools.BlazorBundler.Interfaces.IBase")!;
            //var iTestMEPType = assemblyEmitter.CombinedAssembly.GetType("Blazor.Tools.BlazorBundler.Interfaces.ITestMEP")!;
            //var iTestVMType = assemblyEmitter.CombinedAssembly.GetType("Blazor.Tools.BlazorBundler.Interfaces.ITestVM`2")!;
            //var genericITestVM = iTestVMType.MakeGenericType(testMType, iTestMEPType);
            var genericITestVM = typeof(ITestVM<,>).MakeGenericType(testMType, typeof(ITestMEP));
            var typeNames = testVMType.Assembly.GetTypes().Select(dynamicType => dynamicType.FullName);

            var sb = new StringBuilder();
            sb.AppendLine("Registered types in Models.TestVM:");
            sb.AppendLine("----------------------------------------");
            foreach (var typeName in typeNames)
            {
                sb.AppendLine(typeName);
            }

            AppLogger.WriteInfo($"Generic ITestVM: {genericITestVM.FullName}");
            AppLogger.WriteInfo(sb.ToString());


            // Check if testVMType is assignable to ITestVM<IBase, ITestMEP>
            // Create a closed generic type for ITestVM<IBase, ITestMEP>
            var iTestVMType = typeof(ITestVM<,>).MakeGenericType(typeof(IBase), typeof(ITestMEP));

            // Now check if the dynamic type is assignable to the closed generic interface
            var isAssignable = iTestVMType.IsAssignableFrom(testVMType);

            AppLogger.WriteInfo($"ITestVM<,> Assembly: {typeof(ITestVM<,>).Assembly.FullName}");
            AppLogger.WriteInfo($"TestVM Assembly: {testVMType.Assembly.FullName}");

            Assert.IsTrue(isAssignable, "The dynamic type does not implement ITestVM<IBase, ITestMEP>");

            // Create an instance of the dynamically generated type
            var dynamicInstance = (ITestVM<IBase, ITestMEP>)Activator.CreateInstance(testVMType)!;

            // Use reflection to set the FirstName property
            var firstNameProperty = testVMType.GetProperty("FirstName");
            if (firstNameProperty != null)
            {
                firstNameProperty.SetValue(dynamicInstance, "John");
            }

            // Create a mock of the interface implemented by TestVM using Moq
            var mockDynamicInstance = new Mock<ITestVM<IBase, ITestMEP>>();
            // Setup the mock to return some expected value
            mockDynamicInstance.SetupGet(x => x.IsEditMode).Returns(true);
            mockDynamicInstance.Setup(x => x.GetMessage()).Returns("Hello from Mock");

            // Act: Set the message using the mock
            mockDynamicInstance.Object.SetMessage("Hello from Mock");

            // Render the component and pass the dynamic instance or mock object to the DynamicInstance parameter
            var cut = Render<TestGenericComponent>(parameters => parameters
                .Add(p => p.DynamicInstance, mockDynamicInstance.Object));

            // Check if the DynamicInstance is not null
            Assert.IsNotNull(cut.Instance.DynamicInstance, "DynamicInstance should not be null");

            // Assert that the rendered markup matches the expected output
            cut.MarkupMatches(@"<div><input type=""text"" value=""Hello from Mock""></div>");

            // Assert that the FirstName property is set correctly using reflection
            var firstNameValue = firstNameProperty?.GetValue(dynamicInstance);
            Assert.AreEqual("John", firstNameValue);


        }

    }
}
