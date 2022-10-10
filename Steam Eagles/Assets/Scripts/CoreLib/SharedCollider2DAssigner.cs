using CoreLib;
using UnityEngine;

namespace StateMachine
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class SharedCollider2DAssigner : SharedVariableAssigner<PolygonCollider2D, SharedCollider2D>
    {
        protected override PolygonCollider2D GetVariableAssignment()
        {
            return GetComponent<PolygonCollider2D>();
        }
    }
}