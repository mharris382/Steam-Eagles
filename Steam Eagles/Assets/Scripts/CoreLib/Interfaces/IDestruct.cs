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
        public readonly Vector3Int cellPosition;
        public readonly Vector2 position;
        public readonly Vector2 direction;
        public readonly bool isCellTarget;

        public DestructParams(Vector2 position, Vector2 direction)
        {
            this.position = position;
            this.direction = direction;
            isCellTarget = false;
            cellPosition = default;
        }
        public DestructParams(Vector2Int cellPosition)
        {
            this.cellPosition = (Vector3Int)cellPosition;
            isCellTarget = true;
            position = default;
            direction = default;
        }
        public DestructParams(Vector3Int cellPosition)
        {
            this.cellPosition = cellPosition;
            isCellTarget = true;
            position = default;
            direction = default;
        }
        


        public static implicit operator DestructParams(RaycastHit2D hit) => new(hit.point, -hit.normal);
    }
}