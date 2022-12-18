
using UnityEngine;

namespace Core.SharedVariables
{
    public class SharedTransformAssigner : SharedVariableAssigner<Transform, SharedTransform>
    {
        protected override Transform ResolveValue() => transform;
    }
}