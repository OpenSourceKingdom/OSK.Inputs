using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal;
internal class RuntimeInputDevice(InputDeviceIdentifier deviceIdentifier, IInputDeviceConfiguration configuration,
    IInputDeviceReader inputReader) : IDisposable
{
    #region Variables

    public InputDeviceIdentifier DeviceIdentifier => deviceIdentifier;

    public IInputDeviceConfiguration Configuration => configuration;

    public IInputDeviceReader InputReader => inputReader;

    public InputDeviceActionMap? ActionMap { get; private set; }

    private readonly DeviceInputReadContext _readContext = new DeviceInputReadContext(deviceIdentifier.DeviceName);
    private readonly Dictionary<int, VirtualInput> _virtualInputLookup = [];

    #endregion

    #region Public

    public async Task<IEnumerable<ActivatedInput>> ReadInputsAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return [];
        }

        _readContext.Reset();

        foreach (var input in _readContext.ActiveInputs)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await InputReader.ReadInputAsync(_readContext, input, cancellationToken);
        }

        var activatedInputLookup = _readContext.GetInputActivations()
            .Where(input => input.TriggeredPhase is not InputPhase.Idle)
            .ToDictionary(input => input.Input.Id);

        foreach (var virtualInput in _virtualInputLookup.Values)
        {
            if (virtualInput is CombinationInput combinationInput
                 && combinationInput.Inputs.All(input => activatedInputLookup.TryGetValue(input.Id, out _)))
            {
                ActivatedInput? selectedActiveInput = null;
                foreach (var input in combinationInput.Inputs)
                {
                    var activatedInput = activatedInputLookup[input.Id];

                    // Goal: Keep selected active input as base for the virtual input phase
                    selectedActiveInput = (selectedActiveInput?.TriggeredPhase, activatedInput.TriggeredPhase) switch
                    {
                        (InputPhase.Start, InputPhase.Active) => selectedActiveInput, // Start is preferred over Active
                        (InputPhase.Active, InputPhase.Active) => selectedActiveInput, // Active can only be active if all inputs are active
                        (InputPhase.End, _) => selectedActiveInput, // End is always preferred

                        _ => activatedInput
                    };

                    activatedInputLookup.Remove(input.Id);
                }

                activatedInputLookup[virtualInput.Id] = new ActivatedInput(
                    DeviceIdentifier.DeviceName,
                    virtualInput,
                    selectedActiveInput!.TriggeredPhase,
                    selectedActiveInput.InputPower,
                    selectedActiveInput.PointerInformation);
            }
        }

        return activatedInputLookup.Values;
    }

    public void SetActiveInputs(InputDeviceActionMap actionMap, IEnumerable<IInput> inputs) 
    {
        ActionMap = actionMap;
        _virtualInputLookup.Clear();

        Dictionary<int, IInput> inputsToRead = [];
        foreach (var input in inputs)
        {
            IEnumerable<IInput> newInputs = [ input ];
            if (input is CombinationInput combinationInput)
            {
                newInputs = combinationInput.Inputs;
                _virtualInputLookup[input.Id] = combinationInput;
            }

            foreach (var newInput in newInputs)
            {
                inputsToRead[newInput.Id] = newInput;
            }
        }

        _readContext.ActiveInputs = inputsToRead.Values;
    }

    public void Dispose()
    {
        inputReader.Dispose();
    }

    #endregion
}
