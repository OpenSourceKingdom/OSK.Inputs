using System;
using System.Threading;
using System.Threading.Tasks;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Ports;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Internal.Services;

internal class InputSystem(IInputConfigurationProvider configurationProvider, IInputUserManager userManager,
    IInputProcessor inputProcessor, IInputNotificationPublisher notificationPublisher, IInputSchemeRepository schemeRepository,
    IOutputFactory<InputSystem> outputFactory) : IInputSystem
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

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await UserManager.LoadUserConfigurationAsync(cancellationToken);
    }

    public async Task<IOutput> DeleteCustomSchemeAsync(string definitionName, string schemeName, CancellationToken cancellationToken = default)
    {
        if (!AllowCustomSchemes 
            || string.IsNullOrWhiteSpace(definitionName)
            || string.IsNullOrWhiteSpace(schemeName)
            || configurationProvider.Configuration.GetDefinition(definitionName) is null)
        {
            return outputFactory.Succeed();
        }

        return await schemeRepository.DeleteCustomSchemeAsync(definitionName, schemeName, cancellationToken);
    }

    public async Task<IOutput> SaveCustomSchemeAsync(CustomInputScheme scheme, CancellationToken cancellationToken = default)
    {
        if (scheme is null)
        {
            throw new ArgumentNullException(nameof(scheme));
        }

        if (!AllowCustomSchemes)
        {
            return outputFactory.Fail("Custom input schemes are not allowed with the input system. If it is desired, please register a scheme repository that can support it.");
        }

        if (string.IsNullOrWhiteSpace(scheme.DefinitionName))
        {
            return outputFactory.Fail("Definition name can not be empty.");
        }
        if (string.IsNullOrWhiteSpace(scheme.Name))
        {
            return outputFactory.Fail("Name can not be empty.");
        }
        if (configurationProvider.Configuration.GetDefinition(scheme.DefinitionName) is null)
        {
            return outputFactory.Fail($"No input definition with the name {scheme.DefinitionName} exists.");
        }

        return await schemeRepository.SaveCustomInputScheme(scheme, cancellationToken);
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
