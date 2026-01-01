using System.Collections.Generic;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Internal.Models;

internal class InputUser(int id, ActiveInputScheme activeScheme): IInputUser
{
    #region Variables

    private Dictionary<int, PairedDevice> _pairedDevices = [];

    #endregion

    #region IApplicationUser

    public int Id => id;

    public ActiveInputScheme ActiveScheme { get; set; } = activeScheme;

    public IReadOnlyCollection<PairedDevice> PairedDevices => _pairedDevices.Values;

    public PairedDevice? GetDevice(int deviceId)
        => _pairedDevices.TryGetValue(deviceId, out var device)
            ? device
            : null;

    #endregion

    #region Helpers

    public void AddDevice(RuntimeDeviceIdentifier deviceIdentifier)
    {
        _pairedDevices[deviceIdentifier.DeviceId] = new PairedDevice(Id, deviceIdentifier);
    }

    public PairedDevice? RemoveDevice(int deviceId)
    {
        if (_pairedDevices.TryGetValue(deviceId, out var device))
        {
            _pairedDevices.Remove(deviceId);
            return device;
        }

        return null;
    }

    public IReadOnlyCollection<PairedDevice> GetPairedDevices()
        => _pairedDevices.Values;

    public PairedDevice? GetPairedDevice(int id)
        => _pairedDevices.TryGetValue(id, out var device)
            ? device
            : null;

    #endregion
}
