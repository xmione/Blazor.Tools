using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.BlazorBundler.Utilities.Assemblies;
using Blazor.Tools.Components.Pages;
using Bunit;
using DocumentFormat.OpenXml.Spreadsheet;
using Moq;

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
            var classCSourceCode = @"
using System;
using System.Threading.Tasks;
using Blazor.Tools.BlazorBundler.Interfaces;

namespace Models
{
    public class ClassC : ITestVM<IBase, ITestMEP>, IBase, ITestMEP
    {
        public int ID { get; set; }
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
            var combinedAssemblyBytes = assemblyEmitter.EmitAssemblyToMemory("CombinedAssembly", classCSourceCode);
            assemblyEmitter.LoadAssembly();

            // Get the dynamically generated type ClassC
            var dynamicType = assemblyEmitter.CombinedAssembly.GetType("Models.ClassC")!;

            // Create an instance of the dynamically generated type
            var dynamicInstance = (ITestVM<IBase, ITestMEP>)Activator.CreateInstance(dynamicType)!;

            // Create a mock of the interface implemented by ClassC using Moq
            var mockDynamicInstance = new Mock<ITestVM<IBase, ITestMEP>>();
            mockDynamicInstance.SetupGet(x => x.IsEditMode).Returns(true);

            // Set the message and use the callback to simulate dynamic behavior
            mockDynamicInstance.Setup(x => x.SetMessage(It.IsAny<string>())).Callback<string>(msg =>
            {
                mockDynamicInstance.Setup(x => x.GetMessage()).Returns(msg);
            });

            // Act: Set the message using the mock
            mockDynamicInstance.Object.SetMessage("Hello from Mock");

            // Render the component and pass the mock object to the DynamicInstance parameter
            var cut = Render<TestGenericComponent>(parameters => parameters
                .Add(p => p.DynamicInstance, mockDynamicInstance.Object));

            // Assert that the rendered markup matches the expected output
            cut.MarkupMatches(@"<div><input type=""text"" value=""Hello from Mock""></div>");
        }
    }
}
