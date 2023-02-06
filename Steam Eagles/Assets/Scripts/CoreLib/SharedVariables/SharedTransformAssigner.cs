using UnityEngine;

namespace CoreLib.SharedVariables
{
    public class SharedTransformAssigner : SharedVariableAssigner<Transform, SharedTransform>
    {
        protected override Transform GetVariableAssignment() => transform;
    }
}