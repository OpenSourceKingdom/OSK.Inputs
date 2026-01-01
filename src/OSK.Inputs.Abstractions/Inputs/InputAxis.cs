using System;

namespace OSK.Inputs.Abstractions.Inputs;

public readonly struct InputAxis(string name, int id) : IEquatable<InputAxis>
{
    #region Variables

    public int Id => id;

    public string Name => name ?? string.Empty;

    #endregion

    #region Overrides

    public override string ToString()
    {
        return $"Axis: {Name}, {id}";
    }

    public override bool Equals(object obj)
    {
        return obj is InputAxis other && Equals(other);
    }

    public override int GetHashCode()
    {
        return id;
    }

    public bool Equals(InputAxis other)
    {
        return Id == id;
    }

    #endregion
}
