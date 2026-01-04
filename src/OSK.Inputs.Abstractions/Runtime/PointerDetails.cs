using System;
using System.Linq;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// Provides data for all pointer details associated with the input system
/// </summary>
/// <param name="pointers">The collection of pointers in the input system, this should be a unique collection (i.e. unique id per pointer)</param>
public class PointerDetails(PointerData[] pointers)
{
    #region Static

    /// <summary>
    /// A default empty state for pointers. This possess no pointer data.
    /// </summary>
    public static PointerDetails Empty = new([]);

    #endregion

    #region Public

    /// <summary>
    /// Whether there is pointer data or not
    /// </summary>
    public bool HasPointers { get; } = pointers.Length > 0;

    /// <summary>
    /// The total number of pointers available
    /// </summary>
    public int TotalPointers => pointers.Length;

    /// <summary>
    /// Retrieves a pointer based on an index into the details collection.
    /// 
    /// <br />
    /// Note: The pointers in this array may be considered complete in most cases - that is that all pointers on the devices
    /// will be returned here, but that is not guaranteed. In multi-user scenarios, the collection will only contain the pointers
    /// associated to a specific user, so using a pointer id to get to this pointer is not recommened in multi-user scenarios.
    /// 
    /// If getting a pointer by id is required, please use <see cref="GetPointerById(int)"/>
    /// </summary>
    /// <param name="id">The indexable id for the collection</param>
    /// <returns>A pointer that is in the collection</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the index is outside the bounds (i.e. id must be between 0 and <see cref="TotalPointers"/> </exception>
    public PointerData this[int id] => 
        id < 0 || id >= TotalPointers
        ? throw new ArgumentOutOfRangeException(nameof(id))
        : pointers[id];

    /// <summary>
    /// Attempts to get a pointer by id. This will iterate over the collection of pointers in an attempt to get the pointer, 
    /// though it is not guaranteed the user possesses the pointer in multi-user scenarios.
    /// </summary>
    /// <param name="pointerId">The specific pointer id to get</param>
    /// <returns>The <see cref="PointerData"/> for the given pointer id if it is available, otherwise null</returns>
    public PointerData? GetPointerById(int pointerId)
        => pointers.FirstOrDefault(p => p.Id == pointerId);

    #endregion
}
