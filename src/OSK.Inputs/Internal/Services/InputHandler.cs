using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal class InputHandler: IInputHandler
{
    #region Variables

    private IInputControllerListener _activeController;
    private readonly IEnumerable<IInputControllerListener> _controllerListener;
    private readonly IOptions<InputHandlerOptions> _options;

    #endregion

    #region Constructors

    public InputHandler(IEnumerable<IInputControllerListener> controllerListeners, IOptions<InputHandlerOptions> options)
    {
        _controllerListener = controllerListeners ?? throw new ArgumentNullException(nameof(controllerListeners));
        if (!controllerListeners.Any())
        {
            throw new ArgumentException(nameof(controllerListeners));
        }

        _options = options ?? throw new ArgumentNullException(nameof(options));

        _activeController = _controllerListener.First();
    }

    #endregion

    #region IInputHandler

    public event Action<InputControllerConfiguration>? OnControllerChanged;

    public async Task<IEnumerable<ActivatedInput>> ReadInputsAsync()
    {
        var activatedInputs = await _activeController.ReadInputsAsync(_options.Value);
        if (activatedInputs.Any())
        {
            return activatedInputs;
        }

        foreach (var inactiveController in _controllerListener.Where(controller => controller != _activeController))
        {
            activatedInputs = await inactiveController.ReadInputsAsync(_options.Value);
            if (activatedInputs.Any())
            {
                if (OnControllerChanged is not null)
                {
                    OnControllerChanged(inactiveController.ControllerConfiguration);
                }
                _activeController = inactiveController;
                return activatedInputs;
            }
        }

        return [];
    }

    public void Dispose()
    {
        foreach (var controllerHandler in _controllerListener)
        {
            controllerHandler.Dispose();
        }
    }

    #endregion

    #region Helpers

    #endregion
}
