using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OSK.Functions.Outputs.Abstractions;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal class InputManager(IEnumerable<InputDefinition> inputDefinitions, 
    IInputValidationService validationService, IInputSchemeRepository inputSchemeRepository, 
    IServiceProvider serviceProvider, IOutputFactory outputFactory) : IInputManager
{
    #region IInputManager

    public ValueTask<IOutput<IEnumerable<InputDefinition>>> GetInputDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        if (inputDefinitions.Any(inputDefinition => inputDefinition.AllowCustomInputSchemes))
        {
            return new ValueTask<IOutput<IEnumerable<InputDefinition>>>(GetInputDefinitionWithCustomSchemesAsync(cancellationToken));
        }

        return new ValueTask<IOutput<IEnumerable<InputDefinition>>>(outputFactory.Succeed(inputDefinitions.Select(definition => definition.Clone())));
    }

    public async Task<IOutput<IInputHandler>> GetInputHandlerAsync(string inputDefinitionName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inputDefinitionName))
        {
            throw new ArgumentNullException(nameof(inputDefinitionName));
        }

        var inputDefinition = inputDefinitions.FirstOrDefaultByString(definition => definition.Name, inputDefinitionName);
        if (inputDefinition is null)
        {
            return outputFactory.NotFound<IInputHandler>($"Input definition {inputDefinitionName} was not found");
        }

        IEnumerable<InputScheme> customInputSchemes = Enumerable.Empty<InputScheme>();
        if (inputDefinition.AllowCustomInputSchemes)
        {
            var getCustomSchemeOutput = await inputSchemeRepository.GetInputSchemesAsync(inputDefinition.Name, cancellationToken);
            if (!getCustomSchemeOutput.IsSuccessful)
            {
                return getCustomSchemeOutput.AsOutput<IInputHandler>();
            }

            customInputSchemes = getCustomSchemeOutput.Value;
        }

        var getActiveSchemesOutput = await inputSchemeRepository.GetActiveInputSchemesAsync(inputDefinition.Name, cancellationToken);
        if (!getActiveSchemesOutput.IsSuccessful)
        {
            return getActiveSchemesOutput.AsOutput<IInputHandler>();
        }

        var activeSchemeLookup = getActiveSchemesOutput.Value.ToDictionary(scheme => scheme.ControllerName);
        List<RuntimeControllerListener> activeControllers = [];
        foreach (var controller in inputDefinition.SupportedInputControllers)
        {
            activeSchemeLookup.TryGetValue(controller.ControllerName, out var activeControllerScheme);
            var controllerScheme = controller.GetActiveScheme(preferredScheme: activeControllerScheme);

            var validationContext = validationService.ValidateInputScheme(inputDefinition, controllerScheme);
            if (validationContext.Errors.Any())
            {
                return outputFactory.Fail<IInputHandler>($"The {controller.ControllerName} controller's input scheme {controllerScheme.SchemeName} was not compatible with the input definition {inputDefinition.Name}. The custom scheme data file is corrupted.");
            }

            var inputReceivers = controller.ReceiverDescriptors.Select(receiverDescriptor =>
            {
                var inputConfiguration = controllerScheme.ReceiverConfigurations.FirstByString(configuration 
                    => configuration.InputReceiverName, receiverDescriptor.ReceiverName);

                return (IInputReceiver)ActivatorUtilities.CreateInstance(serviceProvider, receiverDescriptor.InputReceiverType,
                    inputConfiguration);
            }).ToArray();

            activeControllers.Add(new RuntimeControllerListener(controller, inputReceivers));
        }

        return outputFactory.Succeed((IInputHandler)ActivatorUtilities.CreateInstance<InputHandler>(serviceProvider, activeControllers));
    }

    public Task<IOutput<InputScheme>> SaveInputSchemeAsync(InputScheme inputScheme, CancellationToken cancellationToken = default)
    {
        if (inputScheme is null)
        {
            throw new ArgumentNullException(nameof(inputScheme));
        }
        if (string.IsNullOrWhiteSpace(inputScheme.InputDefinitionName))
        {
            return Task.FromResult(outputFactory.Fail<InputScheme>("Input definition name must be set on an input scheme."));
        }

        var inputDefinition = inputDefinitions.FirstOrDefaultByString(inputDefinition 
                => inputDefinition.Name, inputScheme.InputDefinitionName);
        if (inputDefinition is null)
        {
            return Task.FromResult(outputFactory.NotFound<InputScheme>($"Input Definition {inputScheme.InputDefinitionName} was not found."));
        }

        if (!inputDefinition.AllowCustomInputSchemes)
        {
            return Task.FromResult(outputFactory.Fail<InputScheme>($"The input definition {inputDefinition.Name} does not supported custom schemes."));
        }

        var validationContext = validationService.ValidateInputScheme(inputDefinition, inputScheme);
        if (validationContext.Errors.Any())
        {
            var error = string.Join("\n", validationContext.Errors.Select(e => e.Message));
            return Task.FromResult(outputFactory.Fail<InputScheme>($"Unable to save input scheme as there were validation errors.\n{error}"));
        }

        return inputSchemeRepository.SaveInputSchemeAsync(inputDefinition.Name, inputScheme, cancellationToken);
    }

    public async Task<IOutput> DeleteInputSchemeAsync(string inputDefinitionName, string controllerName, string schemeName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inputDefinitionName))
        {
            throw new ArgumentNullException(nameof(inputDefinitionName));
        }
        if (string.IsNullOrWhiteSpace(controllerName))
        {
            throw new ArgumentNullException(nameof(controllerName));
        }
        if (string.IsNullOrWhiteSpace(schemeName))
        {
            throw new ArgumentNullException(nameof(schemeName));
        }

        var inputDefinition = inputDefinitions.FirstOrDefaultByString(definition => definition.Name, inputDefinitionName);
        if (inputDefinition is null)
        {
            return outputFactory.Succeed();
        }

        var controller = inputDefinition.SupportedInputControllers.FirstOrDefaultByString(controller => controller.ControllerName,
            controllerName);
        if (controller is null)
        {
            return outputFactory.Succeed();
        }

        var inputScheme = controller.InputSchemes.FirstOrDefaultByString(scheme => scheme.SchemeName, schemeName);
        if (inputScheme is BuiltInInputScheme)
        {
            return outputFactory.Fail($"Input controller {controllerName} input scheme {inputScheme} for input definition {inputDefinitionName} can not be deleted because it is built in.");
        }

        if (!inputDefinition.AllowCustomInputSchemes)
        {
            return outputFactory.Succeed();
        }

        var deleteResult = await inputSchemeRepository.DeleteInputSchemeAsync(inputDefinitionName, controllerName, schemeName, cancellationToken);
        return deleteResult.IsSuccessful || deleteResult.StatusCode.SpecificityCode == OutputSpecificityCode.DataNotFound
            ? outputFactory.Succeed()
            : deleteResult;
    }

    #endregion

    #region Helpers

    private async Task<IOutput<IEnumerable<InputDefinition>>> GetInputDefinitionWithCustomSchemesAsync(CancellationToken cancellationToken)
    {
        List<InputDefinition> definitions = [];
        foreach (var inputDefinition in inputDefinitions)
        {
            if (!inputDefinition.AllowCustomInputSchemes)
            { 
                definitions.Add(inputDefinition.Clone());
                continue;
            }

            var getCustomInputSchemesOutput = await inputSchemeRepository.GetInputSchemesAsync(inputDefinition.Name, cancellationToken);
            if (!getCustomInputSchemesOutput.IsSuccessful)
            {
                return getCustomInputSchemesOutput.AsOutput<IEnumerable<InputDefinition>>();
            }

            definitions.Add(inputDefinition.Clone(additionalInputSchemes: getCustomInputSchemesOutput.Value));
        }

        return outputFactory.Succeed((IEnumerable<InputDefinition>)definitions);
    }

    #endregion
}
