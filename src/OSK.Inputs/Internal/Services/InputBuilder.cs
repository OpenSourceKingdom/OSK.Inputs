using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal class InputBuilder(IServiceCollection services, Func<string, IInputDefinitionBuilder>? definitionBuilderFactory = null) : IInputBuilder
{
    #region Variables

    private Action<InputHandlerOptions> _optionConfiguration = options =>
    {
        options.RunInputReceiversInParallel = false;
        options.MaxConcurrentReceivers = 2;
        options.ControllerReadTime = TimeSpan.FromMilliseconds(20);
    };
    private Type? _inputSchemeRepositoryType;
    private readonly Dictionary<string, Action<IInputDefinitionBuilder>> _builderActions = [];

    #endregion

    #region IInputBuilder

    public IInputBuilder AddInputDefinition(string name, Action<IInputDefinitionBuilder> buildAction)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        if (buildAction is null)
        {
            throw new ArgumentNullException(nameof(buildAction));
        }
        if (_builderActions.TryGetValue(name, out var action))
        {
            throw new DuplicateNameException($"An input definition with the name {name} has already been added.");
        }

        _builderActions.Add(name, buildAction);
        return this;
    }

    public IInputBuilder WithHandlerOptions(Action<InputHandlerOptions> optionConfiguration)
    {
        if (optionConfiguration is null)
        {
            throw new ArgumentNullException(nameof(optionConfiguration));
        }

        _optionConfiguration = optionConfiguration;
        return this;
    }

    public IInputBuilder UseInputSchemeRepository<TRepository>()
        where TRepository : IInputSchemeRepository
    {
        _inputSchemeRepositoryType = typeof(TRepository);
        return this;
    }

    #endregion

    #region Helpers

    internal void ApplyInputDefinitions()
    {
        definitionBuilderFactory ??= name =>
        {
            return new InputDefinitionBuilder(name, new InputValidationService());
        };

        foreach (var action in _builderActions)
        {
            var inputDefinitionBuilder = definitionBuilderFactory(action.Key);
            action.Value(inputDefinitionBuilder);
            
            var definition = inputDefinitionBuilder.Build();

            services.AddSingleton(definition);
        }

        services.Configure(_optionConfiguration);
        services.AddTransient(typeof(IInputSchemeRepository), _inputSchemeRepositoryType ?? typeof(NoOpInputSchemeRepository));
    }

    #endregion
}
