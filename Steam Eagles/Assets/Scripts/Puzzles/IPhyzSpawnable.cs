using UnityEngine;

namespace Puzzles
{
    public class IPhyzSpawnable
    {
        public Rigidbody2D SpawnPrefab { get; }
    }

    public interface IPhyzSpawnInstance
    {
        public Rigidbody2D Rigidbody2D { get; }
        public Collider2D Collider2D { get; }
        void DeSpawnInstance();
    }


}