using System;
using System.Threading;
using System.Threading.Tasks;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Ports;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Notifications;
using OSK.Inputs.Models;
using OSK.Inputs.Exceptions;
using OSK.Operations.Outputs;
using OSK.Operations.Outputs.Models;

namespace OSK.Inputs.Internal.Services;

internal class InputSystem(IInputConfigurationProvider configurationProvider, IInputUserManager userManager,
    IInputProcessor inputProcessor, IInputNotificationPublisher notificationPublisher, IInputSchemeRepository schemeRepository,
    IInputSystemConfigurationValidator validator) : IInputSystem
{
    #region IInputSystem

    public InputSystemConfiguration Configuration => configurationProvider.Configuration;

    public IInputSystemNotifier Notifier => notificationPublisher;

    public IInputUserManager UserManager => userManager;

    public bool AllowCustomSchemes => schemeRepository.AllowCustomSchemes;

    public bool PausedInput { get; internal set; }

    public void ToggleInputProcessing(bool pause)
    {
        if (pause == PausedInput)
        {
            return;
        }

        PausedInput = pause;
        inputProcessor.ToggleInputProcessing(pause);
        notificationPublisher.Notify(new InputProcessingStateChangedNotification(!pause));
    }

    public async Task<Output> InitializeAsync(InputSystemConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var validationResult = validator.Validate(configuration);
        if (!validationResult.IsValid)
        {
            throw new InputSystemValidationException($"The provided input configuration was invalid. Message: {validationResult}");
        }

        configurationProvider.Configuration = configuration;
        return await UserManager.LoadUserConfigurationAsync(cancellationToken);
    }

    public async Task<Output> DeleteCustomSchemeAsync(string definitionName, string schemeName, CancellationToken cancellationToken = default)
    {
        if (!AllowCustomSchemes 
            || string.IsNullOrWhiteSpace(definitionName)
            || string.IsNullOrWhiteSpace(schemeName)
            || configurationProvider.Configuration.GetDefinition(definitionName) is null)
        {
            return Out.Success();
        }

        return await schemeRepository.DeleteCustomSchemeAsync(definitionName, schemeName, cancellationToken);
    }

    public async Task<Output> SaveCustomSchemeAsync(CustomInputScheme scheme, SchemeSaveFlags saveFlags, CancellationToken cancellationToken = default)
    {
        if (scheme is null)
        {
            throw new ArgumentNullException(nameof(scheme));
        }

        if (!AllowCustomSchemes)
        {
            return Out.InvalidRequest("Custom input schemes are not allowed with the input system. If it is desired, please register a scheme repository that can support it.");
        }

        var schemeValidation = validator.ValidateCustomScheme(configurationProvider.Configuration, scheme, saveFlags.HasFlag(SchemeSaveFlags.Overwrite));
        if (!schemeValidation.IsValid)
        {
            return schemeValidation.Result is InputConfigurationValidation.DuplicateData
                ? Out.DuplicateData($"The scheme name {scheme.Name} already exists on input definition {scheme.DefinitionName}, if overwriting is desired then ensure the save flag is set correctly.")
                : Out.InvalidRequest($"There was a validation error with the custom scheme: {Environment.NewLine}{schemeValidation.Message}");
        }

        var saveOutput = await schemeRepository.SaveCustomInputScheme(scheme, cancellationToken);
        if (!saveOutput.IsSuccessful)
        {
            return saveOutput;
        }

        return await userManager.LoadUserConfigurationAsync();
    }

    public void Update(TimeSpan deltaTime)
    {
        if (PausedInput)
        {
            return;
        }

        inputProcessor.Update(deltaTime);
    }

    #endregion
}
