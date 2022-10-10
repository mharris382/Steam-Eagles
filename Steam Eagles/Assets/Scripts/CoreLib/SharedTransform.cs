using CoreLib;
using UnityEngine;

namespace StateMachine
{
    [CreateAssetMenu(menuName = "Shared Variables/Create Shared Transform", fileName = "SharedTransform", order = 0)]
    public class SharedTransform : SharedVariable<Transform> { }
}