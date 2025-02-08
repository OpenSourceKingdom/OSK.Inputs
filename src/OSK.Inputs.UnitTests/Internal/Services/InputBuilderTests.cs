using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputBuilderTests
{
    #region Variables

    private readonly Mock<IInputDefinitionBuilder> _definitionBuilder;

    private readonly InputBuilder _builder;

    #endregion

    #region Constructors

    public InputBuilderTests()
    {
        _definitionBuilder = new Mock<IInputDefinitionBuilder>();

        _builder = new(new ServiceCollection(), _ => _definitionBuilder.Object);
    }

    #endregion

    #region AddInputDefinition

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void AddInputDefinition_InvalidDefinitionName_ThrowsArgumentNullException(string definitionName)
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddInputDefinition(definitionName, _ => { }));
    }

    [Fact]
    public void AddInputDefinition_NullAction_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddInputDefinition("test", null!));
    }

    [Fact]
    public void AddInputDefinition_DuplicateDefinitionName_ThrowsDuplicateNameException()
    {
        // Arrange
        _builder.AddInputDefinition("test", _ => { });

        // Act/Assert
        Assert.Throws<DuplicateNameException>(() => _builder.AddInputDefinition("test", _ => { }));
    }

    [Fact]
    public void AddInputDefinition_Valid_ReturnsSuccesfully()
    {
        // Arrange/Act/Assert
        _builder.AddInputDefinition("test", _ => { });
    }

    #endregion

    #region WithHandlerOptions

    [Fact]
    public void WithHandlerOptions_NullActionConfiguration_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.WithHandlerOptions(null!));
    }

    [Fact]
    public void WithHandlerOptions_Valid_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.WithHandlerOptions(_ => { });
    }

    #endregion

    #region UseInputSchemeRepository

    [Fact]
    public void UseInputSchemeRepository_Valid_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.UseInputSchemeRepository<TestSchemeRepository>();
    }

    #endregion

    #region ApplyInputDefinitions (Internal)

    [Fact]
    public void ApplyInputDefinitions_DefinitionAllowsCustomSchemes_SetsCustomSchemeRepository_ReturnsSuccessfully()
    {
        // Arrange
        _definitionBuilder.Setup(m => m.Build())
            .Returns(new InputDefinition("", false, [], []));

        _builder
            .AddInputDefinition("test", definitionBuilder =>
            {
                definitionBuilder.AllowCustomInputSchemes();
            })
            .UseInputSchemeRepository<TestSchemeRepository>(); ;

        // Act/Assert
        _builder.ApplyInputDefinitions();
    }

    [Fact]
    public void ApplyInputDefinitions_DefinitionDoesNotAllowCustomSchemes_DoesNotCustomSchemeRepository_ReturnsSuccessfully()
    {
        // Arrange
        _definitionBuilder.Setup(m => m.Build())
            .Returns(new InputDefinition("", false, [], []));

        _builder 
            .AddInputDefinition("test", definitionBuilder => { })
            .UseInputSchemeRepository<TestSchemeRepository>(); ;

        // Act/Assert
        _builder.ApplyInputDefinitions();
    }

    #endregion
}
