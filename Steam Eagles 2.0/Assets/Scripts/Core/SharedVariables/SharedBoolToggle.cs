using UnityEngine;

namespace Core.SharedVariables
{
    /// <summary>
    /// updates the value of the shared bool when component is enabled and disabled
    /// <see cref="SharedBool"/>
    /// </summary>
    public class SharedBoolToggle : SharedVariableToggleAssigner<bool, SharedBool>
    {
        protected override bool ResolveValue(bool isOn) => isOn;
    }
}