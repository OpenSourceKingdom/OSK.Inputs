using System.Collections.Generic;
using System.Numerics;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Internal.Models;

internal class InputPointerState(int pointerId, int maxRecords): DeviceInputState
{
    #region Variables

    private readonly Queue<PointerLocationRecord> _pointerRecords = [];

    #endregion

    #region Api

    public int PointerId => pointerId;

    public (Vector2, PointerMotion)? GetCurrentPositionAndMotionData()
    {
        PointerLocationRecord? currentRecord = null;
        var currentVelocity = Vector2.Zero;
        var currentAcceleration = Vector2.Zero;

        foreach (var record in _pointerRecords)
        {
            if (currentRecord is null)
            {
                currentRecord = record;
                continue;
            }

            var deltaTime = (record.Time - currentRecord.Value.Time).TotalSeconds;
            var newVelocity = deltaTime > 0
                ? (record.Position - currentRecord.Value.Position) / (float) deltaTime
                : Vector2.Zero;

            currentAcceleration = deltaTime > 0
                ? (newVelocity - currentVelocity) / (float)deltaTime
                : Vector2.Zero;
            currentVelocity = newVelocity;
            currentRecord = record;
        }

        return currentRecord is null
            ? null
            : (currentRecord.Value.Position, new PointerMotion(currentVelocity, currentAcceleration));
    }

    public void AddRecord(Vector2 position)
    {
        _pointerRecords.Enqueue(new PointerLocationRecord(position, Duration));
        if (_pointerRecords.Count > maxRecords)
        {
            _pointerRecords.Dequeue();
        }
    }

    #endregion
}
