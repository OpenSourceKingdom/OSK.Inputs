using System;

namespace OSK.Inputs.Abstractions;

public class PointerDetails(PointerData[] pointers)
{
    #region Static

    public static PointerDetails Empty = new([]);

    #endregion

    #region Public

    public bool HasPointers => pointers.Length > 0;

    public int TotalPointers => pointers.Length;

    public PointerData this[int key] =>
       key < 0 || key >= pointers.Length 
        ? throw new ArgumentOutOfRangeException(nameof(key))
        : pointers[key];

    #endregion
}
