using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Functions.Outputs.Mocks;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Notifications;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Internal;
using OSK.Inputs.Internal.Models;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Options;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputUserManagerTests
{
    #region Variables

    private readonly Mock<IInputConfigurationProvider> _mockConfigurationProvider;
    private readonly Mock<IInputNotificationPublisher> _mockNotificationPublisher;
    private readonly Mock<IInputSchemeRepository> _mockSchemeRepository;

    private readonly IOutputFactory<InputUserManager> _outputFactory;

    private readonly InputUserManager _manager;

    #endregion

    #region Constructors

    public InputUserManagerTests()
    {
        _mockConfigurationProvider = new();
        _mockNotificationPublisher = new();
        _mockSchemeRepository = new();
        _outputFactory = new MockOutputFactory<InputUserManager>();

        _manager = new InputUserManager(_mockConfigurationProvider.Object, _mockNotificationPublisher.Object, _mockSchemeRepository.Object,
            Mock.Of<ILogger<InputUserManager>>(), _outputFactory);
    }

    #endregion

    #region CreateUser

    [Fact]
    public void CreateUser_NullOptions_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _manager.CreateUser(null!));
    }

    [Fact]
    public void CreateUser_UsersAtMaxLimit_ReturnsError()
    {
        // Arrange
        _mockConfigurationProvider.Setup(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [], new(), new InputSystemJoinPolicy() { MaxUsers = 0 }));

        // Act
        var output = _manager.CreateUser(new UserJoinOptions());

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public void CreateUser_JoinOptionsIncludeDevices_DeviceAlreadyPaired_ReturnsError()
    {
        // Arrange
        _mockConfigurationProvider.Setup(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [], new(), new InputSystemJoinPolicy() { MaxUsers = 2 }));

        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));
        _manager._users[1] = user;

        // Act
        var output = _manager.CreateUser(new UserJoinOptions() { DevicesToPair = [new RuntimeDeviceIdentifier(1, TestIdentity.Identity1)] });

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public void CreateUser_NoOptions_UsesDefaultDefinition_ReturnsSuccessfullyAndNotifiesNewUser()
    {
        // Arrange
        _mockConfigurationProvider.Setup(m => m.Configuration)
            .Returns(new InputSystemConfiguration([],
                [
                    new InputDefinition("Abc",
                    [
                        new InputScheme("Abc", [], false, false)
                    ], [], false),
                    new InputDefinition("Def",
                    [
                        new InputScheme("Def", [], false, false)
                    ], [], true)
                ],
                new(), new InputSystemJoinPolicy() { MaxUsers = 2 }));

        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));
        _manager._users[1] = user;

        // Act
        var output = _manager.CreateUser(new UserJoinOptions());

        // Assert
        Assert.True(output.IsSuccessful);
        Assert.Equal("Def", output.Value.ActiveScheme.DefinitionName);

        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<IInputNotification>(i => i is InputUserJoinedNotification)), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateUser_OptionsUseInvalidActiveSchemeDefinitionName_ReturnsSuccessfully(string? definitionName)
    {
        // Arrange
        _mockConfigurationProvider.Setup(m => m.Configuration)
            .Returns(new InputSystemConfiguration([],
                [
                    new InputDefinition("Abc",
                    [
                        new InputScheme("Abc", [], false, false)
                    ], [], false),
                    new InputDefinition("Def",
                    [
                        new InputScheme("Def", [], false, false)
                    ], [], false)
                ],
                new(), new InputSystemJoinPolicy() { MaxUsers = 2 }));

        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));
        _manager._users[1] = user;

        // Act
        var output = _manager.CreateUser(new UserJoinOptions() { ActiveScheme = new ActiveInputScheme(definitionName!, "Abc") });

        // Assert
        Assert.True(output.IsSuccessful);
        Assert.Equal("Abc", output.Value.ActiveScheme.DefinitionName);

        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<IInputNotification>(i => i is InputUserJoinedNotification)), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateUser_OptionsUseInvalidActiveSchemeSchemeName_NoDefaultDefinitionOrScheme_ReturnsSuccessfully(string? schemeName)
    {
        // Arrange
        _mockConfigurationProvider.Setup(m => m.Configuration)
            .Returns(new InputSystemConfiguration([],
                [
                    new InputDefinition("Abc",
                    [
                        new InputScheme("Abc", [], false, false),
                        new InputScheme("Def", [], true, false)
                    ], [], false),
                    new InputDefinition("Def",
                    [
                        new InputScheme("Def", [], false, false)
                    ], [], false)
                ],
                new(), new InputSystemJoinPolicy() { MaxUsers = 2 }));

        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));
        _manager._users[1] = user;

        // Act
        var output = _manager.CreateUser(new UserJoinOptions() { ActiveScheme = new ActiveInputScheme("Abc", schemeName!) });

        // Assert
        Assert.True(output.IsSuccessful);
        Assert.Equal("Abc", output.Value.ActiveScheme.DefinitionName);
        Assert.Equal("Def", output.Value.ActiveScheme.SchemeName);

        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<IInputNotification>(i => i is InputUserJoinedNotification)), Times.Once);
    }

    [Fact]
    public void CreateUser_OptionsUsesActiveSchemeThatDoesNotExist_UsesDefaults_ReturnsSuccessfully()
    {
        // Arrange
        _mockConfigurationProvider.Setup(m => m.Configuration)
            .Returns(new InputSystemConfiguration([],
                [
                    new InputDefinition("Abc",
                    [
                        new InputScheme("Abc", [], false, false),
                        new InputScheme("Def", [], true, false)
                    ], [], true),
                    new InputDefinition("Def",
                    [
                        new InputScheme("Def", [], false, false)
                    ], [], false)
                ],
                new(), new InputSystemJoinPolicy() { MaxUsers = 2 }));

        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));
        _manager._users[1] = user;

        // Act
        var output = _manager.CreateUser(new UserJoinOptions() { ActiveScheme = new ActiveInputScheme("QHa", "bad day") });

        // Assert
        Assert.True(output.IsSuccessful);
        Assert.Equal("Abc", output.Value.ActiveScheme.DefinitionName);
        Assert.Equal("Def", output.Value.ActiveScheme.SchemeName);

        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<IInputNotification>(i => i is InputUserJoinedNotification)), Times.Once);
    }

    [Fact]
    public void CreateUser_OptionsIncludeUnpairedDevice_UsesDefaultDefinition_ReturnsSuccessfullyAndNotifiesNewUserAndPairedDevice()
    {
        // Arrange
        _mockConfigurationProvider.Setup(m => m.Configuration)
            .Returns(new InputSystemConfiguration([],
                [
                    new InputDefinition("Abc",
                    [
                        new InputScheme("Abc", [], false, false)
                    ], [], false),
                    new InputDefinition("Def",
                    [
                        new InputScheme("Def", [], false, false)
                    ], [], true)
                ],
                new(), new InputSystemJoinPolicy() { MaxUsers = 2 }));

        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));
        _manager._users[1] = user;

        // Act
        var output = _manager.CreateUser(new UserJoinOptions() { DevicesToPair = [new RuntimeDeviceIdentifier(2, TestIdentity.Identity1)] });

        // Assert
        Assert.True(output.IsSuccessful);
        Assert.Single(output.Value.PairedDevices);
        Assert.Equal("Def", output.Value.ActiveScheme.DefinitionName);

        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<IInputNotification>(i => i is InputUserJoinedNotification)), Times.Once);
        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<IInputNotification>(i => i is DevicePairedNotification)), Times.Once);
    }

    #endregion

    #region SetActiveDefinition

    [Fact]
    public void SetActiveDefinition_UserDoesNotExist_ReturnsNotFound()
    {
        // Arrange/Act
        var output = _manager.SetActiveDefinition(2, "Abc");

        // Assert
        Assert.False(output.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataNotFound, output.StatusCode.SpecificityCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void SetActiveDefinition_InvalidDefinitionName_ReturnsError(string? definitionName)
    {
        // Arrange
        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));
        _manager._users[1] = user;

        // Act
        var output = _manager.SetActiveDefinition(1, definitionName!);

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public void SetActiveDefinition_DefinitionNameDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockConfigurationProvider.Setup(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [], new(), new()));

        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));
        _manager._users[1] = user;

        // Act
        var output = _manager.SetActiveDefinition(1, "Abc");

        // Assert
        Assert.False(output.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataNotFound, output.StatusCode.SpecificityCode);
    }

    [Fact]
    public void SetActiveDefinition_Valid_ReturnsSuccessfully()
    {
        // Arrange
        _mockConfigurationProvider.Setup(m => m.Configuration)
            .Returns(new InputSystemConfiguration([],
            [
                new InputDefinition("Abc",
                [
                    new InputScheme("Abc", [], false, false)
                ], [], false)
            ], new(), new()));

        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));
        _manager._users[1] = user;

        // Act
        var output = _manager.SetActiveDefinition(1, "Abc");

        // Assert
        Assert.True(output.IsSuccessful);
    }

    #endregion

    #region GetInputUserForDevice

    [Fact]
    public void GetInputUserForDevice_NoUsers_ReturnsNull()
    {
        // Arrange/Act
        var user = _manager.GetInputUserForDevice(1);

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public void GetInputUserForDevice_HasUsers_NoneHasDevice_ReturnsNull()
    {
        // Arrange
        _manager._users[1] = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));

        // Act
        var user = _manager.GetInputUserForDevice(1);

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public void GetInputUserForDevice_HasUsers_UserHasDeviceWithId_ReturnsExpectedUser()
    {
        // Arrange
        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));
        _manager._users[1] = user;

        // Act
        var foundUser = _manager.GetInputUserForDevice(1);

        // Assert
        Assert.NotNull(foundUser);
        Assert.Equal(user, foundUser);
    }

    #endregion

    #region GetUser

    [Fact]
    public void GetUser_UserIdDoesNotExist_ReturnsNull()
    {
        // Arrange/Act
        var user = _manager.GetUser(100);

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public void GetUser_Valid_ReturnsUser()
    {
        // Arrange
        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));
        _manager._users[1] = user;

        // Act
        var u = _manager.GetUser(1);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(user, u);
    }

    #endregion

    #region GetUsers

    [Fact]
    public void GetUsers_NoUsers_ReturnsEmpty()
    {
        // Arrange/Act/Assert
        Assert.Empty(_manager.GetUsers());
    }

    [Fact]
    public void GetUsers_HasUsers_ReturnsList()
    {
        // Arrange
        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));
        _manager._users[1] = user;

        // Act
        var users = _manager.GetUsers();

        // Assert
        Assert.Single(users);
        Assert.Equal(user, users.ElementAt(0));
    }

    #endregion

    #region RemoveUser

    [Fact]
    public void RemoveUser_UserDoesNotExist_ReturnsFalse()
    {
        // Arrange/Act
        var result = _manager.RemoveUser(10);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RemoveUser_UserExists_ReturnsTrue()
    {
        // Arrange
        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));
        _manager._users[1] = user;

        // Act
        var result = _manager.RemoveUser(1);

        // Assert
        Assert.True(result);
        Assert.Empty(_manager._users);
    }

    #endregion

    #region PairDevice

    [Fact]
    public void PairDevice_UserDoesNotExist_ReturnsNotFound()
    {
        // Arrange/Act
        var output = _manager.PairDevice(10, new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));

        // Assert
        Assert.False(output.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataNotFound, output.StatusCode.SpecificityCode);
    }

    [Fact]
    public void PairDevice_DeviceAlreadyPairedToAnotherUser_ReturnsError()
    {
        // Arrange
        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));
        _manager._users[1] = user;

        var user2 = new InputUser(2, new ActiveInputScheme("Abc", "Abc"));
        _manager._users[2] = user2;

        // Act
        var output = _manager.PairDevice(2, new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public void PairDevice_UserExists_DeviceNotPaired_ReturnsSuccessfully()
    {
        // Arrange
        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        _manager._users[1] = user;

        // Act
        var output = _manager.PairDevice(1, new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));

        // Assert
        Assert.True(output.IsSuccessful);
        Assert.Single(user._pairedDevices);
    }

    [Fact]
    public void PairDevice_UserExists_DeviceAlreadyPaired_ReturnsSuccessfully()
    {
        // Arrange
        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));

        _manager._users[1] = user;

        // Act
        var output = _manager.PairDevice(1, new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));

        // Assert
        Assert.True(output.IsSuccessful);
        Assert.Single(user._pairedDevices);
    }

    #endregion

    #region UnpairDevice

    [Fact]
    public void UnpairDevice_UserDoesNotExist_ReturnsFalse()
    {
        // Arrange/Act
        var result = _manager.UnpairDevice(2, 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UnpairDevice_UserExists_DeviceNotPairedToUser_ReturnsFalse()
    {
        // Arrange
        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        _manager._users[1] = user;

        // Act
        var result = _manager.UnpairDevice(1, 1);

        // Assert
        Assert.False(result);
    }


    [Fact]
    public void UnpairDevice_UserExists_DevicePairedToUser_ReturnsTrueAndNotifiesUnpairedEvent()
    {
        // Arrange
        var user = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        user.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));

        _manager._users[1] = user;

        // Act
        var result = _manager.UnpairDevice(1, 1);

        // Assert
        Assert.True(result);
        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<IInputNotification>(i => i is DeviceUnpairedNotification)), Times.Once);
    }

    #endregion

    #region SavePreferredSchemeAsync

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    public async Task SavePreferredSchemeAsync_InvalidUserIdForJoinPolicy_ReturnsError(int userId)
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [], new(), new() { MaxUsers = 10 }));

        // Act
        var output = await _manager.SavePreferredSchemeAsync(new PreferredInputScheme() { UserId = userId, DefinitionName = "Abc", SchemeName = "Abc" });

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task SavePreferredSchemeAsync_InvalidDefinitionName_ReturnsError(string? name)
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [], new(), new() { MaxUsers = 1 }));

        // Act
        var output = await _manager.SavePreferredSchemeAsync(new PreferredInputScheme() { UserId = 1, DefinitionName = name!, SchemeName = "Abc" });

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public async Task SavePreferredSchemeAsync_DefinitionNameNotFound_ReturnsError()
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [], new(), new() { MaxUsers = 1 }));

        // Act
        var output = await _manager.SavePreferredSchemeAsync(new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", SchemeName = "Abc" });

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task SavePreferredSchemeAsync_InvalidSchemeName_ReturnsError(string? name)
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [], new(), new() { MaxUsers = 1 }));

        // Act
        var output = await _manager.SavePreferredSchemeAsync(new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", SchemeName = name! });

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public async Task SavePreferredSchemeAsync_InputSchemeNameNotFound_ReturnsError()
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], 
            [
                new InputDefinition("Abc", [], [], false)
            ], new(), new() { MaxUsers = 1 }));

        // Act
        var output = await _manager.SavePreferredSchemeAsync(new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", SchemeName = "Abc" });

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public async Task SavePreferredSchemeAsync_Valid_ReturnsSuccessfully()
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([],
            [
                new InputDefinition("Abc", [], [], false)
            ], new(), new() { MaxUsers = 1 }));

        _mockSchemeRepository.Setup(m => m.SavePreferredSchemeAsync(It.IsAny<PreferredInputScheme>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PreferredInputScheme p, CancellationToken _) => _outputFactory.Succeed(p));

        // Act
        var output = await _manager.SavePreferredSchemeAsync(new PreferredInputScheme() { UserId = 1, DefinitionName = "Abc", SchemeName = "Abc" });

        // Assert
        Assert.False(output.IsSuccessful);
    }

    #endregion

    #region LoadUserConfigurationAsync

    [Fact]
    public async Task LoadUserConfigurationAsync_SchemeRepositoryReturnsErrors_ReturnsSuccessfullyWithDefaults()
    {
        // Arrnage
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [], new(), new()));

        _mockSchemeRepository.Setup(m => m.GetCustomSchemesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Fail<IEnumerable<CustomInputScheme>>("A bad day"));
        _mockSchemeRepository.Setup(m => m.GetPreferredSchemesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Fail<IEnumerable<PreferredInputScheme>>("A bad day"));

        // Act
        var output = await _manager.LoadUserConfigurationAsync();

        // Assert
        Assert.True(output.IsSuccessful);
    }

    [Fact]
    public async Task LoadUserConfigurationAsync_VariousPreferredSchemesThatDoAndDontExistOrAreMultiples_ReturnsSuccessfullyWithExpectedLookup()
    {
        // Arrnage
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([],
            [
                new InputDefinition("Abc", [
                    new InputScheme("Abc", [], false, false)
                    ], [], false)
            ], new(), new() { MaxUsers = 2 }));

        _mockSchemeRepository.Setup(m => m.GetCustomSchemesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Fail<IEnumerable<CustomInputScheme>>("A bad day"));
        _mockSchemeRepository.Setup(m => m.GetPreferredSchemesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed((IEnumerable<PreferredInputScheme>)[
                    new PreferredInputScheme() { UserId = -1, SchemeName = "Abc", DefinitionName = "Abc" },
                    new PreferredInputScheme() { UserId = 100, SchemeName = "Abc", DefinitionName = "Abc" },
                    new PreferredInputScheme() { UserId = 1, SchemeName = "Abc", DefinitionName = "Abc" },
                    new PreferredInputScheme() { UserId = 1, SchemeName = "Abc", DefinitionName = "Abc" },
                    new PreferredInputScheme() { UserId = 2, SchemeName = "Abc", DefinitionName = "Abc" }
                ]));

        // Act
        var output = await _manager.LoadUserConfigurationAsync();

        // Assert
        Assert.True(output.IsSuccessful);
        Assert.Equal(2, _manager._userPreferredSchemesLookup.Count);
        Assert.True(_manager._userPreferredSchemesLookup.TryGetValue(1, out var preferences) && preferences.Count is 1 && preferences.TryGetValue("Abc", out _));
        Assert.True(_manager._userPreferredSchemesLookup.TryGetValue(2, out preferences) && preferences.Count is 1 && preferences.TryGetValue("Abc", out _));
    }

    #endregion
}
