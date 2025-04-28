using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Internal;

/// <summary>
/// The purpose of this object is simply to 'mask' an input in the case of virtual inputs. A prominent example would be combination inputs in which
/// an input action is triggered for either a combination or a singular input that consists of the same input (i.e. Shift + A or simply A) 
/// In this particular case, we need to be able to differentiate the input and trigger phase for the A key in 2 or more combinations, but implementations will have
/// to deal with the combination logic in similar ways. Allowing a masked input will help in tracking teh input triggers for these virtual use cases by allow us to change
/// values sent to input readers as needed to accomodate shared logic.
/// </summary>
internal class MaskedInput: IInput
{
    #region Variables

    private readonly int _maskedId;
    private readonly IInput _input;
    private readonly IInput? _parentInput;

    #endregion

    #region Constructors

    public MaskedInput(IInput input, IInput? parentInput = null)
    {
        _input = input;
        _parentInput = parentInput;
        _maskedId = GenerateUniqueId(input.Id, parentInput?.Id);
    }

    #endregion

    #region IInput

    public int Id => _maskedId;

    public string DeviceType => _input.DeviceType;

    public string Name => _input.Name;

    #endregion

    #region Public

    public IInput RawInput => _input;

    public IInput? ParentInput => _parentInput;

    #endregion

    #region Helpers

    private int GenerateUniqueId(int id, int? parentId)
    {
        return parentId is null
            ? id
            : (parentId.Value << 16) | (id & 0xFFFF);
    }

    #endregion
}
