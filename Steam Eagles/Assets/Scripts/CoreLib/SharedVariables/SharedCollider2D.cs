using CoreLib;
using UnityEngine;

namespace StateMachine
{
    [CreateAssetMenu(menuName = "Shared Variables/Create Shared Collider2D", fileName = "Shared Collider2D", order = 0)]
    public class SharedCollider2D : SharedVariable<PolygonCollider2D> { }
}