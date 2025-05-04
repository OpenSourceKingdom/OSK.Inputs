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

    public event Action<ApplicationUserInputDeviceEvent> OnInputDeviceDisconnected = _ => { };
    public event Action<ApplicationUserInputDeviceEvent> OnInputDeviceReconnected = _ => { };
    public event Action<ApplicationUserInputDeviceEvent> OnInputDeviceAdded = _ => { };

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

    public async Task<IOutput<ActiveInputScheme>> SetActiveInputSchemeAsync(int userId, string inputDefinitionId, string controllerId,
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
        if (inputSystemConfiguration.InputControllers.FirstOrDefaultByString(controller => controller.ControllerName, controllerId) is null)
        {
            return outputFactory.NotFound<ActiveInputScheme>($"The input controller with the name {controllerId} is not supported with the input system.");
        }
        if (getInputDefinitionOutput.Value.InputSchemes.FirstOrDefaultByString(scheme => scheme.Name, schemeId) is null)
        {
            return outputFactory.NotFound<ActiveInputScheme>($"The input scheme with the name {schemeId} was not found for the {controllerId} controller");
        }

        var saveActiveSchemeOutput = await inputSchemeRepository.SaveActiveInputSchemeAsync(new ActiveInputScheme(userId, inputDefinitionId, controllerId, schemeId), cancellationToken);
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

    public async Task<IOutput> ResetUserActiveInputSchemeAsync(int userId, string inputDefinitionId, string controllerId, CancellationToken cancellationToken = default)
    {
        if (!_userLookup.TryGetValue(userId, out var user))
        {
            return outputFactory.NotFound<ActiveInputScheme>($"User id {userId} was not found.");
        }

        var deleteActiveSchemeResult = await inputSchemeRepository.DeleteActiveInputSchemeAsync(userId, inputDefinitionId, controllerId, cancellationToken);
        if (deleteActiveSchemeResult.IsSuccessful && user.ActiveInputDefinition.Name.Equals(inputDefinitionId, StringComparison.Ordinal) 
                && user.GetActiveInputScheme(controllerId) != null)
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

    public async Task<IOutput> DeleteCustomInputSchemeAsync(string inputDefinitionName, string controllerId, string schemeName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inputDefinitionName))
        {
            throw new ArgumentNullException(nameof(inputDefinitionName));
        }
        if (string.IsNullOrWhiteSpace(controllerId))
        {
            throw new ArgumentNullException(nameof(controllerId));
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

        var inputScheme = inputDefinition.InputSchemes.Where(scheme => scheme.ControllerId.Equals(controllerId, StringComparison.Ordinal))
            .FirstOrDefaultByString(scheme => scheme.Name, schemeName);
        if (inputScheme is null)
        {
            return outputFactory.Succeed();
        }
        
        if (inputScheme is BuiltInInputScheme)
        {
            return outputFactory.Fail($"Input controller {controllerId} input scheme {inputScheme} for input definition {inputDefinitionName} can not be deleted because it is built in.");
        }

        if (!inputSystemConfiguration.AllowCustomInputSchemes)
        {
            return outputFactory.Succeed();
        }

        var deleteResult = await inputSchemeRepository.DeleteCustomInputSchemeAsync(inputDefinitionName, controllerId, schemeName, cancellationToken);
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
        user = new ApplicationInputUser(userId, inputSystemConfiguration);

        var inputDevices = options.DeviceIdentifiers.Select(deviceIdentifier => GetInputDevice(userId, deviceIdentifier));
        user.AddInputDevices(inputDevices.ToArray());
        user.SetActiveInputDefinition(getInputDefinitionOutput.Value, getActiveInputSchemesOutput.Value);

        user.OnInputDeviceReconnected += NotifiyUserInputDeviceConnected;
        user.OnInputDeviceDisconnected += NotifiyUserInputDeviceDisconnected;


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

    public void PairDevice(int userId, InputDeviceIdentifier deviceIdentifier)
    {
        var pairedUser = _userLookup.Values.FirstOrDefault(user => user.TryGetDevice(deviceIdentifier.DeviceId, out _));
        if (pairedUser is not null)
        {
            if (pairedUser.Id == userId)
            {
                return;
            };

            throw new InvalidOperationException($"Unable to pair a device to user {userId} since it is already paired to user {pairedUser.Id}");
        }

        if (!_userLookup.TryGetValue(userId, out var user))
        {
            throw new InvalidOperationException($"User with id {userId} has not been added to the input system");
        }

        var inputDevice = GetInputDevice(userId, deviceIdentifier);
        user.AddInputDevices(inputDevice);
        OnInputDeviceAdded(new ApplicationUserInputDeviceEvent(user, deviceIdentifier));
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
        CancellationToken[] cancellationTokens = readOptions.DeviceReadTime.HasValue && readOptions.DeviceReadTime > TimeSpan.Zero
            ? [cancellationToken, new CancellationTokenSource(readOptions.DeviceReadTime.Value).Token]
            : [cancellationToken];

        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokens);

        var userInputs = await _userLookup.Values.ExecuteConcurrentResultAsync(
            user => user.ReadInputsAsync(readOptions.MaxConcurrentDevices, cancellationTokenSource.Token),
            maxDegreesOfConcrruency: readOptions.MaxConcurrenUsers,
            isParallel: readOptions.RunInputUsersInParallel,
            cancellationToken: cancellationTokenSource.Token);

        return new InputActivationContext(serviceProvider, userInputs.SelectMany(inputs => inputs));
    }

    #endregion

    #region Helpers

    private void NotifiyUserInputDeviceConnected(int userId, RuntimeInputDevice device)
    {
        if (_userLookup.TryGetValue(userId, out var applicationInputUser))
        {
            OnInputDeviceReconnected(new ApplicationUserInputDeviceEvent(applicationInputUser, device.DeviceIdentifier));
        }
    }

    private void NotifiyUserInputDeviceDisconnected(int userId, RuntimeInputDevice device)
    {
        if (_userLookup.TryGetValue(userId, out var applicationInputUser))
        {
            OnInputDeviceDisconnected(new ApplicationUserInputDeviceEvent(applicationInputUser, device.DeviceIdentifier));
        }
    }

    private RuntimeInputDevice GetInputDevice(int userId, InputDeviceIdentifier deviceIdentifier)
    {
        var deviceConfiguration = inputSystemConfiguration.SupportedInputDevices.FirstOrDefaultByString(configuration
            => configuration.DeviceName.Name, deviceIdentifier.DeviceName.Name);
        if (deviceConfiguration is null)
        {
            throw new InvalidOperationException($"No device with the name of {deviceIdentifier.DeviceName} was configured for support with the input system.");
        }

        var inputReader = inputReaderProvider.GetInputReader(deviceConfiguration, deviceIdentifier);
        return new RuntimeInputDevice(userId, deviceIdentifier, deviceConfiguration, inputReader);
    }

    private async Task<IOutput<IEnumerable<InputScheme>>> GetActiveInputSchemesForUserAsync(int userId, InputDefinition inputDefinition, CancellationToken cancellationToken)
    {
        var getActiveInputSchemesOutput = await inputSchemeRepository.GetActiveInputSchemesAsync(userId, inputDefinition.Name, cancellationToken);
        if (!getActiveInputSchemesOutput.IsSuccessful)
        {
            return getActiveInputSchemesOutput.AsOutput<IEnumerable<InputScheme>>();
        }

        var userActiveInputSchemeLookup = getActiveInputSchemesOutput.Value.ToDictionary(scheme => scheme.ControllerName);
        var activeInputSchemes = inputDefinition.InputSchemes.GroupBy(scheme => scheme.ControllerId).Select(controllerSchemeGroup =>
        {
            var activeScheme = userActiveInputSchemeLookup.TryGetValue(controllerSchemeGroup.Key, out var activeInputScheme)
                ? controllerSchemeGroup.FirstOrDefaultByString(scheme => scheme.Name, activeInputScheme.ActiveInputSchemeName)
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
