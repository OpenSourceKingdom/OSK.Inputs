using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal class InputSystemBuilder(IServiceCollection services) : IInputSystemBuilder
{
    #region IInputSystemBuilder

    public IInputSystemBuilder UseSchemeRepository<TSchemeRepository>() 
        where TSchemeRepository : class, IInputSchemeRepository
    {
        services.TryAddSingleton<IInputSchemeRepository, TSchemeRepository>();

        return this;
    }

    #endregion
}
