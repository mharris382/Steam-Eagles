using UnityEngine;

namespace Tools.DestructTool.Helpers
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class DestructionCircleCollider : DestructionCollider
    {
        public override float PlacementRadius => (Collider as CircleCollider2D).radius;
    }
}