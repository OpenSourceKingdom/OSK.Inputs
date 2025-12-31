using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions;

public interface IInputSystemConfigurationBuilder
{
    InputSystemConfiguration Build();
}
