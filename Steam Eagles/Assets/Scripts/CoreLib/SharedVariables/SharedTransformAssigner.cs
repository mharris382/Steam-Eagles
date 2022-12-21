using CoreLib;
using UnityEngine;

namespace StateMachine
{
    public class SharedTransformAssigner : SharedVariableAssigner<Transform, SharedTransform>
    {
        protected override Transform GetVariableAssignment() => transform;
    }
}