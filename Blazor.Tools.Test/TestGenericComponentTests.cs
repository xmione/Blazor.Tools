using Blazor.Tools.Components.Pages;
using Bunit;
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
            var mockDynamicInstance = new Mock<IModelExtendedProperties>();
            mockDynamicInstance.SetupGet(x => x.IsEditMode).Returns(true);
            mockDynamicInstance.Setup(x => x.GetMessage()).Returns("Hello from Mock");

            // Act
            var cut = Render<TestGenericComponent>(parameters => parameters
                .Add(p => p.DynamicInstance, mockDynamicInstance.Object));

            // Assert
            cut.MarkupMatches(@"<div><input type=""text"" value=""Hello from Mock""></div>");
        }
    }

    // Mock interfaces for testing
    public interface IModelExtendedProperties
    {
        bool IsEditMode { get; set; }
        string GetMessage();
    }

    public interface IViewModel<TModel, TIModel>
    {
        Task<IViewModel<TModel, TIModel>> SetEditMode(bool isEditMode);
    }
}
