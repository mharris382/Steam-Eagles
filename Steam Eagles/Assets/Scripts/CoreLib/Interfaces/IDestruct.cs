using UnityEngine;

namespace CoreLib.Interfaces
{
    public interface IDestruct
    {
        public bool IsFullyDestroyed { get; }
        bool TryToDestruct(DestructParams destructParams);

        bool TryToDestruct(Vector2 position, Vector2 direction) => TryToDestruct(new DestructParams(position, direction));
    }
    
    public struct DestructParams
    {
        public readonly Vector2 position;
        public readonly Vector2 direction;

        public DestructParams(Vector2 position, Vector2 direction)
        {
            this.position = position;
            this.direction = direction;
        }


        public static implicit operator DestructParams(RaycastHit2D hit) => new(hit.point, -hit.normal);
    }
}