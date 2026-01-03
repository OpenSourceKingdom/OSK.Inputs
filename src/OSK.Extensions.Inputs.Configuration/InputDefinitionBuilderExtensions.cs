using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using OSK.Expressions.Invoker;
using OSK.Extensions.Inputs.Configuration.Attributes;
using OSK.Extensions.Inputs.Configuration.Options;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Extensions.Inputs.Configuration;

public static class InputDefinitionBuilderExtensions
{
    extension(IInputDefinitionBuilder builder)
    {
        public IInputDefinitionBuilder WithActions<TService>()
            where TService : class
        {
            var methodsToRegister = typeof(TService).GetMethods().Where(method =>
            {
                var methodParameters = method.GetParameters();
                return methodParameters.Length is 1 && methodParameters[0].ParameterType == typeof(InputEventContext)
                 && method.ReturnParameter.ParameterType == typeof(void);
            });

            foreach (var method in methodsToRegister)
            {
                var inputActionAttribute = method.GetCustomAttribute<InputActionAttribute>();
                var inputActionName = string.IsNullOrWhiteSpace(inputActionAttribute?.ActionName)
                    ? method.Name
                    : inputActionAttribute.ActionName;
                var invoker = InvokerFactory.CreateInvoker<TService>(method);

                builder.WithAction(inputActionName,
                    inputEventContext => invoker.FastInvoke(inputEventContext.Services.GetRequiredService<TService>(), [inputEventContext]),
                    inputActionAttribute?.TriggerPhases ?? [InputPhase.Start],
                    new InputActionOptions() 
                    { 
                        Description = inputActionAttribute?.Description, 
                        IncludePointerInformation = inputActionAttribute?.IncludePointerDetails ?? false
                    });
            }

            return builder;
        }
    }
}
