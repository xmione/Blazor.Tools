using Blazor.Tools.BlazorBundler.Interfaces;
using Blazor.Tools.Components.Pages;
using Bunit;
using DocumentFormat.OpenXml.Spreadsheet;
using Moq;

namespace Blazor.Tools.Test
{
    [TestClass]
    public class TestGenericComponentTests : BunitContext
    {
        [TestMethod]
        public void TestGenericComponent_ShouldRenderCorrectly()
        {
            // Arrange
            var mockDynamicInstance = new Mock<ClassC>();
            mockDynamicInstance.SetupGet(x => x.IsEditMode).Returns(true);

            // Set the message first
            mockDynamicInstance.Setup(x => x.SetMessage(It.IsAny<string>())).Callback<string>(msg =>
            {
                mockDynamicInstance.Setup(x => x.GetMessage()).Returns(msg);
            });

            // Act
            mockDynamicInstance.Object.SetMessage("Hello from Mock");
             
            var cut = Render<TestGenericComponent>(parameters => parameters
                .Add(p => p.DynamicInstance, mockDynamicInstance.Object));

            // Assert
            cut.MarkupMatches(@"<div><input type=""text"" value=""Hello from Mock""></div>");
        }
    }

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
