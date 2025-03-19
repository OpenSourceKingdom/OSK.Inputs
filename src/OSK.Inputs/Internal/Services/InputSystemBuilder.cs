using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal class InputSystemBuilder(IServiceCollection services, 
    Func<string, IEnumerable<IInputControllerConfiguration>, IInputDefinitionBuilder>? definitionBuilderFactory = null,
    Func<IInputValidationService>? validationServiceFactory = null) : IInputSystemBuilder
{
    #region Variables

    private Type? _inputSchemeRepositoryType;
    private int _maxLocalUsers = 1;
    private bool _allowCustomSchemes;
    private readonly Dictionary<string, Action<IInputDefinitionBuilder>> _builderActionLookup = new Dictionary<string, Action<IInputDefinitionBuilder>>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IInputControllerConfiguration> _controllerConfigurationLookup = new Dictionary<string, IInputControllerConfiguration>(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region IInputBuilder

    public IInputSystemBuilder AddInputDefinition(string name, Action<IInputDefinitionBuilder> buildAction)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        if (buildAction is null)
        {
            throw new ArgumentNullException(nameof(buildAction));
        }
        if (_builderActionLookup.TryGetValue(name, out var action))
        {
            throw new DuplicateNameException($"An input definition with the name {name} has already been added.");
        }

        _builderActionLookup.Add(name, buildAction);
        return this;
    }

    public IInputSystemBuilder AddInputController(IInputControllerConfiguration controllerConfiguration)
    {
        if (controllerConfiguration is null)
        {
            throw new ArgumentNullException(nameof(controllerConfiguration));
        }
        if (string.IsNullOrWhiteSpace(controllerConfiguration.ControllerName))
        {
            throw new ArgumentNullException(nameof(controllerConfiguration.ControllerName));
        }
        if (_controllerConfigurationLookup.TryGetValue(controllerConfiguration.ControllerName, out var action))
        {
            throw new DuplicateNameException($"An input controller with the name {controllerConfiguration.ControllerName} has already been added.");
        }

        _controllerConfigurationLookup.Add(controllerConfiguration.ControllerName, controllerConfiguration);
        return this;
    }

    public IInputSystemBuilder WithMaxLocalUsers(int maxLocalUsers)
    {
        if (maxLocalUsers < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLocalUsers));
        }
        _maxLocalUsers = maxLocalUsers;
        return this;
    }

    public IInputSystemBuilder AllowCustomSchemes()
    {
        _allowCustomSchemes = true;
        return this;
    }

    public IInputSystemBuilder UseInputSchemeRepository<TRepository>()
        where TRepository : IInputSchemeRepository
    {
        _inputSchemeRepositoryType = typeof(TRepository);
        return this;
    }

    #endregion

    #region Helpers

    internal void ApplyInputSystemConfiguration()
    {
        definitionBuilderFactory ??= (name, controllerConfigurations) => new InputDefinitionBuilder(name, controllerConfigurations);

        List<InputDefinition> inputDefinitions = [];
        foreach (var action in _builderActionLookup)
        {
            var inputDefinitionBuilder = definitionBuilderFactory(action.Key, _controllerConfigurationLookup.Values);
            action.Value(inputDefinitionBuilder);
            
            var definition = inputDefinitionBuilder.Build();
            inputDefinitions.Add(definition);
        }

        var inputSystemConfiguration = new InputSystemConfiguration(inputDefinitions, _controllerConfigurationLookup.Values, _allowCustomSchemes, _maxLocalUsers);
        var validationService = validationServiceFactory?.Invoke() ?? new InputValidationService();

        var validationContext = validationService.ValidateInputSystemConfiguration(inputSystemConfiguration);
        validationContext.EnsureValid();

        services.AddTransient(typeof(IInputSchemeRepository), _inputSchemeRepositoryType ?? typeof(InMemoryInputSchemeRepository));
        services.AddSingleton(inputSystemConfiguration);
    }

    #endregion
}
