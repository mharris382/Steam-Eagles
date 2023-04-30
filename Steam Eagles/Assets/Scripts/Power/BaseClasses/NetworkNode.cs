using System.Collections.Generic;
using Buildings;
using UnityEngine;

namespace Power.Steam
{
    public abstract class NetworkNode
    {
        private readonly Vector3Int _cell;


        protected NetworkNode(Vector3Int cell)
        {
            _cell = cell;
        }

        public Vector3Int Cell => _cell;
        
        public abstract BuildingLayers Layer { get; }

        public IEnumerable<Vector3Int> GetNeighbors()
        {
            yield return _cell + Vector3Int.up;
            yield return _cell + Vector3Int.down;
            yield return _cell + Vector3Int.left;
            yield return _cell + Vector3Int.right;
        }
    }
}