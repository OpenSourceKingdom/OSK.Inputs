using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Events;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal class InputManager(InputSystemConfiguration inputSystemConfiguration, IInputValidationService validationService, IInputSchemeRepository inputSchemeRepository,
    IInputReaderProvider inputReaderProvider, IServiceProvider serviceProvider, IOutputFactory<InputManager> outputFactory) : IInputManager
{
    #region Variables
    
    private readonly Dictionary<int, ApplicationInputUser> _userLookup = [];

    #endregion

    #region IInputManager

    public InputSystemConfiguration Configuration => inputSystemConfiguration;

    public event Action<ApplicationUserInputControllerEvent> OnInputControllerDisconnected = _ => { };
    public event Action<ApplicationUserInputControllerEvent> OnInputControllerConnected = _ => { };
    public event Action<ApplicationUserInputControllerEvent> OnInputControllerAdded = _ => { };

    public ValueTask<IOutput<IEnumerable<InputDefinition>>> GetInputDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        if (inputSystemConfiguration.AllowCustomInputSchemes)
        {
            return new ValueTask<IOutput<IEnumerable<InputDefinition>>>(GetInputDefinitionsWithCustomSchemesAsync(cancellationToken));
        }

        return new ValueTask<IOutput<IEnumerable<InputDefinition>>>(outputFactory.Succeed(inputSystemConfiguration.InputDefinitions.Select(definition => definition.Clone())));
    }

    public async Task<IOutput> SetUserActiveInputDefinitionAsync(int userId, string inputDefinitionName, CancellationToken cancellationToken = default)
    {
        if (!_userLookup.TryGetValue(userId, out var user))
        {
            return outputFactory.NotFound($"User id {userId} was not found.");
        }
        if (user.ActiveInputDefinition.Name.Equals(inputDefinitionName, StringComparison.OrdinalIgnoreCase))
        {
            return outputFactory.Succeed();
        }

        var getInputDefinitionOutput = await GetInputDefinitionWithCustomSchemesAsync(inputDefinitionName, cancellationToken);
        if (!getInputDefinitionOutput.IsSuccessful)
        {
            return getInputDefinitionOutput;
        }

        var getActiveInputSchemes = await GetActiveInputSchemesForUserAsync(userId, getInputDefinitionOutput.Value, cancellationToken);
        if (!getActiveInputSchemes.IsSuccessful)
        {
            return outputFactory.Fail("Unable to set the active input scheme for the input definition.");
        }
        user.SetActiveInputDefinition(getInputDefinitionOutput.Value, getActiveInputSchemes.Value);

        return outputFactory.Succeed();
    }

    public async Task<IOutput<ActiveInputScheme>> SetActiveInputSchemeAsync(int userId, string inputDefinitionId, InputDeviceName deviceName,
        string schemeId, CancellationToken cancellationToken = default)
    {
        if (!_userLookup.TryGetValue(userId, out var user))
        {
            return outputFactory.NotFound<ActiveInputScheme>($"User id {userId} was not found.");
        }

        var getInputDefinitionOutput = await GetInputDefinitionWithCustomSchemesAsync(inputDefinitionId, cancellationToken);
        if (!getInputDefinitionOutput.IsSuccessful)
        {
            return getInputDefinitionOutput.AsOutput<ActiveInputScheme>();
        }
        if (inputSystemConfiguration.SupportedInputControllers.FirstOrDefaultByString(controllerConfiguration => controllerConfiguration.ControllerName.Name, deviceName.Name) is null)
        {
            return outputFactory.NotFound<ActiveInputScheme>($"The input controller with the name {deviceName} is not supported with the input system.");
        }
        if (getInputDefinitionOutput.Value.InputSchemes.FirstOrDefaultByString(scheme => scheme.SchemeName, schemeId) is null)
        {
            return outputFactory.NotFound<ActiveInputScheme>($"The input scheme with the name {schemeId} was not found for the {deviceName} controller");
        }

        var saveActiveSchemeOutput = await inputSchemeRepository.SaveActiveInputSchemeAsync(new ActiveInputScheme(userId, inputDefinitionId, deviceName.Name, schemeId), cancellationToken);
        if (!saveActiveSchemeOutput.IsSuccessful)
        {
            return saveActiveSchemeOutput;
        }

        if (user.ActiveInputDefinition.Name.Equals(inputDefinitionId, StringComparison.OrdinalIgnoreCase))
        {
            var getActiveInputSchemes = await GetActiveInputSchemesForUserAsync(userId, user.ActiveInputDefinition, cancellationToken);
            if (!getActiveInputSchemes.IsSuccessful)
            {
                return outputFactory.Fail<ActiveInputScheme>("Unable to set the active input scheme after saving.", getActiveInputSchemes.StatusCode.SpecificityCode);
            }
            user.SetActiveInputDefinition(user.ActiveInputDefinition, getActiveInputSchemes.Value);
        }

        return saveActiveSchemeOutput;
    }

    public async Task<IOutput> ResetUserActiveInputSchemeAsync(int userId, string inputDefinitionId, InputDeviceName deviceName, CancellationToken cancellationToken = default)
    {
        if (!_userLookup.TryGetValue(userId, out var user))
        {
            return outputFactory.NotFound<ActiveInputScheme>($"User id {userId} was not found.");
        }

        var deleteActiveSchemeResult = await inputSchemeRepository.DeleteActiveInputSchemeAsync(userId, inputDefinitionId, deviceName, cancellationToken);
        if (deleteActiveSchemeResult.IsSuccessful && user.ActiveInputDefinition.Name.Equals(inputDefinitionId, StringComparison.Ordinal) 
                && user.GetActiveInputScheme(deviceName) != null)
        {
            var getActiveInputSchemesOutput = await GetActiveInputSchemesForUserAsync(userId, user.ActiveInputDefinition, cancellationToken);
            if (!getActiveInputSchemesOutput.IsSuccessful)
            {
                return getActiveInputSchemesOutput;
            }

            user.SetActiveInputDefinition(user.ActiveInputDefinition, getActiveInputSchemesOutput.Value);
        }

        return deleteActiveSchemeResult;
    }

    public Task<IOutput<InputScheme>> SaveCustomInputSchemeAsync(InputScheme inputScheme, CancellationToken cancellationToken = default)
    {
        if (inputScheme is null)
        {
            throw new ArgumentNullException(nameof(inputScheme));
        }
        if (string.IsNullOrWhiteSpace(inputScheme.InputDefinitionName))
        {
            return Task.FromResult(outputFactory.Fail<InputScheme>("Input definition name must be set on an input scheme."));
        }

        var inputDefinition = inputSystemConfiguration.InputDefinitions.FirstOrDefaultByString(inputDefinition 
                => inputDefinition.Name, inputScheme.InputDefinitionName);
        if (inputDefinition is null)
        {
            return Task.FromResult(outputFactory.NotFound<InputScheme>($"Input Definition {inputScheme.InputDefinitionName} was not found."));
        }

        if (!inputSystemConfiguration.AllowCustomInputSchemes)
        {
            return Task.FromResult(outputFactory.Fail<InputScheme>($"The input definition {inputDefinition.Name} does not supported custom schemes."));
        }

        var validationContext = validationService.ValidateCustomInputScheme(inputSystemConfiguration, inputScheme);
        if (validationContext.Errors.Any())
        {
            var error = string.Join("\n", validationContext.Errors.Select(e => e.Message));
            return Task.FromResult(outputFactory.Fail<InputScheme>($"Unable to save input scheme as there were validation errors.\n{error}"));
        }

        return inputSchemeRepository.SaveCustomInputSchemeAsync(inputDefinition.Name, inputScheme, cancellationToken);
    }

    public async Task<IOutput> DeleteCustomInputSchemeAsync(string inputDefinitionName, InputDeviceName deviceName, string schemeName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inputDefinitionName))
        {
            throw new ArgumentNullException(nameof(inputDefinitionName));
        }
        if (string.IsNullOrWhiteSpace(deviceName.Name))
        {
            throw new ArgumentNullException(nameof(deviceName));
        }
        if (string.IsNullOrWhiteSpace(schemeName))
        {
            throw new ArgumentNullException(nameof(schemeName));
        }

        var inputDefinition = inputSystemConfiguration.InputDefinitions.FirstOrDefaultByString(definition => definition.Name, inputDefinitionName);
        if (inputDefinition is null)
        {
            return outputFactory.Succeed();
        }

        var controller = inputSystemConfiguration.SupportedInputControllers.FirstOrDefaultByString(controller => controller.ControllerName.Name,
            deviceName.Name);
        if (controller is null)
        {
            return outputFactory.Succeed();
        }

        var inputScheme = inputDefinition.InputSchemes.FirstOrDefaultByString(scheme => scheme.SchemeName, schemeName);
        if (inputScheme is BuiltInInputScheme)
        {
            return outputFactory.Fail($"Input controller {deviceName} input scheme {inputScheme} for input definition {inputDefinitionName} can not be deleted because it is built in.");
        }

        if (!inputSystemConfiguration.AllowCustomInputSchemes)
        {
            return outputFactory.Succeed();
        }

        var deleteResult = await inputSchemeRepository.DeleteCustomInputSchemeAsync(inputDefinitionName, deviceName, schemeName, cancellationToken);
        return deleteResult.IsSuccessful || deleteResult.StatusCode.SpecificityCode == OutputSpecificityCode.DataNotFound
            ? outputFactory.Succeed()
            : deleteResult;
    }

    public async Task<IOutput<IApplicationInputUser>> JoinUserAsync(int userId, JoinUserOptions options, CancellationToken cancellationToken = default)
    {
        if (_userLookup.TryGetValue(userId, out var user))
        {
            return outputFactory.Succeed((IApplicationInputUser)user);
        }
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }
        if (_userLookup.Count >= inputSystemConfiguration.MaxLocalUsers)
        {
            return outputFactory.Fail<IApplicationInputUser>($"Unable to add another user to input system since the limit is {inputSystemConfiguration.MaxLocalUsers}");
        }

        var activeDefinitionName = options.ActiveInputDefinitionName ?? inputSystemConfiguration.InputDefinitions.First().Name;
        var getInputDefinitionOutput = await GetInputDefinitionWithCustomSchemesAsync(activeDefinitionName, cancellationToken);
        if (!getInputDefinitionOutput.IsSuccessful)
        {
            return getInputDefinitionOutput.AsOutput<IApplicationInputUser>();
        }

        var getActiveInputSchemesOutput = await GetActiveInputSchemesForUserAsync(userId, getInputDefinitionOutput.Value, cancellationToken);
        if (!getActiveInputSchemesOutput.IsSuccessful)
        {
            return getActiveInputSchemesOutput.AsOutput<IApplicationInputUser>();
        }

        var definition = inputSystemConfiguration.InputDefinitions.FirstByString(definition => definition.Name, activeDefinitionName);
        var inputControllers = options.ControllerIdentifiers.Select(GetInputController);
        user = new ApplicationInputUser(userId, inputSystemConfiguration);
        user.SetActiveInputDefinition(getInputDefinitionOutput.Value, getActiveInputSchemesOutput.Value);

        user.OnInputControllerConnected += NotifiyUserInputControllerConnected;
        user.OnInputControllerDisconnected += NotifiyUserInputControllerDisconnected;

        user.AddInputControllers(inputControllers.ToArray());

        _userLookup.Add(userId, user);
        return outputFactory.Succeed((IApplicationInputUser)user);
    }

    public void RemoveUser(int userId)
    {
        if (_userLookup.TryGetValue(userId, out var user))
        {
            _userLookup.Remove(userId);
            user.Dispose();
        }
    }

    public void PairController(int userId, InputControllerIdentifier controllerIdentifier)
    {
        var pairedUser = _userLookup.Values.FirstOrDefault(user => user.TryGetController(controllerIdentifier.ControllerId, out _));
        if (pairedUser is not null)
        {
            if (pairedUser.Id == userId)
            {
                return;
            };

            throw new InvalidOperationException($"Unable to pair a controller to user {userId} since it is already paired to user {pairedUser.Id}");
        }

        if (!_userLookup.TryGetValue(userId, out var user))
        {
            throw new InvalidOperationException($"User with id {userId} has not been added to the input system");
        }

        var inputController = GetInputController(controllerIdentifier);
        user.AddInputControllers(inputController);
        OnInputControllerAdded(new ApplicationUserInputControllerEvent(user, controllerIdentifier));
    }

    public IApplicationInputUser GetApplicationInputUser(int userId)
        => _userLookup.TryGetValue(userId, out var applicationInputUser)
            ? applicationInputUser
            : throw new InvalidOperationException($"No user with the id {userId} was added to the input system.");

    public IEnumerable<IApplicationInputUser> GetApplicationInputUsers()
    {
        return _userLookup.Values;
    }

    public async Task<InputActivationContext> ReadInputsAsync(InputReadOptions readOptions, CancellationToken cancellationToken = default)
    {
        CancellationToken[] cancellationTokens = readOptions.ControllerReadTime.HasValue && readOptions.ControllerReadTime > TimeSpan.Zero
            ? [cancellationToken, new CancellationTokenSource(readOptions.ControllerReadTime.Value).Token]
            : [cancellationToken];

        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokens);

        var userInputs = await _userLookup.Values.ExecuteConcurrentResultAsync(
            user => user.ReadInputsAsync(cancellationTokenSource.Token),
            maxDegreesOfConcrruency: readOptions.MaxConcurrentControllers,
            isParallel: readOptions.RunInputUsersInParallel,
            cancellationToken: cancellationTokenSource.Token);

        return new InputActivationContext(serviceProvider, userInputs.SelectMany(inputs => inputs));
    }

    #endregion

    #region Helpers

    private void NotifiyUserInputControllerConnected(int userId, InputController controller)
    {
        if (_userLookup.TryGetValue(userId, out var applicationInputUser))
        {
            OnInputControllerConnected(new ApplicationUserInputControllerEvent(applicationInputUser, controller.ControllerIdentifier));
        }
    }

    private void NotifiyUserInputControllerDisconnected(int userId, InputController controller)
    {
        if (_userLookup.TryGetValue(userId, out var applicationInputUser))
        {
            OnInputControllerDisconnected(new ApplicationUserInputControllerEvent(applicationInputUser, controller.ControllerIdentifier));
        }
    }

    private InputController GetInputController(InputControllerIdentifier controllerIdentifier)
    {
        var controllerConfiguration = inputSystemConfiguration.SupportedInputControllers.FirstOrDefaultByString(controllerConfiguration
            => controllerConfiguration.ControllerName.Name, controllerIdentifier.ControllerName.Name);
        if (controllerConfiguration is null)
        {
            throw new InvalidOperationException($"No controller with the name of {controllerIdentifier.ControllerName} was configured for support with the input system.");
        }

        var inputReader = inputReaderProvider.GetInputReader(controllerConfiguration, controllerIdentifier);
        return new InputController(controllerIdentifier, controllerConfiguration, inputReader);
    }

    private async Task<IOutput<IEnumerable<InputScheme>>> GetActiveInputSchemesForUserAsync(int userId, InputDefinition inputDefinition, CancellationToken cancellationToken)
    {
        var getActiveInputSchemesOutput = await inputSchemeRepository.GetActiveInputSchemesAsync(userId, inputDefinition.Name, cancellationToken);
        if (!getActiveInputSchemesOutput.IsSuccessful)
        {
            return getActiveInputSchemesOutput.AsOutput<IEnumerable<InputScheme>>();
        }

        var userActiveInputSchemeLookup = getActiveInputSchemesOutput.Value.ToDictionary(scheme => scheme.ControllerName);
        var activeInputSchemes = inputDefinition.InputSchemes.GroupBy(scheme => scheme.ControllerName.Name).Select(controllerSchemeGroup =>
        {
            var activeScheme = userActiveInputSchemeLookup.TryGetValue(controllerSchemeGroup.Key, out var activeInputScheme)
                ? controllerSchemeGroup.FirstOrDefaultByString(scheme => scheme.SchemeName, activeInputScheme.ActiveInputSchemeName)
                : null;

            return activeScheme ?? controllerSchemeGroup.FirstOrDefault(scheme => scheme.IsDefault) ?? controllerSchemeGroup.First();
        }).ToArray();

        return outputFactory.Succeed((IEnumerable<InputScheme>)activeInputSchemes);
    }

    private async Task<IOutput<IEnumerable<InputDefinition>>> GetInputDefinitionsWithCustomSchemesAsync(CancellationToken cancellationToken)
    {
        List<InputDefinition> definitions = [];
        foreach (var inputDefinition in inputSystemConfiguration.InputDefinitions)
        {
            var getInputDefinitionOutput = await GetInputDefinitionWithCustomSchemesAsync(inputDefinition.Name, cancellationToken);
            if (!getInputDefinitionOutput.IsSuccessful)
            {
                return getInputDefinitionOutput.AsOutput<IEnumerable<InputDefinition>>();
            }

            definitions.Add(getInputDefinitionOutput.Value);
        }

        return outputFactory.Succeed((IEnumerable<InputDefinition>)definitions);
    }

    private async Task<IOutput<InputDefinition>> GetInputDefinitionWithCustomSchemesAsync(string inputDefinitionName, CancellationToken cancellationToken)
    {
        var inputDefinition = inputSystemConfiguration.InputDefinitions.FirstOrDefaultByString(definition => definition.Name, inputDefinitionName);
        if (inputDefinition is null)
        {
            return outputFactory.NotFound<InputDefinition>($"Input definition with name {inputDefinitionName} was not configured with the input system.");
        }

        IEnumerable<InputScheme> customSchemes = [];
        if (inputSystemConfiguration.AllowCustomInputSchemes)
        {
            var getCustomInputSchemesOutput = await inputSchemeRepository.GetCustomInputSchemesAsync(inputDefinition.Name, cancellationToken);
            if (!getCustomInputSchemesOutput.IsSuccessful)
            {
                return getCustomInputSchemesOutput.AsOutput<InputDefinition>();
            }

            customSchemes = getCustomInputSchemesOutput.Value;
        }

        return outputFactory.Succeed(inputDefinition.Clone(additionalInputSchemes: customSchemes));
    }

    #endregion
}
