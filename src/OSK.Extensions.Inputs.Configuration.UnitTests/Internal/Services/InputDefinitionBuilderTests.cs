using Moq;
using OSK.Extensions.Inputs.Configuration.Internal.Services;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Extensions.Inputs.Configuration.UnitTests._Helpers;
using OSK.Extensions.Inputs.Configuration.Ports;

namespace OSK.Extensions.Inputs.Configuration.UnitTests.Internal.Services;

public class InputDefinitionBuilderTests
{
    #region WithActions

    [Fact]
    public void WithActions_TestRegistrationService_Valid_ReturnsBuiltInputDefinition()
    {
        // Arrange
        var builder = new InputDefinitionBuilder("Abc", new Mock<IInputSystemConfigurationBuilder>().Object);

        builder.WithActions<TestRegistrationService>();

        // Act
        var definition = builder.Build();

        // Assert
        Assert.NotNull(definition);

        Assert.Equal(2, definition.Actions.Count);

        Assert.True(definition.Actions.Count(action => nameof(TestRegistrationService.ValidMethodA).Equals(action.Name)) == 1);
        Assert.True(definition.Actions.Count(action => "SpecialAction".Equals(action.Name)) == 1);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(m => m.GetService(It.Is<Type>(type => type == typeof(TestRegistrationService))))
            .Returns(new TestRegistrationService());

        definition.Actions.First().ActionExecutor(new InputEventContext(1, null!, null!, null!, mockServiceProvider.Object));
    }

    #endregion
}
